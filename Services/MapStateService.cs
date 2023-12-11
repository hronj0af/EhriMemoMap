using EhriMemoMap.Models;
using Microsoft.JSInterop;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Radzen;
using System.Net.NetworkInformation;
using System.Reflection.Metadata.Ecma335;

namespace EhriMemoMap.Services
{

    /// <summary>
    /// Pomocné metody k nastavení mapy
    /// </summary>
    public class MapStateService
    {
        private readonly IJSRuntime _js;
        public MapStateService(IJSRuntime js)
        {
            _js = js;
        }

        public bool DialogIsFullscreen = false;

        public void SetDialogIsFullscreen(bool value)
        {
            DialogIsFullscreen = value;
            NotifyStateChanged();
        }

        public string WidthOfDialog = "33%";
        public string HeightOfDialog = "50%";
        
        public bool MapStateWasInit = false;

        public event Action OnChange;
        private void NotifyStateChanged()
            => OnChange?.Invoke();


        private bool isMobileBrowser;
        /// <summary>
        /// Prohlíží si uživatel mapu na mobilu?
        /// </summary>
        public bool IsMobileBrowser
        {
            get { return isMobileBrowser; }
            set
            {
                isMobileBrowser = value;
                NotifyStateChanged();
            }
        }

        public DialogTypeEnum DialogType = DialogTypeEnum.None;
        public void SetDialogType(DialogTypeEnum value)
        {
            DialogType = value;
            NotifyStateChanged();
        }

        private int topOfElement = 30;
        private string rightOfElement = "0%";

        /// <summary>
        /// Aktuálně nastavený zoom mapy
        /// </summary>
        public int MapZoom;

        /// <summary>
        /// Koordináty levého horního rohu zobrazené mapy
        /// </summary>
        public Coordinate MapSouthWestPoint;

        /// <summary>
        /// Koordináty pravého dolního rohu zobrazené mapy
        /// </summary>
        public Coordinate MapNorthEastPoint;

        public void SetBBox(Coordinate[] coordinates)
        {
            if (coordinates == null || coordinates.Length != 2)
                return;
            MapSouthWestPoint = coordinates[0];
            MapNorthEastPoint = coordinates[1];
        }

        /// <summary>
        /// Vrátí informace o stylu mapových čudlíků a boxíků, aby se to zobrazovalo na správném místě a bylo to vidět
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public async Task<string> GetStyleOfMapComponent(VerticalPositionEnum verticalPosition, HorizontalPositionEnum horizontalPosition, int horizontalMargin = 0, int verticalMargin = 0)
        {
            var y = verticalMargin > 0 ? verticalMargin : topOfElement;
            var x = DialogType != DialogTypeEnum.None && !IsMobileBrowser && horizontalPosition == HorizontalPositionEnum.Right
                    ? WidthOfDialog 
                    : $"{horizontalMargin}px";

            var buttonStyle = $"cursor:pointer;position:absolute;{verticalPosition.ToString().ToLower()}:{y}px;{horizontalPosition.ToString().ToLower()}:{x};z-index:6000";

            return buttonStyle;
        }

        /// <summary>
        /// Seznam podkladových map
        /// </summary>
        public MapModel Map { get; set; }

        public string GetMapInfoForLeaflet()
        {
            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var result = Map.Layers?.Where(a => a.Type != LayerType.Objects).Select(a => new LayerForLeafletModel
            {
                Name = a.Name,
                Selected = a.Selected,
                Attribution = a.Attribution,
                Url = a.Url,
                Type = a.Type?.ToString(),
                MapParameter = a.MapParameter,
                LayersParameter = a.LayersParameter
            }).
            Union(new List<LayerForLeafletModel>
            {
                new LayerForLeafletModel { Name = LayerType.Objects.ToString(), Type = LayerType.Objects.ToString(), Selected = true },
                new LayerForLeafletModel { Name = LayerType.Polygons.ToString(), Type = LayerType.Polygons.ToString(), Selected = true },
                new LayerForLeafletModel { Name = "AdditionalObjects", Type = LayerType.Polygons.ToString(), Selected = true, ZIndex = 9999 }
            }).
            ToList();

            return JsonConvert.SerializeObject(result, serializerSettings);

        }

        /// <summary>
        /// Initializace tohoto objektu na základě mapsettings.json, případně podle parametrů v url
        /// </summary>
        public void Init(string? layers = null, string? timelinePoint = null)
        {
            var settings = JsonConvert.DeserializeObject<MapStateService>(GetJsonSettingsContent());
            if (settings == null)
                return;
            Map = settings.Map;

            
            if (!string.IsNullOrEmpty(layers))
                InitInfoAboutLayersSelection(layers.Split(','));
            else
                InitInfoAboutLayersSelection();

            InitInfoAboutTimeline(timelinePoint);

            MapStateWasInit = true;
            NotifyStateChanged();
        }

