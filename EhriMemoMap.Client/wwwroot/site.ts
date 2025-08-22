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
    let blazorMapObject = null;
    let groups: L.FeatureGroup[] = [];
    let trackingInterval: number = null;
    let _isMobileView: boolean = null;
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

        const mapSettings = JSON.parse(jsonMapSettings) as MapSettingsForLeafletModel;
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
                selected: mapSettings.layers[i].selected
            });
            group.setZIndex(mapSettings.layers[i].zIndex);
            groups.push(group);

            if (mapSettings.layers[i].selected)
                group.addTo(map);

            const layer = convertMapSettingsObjectToMapLayer(mapSettings.layers[i]);
            if (layer != null)
                group.addLayer(layer);
        }
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
        blazorMapObject.invokeMethodAsync("SetMobileView", isMobileView());

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
        if (mapSettingsObject.type == 'Base') {
            return new L.TileLayer(mapSettingsObject.url, {
                maxZoom: 18,
                attribution: mapSettingsObject.attribution,
            });
        }
        else if (mapSettingsObject.type == 'WMS') {
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

    // připraví instanci blazor třídy Map.razor, abych ji pak mohl později odsud volat
    export function initBlazorMapObject(dotNetObject) {
        blazorMapObject = dotNetObject;
    }
    export function callBlazor_RefreshObjectsOnMap() {
        const polygonsGroup = groups.find(a => a.options.id == "Polygons_group");
        const mapHasPolygonsYet = polygonsGroup.getLayers().length > 0;
        blazorMapObject.invokeMethodAsync("RefreshObjectsOnMap", !mapHasPolygonsYet, map.getZoom(), getMapBoundsForMapState());
    }

    export function callBlazor_ShowPlaceInfo(event) {

        // nejdřív odstraníme všechny dodatečné objekty
        removeAdditionalObjects();
        unselectAllSelectedPoints();

        // a pak obarvíme vybraný polygon, respektive přidáme špendlík
        if (event.target._latlng == undefined)
            (event.target as L.Polygon).setStyle({ fillColor: polygonColorSelected });
        else {
            event.target._icon.className = event.target._icon.className.replace('map-point-selected', 'map-point').replace('map-point', 'map-point-selected');
            //event.target._icon.style.zIndex = '200';
        }

        const point = event.target._latlng != undefined ? event.target._latlng : [lat, lng];
        const bbox = convertObjectPositionToBBoxParameter(point);
        blazorMapObject.invokeMethodAsync("ShowPlaceInfo", map.getZoom(), bbox, coorx);
    }

    //////////////////////////
    /// OBJECTS
    //////////////////////////

    // refreshuje se všechno, kromě nepřístupných míst, protože ta jsou vidět pořád
    export function refreshObjectsOnMap(objectJson) {
        const objectsGroup = groups.find(a => a.options.id == "Objects_group");
        const polygonsGroup = groups.find(a => a.options.id == "Polygons_group");

        objectsGroup.clearLayers();

        const objects = JSON.parse(objectJson) as MapObjectForLeafletModel[];


        for (let i = 0; i < objects.length; i++) {
            let newObject;

            // polygony
            if (objects[i].mapPolygon != null) {

                objects[i].mapPolygonModel = JSON.parse(objects[i].mapPolygon) as PolygonModel;
                newObject = getPolygon(objects[i]);

                if (objects[i].placeType == "Inaccessible")
                    newObject.addTo(polygonsGroup);
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
        polygonsGroup.bringToFront();

        // pokud je heatmapa, tak ji zobrazíme
        if (heatmapLayer) {
            map.removeLayer(heatmapLayer);
        }

        var heatmapIndex = getIndexOfHeatmapData(objects);
        if (heatmapIndex != null) {
            seedHeatmapData(objects, heatmapIndex);
            showHeatmap(heatmapData.find(a => a.id === heatmapIndex).heatmapdata);
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

            if (parsedLocation.type == "Point") {
                parsedObjects[i].mapPointModel = parsedLocation;
                newObject = getPoint(
                    parsedObjects[i],
                    'z-index-999',
                    parsedObjects[i].placeType == NarrativeMapStopPlaceType.Main ? scrollToStop : null)
            } else {
                parsedObjects[i].mapPolygonModel = parsedLocation;
                newObject = getPolygon(parsedObjects[i]);
            }

            newObject.addTo(objectsGroup);
        }
        toggleLayerGroup(group, false);
        toggleLayerGroup(group, true);
        drawCurveBettwenPoints(groupName);
        fitMapToGroup(groupName);
    }

    export function drawCurveBettwenPoints(groupName) {
        const objectsGroup = groups.find(a => a.options.id == groupName);
        const layers = objectsGroup.getLayers() as L.Marker[];
        var pointA = null;
        var pointB = null;
        for (let i = 0; i < layers.length; i++) {
            if (layers[i].options.type == NarrativeMapStopPlaceType.Trajectory) {
                if (pointA == null) {
                    pointA = layers[i];
                } else if (pointB == null) {
                    pointB = layers[i];
                    getCurve(pointA.getLatLng(), pointB.getLatLng()).addTo(objectsGroup);
                    pointA = pointB;
                    pointB = null;
                }
            }
        }
    }

    export function getCurve(pointA: L.LatLng, pointB: L.LatLng) {

        // Vypočítání kontrolního bodu pro Bezierovu křivku
        const midLat = (pointA.lat + pointB.lat) / 2;
        const midLng = (pointA.lng + pointB.lng) / 2;
        const controlPoint = [midLat + 0.15, midLng]; // Kontrolní bod trochu posunutý nahoru

        function sampleCurve(start: number[], control: number[], end: number[], segments = 50) {
            const points = [];
            for (let t = 0; t <= 1; t += 1 / segments) {
                const x =
                    (1 - t) * (1 - t) * start[1] +
                    2 * (1 - t) * t * control[1] +
                    t * t * end[1];
                const y =
                    (1 - t) * (1 - t) * start[0] +
                    2 * (1 - t) * t * control[0] +
                    t * t * end[0];
                points.push([y, x]);
            }
            return points;
        }

        // Vygenerování bodů pro křivku
        const curvePoints = sampleCurve([pointA.lat, pointA.lng], controlPoint, [pointB.lat, pointB.lng]);

        // Přidání zakřivené čáry jako `L.Polyline`
        const curveLine = L.polyline(curvePoints, {
            color: '#C44527',
            weight: 2,
            dashArray: '10, 10', // Střídání délky čáry a mezery
            dashOffset: '0'
        }).arrowheads({
            frequency: 'endonly',
            fill: true,
            size: '10px',
            color: '#C44527'
        });

        return curveLine;

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
            iconOptions = { stopId: markerObject.stopId, type: markerObject.placeType, icon: new L.DivIcon({ className: className, html: markerObject.htmlIcon }) }

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

        const polygonsGroup = groups.find(a => a.options.id == "Polygons_group");
        polygonsGroup.eachLayer(function (item: L.Polygon) {
            if (item.options.fillColor != undefined || item.options.fillColor == polygonColorSelected) {
                item.setStyle({ fillColor: polygonColor });
            }
        });

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

    export function hideAllLayers() {
        groups.forEach(group => {
            if (group.options.type == "Base")
                return;
            map.eachLayer(function (layer) {
                if (layer.options.id == group.options.id)
                    layer.remove();
            })
        });
    }

    export function showAllLayers() {
        // nejdřív všechny vrstvy odstraníme, aby se pak na mapě zbytečně nepřekrývaly
        hideAllLayers();
        groups.forEach(group => {
            if (group.options.type == "Base" || group.options.type == "Narration")
                return;
            if (group.options.selected)
                group.addTo(map);
        });
    }

    export function toggleLayerGroup(name: string, selected: boolean): void {
        const groupName = name + "_group";
        const groupToAdd = groups.find(a => a.options.id == groupName);
        groupToAdd.options.selected = selected;

        // nejdřív vrstvu odstraníme, abych pak nepřidával přes starou další a další a další...
        map.eachLayer(function (layer) { if (layer.options.id == groupName) layer.remove(); })

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

    export function scrollToStop(event): void {

        const point = event.target as L.Marker;
        const targetElement = document.getElementById("narrative-stop-" + point.options.stopId);
        const container = document.querySelector("aside");

        if (container && targetElement) {
            // Získáme pozici cílového prvku vůči kontejneru
            const rect = targetElement.getBoundingClientRect();
            const containerRect = container.getBoundingClientRect();

            // Vypočítáme pozici scrollu v kontejneru
            const scrollPosition = container.scrollTop + (rect.top - containerRect.top) - 20; // Odečteme 20px

            // Posuneme kontejner na vypočtenou pozici
            container.scrollTo({ top: scrollPosition, behavior: "smooth" });
        }
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
    customTooltipClass: string | null;
    customPolygonClass: string | null;
    stopId: number | null;
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

