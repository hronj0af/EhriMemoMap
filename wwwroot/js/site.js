// FUNKCE PRO OVLÁDÁNÍ MAPY

var mapAPI = {

    map: null,
    lat: null,
    lng: null,
    coorx: null,
    coory: null,
    polygons: [],
    bluepointIcon: null,
    trackingInterval: null,
    groups: [],
    addressIcon: null,
    incidentIcon: null,
    interestIcon: null,
    blazorMapObject: null,

    //////////////////////////
    /// INIT
    //////////////////////////


    // připraví mapu do úvodního stavu
    initMap: function (jsonMapSettings) {

        this.fitMapToWindow();

        this.incidentIcon = L.divIcon({ className: 'leaflet-incident-icon' });
        this.addressIcon = L.divIcon();
        this.interestIcon = L.divIcon({ className: 'leaflet-interest-icon' });

        this.bluepointIcon = L.icon({
            iconUrl: 'images/blue-point.png',
            iconSize: [20, 20], // size of the icon
            iconAnchor: [10, 10], // point of the icon which will correspond to marker's location
        });

        this.map = L.map('map', { zoomControl: false });

        if (!this.setMapWithInfoFromUrl())
            this.map.setView([50.07905886, 14.43715096], 14);

        // update url a obrázkových vrstev po té, co se změní poloha mapy
        this.map.on("moveend", function (e) {
            document.getElementById("map").style.cursor = 'default';
            mapAPI.setUrlByMapInfo();
            mapAPI.callBlazor_RefreshObjectsOnMap();
        });

        // při změně polohy myši se zapíšou její souřadnice do proměnných
        this.map.addEventListener('mousemove', function (ev) {
            mapAPI.lat = ev.latlng.lat;
            mapAPI.lng = ev.latlng.lng;
            mapAPI.coorx = ev.containerPoint.x;
            mapAPI.coory = ev.containerPoint.y;
        });

        var mapSettings = JSON.parse(jsonMapSettings);

        // konverze json objektu do jednotlivých vrstev mapy a jejich přidání k mapě
        for (var i = 0; i < mapSettings.length; i++) {
            var group = L.layerGroup(layer, { id: mapSettings[i].name + "_group" });
            group.setZIndex(mapSettings[i].zIndex);
            this.groups.push(group);

            if (mapSettings[i].selected)
                group.addTo(this.map);

            var layer = this.convertMapSettingsObjectToMapLayer(mapSettings[i]);
            if (layer != null)
                group.addLayer(layer);
        }
    },

    // nastaví mapu, aby lícovala s oknem
    fitMapToWindow: function () {

        var mapElement = document.getElementById("map");

        if (mapElement == null || mapElement.length == 0)
            return;

        var mapHeight = window.innerHeight - mapElement.offsetTop - 1;
        mapElement.style.height = mapHeight + "px";
    },

    //////////////////////////
    /// URL
    //////////////////////////


    // získá informace z URL pro nastavení polohy mapy
    getMapInfoFromUrl: function () {
        var queryString = window.location.search;
        const urlParams = new URLSearchParams(queryString);

        var urlBounds = urlParams.get("bounds");

        if (urlBounds == null)
            return null;

        var bounds = urlBounds.split(",");

        var southWest = L.latLng(bounds[0], bounds[1]),
            northEast = L.latLng(bounds[2], bounds[3]);

        var zoom = urlParams.get("zoom");

        return {
            bounds: L.latLngBounds(southWest, northEast),
            zoom: zoom ?? "13"
        };
    },

    // nastaví URL parametry pro pozdější nastavení mapy
    setUrlByMapInfo: function () {
        var bounds = this.map.getBounds();

        var urlBounds = bounds._southWest.lat + "," + bounds._southWest.lng + "," + bounds._northEast.lat + "," + bounds._northEast.lng;

        this.setUrlParam('bounds', urlBounds);
        this.setUrlParam('zoom', this.map.getZoom());
    },

    // nastaví jednotlivý parametr url
    setUrlParam: function (paramName, paramValue) {
        const urlParams = new URLSearchParams(window.location.search);
        urlParams.set(paramName, paramValue);
        history.pushState({ pageID: '100' }, 'Mapa', "?" + urlParams);
    },

    // získá parametr z url
    getUrlParam: function (paramName) {
        const urlParams = new URLSearchParams(window.location.search);
        return urlParams.get(paramName);
    },

    // nastaví mapu podle parametrů url
    setMapWithInfoFromUrl: function () {
        var infoFromUrl = this.getMapInfoFromUrl();
        if (infoFromUrl == null)
            return false;
        this.map.fitBounds(infoFromUrl.bounds);

        var latAvg = (infoFromUrl.bounds._southWest.lat + infoFromUrl.bounds._northEast.lat) / 2;
        var lngAvg = (infoFromUrl.bounds._southWest.lng + infoFromUrl.bounds._northEast.lng) / 2;

        this.map.setView([latAvg, lngAvg], infoFromUrl.zoom);

        return true;
    },


    // konvertuje objekt s nastavením mapových vrstev do mapových vrstev leafletu
    convertMapSettingsObjectToMapLayer: function (mapSettingsObject) {
        if (mapSettingsObject.type == 'Base') {
            return L.tileLayer(mapSettingsObject.url, {
                maxZoom: 18,
                attribution: mapSettingsObject.attribution,
            });
        }
        else if (mapSettingsObject.type == 'WMS') {
            return L.tileLayer.wms(mapSettingsObject.url, {
                tileSize: 512,
                map: mapSettingsObject.mapParameter,
                layers: mapSettingsObject.layersParameter
            });
        }
        return null;
    },

    //////////////////////////
    /// BLAZOR
    //////////////////////////

    // připraví instanci blazor třídy Map.razor, abych ji pak mohl později odsud volat
    initBlazorMapObject: function (dotNetObject) {
        this.blazorMapObject = dotNetObject;
    },

    callBlazor_RefreshObjectsOnMap: function () {
        var polygonsGroup = this.groups.find(a => a.options.id == "Polygons_group");
        var mapHasPolygonsYet = polygonsGroup.getLayers().length > 0;
        mapAPI.blazorMapObject.invokeMethodAsync("RefreshObjectsOnMap", !mapHasPolygonsYet, mapAPI.map.getZoom(), mapAPI.getMapBoundsForMapState());
    },

    callBlazor_ShowPlaceInfo: function () {
        var bbox = mapAPI.convertMousePositionToBBoxParameter();
        mapAPI.blazorMapObject.invokeMethodAsync("ShowPlaceInfo", mapAPI.map.getZoom(), bbox);
    },

    //////////////////////////
    /// OBJECTS
    //////////////////////////

    // refreshují se pouze špendlíky, polygony se přidají na začátku a pak už se s nimi nic nedělá
    // (protože jich není moc, tak se můžou zobrazovat pořád)
    refreshObjectsOnMap: function (objectJson) {
        var objectsGroup = this.groups.find(a => a.options.id == "Objects_group");
        var polygonsGroup = this.groups.find(a => a.options.id == "Polygons_group");
        objectsGroup.clearLayers();
        var objects = JSON.parse(objectJson);

        for (var i = 0; i < objects.length; i++) {
            var newObject;

            if (objects[i].mapPolygon != null) {
                var polygonObject = JSON.parse(objects[i].mapPolygon);
                newObject = this.getPolygon(polygonObject, objects[i].label);
                newObject.addTo(polygonsGroup);
            } else {
                var markerObject = JSON.parse(objects[i].mapPoint);
                newObject = this.getPoint(markerObject, objects[i].clickable, objects[i].label, objects[i].htmlIcon);
                newObject.addTo(objectsGroup);
            }
        }
    },

    addObjectFromJsonString: function (jsonInfo) {
        var parsedObject = JSON.parse(jsonInfo);
        var newObject = parsedObject.type == "Point" ? this.getPoint(parsedObject) : this.getPolygon(parsedObject, null, "#e500ff");
        if (parsedObject.type == "MultiPolygon")
            newObject.options.zIndex = 99999;
        var objectsGroup = this.groups.find(a => a.options.id == "AdditionalObjects_group");
        objectsGroup.clearLayers();
        newObject.addTo(objectsGroup);
    },

    getPoint: function (markerObject, clickable, label, htmlIcon) {
        var iconOptions = null;
        if (htmlIcon != undefined && htmlIcon != null)
            iconOptions = { icon: L.divIcon({ className: "", html: htmlIcon }) };

        var result = L.marker([markerObject.coordinates[1], markerObject.coordinates[0]], iconOptions);

        if (!mapAPI.isMobileBrowser() && label != undefined && label != null) {
            result.bindTooltip(label, { sticky: true });
        }

        if (clickable) {
            result.on('click', this.callBlazor_ShowPlaceInfo);
        }

        return result;
    },

    getPolygon: function (polygonObject, label, color) {

        var pointsArray = [];
        for (var j = 0; j < polygonObject.coordinates.length; j++) {
            for (var m = 0; m < polygonObject.coordinates[j].length; m++) {
                var polygon = polygonObject.coordinates[j][m];
                pointsArray.push([]);
                var innerArray = [];
                for (var k = 0; k < polygon.length; k++) {
                    innerArray.push([polygon[k][1], polygon[k][0]]);
                }
                pointsArray[j].push(innerArray);
            }
        }
        result = L.polygon(pointsArray, { fillColor: color != undefined && color != null ? color : '#E47867', color: '#222', weight: 0.5, fillOpacity: 0.8, opacity: 1 })
            .on('click', this.callBlazor_ShowPlaceInfo);


        if (!mapAPI.isMobileBrowser() && label != undefined && label != null) {
            result.bindTooltip(label, { sticky: true });
        }

        return result;


    },


    removeAdditionalObjects: function () {
        var objectsGroup = this.groups.find(a => a.options.id == "AdditionalObjects_group");
        objectsGroup.eachLayer(function (item) {
            if (item.options.type == undefined || item.options.type !== "bluepoint") {
                item.remove();
            }
    	});
    },

    //////////////////////////
    /// HELPER METHODS
    //////////////////////////

    toggleLayerGroup: function (name, selected) {
        var groupName = name + "_group";
        if (!selected) {
            this.map.eachLayer(function (layer) { if (layer.options.id == groupName) layer.remove(); })
        } else {
            var groupToAdd = this.groups.find(a => a.options.id == groupName);
            groupToAdd.addTo(this.map);
        }
    },

    // převede pozici myši nad mapou do parametru bbox pro získání informací o daném místě (dotaz na QGIS server)
    convertMousePositionToBBoxParameter: function () {

        var sizeOfBox = 5;

        var minx = this.coorx < sizeOfBox / 2 ? 0 : this.coorx - sizeOfBox / 2;
        var miny = this.coory < sizeOfBox / 2 ? 0 : this.coory + sizeOfBox / 2;
        var maxx = this.coorx < sizeOfBox / 2 ? 0 : this.coorx + sizeOfBox / 2;
        var maxy = this.coory < sizeOfBox / 2 ? 0 : this.coory - sizeOfBox / 2;

        var southWest = this.map.containerPointToLatLng([minx, maxy]);
        var northEast = this.map.containerPointToLatLng([maxx, miny]);

        return [{ X: southWest.lng, Y: southWest.lat }, { X: northEast.lng, Y: northEast.lat }];
    },

    getWindowWidth: function () {
        return window.innerWidth;
    },

    getWindowHeight: function () {
        return window.innerHeight;
    },

    getMapBoundsForMapState: function () {
        var bounds = mapAPI.map.getBounds();
        return [{ X: bounds._southWest.lng, Y: bounds._southWest.lat }, { X: bounds._northEast.lng, Y: bounds._northEast.lat }];
    },

    getWindowLocationSearch: function () {
        return window.location.search;
    },

    _isMobileBrowser: null,

    isMobileBrowser: function () {
        if (mapAPI._isMobileBrowser != null)
            return mapAPI._isMobileBrowser;

        if (navigator.userAgent.match(/Android/i)
            || navigator.userAgent.match(/webOS/i)
            || navigator.userAgent.match(/iPhone/i)
            || navigator.userAgent.match(/iPad/i)
            || navigator.userAgent.match(/iPod/i)
            || navigator.userAgent.match(/BlackBerry/i)
            || navigator.userAgent.match(/Windows Phone/i))
            mapAPI._isMobileBrowser = true;
        else
            mapAPI._isMobileBrowser = false;

        return mapAPI._isMobileBrowser;
    },

    //////////////////////////
    /// TRACKING
    //////////////////////////

    applicationIsTrackingLocation: false,
    actualLocation: null,

    turnOnLocationTracking: function () {
        if (!navigator.geolocation) {
            console.log("Your browser doesn't support geolocation feature!")
        } else {
            navigator.geolocation.getCurrentPosition(mapAPI.showMyLocation)
            this.trackingInterval = setInterval(() => {
                navigator.geolocation.getCurrentPosition(mapAPI.showMyLocation)
            }, 5000);
        };

    },

    goToLocation: function (pointString, zoom) {
        var point = JSON.parse(pointString).coordinates;
        this.map.setView([point[1], point[0]], zoom);
    },

    goToMyLocation: function () {
        lat = mapAPI.actualLocation.coords.latitude;
        long = mapAPI.actualLocation.coords.longitude;
        mapAPI.map.setView([lat, long], 15);
    },

    showMyLocation: function (position) {
        mapAPI.actualLocation = position;
        lat = position.coords.latitude;
        long = position.coords.longitude;

        mapAPI.removeBluepoint();
        mapAPI.addBluepoint([lat, long], mapAPI.bluepointIcon);

        if (!mapAPI.applicationIsTrackingLocation) {
            mapAPI.goToMyLocation();
            mapAPI.applicationIsTrackingLocation = true;
        }
    },

    hideMyLocation: function () {
        clearInterval(this.trackingInterval);
        mapAPI.applicationIsTrackingLocation = false;
        mapAPI.removeBluepoint();
    },

    addBluepoint: function (point, icon) {
        var objectsGroup = this.groups.find(a => a.options.id == "AdditionalObjects_group");
        L.marker(point, { icon: icon, type: "bluepoint" }).addTo(objectsGroup);
    },
    removeBluepoint: function () {
        var objectsGroup = this.groups.find(a => a.options.id == "AdditionalObjects_group");
        objectsGroup.eachLayer(function (item) {
            if (item.options.type !== undefined && item.options.type == "bluepoint") {
                item.remove();
            }
        });
    },

    //////////////////////////
    /// DIALOG
    //////////////////////////

    dialogWidth: null,
    dialogHeight: null,
    isFullscreen: false,

    fullscreenDialog: function (value) {
        if (value) {
            if (!this.isFullscreen) {
                this.dialogWidth = document.querySelector("aside").style.width;
                this.dialogHeight = document.querySelector("aside").style.height;
            }
            this.isFullscreen = true;
            document.querySelector("aside").style.width = "100%";
            document.querySelector("aside").style.height = "100%";
        } else if (!value && this.isFullscreen && this.dialogHeight != null && this.dialogWidth != null) {
            this.isFullscreen = false;
            document.querySelector("aside").style.width = this.dialogWidth;
            document.querySelector("aside").style.height = this.dialogHeight;

        }


    }
}

window.addEventListener("resize", mapAPI.fitMapToWindow);
