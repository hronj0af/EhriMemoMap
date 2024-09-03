using EhriMemoMap.Models;
using EhriMemoMap.Shared;
using Microsoft.JSInterop;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Radzen;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace EhriMemoMap.Services
{

    /// <summary>
    /// Pomocné metody k nastavení mapy
    /// </summary>
    public class MapStateService
    {
        private readonly IJSRuntime _js;
        private readonly HttpClient _client;
        private string _appUrl;
        public AppStateEnum AppState;


        public MapStateService(IJSRuntime js, HttpClient client, IConfiguration configuration)
        {
            _js = js;
            _client = client;
            _appUrl = configuration?.GetSection("App")["AppURL"] ?? "";
            AppState = configuration?.GetSection("App")["AppState"] == "Development" ? AppStateEnum.Development : AppStateEnum.Production;
            WidthOfDialogPercent = Math.Round(100 * WidthOfDialogRatio) + "%";
        }

        public decimal WidthOfDialogRatio = 1 / (decimal)3;
        public string WidthOfDialogPercent;
        public int HeightOfDialog = 50;
        
        public int WindowHeight = 0;
        public int WindowWidth = 0;

        public bool IsDialogFullScreen = false;

        public string SearchedPlaceString = "";
        public IEnumerable<Place>? SearchedPlaces;

        public bool MapStateWasInit = false;

        public event Action OnChange;
        public void NotifyStateChanged()
        {
            OnChange?.Invoke();
        }


        private bool isMobileView;
        /// <summary>
        /// Prohlíží si uživatel mapu na mobilu?
        /// </summary>
        public bool IsMobileView
        {
            get { return isMobileView; }
            set
            {
                isMobileView = value;
                NotifyStateChanged();
            }
        }

        private bool showLayersForce;
        /// <summary>
        /// Mají se zobrazit vrstvy bez ohledu na aktuální zoom mapy?
        /// </summary>
        public bool ShowLayersForce
        {
            get { return showLayersForce; }
            set
            {
                showLayersForce = value;
                _js.InvokeVoidAsync("mapAPI.callBlazor_RefreshObjectsOnMap");
                NotifyStateChanged();
            }
        }

        public DialogTypeEnum DialogType = DialogTypeEnum.None;
        public void SetDialogType(DialogTypeEnum value)
        {
            DialogType = value;
            int? height = value == DialogTypeEnum.Welcome || value == DialogTypeEnum.None ? 0 : HeightOfDialog;
            _js.InvokeVoidAsync("mapAPI.fitMapToWindow", height);
            NotifyStateChanged();
        }

        private int topOfElement = 30;

        /// <summary>
        /// Aktuálně nastavený zoom mapy
        /// </summary>
        public float MapZoom;

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
        //public string GetStyleOfMapComponent(VerticalPositionEnum verticalPosition, HorizontalPositionEnum horizontalPosition, int horizontalMargin = 0, int verticalMargin = 0)
        //{
        //    var y = verticalMargin > 0 ? verticalMargin : topOfElement;
        //    var x = DialogType != DialogTypeEnum.None && !IsMobileView && horizontalPosition == HorizontalPositionEnum.Right
        //            ? WidthOfDialogPercent
        //            : $"{horizontalMargin}px";

        //    var buttonStyle = $"cursor:pointer;position:absolute;{verticalPosition.ToString().ToLower()}:{y}px;{horizontalPosition.ToString().ToLower()}:{x};z-index:6000";

        //    return buttonStyle;
        //}

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
                new() { Name = LayerType.Objects.ToString(), Type = LayerType.Objects.ToString(), Selected = true },
                new() { Name = LayerType.Polygons.ToString(), Type = LayerType.Polygons.ToString(), Selected = true },
                new() { Name = "AdditionalObjects", Type = LayerType.Polygons.ToString(), Selected = true, ZIndex = 9999 }
            }).
            ToList();

            return JsonConvert.SerializeObject(new { initialVariables = Map.InitialVariables, layers = result }, serializerSettings);

        }

        /// <summary>
        /// Initializace tohoto objektu na základě mapsettings.json, případně podle parametrů v url
        /// </summary>
        public async Task Init(string? layers = null, string? timelinePoint = null)
        {
            var json = await _client.GetStringAsync(_appUrl + "mapsettings.json"); 
            var settings = JsonConvert.DeserializeObject<MapStateService>(json);
            if (settings == null)
                return;
            Map = settings.Map;
            Map.InitialVariables.HeightOfDialog = HeightOfDialog;
            Map.InitialVariables.WidthOfDialogRatio = WidthOfDialogRatio;

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
            if (Map.Timeline == null || !Map.Timeline.Any(a => a.Selected && a.From != null))
                return null;

            return Map.Timeline.FirstOrDefault(a => a.Selected && a.From != null)?.From;
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
            //var height = await _js.InvokeAsync<int>("mapAPI.getWindowHeight");
            return new SideDialogOptions()
            {
                ShowClose = true,
                ShowTitle = false,
                Position = IsMobileView ? DialogPosition.Bottom : DialogPosition.Right,
                ShowMask = false,
                CssClass = IsMobileView ? "" : "side-dialog",
                Style = IsMobileView ? (DialogType == DialogTypeEnum.Help || DialogType == DialogTypeEnum.Welcome ? "z-index:50000" : "z-index:10000") : "",
                Height = IsMobileView ? (DialogType == DialogTypeEnum.Help || DialogType == DialogTypeEnum.Welcome ? WindowHeight + "px" : HeightOfDialog + "%") : "",
                Width = !IsMobileView ? WidthOfDialogPercent : ""
            };

        }
    }
}