        /// <summary>
        /// Když se načítá poprvé nastavení mapy, nastav správně příznak Selected u všech vrstev;
        /// pokud je ale zadaný parametr layers s vybranými vrstvami, pak nastav příznak Selected jen u těchto vybraných
        /// </summary>
        private void InitInfoAboutLayersSelection(string[]? layers = null)
        {

            Map.Layers?.ForEach(layer =>
            {
                if (layer.Type == LayerType.Base || layers == null || layers.Contains(layer.Name))
                    layer.Selected = true;
                else
                    layer.Selected = false;
            });

            foreach (var layer in Map.Timeline?.Where(a=>a.AdditionalLayers != null).SelectMany(a=>a.AdditionalLayers))
            {
                if (layers == null || layers.Contains(layer.Name))
                    layer.Selected = true;
                else
                    layer.Selected = false;
            }
        }

        /// <summary>
        /// Když se načítá poprvé nastavení mapy, nastav správně příznak Selected u bodů časové osy;
        /// </summary>
        private void InitInfoAboutTimeline(string? timelinePoint)
        {
            if (Map.Timeline == null)
                return;
            
            Map.Timeline?.ForEach(point =>
            {
                if ((string.IsNullOrEmpty(timelinePoint) && point.From == null) || point.Name == timelinePoint)
                    point.Selected = true;
                else
                    point.Selected = false;
            });
        }

        /// <summary>
        /// Vrátí obsah json souboru, který obsahuje nastavení mapy
        /// </summary>
        /// <returns></returns>
        public string GetJsonSettingsContent()
        {
            var jsonFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mapSettings.json");
            using StreamReader reader = new(jsonFile);
            var json = reader.ReadToEnd();
            return json;
        }

        /// <summary>
        /// Vrátí všechny vrstvy z nastavení mapy, které nejsou základní (OSM); 
        /// </summary>
        /// <returns></returns>
        public List<LayerModel> GetNotBaseLayers(bool onlySelected = false)
        {
            if (Map == null || Map.Layers == null || !Map.Layers.Any(a => a.Type != LayerType.Base))
                return new List<LayerModel>();

            var result = Map.Layers.Where(a => a.Type != LayerType.Base && (!onlySelected || a.Selected)).ToList();

            if (Map.Timeline == null || !Map.Timeline.Any(a => a.Selected && a.AdditionalLayers != null))
                return result;

            result.AddRange(Map.Timeline.
                Where(a => a.Selected && a.AdditionalLayers != null).
                SelectMany(a => a.AdditionalLayers.Where(b => !onlySelected || b.Selected)));

            return result;
        }

        /// <summary>
        /// Vrátí všechny kolekce v mapách podle aktuálně vybraného jazyka
        /// </summary>
        /// <returns></returns>
        public List<TimelinePointModel>? GetTimeline()
            => Map?.Timeline;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        public void ToggleTimeLinePoint(TimelinePointModel point)
        {
            if (Map.Timeline == null)
                return;
            Map.Timeline.ForEach(a => a.Selected = point.Name == a.Name ? point.Selected : false);
            point.Selected = !point.Selected;
        }

        public DateTime? GetTimelinePoint()
        {
            if (Map.Timeline == null || !Map.Timeline.Any(a => a.Selected && a.To != null))
                return null;

            return Map.Timeline.FirstOrDefault(a => a.Selected && a.To != null)?.To;
        }

        /// <summary>
        /// Vrátí názvy vybraných mapových vrstev pro nastavení v url query
        /// </summary>
        /// <returns></returns>
        public string? GetLayersForUrlParameter()
            => GetNotBaseLayers()?.Count(a => a.Selected) == 0
                ? ""
                : GetNotBaseLayers()?.Where(a => a.Selected).Select(a => a.Name)?.Aggregate((x, y) => x + "," + y);

        /// <summary>
        /// Vrátí název vybraného bodu na časové ose pro nastavení v url query
        /// </summary>
        /// <returns></returns>
        public string? GetTimelineForUrlParameter()
            => GetTimeline()?.FirstOrDefault(a => a.Selected)?.Name;


        /// <summary>
        /// Vrátí nastavení pro dialogové okno, aby se dobře vykreslovalo na mobilech i desktopech
        /// </summary>
        /// <returns></returns>
        public async Task<SideDialogOptions> GetDialogOptions()
        {
            var height = await _js.InvokeAsync<int>("mapAPI.getWindowHeight");
            return new SideDialogOptions()
            {
                ShowClose = true,
                ShowTitle = false,
                Position = IsMobileBrowser ? DialogPosition.Bottom : DialogPosition.Right,
                ShowMask = false,
                CssClass = IsMobileBrowser ? "" : "side-dialog",
                Style = IsMobileBrowser ? "z-index:10000" : "",
                Height = IsMobileBrowser ? (DialogType == DialogTypeEnum.Help ? height + "px" : (height - 49) + "px") : "",
                Width = !IsMobileBrowser ? WidthOfDialog : ""
            };

        }
    }
}
