// FUNKCE PRO OVLÁDÁNÍ MAPY

interface MapObjectForLeafletModel {
    clickable: boolean;
    placeType: string | null;
    citizens: number | null;
    citizensTotal: number | null;
    id: number | null;
    guid: string | null;
    label: string | null;
    mapPoint: string | null;
    mapPolygon: string | null;
    htmlIcon: string | null;
    customTooltipClass: string | null;
    customPolygonClass: string | null;


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
    bounds: L.LatLngBounds;
}

namespace mapAPI {


    //////////////////////////
    /// VARIABLES
    //////////////////////////

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
    let _isMobileBrowser: boolean = null;
    let applicationIsTrackingLocation: boolean = null;
    let actualLocation: GeolocationPosition = null;
    let dialogWidth: string = null;
    let dialogHeight: string = null;
    let isFullscreen: boolean = null;
    let polygonStrokeColor: string = "#222";
    let polygonColor: string = "#C5222C";
    let polygonColorSelected: string = "#000"; // "#cc1111";

    //////////////////////////
    /// INIT
    //////////////////////////

    // připraví mapu do úvodního stavu
    export function initMap(jsonMapSettings: string): void {

        fitMapToWindow(null);

        incidentIcon = new L.DivIcon({ className: 'leaflet-incident-icon' });
        addressIcon = new L.DivIcon();
        interestIcon = new L.DivIcon({ className: 'leaflet-interest-icon' });

        bluepointIcon = new L.Icon({
            iconUrl: 'images/blue-point.png',
            iconSize: [20, 20], // size of the icon
            iconAnchor: [10, 10], // point of the icon which will correspond to marker's location
        });

        map = new L.Map('map', { zoomControl: false });
        map.attributionControl.setPosition('bottomleft');
        L.control.scale().setPosition(mapAPI.isMobileBrowser() ? 'topright' : 'bottomleft').addTo(map);

        if (!setMapWithInfoFromUrl())
            map.setView([50.07905886, 14.43715096], 14);

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

        const mapSettings = JSON.parse(jsonMapSettings) as LayerForLeafletModel[];

        // konverze json objektu do jednotlivých vrstev mapy a jejich přidání k mapě
        for (let i = 0; i < mapSettings.length; i++) {
            const group = new L.FeatureGroup(null, { id: mapSettings[i].name + "_group" });
            group.setZIndex(mapSettings[i].zIndex);
            groups.push(group);

            if (mapSettings[i].selected)
                group.addTo(map);

            const layer = convertMapSettingsObjectToMapLayer(mapSettings[i]);
            if (layer != null)
                group.addLayer(layer);
        }
    }

