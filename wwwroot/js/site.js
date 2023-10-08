var mapAPI;
(function (mapAPI) {
    class variables {
    }
    variables.map = null;
    variables.lat = 0;
    variables.lng = 0;
    variables.coorx = 0;
    variables.coory = 0;
    variables.bluepointIcon = null;
    variables.addressIcon = null;
    variables.incidentIcon = null;
    variables.interestIcon = null;
    variables.blazorMapObject = null;
    variables.groups = [];
    variables.trackingInterval = null;
    variables.isMobileBrowser = null;
    variables.applicationIsTrackingLocation = null;
    variables.actualLocation = null;
    variables.dialogWidth = null;
    variables.dialogHeight = null;
    variables.isFullscreen = null;
    mapAPI.variables = variables;
    function initMap(jsonMapSettings) {
        fitMapToWindow();
        variables.incidentIcon = new L.DivIcon({ className: 'leaflet-incident-icon' });
        variables.addressIcon = new L.DivIcon();
        variables.interestIcon = new L.DivIcon({ className: 'leaflet-interest-icon' });
        variables.bluepointIcon = new L.Icon({
            iconUrl: 'images/blue-point.png',
            iconSize: [20, 20],
            iconAnchor: [10, 10],
        });
        variables.map = new L.Map('map', { zoomControl: false });
        if (!setMapWithInfoFromUrl())
            variables.map.setView([50.07905886, 14.43715096], 14);
        variables.map.on("moveend", function () {
            document.getElementById("map").style.cursor = 'default';
            setUrlByMapInfo();
            callBlazor_RefreshObjectsOnMap();
        });
        variables.map.addEventListener('mousemove', function (ev) {
            variables.lat = ev.latlng.lat;
            variables.lng = ev.latlng.lng;
            variables.coorx = ev.containerPoint.x;
            variables.coory = ev.containerPoint.y;
        });
        const mapSettings = JSON.parse(jsonMapSettings);
        for (let i = 0; i < mapSettings.length; i++) {
            const group = new L.LayerGroup(null, { id: mapSettings[i].name + "_group" });
            group.setZIndex(mapSettings[i].zIndex);
            variables.groups.push(group);
            if (mapSettings[i].selected)
                group.addTo(variables.map);
            const layer = convertMapSettingsObjectToMapLayer(mapSettings[i]);
            if (layer != null)
                group.addLayer(layer);
        }
    }
    mapAPI.initMap = initMap;
    function fitMapToWindow() {
        const mapElement = document.getElementById("map");
        if (mapElement == null || !mapElement)
            return;
        const mapHeight = window.innerHeight - mapElement.offsetTop - 1;
        mapElement.style.height = mapHeight + "px";
    }
    mapAPI.fitMapToWindow = fitMapToWindow;
    function getMapInfoFromUrl() {
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
            zoom: zoom !== null && zoom !== void 0 ? zoom : "13"
        };
    }
    mapAPI.getMapInfoFromUrl = getMapInfoFromUrl;
    function setUrlByMapInfo() {
        const bounds = variables.map.getBounds();
        const urlBounds = bounds.getSouthWest().lat + "," + bounds.getSouthWest().lng + "," + bounds.getNorthEast().lat + "," + bounds.getNorthEast().lng;
        setUrlParam('bounds', urlBounds);
        setUrlParam('zoom', variables.map.getZoom());
    }
    mapAPI.setUrlByMapInfo = setUrlByMapInfo;
    function setUrlParam(paramName, paramValue) {
        const urlParams = new URLSearchParams(window.location.search);
        urlParams.set(paramName, paramValue.toString());
        history.pushState({ pageID: '100' }, 'Mapa', "?" + urlParams);
    }
    mapAPI.setUrlParam = setUrlParam;
    function getUrlParam(paramName) {
        const urlParams = new URLSearchParams(window.location.search);
        return urlParams.get(paramName);
    }
    mapAPI.getUrlParam = getUrlParam;
    function setMapWithInfoFromUrl() {
        const infoFromUrl = getMapInfoFromUrl();
        if (infoFromUrl == null)
            return false;
        variables.map.fitBounds(infoFromUrl.bounds);
        const latAvg = (infoFromUrl.bounds.getSouthWest().lat + infoFromUrl.bounds.getNorthEast().lat) / 2;
        const lngAvg = (infoFromUrl.bounds.getSouthWest().lng + infoFromUrl.bounds.getNorthEast().lng) / 2;
        variables.map.setView([latAvg, lngAvg], Number(infoFromUrl.zoom));
        return true;
    }
    mapAPI.setMapWithInfoFromUrl = setMapWithInfoFromUrl;
    function convertMapSettingsObjectToMapLayer(mapSettingsObject) {
        if (mapSettingsObject.type == 'Base') {
            return new L.TileLayer(mapSettingsObject.url, {
                maxZoom: 18,
                attribution: mapSettingsObject.attribution,
            });
        }
        else if (mapSettingsObject.type == 'WMS') {
            return new L.TileLayer.WMS(mapSettingsObject.url, {
                tileSize: 512,
                map: mapSettingsObject.mapParameter,
                layers: mapSettingsObject.layersParameter
            });
        }
        return null;
    }
    mapAPI.convertMapSettingsObjectToMapLayer = convertMapSettingsObjectToMapLayer;
    function initBlazorMapObject(dotNetObject) {
        variables.blazorMapObject = dotNetObject;
    }
    mapAPI.initBlazorMapObject = initBlazorMapObject;
    function callBlazor_RefreshObjectsOnMap() {
        const polygonsGroup = variables.groups.find(a => a.options.id == "Polygons_group");
        const mapHasPolygonsYet = polygonsGroup.getLayers().length > 0;
        variables.blazorMapObject.invokeMethodAsync("RefreshObjectsOnMap", !mapHasPolygonsYet, variables.map.getZoom(), getMapBoundsForMapState());
    }
    mapAPI.callBlazor_RefreshObjectsOnMap = callBlazor_RefreshObjectsOnMap;
    function callBlazor_ShowPlaceInfo(event) {
        const point = event.target._latlng != undefined ? event.target._latlng : [variables.lat, variables.lng];
        const bbox = convertObjectPositionToBBoxParameter(point);
        variables.blazorMapObject.invokeMethodAsync("ShowPlaceInfo", variables.map.getZoom(), bbox);
    }
    mapAPI.callBlazor_ShowPlaceInfo = callBlazor_ShowPlaceInfo;
    function refreshObjectsOnMap(objectJson) {
        const objectsGroup = variables.groups.find(a => a.options.id == "Objects_group");
        const polygonsGroup = variables.groups.find(a => a.options.id == "Polygons_group");
        objectsGroup.clearLayers();
        const objects = JSON.parse(objectJson);
        for (let i = 0; i < objects.length; i++) {
            let newObject;
            if (objects[i].mapPolygon != null) {
                const polygonObject = JSON.parse(objects[i].mapPolygon);
                newObject = getPolygon(polygonObject, objects[i].label);
                newObject.addTo(polygonsGroup);
            }
            else {
                const markerObject = JSON.parse(objects[i].mapPoint);
                newObject = getPoint(markerObject, objects[i].clickable, objects[i].label, objects[i].htmlIcon);
                newObject.addTo(objectsGroup);
            }
        }
    }
    mapAPI.refreshObjectsOnMap = refreshObjectsOnMap;
    function addObjectFromJsonString(jsonInfo) {
        const parsedObject = JSON.parse(jsonInfo);
        const newObject = parsedObject.type == "Point" ? getPoint(parsedObject) : getPolygon(parsedObject, null, "#e500ff");
        const objectsGroup = variables.groups.find(a => a.options.id == "AdditionalObjects_group");
        objectsGroup.clearLayers();
        newObject.addTo(objectsGroup);
    }
    mapAPI.addObjectFromJsonString = addObjectFromJsonString;
    function getPoint(markerObject, clickable, label, htmlIcon) {
        let iconOptions = null;
        if (htmlIcon != undefined && htmlIcon != null)
            iconOptions = { icon: new L.DivIcon({ className: "", html: htmlIcon }) };
        const result = new L.Marker([markerObject.coordinates[1], markerObject.coordinates[0]], iconOptions);
        if (!isMobileBrowser() && label != undefined && label != null) {
            result.bindTooltip(label, { sticky: true });
        }
        if (clickable) {
            result.on('click', callBlazor_ShowPlaceInfo);
        }
        return result;
    }
    mapAPI.getPoint = getPoint;
    function getPolygon(polygonObject, label, color) {
        const pointsArray = [];
        for (let j = 0; j < polygonObject.coordinates.length; j++) {
            for (let m = 0; m < polygonObject.coordinates[j].length; m++) {
                const polygon = polygonObject.coordinates[j][m];
                pointsArray.push([]);
                const innerArray = [];
                for (let k = 0; k < polygon.length; k++) {
                    innerArray.push([polygon[k][1], polygon[k][0]]);
                }
                pointsArray[j].push(innerArray);
            }
        }
        const result = new L.Polygon(pointsArray, { fillColor: color != undefined && color != null ? color : '#E47867', color: '#222', weight: 0.5, fillOpacity: 0.8, opacity: 1 })
            .on('click', callBlazor_ShowPlaceInfo);
        if (!isMobileBrowser() && label != undefined && label != null) {
            result.bindTooltip(label, { sticky: true });
        }
        return result;
    }
    mapAPI.getPolygon = getPolygon;
    function removeAdditionalObjects() {
        const objectsGroup = variables.groups.find(a => a.options.id == "AdditionalObjects_group");
        objectsGroup.eachLayer(function (item) {
            if (item.options.type == undefined || item.options.type !== "bluepoint") {
                item.remove();
            }
        });
    }
    mapAPI.removeAdditionalObjects = removeAdditionalObjects;
    function toggleLayerGroup(name, selected) {
        const groupName = name + "_group";
        if (!selected) {
            variables.map.eachLayer(function (layer) { if (layer.options.id == groupName)
                layer.remove(); });
        }
        else {
            const groupToAdd = variables.groups.find(a => a.options.id == groupName);
            groupToAdd.addTo(variables.map);
        }
    }
    mapAPI.toggleLayerGroup = toggleLayerGroup;
    function convertObjectPositionToBBoxParameter(point) {
        const containerPosition = variables.map.latLngToContainerPoint(point);
        const sizeOfBox = 5;
        const minx = containerPosition.x < sizeOfBox / 2 ? 0 : containerPosition.x - sizeOfBox / 2;
        const miny = containerPosition.y < sizeOfBox / 2 ? 0 : containerPosition.y + sizeOfBox / 2;
        const maxx = containerPosition.x < sizeOfBox / 2 ? 0 : containerPosition.x + sizeOfBox / 2;
        const maxy = containerPosition.y < sizeOfBox / 2 ? 0 : containerPosition.y - sizeOfBox / 2;
        const southWest = variables.map.containerPointToLatLng([minx, maxy]);
        const northEast = variables.map.containerPointToLatLng([maxx, miny]);
        return [{ X: southWest.lng, Y: southWest.lat }, { X: northEast.lng, Y: northEast.lat }];
    }
    mapAPI.convertObjectPositionToBBoxParameter = convertObjectPositionToBBoxParameter;
    function getWindowWidth() {
        return window.innerWidth;
    }
    mapAPI.getWindowWidth = getWindowWidth;
    function getWindowHeight() {
        return window.innerHeight;
    }
    mapAPI.getWindowHeight = getWindowHeight;
    function getMapBoundsForMapState() {
        const bounds = variables.map.getBounds();
        return [{ X: bounds.getSouthWest().lng, Y: bounds.getSouthWest().lat }, { X: bounds.getNorthEast().lng, Y: bounds.getNorthEast().lat }];
    }
    mapAPI.getMapBoundsForMapState = getMapBoundsForMapState;
    function getWindowLocationSearch() {
        return window.location.search;
    }
    mapAPI.getWindowLocationSearch = getWindowLocationSearch;
    function isMobileBrowser() {
        if (variables.isMobileBrowser != null)
            return variables.isMobileBrowser;
        if (navigator.userAgent.match(/Android/i)
            || navigator.userAgent.match(/webOS/i)
            || navigator.userAgent.match(/iPhone/i)
            || navigator.userAgent.match(/iPad/i)
            || navigator.userAgent.match(/iPod/i)
            || navigator.userAgent.match(/BlackBerry/i)
            || navigator.userAgent.match(/Windows Phone/i))
            variables.isMobileBrowser = true;
        else
            variables.isMobileBrowser = false;
        return variables.isMobileBrowser;
    }
    mapAPI.isMobileBrowser = isMobileBrowser;
    function getZoom() {
        return variables.map.getZoom();
    }
    mapAPI.getZoom = getZoom;
    function zoomIn() {
        variables.map.zoomIn();
    }
    mapAPI.zoomIn = zoomIn;
    function zoomOut() {
        variables.map.zoomOut();
    }
    mapAPI.zoomOut = zoomOut;
    function turnOnLocationTracking() {
        if (!navigator.geolocation) {
            console.log("Your browser doesn't support geolocation feature!");
        }
        else {
            navigator.geolocation.getCurrentPosition(showMyLocation);
            variables.trackingInterval = setInterval(() => {
                navigator.geolocation.getCurrentPosition(showMyLocation);
            }, 5000);
        }
    }
    mapAPI.turnOnLocationTracking = turnOnLocationTracking;
    function goToLocation(pointString, zoom) {
        const point = JSON.parse(pointString).coordinates;
        variables.map.setView([point[1], point[0]], zoom);
    }
    mapAPI.goToLocation = goToLocation;
    function goToMyLocation() {
        const lat = variables.actualLocation.coords.latitude;
        const long = variables.actualLocation.coords.longitude;
        variables.map.setView([lat, long], 15);
    }
    mapAPI.goToMyLocation = goToMyLocation;
    function showMyLocation(position) {
        variables.actualLocation = position;
        const lat = position.coords.latitude;
        const long = position.coords.longitude;
        removeBluepoint();
        addBluepoint([lat, long], variables.bluepointIcon);
        if (!variables.applicationIsTrackingLocation) {
            goToMyLocation();
            variables.applicationIsTrackingLocation = true;
        }
    }
    mapAPI.showMyLocation = showMyLocation;
    function hideMyLocation() {
        clearInterval(variables.trackingInterval);
        variables.applicationIsTrackingLocation = false;
        removeBluepoint();
    }
    mapAPI.hideMyLocation = hideMyLocation;
    function addBluepoint(point, icon) {
        const objectsGroup = variables.groups.find(a => a.options.id == "AdditionalObjects_group");
        new L.Marker(point, { icon: icon, type: "bluepoint" }).addTo(objectsGroup);
    }
    mapAPI.addBluepoint = addBluepoint;
    function removeBluepoint() {
        const objectsGroup = variables.groups.find(a => a.options.id == "AdditionalObjects_group");
        objectsGroup.eachLayer(function (item) {
            if (item.options.type !== undefined && item.options.type == "bluepoint") {
                item.remove();
            }
        });
    }
    mapAPI.removeBluepoint = removeBluepoint;
    function fullscreenDialog(value) {
        if (value) {
            if (!variables.isFullscreen) {
                variables.dialogWidth = document.querySelector("aside").style.width;
                variables.dialogHeight = document.querySelector("aside").style.height;
            }
            variables.isFullscreen = true;
            document.querySelector("aside").style.width = "100%";
            document.querySelector("aside").style.height = "100%";
        }
        else if (!value && variables.isFullscreen && variables.dialogHeight != null && variables.dialogWidth != null) {
            variables.isFullscreen = false;
            document.querySelector("aside").style.width = variables.dialogWidth;
            document.querySelector("aside").style.height = variables.dialogHeight;
        }
    }
    mapAPI.fullscreenDialog = fullscreenDialog;
})(mapAPI || (mapAPI = {}));
window.addEventListener("resize", mapAPI.fitMapToWindow);
//# sourceMappingURL=site.js.map