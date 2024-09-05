var mapAPI;
(function (mapAPI) {
    let wmsProxyUrl = "";
    let map = null;
    let lat = 0;
    let lng = 0;
    let coorx = 0;
    let coory = 0;
    let bluepointIcon = null;
    let addressIcon = null;
    let incidentIcon = null;
    let interestIcon = null;
    let blazorMapObject = null;
    let groups = [];
    let trackingInterval = null;
    let _isMobileView = null;
    let applicationIsTrackingLocation = null;
    let actualLocation = null;
    let dialogWidth = null;
    let dialogHeight = null;
    let mobileDialogHeight = null;
    let isFullscreen = null;
    let polygonStrokeColor = "#222";
    let polygonColor = "#C5222C";
    let polygonColorSelected = "#000";
    function initMap(jsonMapSettings) {
        const mapSettings = JSON.parse(jsonMapSettings);
        mobileDialogHeight = mapSettings.initialVariables.heightOfDialog;
        wmsProxyUrl = mapSettings.initialVariables.wmsProxyUrl;
        fitMapToWindow();
        incidentIcon = new L.DivIcon({ className: 'leaflet-incident-icon' });
        addressIcon = new L.DivIcon();
        interestIcon = new L.DivIcon({ className: 'leaflet-interest-icon' });
        bluepointIcon = new L.Icon({
            iconUrl: 'images/blue-point.png',
            iconSize: [20, 20],
            iconAnchor: [10, 10],
        });
        map = new L.Map('map', {
            zoomControl: false,
            maxZoom: mapSettings.initialVariables.maxZoom,
            minZoom: mapSettings.initialVariables.minZoom,
            maxBounds: [
                [mapSettings.initialVariables.minBounds.x, mapSettings.initialVariables.minBounds.y],
                [mapSettings.initialVariables.maxBounds.x, mapSettings.initialVariables.maxBounds.y]
            ],
            zoomSnap: mapAPI.isMobileView() ? 0.1 : 1
        });
        map.attributionControl.setPosition('bottomleft');
        L.control.scale().setPosition(mapAPI.isMobileView() ? 'topright' : 'bottomleft').addTo(map);
        if (!setMapWithInfoFromUrl())
            if (mapSettings.initialVariables == null)
                map.setView([50.07905886, 14.43715096], 14);
            else
                map.setView([mapSettings.initialVariables.lat, mapSettings.initialVariables.lng], isMobileView() ? mapSettings.initialVariables.zoomMobile : mapSettings.initialVariables.zoom);
        map.on("moveend", function () {
            document.getElementById("map").style.cursor = 'default';
            setUrlByMapInfo();
            callBlazor_RefreshObjectsOnMap();
        });
        map.addEventListener('mousemove', function (ev) {
            lat = ev.latlng.lat;
            lng = ev.latlng.lng;
            coorx = ev.containerPoint.x;
            coory = ev.containerPoint.y;
        });
        for (let i = 0; i < mapSettings.layers.length; i++) {
            const group = new L.FeatureGroup(null, { id: mapSettings.layers[i].name + "_group" });
            group.setZIndex(mapSettings.layers[i].zIndex);
            groups.push(group);
            if (mapSettings.layers[i].selected)
                group.addTo(map);
            const layer = convertMapSettingsObjectToMapLayer(mapSettings.layers[i]);
            if (layer != null)
                group.addLayer(layer);
        }
    }
    mapAPI.initMap = initMap;
    function onResizeWindow() {
        fitMapToWindow();
        resizePhotoHeight();
        blazorMapObject.invokeMethodAsync("SetMobileView", isMobileView());
    }
    mapAPI.onResizeWindow = onResizeWindow;
    function fitMapToWindow(heightOfDialog = null, mapWidth = null) {
        const pageHeight = window.innerHeight;
        const pageWidth = window.innerWidth;
        const tempHeight = heightOfDialog != null && isMobileView()
            ? pageHeight * (heightOfDialog / 100)
            : pageHeight - (document.getElementById("controlButtonsWrapper") != null
                ? document.getElementById("controlButtonsWrapper").offsetTop
                : 0);
        const mapElement = document.getElementById("map");
        const pageElement = document.getElementsByClassName("page");
        if (mapElement == null || !mapElement)
            return;
        mapElement.style.width = mapWidth == null ? "100%" : mapWidth + "px";
        const mapHeight = !mapAPI.isMobileView()
            ? pageHeight
            : pageHeight - 44 - tempHeight;
        mapElement.style.height = mapHeight + "px";
        if (mapAPI.isMobileView())
            mapElement.style.marginTop = "44px";
        pageElement[0].style.height = !isMobileView() ? pageHeight + "px" : (pageHeight - 44) + "px";
        if (map != null)
            map.invalidateSize();
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
        const bounds = map.getBounds();
        const urlBounds = bounds.getSouthWest().lat + "," + bounds.getSouthWest().lng + "," + bounds.getNorthEast().lat + "," + bounds.getNorthEast().lng;
        setUrlParam('bounds', urlBounds);
        setUrlParam('zoom', map.getZoom());
    }
    mapAPI.setUrlByMapInfo = setUrlByMapInfo;
    function setUrlParam(paramName, paramValue) {
        const urlParams = new URLSearchParams(window.location.search);
        urlParams.set(paramName, paramValue.toString());
        history.replaceState({}, 'Mapa', "?" + urlParams);
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
        map.fitBounds(infoFromUrl.bounds);
        const latAvg = (infoFromUrl.bounds.getSouthWest().lat + infoFromUrl.bounds.getNorthEast().lat) / 2;
        const lngAvg = (infoFromUrl.bounds.getSouthWest().lng + infoFromUrl.bounds.getNorthEast().lng) / 2;
        map.setView([latAvg, lngAvg], Number(infoFromUrl.zoom));
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
            return new L.TileLayer.WMS(wmsProxyUrl, {
                tileSize: 512,
                layers: mapSettingsObject.layersParameter,
                className: "customWmsLayer"
            });
        }
        return null;
    }
    mapAPI.convertMapSettingsObjectToMapLayer = convertMapSettingsObjectToMapLayer;
    function initBlazorMapObject(dotNetObject) {
        blazorMapObject = dotNetObject;
    }
    mapAPI.initBlazorMapObject = initBlazorMapObject;
    function callBlazor_RefreshObjectsOnMap() {
        const polygonsGroup = groups.find(a => a.options.id == "Polygons_group");
        const mapHasPolygonsYet = polygonsGroup.getLayers().length > 0;
        blazorMapObject.invokeMethodAsync("RefreshObjectsOnMap", !mapHasPolygonsYet, map.getZoom(), getMapBoundsForMapState());
    }
    mapAPI.callBlazor_RefreshObjectsOnMap = callBlazor_RefreshObjectsOnMap;
    function callBlazor_ShowPlaceInfo(event) {
        removeAdditionalObjects();
        unselectAllSelectedPoints();
        if (event.target._latlng == undefined)
            event.target.setStyle({ fillColor: polygonColorSelected });
        else {
            event.target._icon.className = event.target._icon.className.replace('map-point-selected', 'map-point').replace('map-point', 'map-point-selected');
            event.target._icon.style.zIndex = '200';
        }
        const point = event.target._latlng != undefined ? event.target._latlng : [lat, lng];
        const bbox = convertObjectPositionToBBoxParameter(point);
        blazorMapObject.invokeMethodAsync("ShowPlaceInfo", map.getZoom(), bbox, coorx);
    }
    mapAPI.callBlazor_ShowPlaceInfo = callBlazor_ShowPlaceInfo;
    function refreshObjectsOnMap(objectJson) {
        const objectsGroup = groups.find(a => a.options.id == "Objects_group");
        const polygonsGroup = groups.find(a => a.options.id == "Polygons_group");
        objectsGroup.clearLayers();
        const objects = JSON.parse(objectJson);
        for (let i = 0; i < objects.length; i++) {
            let newObject;
            if (objects[i].mapPolygon != null) {
                const polygonObject = JSON.parse(objects[i].mapPolygon);
                newObject = getPolygon(polygonObject, objects[i].clickable, objects[i].label, null, objects[i].customTooltipClass, objects[i].customPolygonClass);
                if (objects[i].placeType == "Inaccessible")
                    newObject.addTo(polygonsGroup);
                else
                    newObject.addTo(objectsGroup);
            }
            else {
                const markerObject = JSON.parse(objects[i].mapPoint);
                newObject = getPoint(markerObject, objects[i].clickable, objects[i].label, objects[i].htmlIcon);
                newObject.options.guid = objects[i].guid;
                newObject.addTo(objectsGroup);
            }
        }
        polygonsGroup.bringToFront();
    }
    mapAPI.refreshObjectsOnMap = refreshObjectsOnMap;
    function addObjectFromJsonString(jsonInfo) {
        const parsedObject = JSON.parse(jsonInfo);
        const newObject = parsedObject.type == "Point" ? getPoint(parsedObject) : getPolygon(parsedObject, null, polygonColor);
        const objectsGroup = groups.find(a => a.options.id == "AdditionalObjects_group");
        objectsGroup.clearLayers();
        newObject.addTo(objectsGroup);
    }
    mapAPI.addObjectFromJsonString = addObjectFromJsonString;
    function getPoint(markerObject, clickable, label, htmlIcon) {
        let iconOptions = null;
        if (htmlIcon != undefined && htmlIcon != null)
            iconOptions = { icon: new L.DivIcon({ className: "map-point", html: htmlIcon }) };
        const result = new L.Marker([markerObject.coordinates[1], markerObject.coordinates[0]], iconOptions);
        var pos = map.latLngToLayerPoint(result.getLatLng()).round();
        result.setZIndexOffset(100 - pos.y);
        if (!isMobileView() && label != undefined && label != null) {
            result.bindTooltip(label, { sticky: true });
        }
        if (clickable) {
            result.on('click', callBlazor_ShowPlaceInfo);
        }
        return result;
    }
    mapAPI.getPoint = getPoint;
    function getPolygon(polygonObject, clickable, label, color, customTooltipClass, customPolygonClass) {
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
            ? { className: customPolygonClass, color: null, weight: null, fillOpacity: null, opacity: null, fillColor: null }
            : { fillColor: polygonColor, color: polygonStrokeColor, weight: 0.5, fillOpacity: 0.40, opacity: 1 };
        const result = new L.Polygon(pointsArray, polygonOptions);
        if (clickable) {
            result.on('click', callBlazor_ShowPlaceInfo);
        }
        if (label != undefined && label != null) {
            result.bindTooltip(label, { sticky: true, className: customTooltipClass != undefined && customTooltipClass != null ? customTooltipClass : null });
        }
        return result;
    }
    mapAPI.getPolygon = getPolygon;
    function removeAdditionalObjects() {
        const objectsGroup = groups.find(a => a.options.id == "AdditionalObjects_group");
        objectsGroup.eachLayer(function (item) {
            if (item.options.type == undefined || item.options.type !== "bluepoint") {
                item.remove();
            }
        });
        const polygonsGroup = groups.find(a => a.options.id == "Polygons_group");
        polygonsGroup.eachLayer(function (item) {
            if (item.options.fillColor != undefined || item.options.fillColor == polygonColorSelected) {
                item.setStyle({ fillColor: polygonColor });
            }
        });
    }
    mapAPI.removeAdditionalObjects = removeAdditionalObjects;
    function unselectAllSelectedPoints() {
        var selectedPoints = document.getElementsByClassName('map-point-selected');
        Array.from(selectedPoints).forEach(function (item) {
            item.className = item.className.replace('map-point-selected', 'map-point');
            if (item !== undefined)
                item.style.zIndex = '100';
        });
    }
    mapAPI.unselectAllSelectedPoints = unselectAllSelectedPoints;
    function selectPointOnMap(guidArrayJson) {
        const guidArray = JSON.parse(guidArrayJson);
        const objectsGroup = groups.find(a => a.options.id == "Objects_group");
        objectsGroup.eachLayer(function (item) {
            if (item.options.guid !== undefined && guidArray.indexOf(item.options.guid) > -1 && !item._icon.className.includes('map-point-selected')) {
                item._icon.className = item._icon.className.replace('map-point-selected', 'map-point').replace('map-point', 'map-point-selected');
                item._icon.style.zIndex = '200';
            }
        });
    }
    mapAPI.selectPointOnMap = selectPointOnMap;
    function toggleLayerGroup(name, selected) {
        const groupName = name + "_group";
        if (!selected) {
            map.eachLayer(function (layer) { if (layer.options.id == groupName)
                layer.remove(); });
        }
        else {
            const groupToAdd = groups.find(a => a.options.id == groupName);
            groupToAdd.addTo(map);
        }
    }
    mapAPI.toggleLayerGroup = toggleLayerGroup;
    function convertObjectPositionToBBoxParameter(point) {
        const containerPosition = map.latLngToContainerPoint(point);
        const sizeOfBox = 5;
        const minx = containerPosition.x < sizeOfBox / 2 ? 0 : containerPosition.x - sizeOfBox / 2;
        const miny = containerPosition.y < sizeOfBox / 2 ? 0 : containerPosition.y + sizeOfBox / 2;
        const maxx = containerPosition.x < sizeOfBox / 2 ? 0 : containerPosition.x + sizeOfBox / 2;
        const maxy = containerPosition.y < sizeOfBox / 2 ? 0 : containerPosition.y - sizeOfBox / 2;
        const southWest = map.containerPointToLatLng([minx, maxy]);
        const northEast = map.containerPointToLatLng([maxx, miny]);
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
        const bounds = map.getBounds();
        return [{ X: bounds.getSouthWest().lng, Y: bounds.getSouthWest().lat }, { X: bounds.getNorthEast().lng, Y: bounds.getNorthEast().lat }];
    }
    mapAPI.getMapBoundsForMapState = getMapBoundsForMapState;
    function getWindowLocationSearch() {
        return window.location.search;
    }
    mapAPI.getWindowLocationSearch = getWindowLocationSearch;
    function isMobileView() {
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
    mapAPI.isMobileView = isMobileView;
    function getZoom() {
        return map.getZoom();
    }
    mapAPI.getZoom = getZoom;
    function zoomIn() {
        map.zoomIn();
    }
    mapAPI.zoomIn = zoomIn;
    function zoomOut() {
        map.zoomOut();
    }
    mapAPI.zoomOut = zoomOut;
    function turnOnLocationTracking() {
        if (!navigator.geolocation) {
            console.log("Your browser doesn't support geolocation feature!");
        }
        else {
            navigator.geolocation.getCurrentPosition(showMyLocation);
            trackingInterval = setInterval(() => {
                navigator.geolocation.getCurrentPosition(showMyLocation);
            }, 5000);
        }
    }
    mapAPI.turnOnLocationTracking = turnOnLocationTracking;
    function goToLocation(pointString, zoom) {
        const point = JSON.parse(pointString).coordinates;
        map.flyTo([point[1], point[0]], zoom);
    }
    mapAPI.goToLocation = goToLocation;
    function goToMyLocation() {
        const lat = actualLocation.coords.latitude;
        const long = actualLocation.coords.longitude;
        map.setView([lat, long], 15);
    }
    mapAPI.goToMyLocation = goToMyLocation;
    function showMyLocation(position) {
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
    mapAPI.showMyLocation = showMyLocation;
    function hideMyLocation() {
        clearInterval(trackingInterval);
        applicationIsTrackingLocation = false;
        removeBluepoint();
    }
    mapAPI.hideMyLocation = hideMyLocation;
    function addBluepoint(point, icon) {
        const objectsGroup = groups.find(a => a.options.id == "AdditionalObjects_group");
        new L.Marker(point, { icon: icon, type: "bluepoint" }).addTo(objectsGroup);
    }
    mapAPI.addBluepoint = addBluepoint;
    function removeBluepoint() {
        const objectsGroup = groups.find(a => a.options.id == "AdditionalObjects_group");
        objectsGroup.eachLayer(function (item) {
            if (item.options.type !== undefined && item.options.type == "bluepoint") {
                item.remove();
            }
        });
    }
    mapAPI.removeBluepoint = removeBluepoint;
    function setDialogFullScreen(value) {
        if (value) {
            if (!isFullscreen) {
                dialogWidth = document.querySelector("aside").style.width;
                dialogHeight = document.querySelector("aside").style.height;
            }
            isFullscreen = true;
            document.querySelector("aside").style.width = "100%";
            document.querySelector("aside").style.height = "100%";
            fitMapToWindow(window.innerHeight - 44);
        }
        else if (!value && isFullscreen && dialogHeight != null && dialogWidth != null) {
            isFullscreen = false;
            document.querySelector("aside").style.width = dialogWidth;
            document.querySelector("aside").style.height = dialogHeight;
            fitMapToWindow(dialogHeight);
        }
    }
    mapAPI.setDialogFullScreen = setDialogFullScreen;
    function getBlazorCulture() {
        return window.localStorage['BlazorCulture'];
    }
    mapAPI.getBlazorCulture = getBlazorCulture;
    ;
    function setBlazorCulture(value) {
        window.localStorage['BlazorCulture'] = value;
    }
    mapAPI.setBlazorCulture = setBlazorCulture;
    function resizePhotoHeight() {
        const ratio = 0.875;
        if (document.getElementsByClassName("victim-photo") == null || document.getElementsByClassName("victim-photo").length == 0)
            return;
        const photoWidth = document.getElementsByClassName("victim-photo")[0].clientWidth;
        const photoHeight = photoWidth / ratio;
        document.querySelectorAll(".victim-photo").forEach(function (item) {
            item.style.height = photoHeight + "px";
        });
    }
    mapAPI.resizePhotoHeight = resizePhotoHeight;
})(mapAPI || (mapAPI = {}));
window.addEventListener("resize", mapAPI.onResizeWindow);
//# sourceMappingURL=site.js.map