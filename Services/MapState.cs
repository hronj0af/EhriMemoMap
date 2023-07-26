using EhriMemoMap.Models;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Radzen;
using System.Globalization;

namespace EhriMemoMap.Services
{

    /// <summary>
    /// Pomocné metody k nastavení mapy
    /// </summary>
    public class MapState
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public MapState(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

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
        /// Vrátí informace o stylu mapových čudlíků a boxíků, aby se to zobrazovalo na správném místě a bylo to vidět
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public (string Button, string Box) GetStyleOfMapComponent()
        {
            var order = GetButtonOrderIndex();
            var top = (IsMobileBrowser ? (topOfElement - 5) : topOfElement) + 45 * (order - 1);
            var right = (IsMobileBrowser ? (rightOfElement - 5) : rightOfElement);

            var buttonStyle = $"position:absolute;top:{top}px;right:{right}px;z-index:600";
            var boxStyle = $"position:absolute;top:{top}px;right:{right + 60}px;z-index:600";

            return (buttonStyle, boxStyle);
        }

        /// <summary>
        /// Seznam podkladových map
        /// </summary>
        public List<MapModel>? Maps { get; set; }

        /// <summary>
        /// Parametry pro sbírání informací o místech v mapě
        /// </summary>
        public string? MapFeatureUrl { get; set; }


        /// <summary>
        /// Initializace tohoto objektu na základě mapsettings.json, případně podle parametrů v url
        /// </summary>
        public void Init(string? layers = null, string? collectionName = null)
        {
            var settings = JsonConvert.DeserializeObject<MapState>(GetJsonSettingsContent());
            if (settings == null)
                return;
            Maps = settings.Maps;
            MapFeatureUrl = settings.MapFeatureUrl;
            InitLayersInfoAboutMapName();

            if (!string.IsNullOrEmpty(collectionName))
                SetInfoAboutCollectionSelection(collectionName);

            if (!string.IsNullOrEmpty(layers))
                InitInfoAboutLayersSelection(layers.Split(','));
            else
                InitInfoAboutLayersSelection();

            MapStateWasInit = true;
            NotifyStateChanged();

        }


        /// <summary>
        /// Nastaví vlastnost mapName u všech vrstev
        /// </summary>
        private void InitLayersInfoAboutMapName()
        {
            Maps?.ForEach(map =>
            {
                map.MapLanguages?.ForEach(mapLanguage =>
                {
                    mapLanguage.MapName = map.Name;
                    if (mapLanguage.Layers != null)
                        mapLanguage.Layers?.ForEach(layer => layer.MapName = map.Name);

                    if (mapLanguage.Collections != null)
                        mapLanguage.Collections?.ForEach(collection => collection.MapName = map.Name);
                });
            });

        }

        /// <summary>
        /// Když se načítá poprvé nastavení mapy, nastav správně příznak Selected u všech vrstev;
        /// pokud je ale zadaný parametr layers s vybranými vrstvami, pak nastav příznak Selected jen u těchto vybraných
        /// </summary>
        private void InitInfoAboutLayersSelection(string[]? layers = null)
        {
            Maps?.ForEach(map =>
            {
                map.MapLanguages?.ForEach(mapLanguage =>
                {
                    if (mapLanguage.Collections == null)
                        mapLanguage.Layers?.ForEach(layer =>
                        {
                            if (layers == null || layers.Contains(layer.Code))
                                layer.Selected = true;
                            else
                                layer.Selected = false;

                        });
                });
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
        /// Vrátí vrstvy z nastavení mapy; 
        /// pokaždé vrátí ty vrstvy, které příslušejí přímo určité mapě (nespadají pod žádnou kolekci), 
        /// </summary>
        /// <returns></returns>
        public List<LayerModel>? GetAllLayers()
        {
            if (Maps == null)
                return new List<LayerModel>();

            return Maps.
                Where(a => a.Type.Contains("wms")).
                SelectMany(a => a.MapLanguages.Where(a => a.LanguageCode == CultureInfo.CurrentCulture.ToString() && a.Layers != null)).
                SelectMany(map => map.Layers).
                Union(
                    Maps.
                        Where(a => a.Type.Contains("wms")).
                        SelectMany(a => a.MapLanguages.Where(a => a.LanguageCode == CultureInfo.CurrentCulture.ToString() && a.Layers != null)).
                        SelectMany(c => c.Layers)).
                        Select(b => new LayerModel
                        {
                            MapName = b.MapName,
                            Code = b.Code,
                            Name = b.Name,
                            Title = b.Title,
                            Selected = b.Selected,
                            IsNotQueryable = b.IsNotQueryable,
                            Abstract = b.Abstract,
                            Hidden = b.Hidden
                        }).ToList();
        }

        /// <summary>
        /// Vrátí všechny kolekce v mapách podle aktuálně vybraného jazyka
        /// </summary>
        /// <returns></returns>
        public List<CollectionModel>? GetAllCollections()
        {
            if (Maps == null)
                return new List<CollectionModel>();

            return Maps.
                Where(a => a.Type.Contains("wms")).
                SelectMany(a => a.MapLanguages.Where(a => a.LanguageCode == CultureInfo.CurrentCulture.ToString() && a.Collections != null)).
                SelectMany(map => map.Collections).ToList();
        }

        /// <summary>
        /// Vrátí informace o mapách - ve formátu json stringu - pro LeafletJS
        /// </summary>
        /// <returns></returns>
        public string GetAllMapsInfoForLeaflet()
        {
            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            var result = JsonConvert.SerializeObject(Maps.Select(a => GetMapInfoForLeafletLayer(a.Name)), serializerSettings);
            return result;
        }

        /// <summary>
        /// Vrátí url, aby bylo možno se dotázat QGIS serveru na informace o daném místě
        /// </summary>
        /// <param name="infoFormat">formát, ve kterém se má vrátit odpověď: text/xml, nebo application/json</param>
        /// <returns></returns>
        public string GetFeaturesInfoUrl(string infoFormat)
        {
            if (Maps == null)
                return "";

            var featureMap = Maps.FirstOrDefault(a => a.FeaturesInfo && a.MapLanguages != null && a.MapLanguages.Any(b => b.LanguageCode == CultureInfo.CurrentCulture.ToString()));
            if (featureMap == null)
                return "";

            string? queryLayers = GetAllLayers()?.Where(a => a.Selected && !a.IsNotQueryable).Select(a => a.Name)?.Aggregate((x, y) => x + "," + y);
            string? mapParameter = featureMap.MapLanguages?.FirstOrDefault(a => a.LanguageCode == CultureInfo.CurrentCulture.ToString())?.MapParameter != null
                ? featureMap.MapLanguages?.FirstOrDefault(a => a.LanguageCode == CultureInfo.CurrentCulture.ToString())?.MapParameter
                : featureMap.MapLanguages?.FirstOrDefault(a => a.LanguageCode == CultureInfo.CurrentCulture.ToString())?.Collections?.FirstOrDefault(a => a.Selected)?.MapParameter;

            return MapFeatureUrl +
                "&map=" + mapParameter +
                "&query_layers=" + queryLayers +
                "&layers=" + queryLayers +
                "&info_format=" + infoFormat;
        }


        /// <summary>
        /// Vrátí názvy vybraných mapových vrstev pro nastavení v url query
        /// </summary>
        /// <returns></returns>
        public string? GetLayersForUrlParameter()
            => GetAllLayers()?.Count(a => a.Selected) == 0 ? "" : GetAllLayers()?.Where(a => a.Selected).Select(a => a.Code)?.Aggregate((x, y) => x + "," + y);

        /// <summary>
        /// Vrátí název vybrané kolekce / bodu na časové ose pro nastavení v url query
        /// </summary>
        /// <returns></returns>
        public string? GetCollectionForUrlParameter()
            => GetAllCollections()?.FirstOrDefault(a => a.Selected)?.Name;

        /// <summary>
        /// Vrátí objekt pro nastavení mapy v LeafletJS
        /// </summary>
        /// <param name="mapName"></param>
        /// <returns></returns>
        public MapLeafletModel GetMapInfoForLeafletLayer(string mapName)
        {
            var result = Maps?.Where(a => a.Name == mapName).
                Select(a =>
                {
                    var mapLanguage = a.MapLanguages?.Where(a => a.LanguageCode == CultureInfo.CurrentCulture.ToString()).FirstOrDefault(a => a.MapName == mapName);
                    return new MapLeafletModel
                    {
                        Attribution = a.Attribution,
                        BaseUrl = a.BaseUrl,
                        Name = a.Name,
                        Opacities = a.Opacities,
                        Title = a.Title,
                        Type = a.Type,
                        ZIndex = a.ZIndex,
                        TileSize = a.TileSize,
                        MapParameter = mapLanguage?.MapParameter != null
                            ? mapLanguage?.MapParameter
                            : mapLanguage?.Collections?.FirstOrDefault(a => a.Selected)?.MapParameter,
                        Layers = mapLanguage?.Layers != null ? mapLanguage?.Layers.Where(a=>a.Selected).ToList() : null
                    };
                }).FirstOrDefault() ?? new MapLeafletModel();
            return result;
        }

        /// <summary>
        /// Změň příznak selected u vrstvy podle jejího kódu; změní se tím příznak selected u všech vrstev se stejným kódem
        /// </summary>
        /// <param name="layerCode"></param>
        /// <param name="selected"></param>
        public void SetInfoAboutLayerSelection(LayerModel layer)
        {
            Maps?.ForEach(map =>
            {
                map.MapLanguages?.ForEach(mapLanguage =>
                {
                    mapLanguage.Layers?.ForEach(oldLayer =>
                    {
                        if (oldLayer.Code == layer.Code)
                            oldLayer.Selected = layer.Selected;
                    });
                });
            });
        }

        /// <summary>
        /// Nastaví u vybrané kolekce parametr selected na true a u všech ostatních parametr selected na false
        /// </summary>
        /// <param name="collection"></param>
        public void SetInfoAboutCollectionSelection(CollectionModel collection)
        {
            Maps?.ForEach(map =>
            {
                map.MapLanguages?.ForEach(mapLanguage =>
                {
                    mapLanguage.Collections?.ForEach(oldCollection =>
                    {
                        if (collection.Name == oldCollection.Name)
                            oldCollection.Selected = true;
                        else
                            oldCollection.Selected = false;
                    });
                });
            });
            NotifyStateChanged();
        }

        /// <summary>
        /// Nastaví u vybrané kolekce parametr selected na true a u všech ostatních parametr selected na false
        /// </summary>
        /// <param name="collection"></param>
        public void SetInfoAboutCollectionSelection(string collectionName)
        {
            Maps?.ForEach(map =>
            {
                map.MapLanguages?.ForEach(mapLanguage =>
                {
                    mapLanguage.Collections?.ForEach(oldCollection =>
                    {
                        if (collectionName == oldCollection.Name)
                            oldCollection.Selected = true;
                        else
                            oldCollection.Selected = false;
                    });
                });
            });
            NotifyStateChanged();
        }


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

        private int buttonOrderIndex { get; set; } = 0;
        public int GetButtonOrderIndex() => ++buttonOrderIndex;
    }


}
