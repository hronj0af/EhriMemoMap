// FUNKCE PRO OVLÁDÁNÍ MAPY
/// <reference path="ts/leaflet/index.d.ts" />

namespace mapAPI {


    //////////////////////////
    /// VARIABLES
    //////////////////////////
    let wmsProxyUrl: string = "";
    let map: L.Map = null;
    let lat: number = 0;
    let lng: number = 0;
    let coorx: number = 0;
    let coory: number = 0;
    let bluepointIcon: L.Icon = null;
    let addressIcon: L.DivIcon = null;
    let incidentIcon: L.DivIcon = null;
    let interestIcon: L.DivIcon = null;
    let blazorMapObjects = [];
    let groups: L.FeatureGroup[] = [];
    let trackingInterval: number = null;
    let _isMobileView: boolean = null;
    let mapSettings: MapSettingsForLeafletModel = null;
    let applicationIsTrackingLocation: boolean = null;
    let actualLocation: GeolocationPosition = null;
    let dialogWidth: string = null;
    let dialogHeight: string = null;
    let mobileDialogHeight: number = null;
    let isFullscreen: boolean = null;
    let polygonStrokeColor: string = "#222";
    let polygonColor: string = "#C5222C";
    let polygonColorSelected: string = "#000"; // "#cc1111";
    let heatmapLayer: HeatmapOverlay | null = null; // Globální proměnná pro heatmapu
    let heatmapData: { id: string; heatmapdata: HeatmapData; }[] | null = null;
    let initialVariables: InitialVariables = null;

    //////////////////////////
    /// INIT
    //////////////////////////

    // připraví mapu do úvodního stavu
    export function initMap(jsonMapSettings: string): void {
        //history.replaceState({}, '', "praha");

        mapSettings = JSON.parse(jsonMapSettings) as MapSettingsForLeafletModel;
        mobileDialogHeight = mapSettings.initialVariables.heightOfDialog;
        wmsProxyUrl = mapSettings.initialVariables.wmsProxyUrl;
        initialVariables = mapSettings.initialVariables;

        fitMapToWindow();

        incidentIcon = new L.DivIcon({ className: 'leaflet-incident-icon' });
        addressIcon = new L.DivIcon();
        interestIcon = new L.DivIcon({ className: 'leaflet-interest-icon' });

        bluepointIcon = new L.Icon({
            iconUrl: 'images/blue-point.png',
            iconSize: [20, 20], // size of the icon
            iconAnchor: [10, 10], // point of the icon which will correspond to marker's location
        });


        map = new L.Map('map',
            {
                zoomControl: false,
                maxZoom: initialVariables.maxZoom,
                minZoom: initialVariables.minZoom,
                maxBounds: [
                    [initialVariables.minBounds.x, initialVariables.minBounds.y],
                    [initialVariables.maxBounds.x, initialVariables.maxBounds.y]
                ],
                zoomSnap: mapAPI.isMobileView() ? 0.1 : 1
            });
        map.attributionControl.setPosition('bottomleft');
        L.control.scale().setPosition(mapAPI.isMobileView() ? 'topright' : 'bottomleft').addTo(map);


        if (!setMapWithInfoFromUrl())
            resetMapViewToInitialState();

        // update url a obrázkových vrstev po té, co se změní poloha mapy
        map.on("moveend", function () {
            document.getElementById("map").style.cursor = 'default';
            setUrlByMapInfo();
            callBlazor_RefreshObjectsOnMap();
        });

        // při změně polohy myši se zapíšou její souřadnice do proměnných
        map.addEventListener('mousemove', function (ev) {
            lat = ev.latlng.lat;
            lng = ev.latlng.lng;
            coorx = ev.containerPoint.x;
            coory = ev.containerPoint.y;
        });


        // konverze json objektu do jednotlivých vrstev mapy a jejich přidání k mapě
        for (let i = 0; i < mapSettings.layers.length; i++) {
            const group = new L.FeatureGroup(null, {
                id: mapSettings.layers[i].name + "_group",
                type: mapSettings.layers[i].type,
                selected: mapSettings.layers[i].selected,
                name: mapSettings.layers[i].name
            });
            groups.push(group);

            if (mapSettings.layers[i].selected)
                group.addTo(map);

            const layer = convertMapSettingsObjectToMapLayer(mapSettings.layers[i]);
            if (layer != null)
                group.addLayer(layer);

            group.setZIndex(mapSettings.layers[i].zIndex);

        }
    }

    export function destroyMap(): void {
        // Zastavit tracking interval
        if (trackingInterval != null) {
            clearInterval(trackingInterval);
            trackingInterval = null;
        }

        // Odstranit heatmap layer
        if (heatmapLayer != null && map != null) {
            map.removeLayer(heatmapLayer);
            heatmapLayer = null;
        }

        // Vyčistit heatmap data
        heatmapData = null;

        // Odstranit všechny groups a jejich layers
        if (groups != null && groups.length > 0) {
            groups.forEach(group => {
                if (group != null) {
                    group.clearLayers();
                    if (map != null) {
                        map.removeLayer(group);
                    }
                }
            });
            groups = [];
        }

        // Odstranit mapu
        if (map != null) {
            map.off(); // Odstranit všechny event listenery
            map.remove();
            map = null;
        }

        // Vyčistit další proměnné
        applicationIsTrackingLocation = null;
        actualLocation = null;
        blazorMapObjects = [];
        initialVariables = null;
    }

    export function resetMapViewToInitialState() {
        if (initialVariables == null)
            map.setView([50.07905886, 14.43715096], 14); // defaultně nastavíme mapu na Prahu
        else
            map.setView([initialVariables.lat, initialVariables.lng], /*isMobileView() ? initialVariables.zoomMobile : */initialVariables.zoom);
    }

