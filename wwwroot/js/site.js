﻿
var mapAPI = {

    map: null, lat: null, lng: null, coorx: null, coory: null, polygons: [], isDragging: false,
    fitMapToWindow: function () {

        var mapElement = document.getElementById("map");

        if (mapElement.length == 0)
            return;

        var mapHeight = window.innerHeight - mapElement.offsetTop - 1;
        mapElement.style.height = mapHeight + "px";
    },

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

    setUrlByMapInfo: function () {
        var bounds = this.map.getBounds();

        var urlBounds = bounds._southWest.lat + "," + bounds._southWest.lng + "," + bounds._northEast.lat + "," + bounds._northEast.lng;

        this.setUrlParam('bounds', urlBounds);
        this.setUrlParam('zoom', this.map.getZoom());
    },

    setUrlParam: function (paramName, paramValue) {
        const urlParams = new URLSearchParams(window.location.search);
        urlParams.set(paramName, paramValue);
        history.pushState({ pageID: '100' }, 'Mapa', "?" + urlParams);
    },

    getUrlParam: function (paramName) {
        const urlParams = new URLSearchParams(window.location.search);
        return urlParams.get(paramName);
    },

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
    convertMousePositionToBBoxParameter: function () {

        var sizeOfBox = 5 * this.map.getZoom();

        var minx = this.coorx < sizeOfBox / 2 ? 0 : this.coorx - sizeOfBox / 2;
        var miny = this.coory < sizeOfBox / 2 ? 0 : this.coory + sizeOfBox / 2;
        var maxx = this.coorx < sizeOfBox / 2 ? 0 : this.coorx + sizeOfBox / 2;
        var maxy = this.coory < sizeOfBox / 2 ? 0 : this.coory - sizeOfBox / 2;

        var lefttop = this.map.project(this.map.containerPointToLatLng([minx, miny]), 0);
        var rightbottom = this.map.project(this.map.containerPointToLatLng([maxx, maxy]), 0);

        return "&bbox=" + lefttop.x + "," + lefttop.y + "," + rightbottom.x + "," + rightbottom.y;
    },

    changeLayer: function (jsonMapSettings) {
        var mapSettings = JSON.parse(jsonMapSettings);
        var oldLayer, groupToChange;
        Object.values(mapAPI.map._layers).forEach(layer => {
            if (layer.options.id == mapSettings.name) {
                oldLayer = layer;
            }
            if (layer.options.id == mapSettings.name + "_group") {
                groupToChange = layer;
            }
        });
        var newLayer = this.convertMapSettingsObjectToMapLayer(mapSettings);
        groupToChange.clearLayers();
        groupToChange.addLayer(newLayer);
        groupToChange.setZIndex(mapSettings.zIndex);
    },

    updateImageLayers: function () {
        Object.values(mapAPI.map._layers).forEach(layer => {
            if (layer.options.type != undefined && layer.options.type == "image") {
                layer.setUrl(layer.options.url + this.getImageLayerLimitsUrlParameters());
                layer.on("load", function () {
                    layer.setBounds(mapAPI.map.getBounds());
                });
            }
        });
    },

    getImageLayerLimitsUrlParameters: function () {

        //var bounds = this.map.getBounds();
        //var lefttop = this.map.project(this.map.containerPointToLatLng([bounds.getSouth(), bounds.getEast()]), 0);
        //var rightbottom = this.map.project(this.map.containerPointToLatLng([bounds.getNorth(), bounds.getWest()]), 0);

        //return "&BBOX=" + lefttop.x + "," + lefttop.y + "," + rightbottom.x + "," + rightbottom.y +
        //    "&HEIGHT=" + window.innerHeight +
        //    "&WIDTH=" + window.outerWidth;

        var bounds = this.map.getBounds();

        return "&bbox=" + bounds.getSouth() + "," + bounds.getWest() + "," + bounds.getNorth() + "," + bounds.getEast() +
            "&height=" + (this.map.getSize().y) +
            "&width=" + this.map.getSize().x;

    },

    initMap: function (jsonMapSettings) {

        this.fitMapToWindow();

        this.map = L.map('map', { zoomControl: false });

        if (!this.setMapWithInfoFromUrl())
            this.map.setView([50.07905886, 14.43715096], 14);

        this.map.on("moveend", function (e) {
            mapAPI.setUrlByMapInfo();
            mapAPI.updateImageLayers();
            setTimeout(() => {
                mapAPI.isDragging = false;
            }, 100);

        });

        this.map.on("movestart", function (e) {
            mapAPI.isDragging = true;
        });

        this.map.addEventListener('mousemove', function (ev) {
            mapAPI.lat = ev.latlng.lat;
            mapAPI.lng = ev.latlng.lng;
            mapAPI.coorx = ev.containerPoint.x;
            mapAPI.coory = ev.containerPoint.y;
        });

        var mapSettings = JSON.parse(jsonMapSettings);

        for (var i = 0; i < mapSettings.length; i++) {
            var layer = this.convertMapSettingsObjectToMapLayer(mapSettings[i]);
            var group = L.layerGroup(layer, { id: mapSettings[i].name + "_group" });
            group.addLayer(layer);
            group.setZIndex(mapSettings[i].zIndex);
            group.addTo(this.map);
        }
    },

    convertMapSettingsObjectToMapLayer: function (mapSettingsObject) {
        if (mapSettingsObject.type == 'base') {
            return L.tileLayer(mapSettingsObject.baseUrl, {
                id: mapSettingsObject.name,
                maxZoom: 18,
                attribution: mapSettingsObject.attribution,
            });
        }
        else if (mapSettingsObject.type == 'wms') {
            return L.tileLayer.wms(mapSettingsObject.baseUrl, {
                id: mapSettingsObject.name,
                map: mapSettingsObject.mapParameter,
                tileSize: mapSettingsObject.tileSize != null
                    ? parseInt(mapSettingsObject.tileSize)
                    : 512,
                layers: mapSettingsObject.layers.map(a => a.name).join()
            });
        }
        else if (mapSettingsObject.type == 'wms-image') {
            var bounds = this.map.getBounds();
            var bbox = bounds.getSouth() + "," + bounds.getWest() + "," + bounds.getNorth() + "," + bounds.getEast();
            var url = mapSettingsObject.baseUrl +
                "service=WMS&request=GetMap&version=1.3.0&srs=EPSG:4326&crs=EPSG:4326&map=" +
                mapSettingsObject.mapParameter +
                "&opacities=" + mapSettingsObject.opacities +
                "&format=image/png" +
                "&transparent=true" +
                "&layers=" + mapSettingsObject.layers.map(a => a.name).join();
            console.log(url);
            return L.imageOverlay(url + this.getImageLayerLimitsUrlParameters(), this.map.getBounds(), { id: mapSettingsObject.name, type: "image", url: url });
        }

    },

    convertPoint: function (originalPoint) {
        return this.map.unproject(originalPoint, 0);
    },

    addObjectsFromJsonString: function (jsonInfo) {

        jsonInfo = JSON.parse(jsonInfo);

        for (var i = 0; i < jsonInfo.features.length; i++) {
            this.addObject(jsonInfo.features[i].geometry);
        }
        this.showPolygons();

    },

    addOneObjectFromJsonString: function (jsonInfo) {
        this.addObject(JSON.parse(jsonInfo));
        this.showPolygons();

    },

    addObject: function (objectInfo) {

        switch (objectInfo.type) {

            case "MultiPolygon":
                for (var j = 0; j < objectInfo.coordinates.length; j++) {
                    for (var m = 0; m < objectInfo.coordinates[j].length; m++) {
                        var polygon = objectInfo.coordinates[j][m];
                        var pointsArray = [];
                        for (var k = 0; k < polygon.length; k++) {
                            var convertedPoint = this.convertPoint(polygon[k]);
                            pointsArray.push(convertedPoint);
                        }
                        this.addPolygon(pointsArray);
                    }
                }
                break;

            case "Point":
                var convertedPoint = this.convertPoint(objectInfo.coordinates);
                this.addMarker(convertedPoint);
                break;
        }
    },

    addPolygon: function (pointsArray) {
        this.polygons.push(L.polygon(pointsArray, { color: '#9400D3' }));
    },

    addMarker: function (point) {
        this.polygons.push(L.marker(point));
    },

    showPolygons: function () {
        for (var i = 0; i < this.polygons.length; i++) {
            this.polygons[i].addTo(this.map);
        }
    },

    removeObjects: function () {
        for (var i = 0; i < this.polygons.length; i++) {
            this.polygons[i].remove();
        }
        this.polygons = [];
    },

    goToLocation: function (pointString, zoom) {
        var originalPoint = JSON.parse(pointString).coordinates;
        var convertedPoint = this.convertPoint(originalPoint);
        this.map.setView(convertedPoint, zoom);
    },

    getWindowWidth: function () {
        return window.innerWidth;
    },

    getWindowHeight: function () {
        return window.innerHeight;
    },

    getWindowLocationSearch: function () {
        return window.location.search;
    },

    isMobileBrowser: function () {
        if (navigator.userAgent.match(/Android/i)
            || navigator.userAgent.match(/webOS/i)
            || navigator.userAgent.match(/iPhone/i)
            || navigator.userAgent.match(/iPad/i)
            || navigator.userAgent.match(/iPod/i)
            || navigator.userAgent.match(/BlackBerry/i)
            || navigator.userAgent.match(/Windows Phone/i))
            return true;
        return false;
    },

    getMyLocation: function () {
        if (!navigator.geolocation) {
            console.log("Your browser doesn't support geolocation feature!")
        } else {
            navigator.geolocation.getCurrentPosition(getPosition);
        };

        function getPosition(position) {
            lat = position.coords.latitude
            long = position.coords.longitude
            accuracy = position.coords.accuracy

            mapAPI.removeObjects();
            mapAPI.map.setView([lat, long], 15);
            mapAPI.addMarker([lat, long]);
            mapAPI.showPolygons();

            console.log("Your coordinate is: Lat: " + lat + " Long: " + long + " Accuracy: " + accuracy)
        }
    },

    getIsDragging: function () {
        return this.isDragging;
    }
}

