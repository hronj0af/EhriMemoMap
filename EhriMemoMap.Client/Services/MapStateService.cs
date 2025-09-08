using EhriMemoMap.Client.Components.Dialogs;
using EhriMemoMap.Models;
using EhriMemoMap.Shared;
using Microsoft.JSInterop;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Radzen;

namespace EhriMemoMap.Client.Services
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
        private DialogService _dialogService;


        public MapStateService(IJSRuntime js, HttpClient client, IConfiguration configuration, DialogService dialogService)
        {
            _js = js;
            _client = client;
            _appUrl = configuration?.GetSection("App")["AppURL"] ?? "";
            AppState = configuration?.GetSection("App")["AppState"] == "Development"
                ? AppStateEnum.Development
                    : configuration?.GetSection("App")["AppState"] == "Shutdown"
                        ? AppStateEnum.Shutdown
                        : AppStateEnum.Production;
            WidthOfDialogPercent = Math.Round(100 * WidthOfDialogRatio) + "%";
            _dialogService = dialogService;
        }

        public decimal WidthOfDialogRatio = 1 / (decimal)3;
        public string WidthOfDialogPercent;
        public int HeightOfDialog = 50;

        public int WindowHeight = 0;
        public int WindowWidth = 0;

        public bool IsDialogFullScreen = false;

        public string SearchedPlaceString = "";
        public IEnumerable<SolrPlace>? SearchedPlaces;

        public bool MapStateWasInit = false;

        public event Action OnChange;
        public NarrativeMap? NarrativeMap { get; set; }
        public List<NarrativeMap> AllNarrativeMaps { get; set; } = [];

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


        public DialogParameters DialogParameters { get; set; } = new DialogParameters();

        public DialogTypeEnum DialogType = DialogTypeEnum.None;

        public async Task SetDialog(DialogTypeEnum newDialogType, DialogParameters? parameters = null)
        {
            if (newDialogType == DialogTypeEnum.None)
            {
                await _dialogService.CloseSideAsync();
                await _js.InvokeVoidAsync("mapAPI.removeAdditionalObjects");
                await _js.InvokeVoidAsync("mapAPI.unselectAllSelectedPoints");
            }

            int? height = newDialogType == DialogTypeEnum.Welcome || newDialogType == DialogTypeEnum.None ? 0 : HeightOfDialog;

            await _js.InvokeVoidAsync("mapAPI.fitMapToWindow", height, IsMobileView ? "100%" : newDialogType != DialogTypeEnum.None ? "67%" : "100%");

            if (DialogType == DialogTypeEnum.None && newDialogType != DialogTypeEnum.None)
                _dialogService.OpenSide<DialogContentRouter>("", options: GetDialogOptions());

            DialogType = newDialogType;
            DialogParameters = parameters ?? new DialogParameters();
        }

        public MapTypeEnum MapType = MapTypeEnum.Normal;
        public async Task SetMapType(MapTypeEnum newValue)
        {
            if (newValue == MapTypeEnum.Normal)
            {
                await _js.InvokeVoidAsync("mapAPI.resetMapViewToInitialState");
                await _js.InvokeVoidAsync("mapAPI.showAllLayers");

                NarrativeMap = null;
            }
            else if (newValue == MapTypeEnum.StoryMapOneStop)
            { 
                await _js.InvokeVoidAsync("mapAPI.hideAllLayers"); 
            }

            if (isMobileView)
                await _js.InvokeVoidAsync("mapAPI.toggleScaleVisibility", newValue == MapTypeEnum.Normal);

            MapType = newValue;
        }

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
        /// Seznam podkladových map
        /// </summary>
        public MapModel Map { get; set; }

        public string GetMapInfoForLeaflet(int zoom = 0)
        {
            if (zoom > 0 && Map.InitialVariables != null)
                Map.InitialVariables.Zoom = zoom;

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
                LayersParameter = a.LayersParameter,
                CssClass = a.CssClass ?? "",
            }).
            Union(
            [
                new() { Name = LayerType.Objects.ToString(), Type = LayerType.Objects.ToString(), Selected = true },
                new() { Name = LayerType.Polygons.ToString(), Type = LayerType.Polygons.ToString(), Selected = true },
                new() { Name = "AdditionalObjects", Type = LayerType.Polygons.ToString(), Selected = true, ZIndex = 9999 },
                new() { Name = LayerType.Narration.ToString(), Type = LayerType.Narration.ToString(), Selected = false, ZIndex = 9999 }
            ]).
            ToList();

            return JsonConvert.SerializeObject(new { initialVariables = Map.InitialVariables, layers = result }, serializerSettings);

        }

        /// <summary>
        /// Initializace tohoto objektu na základě mapsettings.json, případně podle parametrů v url
        /// </summary>
        public async Task Init(string? city, string? layers = null, string? timelinePoint = null)
        {
            var json = await _client.GetStringAsync(_appUrl + $"mapsettings.{city}.json");
            var settings = JsonConvert.DeserializeObject<MapStateService>(json);
            if (settings == null)
                return;
            Map = settings.Map;
            Map.InitialVariables.HeightOfDialog = HeightOfDialog;

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
            if (Map.Layers != null)
                Map.Layers?.ForEach(layer =>
                {
                    if (layer.Type == LayerType.Base || (layers == null && layer.Type != LayerType.Heatmap) || (layers?.Contains(layer.Name) ?? false))
                        layer.Selected = true;
                    else
                        layer.Selected = false;
                });

            if (Map.Timeline != null)
                foreach (var layer in Map.Timeline?.Where(a => a.AdditionalLayers != null).SelectMany(a => a.AdditionalLayers ?? []) ?? [])
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
                if (string.IsNullOrEmpty(timelinePoint) && point.From == null || point.Name == timelinePoint)
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
            Map.Timeline.ForEach(a => a.Selected = point.Name == a.Name && point.Selected);
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
        public SideDialogOptions GetDialogOptions()
        {
            //var height = await _js.InvokeAsync<int>("mapAPI.getWindowHeight");
            return new SideDialogOptions()
            {
                ShowClose = true,
                ShowTitle = false,
                Position = IsMobileView ? DialogPosition.Bottom : DialogPosition.Right,
                ShowMask = false,
                CssClass = IsMobileView ? "" : "side-dialog",
                Style = IsMobileView ? DialogType == DialogTypeEnum.Help || DialogType == DialogTypeEnum.Welcome ? "z-index:50000" : "z-index:10000" : "",
                Height = IsMobileView ? DialogType == DialogTypeEnum.Help || DialogType == DialogTypeEnum.Welcome ? WindowHeight + "px" : HeightOfDialog + "%" : "",
                Width = !IsMobileView ? WidthOfDialogPercent : ""
            };

        }
        /// <summary>
        /// Má se mapa posunout na střed, když uživatel klikne na nějaké místo na mapu?
        /// </summary>
        /// <param name="mousePointClickX"></param>
        /// <returns></returns>
        public bool ShouldBeMapCenteredAfterClick(double mousePointClickX)
        {

            // pokud je mobilní zobrazení, tak se mapa posunout má
            if (IsMobileView)
                return true;

            // pokud je bod v oblasti, kde vyskočí dialogové okno, tak se mapa posunout má
            if ((int)mousePointClickX > WindowWidth - WindowWidth * WidthOfDialogRatio)
                return true;
            return false;
        }

    }
}