    export function onResizeWindow(): void {
        fitMapToWindow();
        resizePhotoHeight();
        if (blazorMapObjects["Map"] != null)
            blazorMapObjects["Map"].invokeMethodAsync("SetMobileView", isMobileView());

    }

    // nastaví mapu, aby lícovala s oknem
    export function fitMapToWindow(heightOfDialog = null, mapWidth = null): void {
        const pageHeight = window.innerHeight;
        const pageWidth = window.innerWidth;

        const tempHeight = heightOfDialog != null && isMobileView()
            ? pageHeight * (heightOfDialog / 100)
            : pageHeight - (document.getElementById("controlButtonsWrapper") != null
                ? document.getElementById("controlButtonsWrapper").offsetTop
                : 0);

        const mapElement = document.getElementById("map");
        const pageElement = document.getElementsByClassName("page") as HTMLCollectionOf<HTMLElement>;

        if (mapElement == null || !mapElement)
            return;

        mapElement.style.width = mapWidth == null ? "100%" : mapWidth;

        const mapHeight = !mapAPI.isMobileView()
            ? pageHeight - 1
            : pageHeight - 44 - tempHeight // 44 je horní panel
        mapElement.style.height = mapHeight + "px";

        if (mapAPI.isMobileView())
            mapElement.style.marginTop = "44px";
        pageElement[0].style.height = !isMobileView() ? (pageHeight - 1) + "px" : (pageHeight - 44) + "px";

        if (map != null)
            map.invalidateSize();

    }

    //////////////////////////
    /// URL
    //////////////////////////


    // získá informace z URL pro nastavení polohy mapy
    export function getMapInfoFromUrl(): MapInfo {
        const queryString = window.location.search;
        const urlParams = new URLSearchParams(queryString);

        const urlBounds = urlParams.get("bounds");

        var southWest = null,
            northEast = null,
            bounds = null,
            customCoordinates = null;

        if (urlBounds != null) {
            const splittedBounds = urlBounds.split(",");

            southWest = new L.LatLng(Number(splittedBounds[0]), Number(splittedBounds[1]));
            northEast = new L.LatLng(Number(splittedBounds[2]), Number(splittedBounds[3]));
        }

        const x1 = urlParams.get("x1");
        const y1 = urlParams.get("y1");

        if (x1 != null && y1 != null)
            customCoordinates = { X: x1, Y: y1 };

        const zoom = urlParams.get("zoom");

        return {
            bounds: bounds,
            zoom: zoom ?? "13",
            customCoordinates: customCoordinates
        }
    }

    // nastaví URL parametry pro pozdější nastavení mapy
    export function setUrlByMapInfo(): void {
        const bounds = map.getBounds();

        const urlBounds = bounds.getSouthWest().lat + "," + bounds.getSouthWest().lng + "," + bounds.getNorthEast().lat + "," + bounds.getNorthEast().lng;

        setUrlParam('bounds', urlBounds);
        setUrlParam('zoom', map.getZoom());
    }

    // nastaví jednotlivý parametr url
    export function setUrlParam(paramName: string, paramValue: string | number) {
        const urlParams = new URLSearchParams(window.location.search);
        urlParams.set(paramName, paramValue.toString());
        //history.replaceState({ }, 'Mapa', "praha?" + urlParams);
        history.replaceState({}, 'Mapa', window.location.pathname + "?" + urlParams);
    }

    // získá parametr z url
    export function getUrlParam(paramName: string): string {
        const urlParams = new URLSearchParams(window.location.search);
        return urlParams.get(paramName);
    }

    // nastaví mapu podle parametrů url
    export function setMapWithInfoFromUrl(): boolean {
        const infoFromUrl = getMapInfoFromUrl();
        if (infoFromUrl.bounds != null) {
            map.fitBounds(infoFromUrl.bounds);
            const latAvg = (infoFromUrl.bounds.getSouthWest().lat + infoFromUrl.bounds.getNorthEast().lat) / 2;
            const lngAvg = (infoFromUrl.bounds.getSouthWest().lng + infoFromUrl.bounds.getNorthEast().lng) / 2;
            map.setView([latAvg, lngAvg], Number(infoFromUrl.zoom));
            return true;
        }

        if (infoFromUrl.customCoordinates != null) {
            map.setView([infoFromUrl.customCoordinates.Y, infoFromUrl.customCoordinates.X], Number(infoFromUrl.zoom));
            return true;
        }

        return false;
    }

    // konvertuje objekt s nastavením mapových vrstev do mapových vrstev leafletu
    export function convertMapSettingsObjectToMapLayer(mapSettingsObject): L.TileLayer {
        if (mapSettingsObject.type == LayersTypeEnum.Base) {
            return new L.TileLayer(mapSettingsObject.url, {
                maxZoom: 18,
                attribution: mapSettingsObject.attribution,
            });
        }
        else if (mapSettingsObject.type == LayersTypeEnum.WMS) {
            return new L.TileLayer.WMS(mapSettingsObject.url, {
                tileSize: 512,
                //map: mapSettingsObject.mapParameter,
                layers: mapSettingsObject.layersParameter,
                className: "customWmsLayer"
            });
        }
        return null;
    }

    //////////////////////////
    /// BLAZOR
    //////////////////////////

    export function initBlazorMapObject(dotNetObject: any, name: string) {
        blazorMapObjects[name] = dotNetObject;
    }

    export function callBlazor_RefreshObjectsOnMap() {
        const polygonsGroup = groups.filter(a => a.options.type == "Polygons");
        const mapHasPolygonsYet = polygonsGroup.length > 0;
        blazorMapObjects["Map"].invokeMethodAsync("RefreshObjectsOnMap", !mapHasPolygonsYet, map.getZoom(), getMapBoundsForMapState());
    }