    // nastaví mapu, aby lícovala s oknem
    export function fitMapToWindow(mobileDialogHeightPercents): void {
        const mobileDialogHeight = mobileDialogHeightPercents != null ? window.innerHeight * (mobileDialogHeightPercents / 100) : 0;
        const mapElement = document.getElementById("map");
        const pageElement = document.getElementsByClassName("page");

        if (mapElement == null || !mapElement)
            return;

        const pageHeight = window.innerHeight;
        const mapHeight = !mapAPI.isMobileBrowser() ? pageHeight : pageHeight - 44 - 44 - mobileDialogHeight; // 44 je horní a dolní panel
        mapElement.style.height = mapHeight + "px";

        if (mapAPI.isMobileBrowser())
            mapElement.style.marginTop = "44px";
        pageElement[0].style.height = pageHeight + "px";

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

        if (urlBounds == null)
            return null;

        const bounds = urlBounds.split(",");

        const southWest = new L.LatLng(Number(bounds[0]), Number(bounds[1]));
        const northEast = new L.LatLng(Number(bounds[2]), Number(bounds[3]));

        const zoom = urlParams.get("zoom");

        return {
            bounds: new L.LatLngBounds(southWest, northEast),
            zoom: zoom ?? "13"
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
        history.pushState({ pageID: '100' }, 'Mapa', "?" + urlParams);
    }

    // získá parametr z url
    export function getUrlParam(paramName: string): string {
        const urlParams = new URLSearchParams(window.location.search);
        return urlParams.get(paramName);
    }

    // nastaví mapu podle parametrů url
    export function setMapWithInfoFromUrl(): boolean {
        const infoFromUrl = getMapInfoFromUrl();
        if (infoFromUrl == null)
            return false;
        map.fitBounds(infoFromUrl.bounds);

        const latAvg = (infoFromUrl.bounds.getSouthWest().lat + infoFromUrl.bounds.getNorthEast().lat) / 2;
        const lngAvg = (infoFromUrl.bounds.getSouthWest().lng + infoFromUrl.bounds.getNorthEast().lng) / 2;

        map.setView([latAvg, lngAvg], Number(infoFromUrl.zoom));

        return true;
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
            return new L.TileLayer.WMS("WMSProxy/Get", {
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
            event.target._icon.className = event.target._icon.className.replace('map-point', 'map-point-selected');
        //    const objectsGroup = groups.find(a => a.options.id == "AdditionalObjects_group");
        //    new L.Marker(event.target._latlng).addTo(objectsGroup);
        }

        const point = event.target._latlng != undefined ? event.target._latlng : [lat, lng];
        const bbox = convertObjectPositionToBBoxParameter(point);
        blazorMapObject.invokeMethodAsync("ShowPlaceInfo", map.getZoom(), bbox);
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

            if (objects[i].mapPolygon != null) {
                const polygonObject = JSON.parse(objects[i].mapPolygon) as PolygonModel;
                newObject = getPolygon(polygonObject, objects[i].clickable, objects[i].label, null, objects[i].customTooltipClass, objects[i].customPolygonClass);
                if (objects[i].placeType == "Inaccessible")
                    newObject.addTo(polygonsGroup);
                else
                    newObject.addTo(objectsGroup);
            } else {
                const markerObject = JSON.parse(objects[i].mapPoint) as PointModel;
                newObject = getPoint(markerObject, objects[i].clickable, objects[i].label, objects[i].htmlIcon);
                newObject.addTo(objectsGroup);
            }
        }
        polygonsGroup.bringToFront();
    }

    export function addObjectFromJsonString(jsonInfo) {
        const parsedObject = JSON.parse(jsonInfo);
        const newObject = parsedObject.type == "Point" ? getPoint(parsedObject) : getPolygon(parsedObject, null, polygonColor);
        const objectsGroup = groups.find(a => a.options.id == "AdditionalObjects_group");
        objectsGroup.clearLayers();
        newObject.addTo(objectsGroup);
    }
    export function getPoint(markerObject: PointModel, clickable?: boolean, label?: string, htmlIcon?: string) {
        let iconOptions = null;
        if (htmlIcon != undefined && htmlIcon != null)
            iconOptions = { icon: new L.DivIcon({ className: "map-point", html: htmlIcon }) }

        const result = new L.Marker([markerObject.coordinates[1], markerObject.coordinates[0]], iconOptions);

        if (!isMobileBrowser() && label != undefined && label != null) {
            result.bindTooltip(label, { sticky: true });
        }

        if (clickable) {
            result.on('click', callBlazor_ShowPlaceInfo);
        }

        return result;
    }

    export function getPolygon(polygonObject: PolygonModel, clickable: boolean, label: string, color?: string, customTooltipClass?: string, customPolygonClass?: string): L.Polygon {
        const pointsArray = [];
        for (let j = 0; j < polygonObject.coordinates.length; j++) {
            for (let m = 0; m < polygonObject.coordinates[j].length; m++) {
                const polygon = polygonObject.coordinates[j][m];
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

        const polygonOptions = customPolygonClass != undefined && customPolygonClass != null
            ? { className: customPolygonClass, color: null, weight: null, fillOpacity: null, opacity: null, fillColor: null } // tohle se používá pro statistiku čtvrtí
            : { fillColor: polygonColor, color: polygonStrokeColor, weight: 0.5, fillOpacity: 0.40, opacity: 1 }; // tohle se používá pro nepřístupná místa

        const result = new L.Polygon(pointsArray, polygonOptions);

        if (clickable) {
            result.on('click', callBlazor_ShowPlaceInfo);
        }
        

        if (label != undefined && label != null) {
            result.bindTooltip(label, { sticky: true, className: customTooltipClass != undefined && customTooltipClass != null ? customTooltipClass : null });
        }

        return result;
    }

    export function removeAdditionalObjects(): void {
        const objectsGroup = groups.find(a => a.options.id == "AdditionalObjects_group");
        objectsGroup.eachLayer(function (item) {
            if (item.options.type == undefined || item.options.type !== "bluepoint") {
                item.remove();
            }
        });
        const polygonsGroup = groups.find(a => a.options.id == "Polygons_group");
        polygonsGroup.eachLayer(function (item: L.Polygon) {
            if (item.options.fillColor != undefined || item.options.fillColor == polygonColorSelected) {
                item.setStyle({ fillColor: polygonColor });
            }
        });

    }

    export function unselectAllSelectedPoints(): void {
        var selectedPoints = document.getElementsByClassName('map-point-selected');
        for (var i = 0; i < selectedPoints.length; i++) {
            selectedPoints[i].className = selectedPoints[i].className.replace('map-point-selected', 'map-point');
        }
    }

    //////////////////////////
    /// HELPER METHODS
    //////////////////////////

    export function toggleLayerGroup(name: string, selected: boolean): void {
        const groupName = name + "_group";
        if (!selected) {
            map.eachLayer(function (layer) { if (layer.options.id == groupName) layer.remove(); })
        } else {
            const groupToAdd = groups.find(a => a.options.id == groupName);
            groupToAdd.addTo(map);
        }
    }

    // převede pozici myši nad mapou do parametru bbox pro získání informací o daném místě (dotaz na server)
    export function convertObjectPositionToBBoxParameter(point): Coordinates[] {

        const containerPosition = map.latLngToContainerPoint(point);

        const sizeOfBox = 5;// isMobileBrowser() ? 20 : 5;

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

    export function getWindowLocationSearch(): string {
        return window.location.search;
    }

    export function isMobileBrowser(): boolean {
        return true;
        if (_isMobileBrowser != null)
            return _isMobileBrowser;

        if (navigator.userAgent.match(/Android/i)
            || navigator.userAgent.match(/webOS/i)
            || navigator.userAgent.match(/iPhone/i)
            || navigator.userAgent.match(/iPad/i)
            || navigator.userAgent.match(/iPod/i)
            || navigator.userAgent.match(/BlackBerry/i)
            || navigator.userAgent.match(/Windows Phone/i))
            _isMobileBrowser = true;
        else
            _isMobileBrowser = false;

        return _isMobileBrowser;
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

    //////////////////////////
    /// TRACKING
    //////////////////////////

    export function turnOnLocationTracking(): void {
        if (!navigator.geolocation) {
            console.log("Your browser doesn't support geolocation feature!")
        } else {
            navigator.geolocation.getCurrentPosition(showMyLocation)
            trackingInterval = setInterval(() => {
                navigator.geolocation.getCurrentPosition(showMyLocation)
            }, 5000);
        }

    }

    export function goToLocation(pointString, zoom: number): void {
        const point = (JSON.parse(pointString) as PointModel).coordinates;
        map.setView([point[1], point[0]], zoom);
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

    //////////////////////////
    /// DIALOG
    //////////////////////////


    export function fullscreenDialog(value): void {
        if (value) {
            if (!isFullscreen) {
                dialogWidth = document.querySelector("aside").style.width;
                dialogHeight = document.querySelector("aside").style.height;
            }
            isFullscreen = true;
            document.querySelector("aside").style.width = "100%";
            document.querySelector("aside").style.height = "100%";
        } else if (!value && isFullscreen && dialogHeight != null && dialogWidth != null) {
            isFullscreen = false;
            document.querySelector("aside").style.width = dialogWidth;
            document.querySelector("aside").style.height = dialogHeight;

        }
    }
}

window.addEventListener("resize", mapAPI.fitMapToWindow);
