using EhriMemoMap.Models;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Radzen;
using System.Net.NetworkInformation;

namespace EhriMemoMap.Services
{

    /// <summary>
    /// Pomocné metody k nastavení mapy
    /// </summary>
    public class MapStateService
    {
        public bool DialogIsFullscreen = false;

        public void SetDialogIsFullscreen(bool value)
        {
            DialogIsFullscreen = value;
            NotifyStateChanged();
        }

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

        private bool isLayersOpen = false;
        /// <summary>
        /// Je otevřený seznam vrstev?
        /// </summary>
        public bool IsLayersOpen
        {
            get { return isLayersOpen; }
            set
            {
                isLayersOpen = value;
                if (IsTimelineOpen)
                    isTimelineOpen = !IsTimelineOpen;
                NotifyStateChanged();
            }
        }

        private bool isTimelineOpen = false;
        /// <summary>
        /// Je otevřený seznam s časovou osou?
        /// </summary>
        public bool IsTimelineOpen
        {
            get { return isTimelineOpen; }
            set
            {
                isTimelineOpen = value;
                if (IsLayersOpen)
                    isLayersOpen = !IsLayersOpen;
                NotifyStateChanged();
            }
        }

        private bool isSearchOpen = false;

        /// <summary>
        /// Je otevřené pole pro fulltextové hledání?
        /// </summary>
        public bool IsSearchOpen
        {
            get { return isSearchOpen; }
            set
            {
                isSearchOpen = value;
                NotifyStateChanged();
            }
        }

        private int topOfElement = 10;
        private int rightOfElement = 10;

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
        public async Task<(string Button, string Box)> GetStyleOfMapComponent(PositionEnum position, int index, int? windowWidth = null)
        {
            var y = topOfElement + 45 * index + (position == PositionEnum.Bottom ? 20 : 0);
            var x = rightOfElement;

            var buttonStyle = $"position:absolute;{position.ToString().ToLower()}:{y}px;right:{x}px;z-index:600";
            var boxStyle = $"{(windowWidth != null ? $"width:{windowWidth - 110}px;" : null)}position:absolute;{position.ToString().ToLower()}:{y}px;right:{x + 60}px;z-index:600";

            return (buttonStyle, boxStyle);
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

            var result = Map.Layers?.Where(a => a.Type != LayerType.Objects).Select(a => new LayerModel
            {
                Name = a.Name,
                Opacities = a.Opacities,
                Selected = a.Selected,
                Url = a.Url,
                Type = a.Type,
            }).
            Union(new List<LayerModel>
            {
                new LayerModel { Name = LayerType.Objects.ToString(), Type = LayerType.Objects },
                new LayerModel { Name = LayerType.Images.ToString(), Type = LayerType.Images }
            }).
            ToList();

            return JsonConvert.SerializeObject(result, serializerSettings);

        }

        /// <summary>
        /// Initializace tohoto objektu na základě mapsettings.json, případně podle parametrů v url
        /// </summary>
        public void Init(string? layers = null, string? collectionName = null)
        {
            var settings = JsonConvert.DeserializeObject<MapStateService>(GetJsonSettingsContent());
            if (settings == null)
                return;
            Map = settings.Map;

            if (!string.IsNullOrEmpty(layers))
                InitInfoAboutLayersSelection(layers.Split(','));
            else
                InitInfoAboutLayersSelection();

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
                if (layers == null || layers.Contains(layer.Name))
                    layer.Selected = true;
                else
                    layer.Selected = false;
            });

            Map.Timeline?.Where(a => a.AdditionalLayers != null).ToList().ForEach(layer =>
            {
                if (layers == null || layers.Contains(layer.Name))
                    layer.Selected = true;
                else
                    layer.Selected = false;
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
        {
            if (Map == null || Map.Timeline == null || Map.Timeline.Count == 0)
                return new List<TimelinePointModel>();

            return Map.Timeline;
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
        /// Vrátí název vybrané kolekce / bodu na časové ose pro nastavení v url query
        /// </summary>
        /// <returns></returns>
        public string? GetCollectionForUrlParameter()
            => GetTimeline()?.FirstOrDefault(a => a.Selected)?.Name;


        /// <summary>
        /// Vrátí nastavení pro dialogové okno, aby se dobře vykreslovalo na mobilech i desktopech
        /// </summary>
        /// <returns></returns>
        public SideDialogOptions GetDialogOptions()
        {
            return new SideDialogOptions()
            {
                ShowClose = true,
                ShowTitle = false,
                Position = IsMobileBrowser ? DialogPosition.Bottom : DialogPosition.Left,
                ShowMask = false,
                CssClass = IsMobileBrowser ? "" : "side-dialog",
                Style = "background-color:#eeeeee",
                Height = IsMobileBrowser ? "50%" : "",
                Width = !IsMobileBrowser ? "33%" : ""
            };

        }
    }
}