    export function callBlazor_ShowPlaceInfo(event) {

        // nejdřív odstraníme všechny dodatečné objekty
        removeAdditionalObjects();
        unselectAllSelectedPoints();

        // a pak obarvíme vybraný polygon, respektive přidáme špendlík
        if (event.target._latlng == undefined)
            (event.target as L.Polygon).setStyle({ fillColor: polygonColorSelected });
        else if (event.target._icon != null) {
            event.target._icon.className = event.target._icon.className.replace('map-point-selected', 'map-point').replace('map-point', 'map-point-selected');
            //event.target._icon.style.zIndex = '200';
        }

        const point = event.target._latlng != undefined ? event.target._latlng : [lat, lng];
        const bbox = convertObjectPositionToBBoxParameter(point);
        blazorMapObjects["Map"].invokeMethodAsync("ShowPlaceInfo", map.getZoom(), bbox, coorx);
    }

    //////////////////////////
    /// OBJECTS
    //////////////////////////

    // refreshuje se všechno, kromě nepřístupných míst, protože ta jsou vidět pořád
    export function refreshObjectsOnMap(objectJson) {
        if (groups == null || groups.length == 0)
            return;

        const objectsGroup = groups.find(a => a.options.id == "Objects_group");
        const polygonsGroup = groups.filter(a => a.options.type == "Polygons" || a.options.type == "WFS");

        objectsGroup.clearLayers();

        const objects = JSON.parse(objectJson) as MapObjectForLeafletModel[];


        for (let i = 0; i < objects.length; i++) {
            let newObject;

            // polygony
            if (objects[i].mapPolygon != null) {

                objects[i].mapPolygonModel = JSON.parse(objects[i].mapPolygon) as PolygonModel;
                newObject = getPolygon(objects[i]);

                if (objects[i].layerName != null && objects[i].layerName.toLocaleLowerCase() != "statistics") {
                    var groupToAdd = polygonsGroup.find(a => a.options.name.toLocaleLowerCase() == objects[i].layerName.toLocaleLowerCase());
                    newObject.addTo(groupToAdd);
                }
                else
                    newObject.addTo(objectsGroup);

            } else if (objects[i].mapPoint != null && !objects[i].heatmap) {
                // nepolygony a ne heatmapa

                objects[i].mapPointModel = JSON.parse(objects[i].mapPoint) as PointModel;
                newObject = getPoint(objects[i], "map-point");

                newObject.options.guid = objects[i].guid;
                newObject.addTo(objectsGroup);
            }
        }
        for (let i = 0; i < polygonsGroup.length; i++) {
            polygonsGroup[i].bringToFront();
        }

        // pokud je heatmapa, tak ji zobrazíme
        if (heatmapLayer) {
            map.removeLayer(heatmapLayer);
        }

        var heatmapIndex = getIndexOfHeatmapData(objects);
        if (heatmapIndex != null) {
            seedHeatmapData(objects, heatmapIndex);
            showHeatmap(heatmapData.find(a => a.id === heatmapIndex).heatmapdata);
        }

        setZIndexForGroups();
    }

    export function setZIndexForGroups() {
        for (let i = 0; i < mapSettings.layers.length; i++) {
            var group = groups.find(a => a.options.name == mapSettings.layers[i].name);
            group.setZIndex(mapSettings.layers[i].zIndex);
        }
    }


    export function seedHeatmapData(objects: MapObjectForLeafletModel[], heatmapIndex: string) {
        if (heatmapData != null && heatmapData.find(a => a.id == heatmapIndex) != null)
            return;

        if (heatmapData == null)
            heatmapData = [{ id: heatmapIndex, heatmapdata: { max: 100, data: [] } }];
        else if (heatmapData.find(a => a.id == heatmapIndex) == null)
            heatmapData.push({ id: heatmapIndex, heatmapdata: { max: 100, data: [] } });

        const heatmapEntry = heatmapData.find(a => a.id === heatmapIndex);

        if (!heatmapEntry) {
            console.error(`Heatmap entry with index ${heatmapIndex} was not found.`);
            return;
        }

        for (let i = 0; i < objects.length; i++) {
            if (objects[i].mapPoint != null && objects[i].heatmap) {
                const markerObject = JSON.parse(objects[i].mapPoint) as PointModel;
                heatmapEntry.heatmapdata.data.push({
                    lat: markerObject.coordinates[1],
                    lng: markerObject.coordinates[0],
                    count: objects[i].citizens
                });
            }
        }
    }

    export function getIndexOfHeatmapData(objects: MapObjectForLeafletModel[]): string {
        for (let i = 0; i < objects.length; i++) {
            if (objects[i].heatmap)
                return objects[i].placeType;
        }
        return null;
    }

    export function showHeatmap(heatmapData: HeatmapData) {

        if (heatmapData.data.length == 0)
            return;


        var cfg = {
            // radius should be small ONLY if scaleRadius is true (or small radius is intended)
            // if scaleRadius is false it will be the constant radius used in pixels
            "radius": 20,
            "maxOpacity": 1,
            // scales the radius based on map zoom
            "scaleRadius": false,
            // if set to false the heatmap uses the global maximum for colorization
            // if activated: uses the data maximum within the current map boundaries
            //   (there will always be a red spot with useLocalExtremas true)
            "useLocalExtrema": true,
            latField: 'lat',
            lngField: 'lng',
            valueField: 'count'
        };

        heatmapLayer = new HeatmapOverlay(cfg);
        heatmapLayer.setData(heatmapData);

        heatmapLayer.addTo(map);
    }


    export function addObjectsFromJsonString(jsonInfo: string, group: string) {
        var groupName = group + "_group";
        const objectsGroup = groups.find(a => a.options.id == groupName);
        objectsGroup.clearLayers();
        const parsedObjects = JSON.parse(jsonInfo) as MapObjectForLeafletModel[];
        for (let i = 0; i < parsedObjects.length; i++) {

            const parsedLocation = JSON.parse(parsedObjects[i].mapPoint);
            var newObject = null;

            if (parsedLocation == null)
                continue;

            if (parsedLocation.type == "Point") {
                parsedObjects[i].mapPointModel = parsedLocation;
                newObject = getPoint(
                    parsedObjects[i],
                    'z-index-999',
                    parsedObjects[i].placeType == NarrativeMapStopPlaceType.Main ? openStoryMapTimelineLabel : null)
            } else {
                parsedObjects[i].mapPolygonModel = parsedLocation;
                newObject = getPolygon(parsedObjects[i]);
            }

            newObject.addTo(objectsGroup);
        }
        toggleLayerGroup(group, false);
        toggleLayerGroup(group, true);
        fitMapToGroup(groupName);
        drawCurveBettwenPoints(groupName);

    }

    // Předpokládáme, že 'map', 'groups' a 'NarrativeMapStopPlaceType' jsou dostupné v kontextu
    // Pokud ne, doplňte příslušné importy.

    /**
     * Vykreslí křivky se šipkami mezi body ve skupině.
     * @param groupName ID skupiny
     * @param shortenEndPixels (Volitelné) O kolik pixelů zkrátit konec čáry, aby šipka nepřekrývala cílový marker. Default: 25.
     */
    export function drawCurveBettwenPoints(groupName: string, shortenEndPixels: number = 10) {
        const objectsGroup = groups.find(a => a.options.id == groupName);
        if (!objectsGroup) return;

        // 1. CLEANUP: Odstranění starých křivek před vykreslením nových
        const allLayers = objectsGroup.getLayers();
        allLayers.forEach(layer => {
            // Kontrola, zda jde o námi vytvořenou křivku
            if ((layer as any).options && (layer as any).options.isTrajectoryLine) {
                objectsGroup.removeLayer(layer);
            }
        });

        // 2. Získáme pouze relevantní markery
        const validMarkers: L.Marker[] = [];

        // Filtrace markerů správného typu
        for (const layer of objectsGroup.getLayers()) {
            if (layer instanceof L.Marker && (layer as any).options.type == NarrativeMapStopPlaceType.Trajectory) {
                validMarkers.push(layer);
            }
        }

        // 3. Procházíme seřazené pole a spojujeme vždy i a i+1
        for (let i = 0; i < validMarkers.length - 1; i++) {
            const startMarker = validMarkers[i];
            const endMarker = validMarkers[i + 1];
            const startLatLng = startMarker.getLatLng();
            const endLatLng = endMarker.getLatLng();

            let currentShortenPixels = shortenEndPixels;

            // SPECIFICKÁ LOGIKA PRO POSLEDNÍ ŠIPKU MÍŘÍCÍ DOLŮ
            // 1. Je to poslední segment? (i je předposlední index)
            const isLastSegment = (i === validMarkers.length - 2);

            // 2. Míří šipka dolů? (Cílová latitude je menší než startovní = směr na jih)
            const isPointingSouth = endLatLng.lat < startLatLng.lat;

            if (isLastSegment && isPointingSouth) {
                // Pokud míříme na poslední marker shora, potřebujeme větší mezeru,
                // protože marker je pravděpodobně vysoký (pin) a překryl by šipku.
                // Zvětšíme zkrácení např. 2.2x (z 25px na 55px).
                currentShortenPixels = shortenEndPixels * 3.5;
            }

            // Předáváme vypočítané zkrácení do funkce getCurve
            const curve = getCurve(startLatLng, endLatLng, currentShortenPixels);
            if (curve) {
                curve.addTo(objectsGroup);
            }
        }
    }

    // --- KONFIGURACE PRO KŘIVKY (Magická čísla) ---
    const CURVE_CFG = {
        minArrowDist: 15,          // Minimální vzdálenost konce šipky od cílového bodu (px)
        maxArrowDist: 25,          // Maximální vzdálenost konce šipky od cílového bodu (px)
        maxShortenRatio: 0.4,      // Max zkrácení jako poměr celkové délky
        shortDistThreshold: 100,   // Práh pro přepnutí na vizuální oblouk (px)
        arcHeightMin: 10,          // Minimální výška oblouku (px)
        arcHeightRatio: 0.15,      // Poměr výšky oblouku k vzdálenosti
        curveSegments: 100,        // Počet segmentů pro vzorkování křivky
        fallbackShortenRatio: 0.3  // Max zkrácení pro fallback
    };

    // --- POMOCNÉ FUNKCE PRO KŘIVKY ---

    /** Vypočítá kontrolní bod pro krátké vzdálenosti (vizuální oblouk v pixelech) */
    function getVisualArcControlPoint(p1Pix: L.Point, p2Pix: L.Point, pixDist: number): number[] {
        const midPix = p1Pix.add(p2Pix).divideBy(2);
        const vec = p2Pix.subtract(p1Pix);

        // Kolmice (-y, x) pro určení směru oblouku
        const perpX = -vec.y;
        const perpY = vec.x;
        const len = Math.sqrt(perpX * perpX + perpY * perpY);

        // Výška oblouku - úměrná vzdálenosti, ale s minimem pro viditelnost
        const arcHeight = Math.max(CURVE_CFG.arcHeightMin, pixDist * CURVE_CFG.arcHeightRatio);

        // Normalizace a posun
        const normX = (perpX / len) * arcHeight;
        const normY = (perpY / len) * arcHeight;

        const ctrlPix = L.point(midPix.x + normX, midPix.y + normY);
        const ctrlLatLng = map.containerPointToLatLng(ctrlPix);

        return [ctrlLatLng.lat, ctrlLatLng.lng];
    }

    /** Vypočítá kontrolní bod pro dlouhé vzdálenosti (geografické zakřivení) */
    function getGeoCurveControlPoint(pointA: L.LatLng, pointB: L.LatLng): number[] {
        const distance = map.distance(pointA, pointB);
        const midLat = (pointA.lat + pointB.lat) / 2;
        const midLng = (pointA.lng + pointB.lng) / 2;

        const minDistance = 50;
        const maxDistance = 50000;
        const normalizedDistance = Math.min(Math.max((distance - minDistance) / (maxDistance - minDistance), 0), 1);
        const curveFactor = Math.pow(normalizedDistance, 3);

        const latDiff = Math.abs(pointB.lat - pointA.lat);
        const lngDiff = Math.abs(pointB.lng - pointA.lng);
        const maxDiff = Math.max(latDiff, lngDiff);

        const offsetPercent = 0.02 + 0.06 * curveFactor;
        const offset = maxDiff * offsetPercent;

        return [midLat + offset, midLng];
    }

    /** Vzorkování kvadratické Bézierovy křivky */
    function sampleQuadraticBezier(start: number[], control: number[], end: number[], segments: number): number[][] {
        const points: number[][] = [];
        for (let t = 0; t <= 1; t += 1 / segments) {
            const invT = 1 - t;
            const x = invT * invT * start[1] + 2 * invT * t * control[1] + t * t * end[1];
            const y = invT * invT * start[0] + 2 * invT * t * control[0] + t * t * end[0];
            points.push([y, x]);
        }
        return points;
    }

    /** Oříznutí křivky od konce o definovanou pixelovou vzdálenost (podél křivky) */
    function shortenCurveFromEndByPixels(points: number[][], targetPx: number): number[][] {
        if (points.length < 2 || targetPx <= 0) return points;

        let cumulativeDist = 0;

        // Iterace od konce směrem k začátku
        for (let i = points.length - 1; i > 0; i--) {
            const curr = map.latLngToContainerPoint(L.latLng(points[i][0], points[i][1]));
            const prev = map.latLngToContainerPoint(L.latLng(points[i - 1][0], points[i - 1][1]));
            const segDist = curr.distanceTo(prev);

            cumulativeDist += segDist;

            if (cumulativeDist >= targetPx) {
                // Našli jsme segment k oříznutí - interpolujeme přesnou pozici
                const overflow = cumulativeDist - targetPx;
                if (segDist > 0) {
                    const ratio = overflow / segDist;
                    const interpX = prev.x + (curr.x - prev.x) * ratio;
                    const interpY = prev.y + (curr.y - prev.y) * ratio;

                    const newPt = map.containerPointToLatLng(L.point(interpX, interpY));

                    const result = points.slice(0, i);
                    result.push([newPt.lat, newPt.lng]);
                    return result;
                }
                return points.slice(0, i);
            }
        }

        // Fallback: křivka je kratší než targetPx - zkrátíme max o 30%
        if (points.length >= 2 && cumulativeDist > 0) {
            const actualRatio = Math.min(targetPx / cumulativeDist, CURVE_CFG.fallbackShortenRatio);
            const keepCount = Math.max(2, Math.ceil(points.length * (1 - actualRatio)));
            return points.slice(0, keepCount);
        }

        return points;
    }

    // --- HLAVNÍ FUNKCE ---

    /**
     * Vypočítá a vrátí křivku (polyline) mezi dvěma body.
     * @param pointA Startovní bod
     * @param pointB Cílový bod
     * @param shortenEndPixels Počet pixelů pro zkrácení konce (kvůli šipce)
     */
    export function getCurve(pointA: L.LatLng, pointB: L.LatLng, shortenEndPixels: number) {
        const p1Pix = map.latLngToContainerPoint(pointA);
        const p2Pix = map.latLngToContainerPoint(pointB);
        const pixDist = p1Pix.distanceTo(p2Pix);

        // 1. Early return pro totožné body
        if (pixDist < 1) return null;

        // 2. Normalizace parametru zkrácení
        let finalShortenPx = Math.max(CURVE_CFG.minArrowDist, Math.min(CURVE_CFG.maxArrowDist, shortenEndPixels));
        if (finalShortenPx > pixDist * CURVE_CFG.maxShortenRatio) {
            finalShortenPx = pixDist * CURVE_CFG.maxShortenRatio;
        }

        // 3. Výpočet kontrolního bodu (rozcestník logiky podle vzdálenosti)
        const controlPoint = pixDist < CURVE_CFG.shortDistThreshold
            ? getVisualArcControlPoint(p1Pix, p2Pix, pixDist)
            : getGeoCurveControlPoint(pointA, pointB);

        // 4. Generování bodů křivky
        let curvePoints = sampleQuadraticBezier(
            [pointA.lat, pointA.lng],
            controlPoint,
            [pointB.lat, pointB.lng],
            CURVE_CFG.curveSegments
        );

        // 5. Zkrácení konce pro šipku
        curvePoints = shortenCurveFromEndByPixels(curvePoints, finalShortenPx);

        if (curvePoints.length < 2) return null;

        // 6. Vykreslení
        return L.polyline(curvePoints as L.LatLngExpression[], {
            color: '#771646',
            weight: 1,
            dashArray: '5, 5',
            dashOffset: '0',
            isTrajectoryLine: true
        } as any).arrowheads({
            frequency: 'endonly',
            fill: true,
            size: '15px',
            color: '#771646'
        });
    }
    export function fitMapToGroup(groupName) {

        const objectsGroup = groups.find(a => a.options.id == groupName);

        // Předpokládejme, že `objectsGroup` obsahuje všechny markery, které chcete zobrazit
        var latlngs = [];

        // Procházení všech markerů ve vrstvě `objectsGroup` a rozšíření bounds o jejich pozice
        objectsGroup.eachLayer(function (layer) {
            if (layer instanceof L.Marker) {
                latlngs.push(layer.getLatLng());
            }
        });

        // Nastavení pohledu na mapě tak, aby všechny markery byly vidět
        map.fitBounds(latlngs, { padding: [100, 100] });
    }



    export function getPoint(markerObject: MapObjectForLeafletModel, className?: string, clickFunction?: L.LayersControlEventHandlerFn) {
        let iconOptions = null as L.MarkerOptions;
        if (markerObject.htmlIcon != undefined && markerObject.htmlIcon != null)
            iconOptions = {
                stopId: markerObject.stopId,
                narrativeMapId: markerObject.narrativeMapId,
                type: markerObject.placeType,
                icon: new L.DivIcon({
                    className: className,
                    html: markerObject.htmlIcon,
                    iconAnchor: markerObject.iconAnchor,
                })
            }

        const result = new L.Marker([markerObject.mapPointModel.coordinates[1], markerObject.mapPointModel.coordinates[0]], iconOptions);

        var pos = map.latLngToLayerPoint(result.getLatLng()).round();
        var zIndex = (markerObject.priorityOnMap ?? 0);
        result.setZIndexOffset(zIndex);

        if (markerObject.label != undefined && markerObject.label != null) {
            result.bindTooltip(markerObject.label, { sticky: true });
        }

        if (markerObject.clickable) {
            result.on('click', clickFunction != null ? clickFunction : callBlazor_ShowPlaceInfo);
        }

        result.on('click', function () {
            result.openTooltip();
        });

        // Zavření tooltipu na tapnutí mimo marker
        map.on('click', function () {
            result.closeTooltip();
        });

        return result;
    }

    export function getPolygon(polygonObject: MapObjectForLeafletModel): L.Polygon {
        const pointsArray = [];
        for (let j = 0; j < polygonObject.mapPolygonModel.coordinates.length; j++) {
            for (let m = 0; m < polygonObject.mapPolygonModel.coordinates[j].length; m++) {
                const polygon = polygonObject.mapPolygonModel.coordinates[j][m];
                pointsArray.push([]);
                const innerArray = [];
                for (let k = 0; k < polygon.length; k++) {
                    if (polygon[k][0] != undefined && polygon[k][1] != undefined)
                        innerArray.push([polygon[k][1], polygon[k][0]]);
                    else
                        innerArray.push([polygon[1], polygon[0]]);

                }
                if (innerArray.length > 0)
                    pointsArray[j].push(innerArray);
            }
        }

        const polygonOptions = polygonObject.customPolygonClass != undefined && polygonObject.customPolygonClass != null
            ? { className: polygonObject.customPolygonClass, color: null, weight: null, fillOpacity: null, opacity: null, fillColor: null } // tohle se používá pro statistiku čtvrtí
            : { fillColor: polygonColor, color: polygonStrokeColor, weight: 0.5, fillOpacity: 0.40, opacity: 1 }; // tohle se používá pro nepřístupná místa

        const result = new L.Polygon(pointsArray, polygonOptions);

        if (polygonObject.clickable) {
            result.on('click', callBlazor_ShowPlaceInfo);
        }

        result.on('click', function () {
            result.openTooltip();
        });

        // Zavření tooltipu na tapnutí mimo marker
        map.on('click', function () {
            result.closeTooltip();
        });


        if (polygonObject.label != undefined && polygonObject.label != null) {
            result.bindTooltip(polygonObject.label, { sticky: true, className: polygonObject.customTooltipClass != undefined && polygonObject.customTooltipClass != null ? polygonObject.customTooltipClass : null });
        }

        return result;
    }

    export function removeObjects(group: string): void {
        const objectsGroup = groups.find(a => a.options.id == group + "_group");
        objectsGroup.eachLayer(function (item) {
            if (item.options.type == undefined || item.options.type !== "bluepoint") {
                item.remove();
            }
        });
    }

    export function removeAdditionalObjects(): void {
        removeObjects("AdditionalObjects");

        const polygonsGroups = groups.filter(a => a.options.type == "Polygons");
        for (var i = 0; i < polygonsGroups.length; i++) {
            polygonsGroups[i].eachLayer(function (item: L.Polygon) {
                if (item.options.fillColor != undefined || item.options.fillColor == polygonColorSelected) {
                    item.setStyle({ fillColor: polygonColor });
                }
            });
        }
    }

    export function unselectAllSelectedPoints() {
        const objectsGroup = groups.find(a => a.options.id == "Objects_group");
        objectsGroup.eachLayer((item: any) => {
            const element = item.getElement();
            if (element && element.className) {
                let className: string;

                if (typeof element.className === 'string') {
                    // HTML element s běžným className
                    className = element.className;
                } else if (typeof element.className === 'object' && 'baseVal' in element.className) {
                    // SVG element s className jako SVGAnimatedString
                    className = (element.className as SVGAnimatedString).baseVal;
                } else {
                    // Nepodporovaný typ className
                    return;
                }

                // Kontrola, zda obsahuje "map-point-selected"
                if (className.indexOf("map-point-selected") !== -1) {
                    element.className = className.replace('map-point-selected', 'map-point');
                    element.style.zIndex = (
                        map.latLngToLayerPoint(item.getLatLng()).round().y +
                        item.options.zIndexOffset
                    ).toString();
                }
            }
        });
    }

    export function selectPointOnMap(guidArrayJson: string): void {
        const guidArray = JSON.parse(guidArrayJson) as string[];
        const objectsGroup = groups.find(a => a.options.id == "Objects_group");
        objectsGroup.eachLayer(function (item: L.Marker) {
            if (item.getElement() != null && item.options.guid !== undefined && guidArray.indexOf(item.options.guid) > -1) {
                item.getElement().className = item.getElement().className.replace('map-point-selected', 'map-point').replace('map-point', 'map-point-selected');
                item.getElement().style.zIndex = (map.latLngToLayerPoint(item.getLatLng()).round().y * 2 + item.options.zIndexOffset).toString();
                item.openTooltip();
            }
        });
    }

    //////////////////////////
    /// HELPER METHODS
    //////////////////////////

    export function getGroups() {
        return groups;
    }

    export function getMap() {
        return map;
    }

    export function hideLayersForNormalMap() {
        groups.forEach(group => {
            if (group.options.type == LayersTypeEnum.Base || group.options.type == LayersTypeEnum.WMS)
                return;
            map.eachLayer(function (layer) {
                if (layer.options.id == group.options.id)
                    layer.remove();
            })
        });
    }

    export function showLayersForNormalMap() {
        // nejdřív všechny vrstvy pro jistotu odstraníme, aby se pak na mapě zbytečně nepřekrývaly
        hideLayersForNormalMap();
        groups.forEach(group => {
            if (group.options.type == LayersTypeEnum.Base || group.options.type == LayersTypeEnum.Narration)
                return;
            if (group.options.selected)
                group.addTo(map);
        });
    }

    export function toggleLayerGroup(name: string, selected: boolean): void {
        //const groupName = name + "_group";
        const groupToAdd = groups.find(a => a.options.name == name);
        groupToAdd.options.selected = selected;

        // nejdřív vrstvu odstraníme, abych pak nepřidával přes starou další a další a další...
        map.eachLayer(function (layer) { if (layer.options.name == name) layer.remove(); })

        if (selected) {
            groupToAdd.addTo(map);
        }
    }

    // převede pozici myši nad mapou do parametru bbox pro získání informací o daném místě (dotaz na server)
    export function convertObjectPositionToBBoxParameter(point): Coordinates[] {

        const containerPosition = map.latLngToContainerPoint(point);

        const sizeOfBox = 5;// isMobileView() ? 20 : 5;

        const minx = containerPosition.x < sizeOfBox / 2 ? 0 : containerPosition.x - sizeOfBox / 2;
        const miny = containerPosition.y < sizeOfBox / 2 ? 0 : containerPosition.y + sizeOfBox / 2;
        const maxx = containerPosition.x < sizeOfBox / 2 ? 0 : containerPosition.x + sizeOfBox / 2;
        const maxy = containerPosition.y < sizeOfBox / 2 ? 0 : containerPosition.y - sizeOfBox / 2;

        const southWest = map.containerPointToLatLng([minx, maxy]);
        const northEast = map.containerPointToLatLng([maxx, miny]);

        return [{ X: southWest.lng, Y: southWest.lat }, { X: northEast.lng, Y: northEast.lat }];
    }

    export function getWindowWidth(): number {
        return window.innerWidth;
    }

    export function getWindowHeight(): number {
        return window.innerHeight;
    }

    export function getMapBoundsForMapState(): Coordinates[] {
        const bounds = map.getBounds();
        return [{ X: bounds.getSouthWest().lng, Y: bounds.getSouthWest().lat }, { X: bounds.getNorthEast().lng, Y: bounds.getNorthEast().lat }];
    }

    export function getSelectedPlaceFromUrl(): Coordinates[] {

        const x1 = getUrlParam("x1");
        const y1 = getUrlParam("y1");
        const x2 = getUrlParam("x2");
        const y2 = getUrlParam("y2");

        if (x1 == null || y1 == null || x2 == null || y2 == null)
            return null;

        return [{
            X: parseFloat(x1),
            Y: parseFloat(y1)
        }, {
            X: parseFloat(x2),
            Y: parseFloat(y2)
        }];
    }

    export function setUrlFromSelectedPlace(coordinatesJson: string): void {
        const coordinates = (JSON.parse(coordinatesJson) as Coordinates[]);

        if (coordinates == null || coordinates.length != 2)
            return;

        setUrlParam("x1", coordinates[0].X);
        setUrlParam("y1", coordinates[0].Y);
        setUrlParam("x2", coordinates[1].X);
        setUrlParam("y2", coordinates[1].Y);
    }

    export function getWindowLocationSearch(): string {
        return window.location.search;
    }

    export function isMobileView(): boolean {
        //return true;
        if (window.innerWidth < 768)
            return true;

        if (_isMobileView != null)
            return _isMobileView;

        if (navigator.userAgent.match(/Android/i)
            || navigator.userAgent.match(/webOS/i)
            || navigator.userAgent.match(/iPhone/i)
            || navigator.userAgent.match(/iPad/i)
            || navigator.userAgent.match(/iPod/i)
            || navigator.userAgent.match(/BlackBerry/i)
            || navigator.userAgent.match(/Windows Phone/i))
            _isMobileView = true;
        else
            _isMobileView = false;

        return _isMobileView;
    }

    export function getZoom(): number {
        return map.getZoom();
    }

    export function zoomIn(): void {
        map.zoomIn();
    }

    export function zoomOut(): void {
        map.zoomOut();
    }

    export function toggleScaleVisibility(visible: boolean) {
        const scale = document.getElementsByClassName("leaflet-control-scale")[0] as HTMLElement;
        if (visible)
            scale.style.visibility = '';
        else
            scale.style.visibility = 'hidden';
    }

    //////////////////////////
    /// TRACKING
    //////////////////////////
    export function turnOnLocationTracking(): void {
        if (!navigator.geolocation) {
            console.log("Your browser doesn't support geolocation feature!");
        } else {
            navigator.geolocation.watchPosition(showMyLocation, null, {
                enableHighAccuracy: true,
                timeout: 5000,
                maximumAge: 0
            });
        }
    }

    export function goToLocation(pointString, zoom: number): void {
        const point = (JSON.parse(pointString) as PointModel).coordinates;
        map.flyTo([point[1], point[0]], zoom);
    }

    export function goToMyLocation(): void {
        const lat = actualLocation.coords.latitude;
        const long = actualLocation.coords.longitude;
        map.setView([lat, long], 15);
    }

    export function showMyLocation(position): void {
        actualLocation = position;
        const lat = position.coords.latitude;
        const long = position.coords.longitude;

        removeBluepoint();
        addBluepoint([lat, long], bluepointIcon);

        if (!applicationIsTrackingLocation) {
            goToMyLocation();
            applicationIsTrackingLocation = true;
        }
    }

    export function hideMyLocation(): void {
        clearInterval(trackingInterval);
        applicationIsTrackingLocation = false;
        removeBluepoint();
    }

    export function addBluepoint(point, icon: L.Icon): void {
        const objectsGroup = groups.find(a => a.options.id == "AdditionalObjects_group");
        new L.Marker(point, { icon: icon, type: "bluepoint" }).addTo(objectsGroup);
    }

    export function removeBluepoint(): void {
        const objectsGroup = groups.find(a => a.options.id == "AdditionalObjects_group");
        objectsGroup.eachLayer(function (item) {
            if (item.options.type !== undefined && item.options.type == "bluepoint") {
                item.remove();
            }
        });
    }

    export function openStoryMapTimelineLabel(event): void {
        const point = event.target as L.Marker;
        blazorMapObjects["Map"].invokeMethodAsync("OpenStoryMapTimelineLabel", point.options.stopId, point.options.narrativeMapId);
    }

    //////////////////////////
    /// DIALOG
    //////////////////////////


    export function setDialogFullScreen(value): void {
        if (value) {
            if (!isFullscreen) {
                dialogWidth = document.querySelector("aside").style.width;
                dialogHeight = document.querySelector("aside").style.height;
            }
            isFullscreen = true;
            document.querySelector("aside").style.width = "100%";
            document.querySelector("aside").style.height = "100%";
            fitMapToWindow(window.innerHeight - 44);
        } else if (!value && isFullscreen && dialogHeight != null && dialogWidth != null) {
            isFullscreen = false;
            document.querySelector("aside").style.width = dialogWidth;
            document.querySelector("aside").style.height = dialogHeight;
            fitMapToWindow(dialogHeight);

        }
    }

    export function getBlazorCulture(): string {
        return window.localStorage['BlazorCulture'];
    };

    export function setBlazorCulture(value): void {
        window.localStorage['BlazorCulture'] = value;
    }

    export function resizePhotoHeight(): void {
        const ratio = 0.875;

        if (document.getElementsByClassName("victim-photo") == null || document.getElementsByClassName("victim-photo").length == 0)
            return;

        const photoWidth = document.getElementsByClassName("victim-photo")[0].clientWidth;
        const photoHeight = photoWidth / ratio;

        document.querySelectorAll<HTMLElement>(".victim-photo").forEach(function (item) {
            item.style.height = photoHeight + "px";
        });
    }
}

window.addEventListener("resize", mapAPI.onResizeWindow);

//////////////////////////
/// INTERFACES
//////////////////////////

declare class HeatmapOverlay extends L.Layer {
    constructor(config: HeatmapOverlayConfig);
    setData(data: HeatmapData): void;
    addData(data: HeatmapData['data']): void;
}

interface HeatmapOverlayConfig {
    radius?: number;
    maxOpacity?: number;
    scaleRadius?: boolean;
    useLocalExtrema?: boolean;
    latField?: string;
    lngField?: string;
    valueField?: string;
}

interface HeatmapData {
    max: number;
    data: {
        lat: number;
        lng: number;
        count: number;
    }[];
}
interface MapObjectForLeafletModel {
    heatmap: boolean;
    clickable: boolean;
    layerName: string | null;
    placeType: string | null;
    citizens: number | null;
    citizensTotal: number | null;
    id: string | null;
    guid: string | null;
    label: string | null;
    mapPoint: string | null;
    mapPointModel: PointModel | null;
    mapPolygon: string | null;
    mapPolygonModel: PolygonModel | null;
    htmlIcon: string | null;
    iconAnchor: L.PointExpression | null;
    customTooltipClass: string | null;
    customPolygonClass: string | null;
    stopId: number | null;
    narrativeMapId: number | null;
    priorityOnMap: number | null;


}

interface MapSettingsForLeafletModel {
    initialVariables: InitialVariables;
    layers: LayerForLeafletModel[];
}

interface InitialVariables {
    zoom: number | null;
    zoomMobile: number | null;
    lat: number | null;
    lng: number | null;
    minBounds: any | null;
    maxBounds: any | null;
    minZoom: number | null;
    maxZoom: number | null;
    wmsProxyUrl: string | null;
    heightOfDialog: number | null;
}

interface LayerForLeafletModel {
    name: string | null;
    url: string | null;
    type: string | null;
    selected: boolean | null;
    attribution: string | null;
    mapParameter: string | null;
    layersParameter: string | null;
    zIndex: number | null;
}

interface PolygonModel {
    type: string;
    coordinates: number[][][]
}

interface PointModel {
    type: string;
    coordinates: number[]
}

interface Coordinates {
    X: number;
    Y: number;
}

interface MapInfo {
    zoom: string;
    bounds: L.LatLngBounds | null;
    customCoordinates: Coordinates | null;
}

enum NarrativeMapStopPlaceType {
    Main = "main point",
    Context = "context point",
    Trajectory = "trajectory point",
}

enum MapTypeEnum {
    Normal,
    AllStoryMaps,
    StoryMapWhole,
    StoryMapOneStop
}

enum LayersTypeEnum {
    Base = "Base",
    WMS = "WMS",
    Narration = "Narration"
}