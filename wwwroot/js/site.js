
var mapAPI = {

    map: null, lat: null, lng: null, coorx: null, coory: null, polygons: [], isDragging: 0,

    // nastaví mapu, aby lícovala s oknem
    fitMapToWindow: function () {

        var mapElement = document.getElementById("map");

        if (mapElement == null || mapElement.length == 0)
            return;

        var mapHeight = window.innerHeight - mapElement.offsetTop - 1;
        mapElement.style.height = mapHeight + "px";
    },

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

    // převede pozici myši nad mapou do parametru bbox pro získání informací o daném místě (dotaz na QGIS server)
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

    // přidá k vrstvám mapy vrstvu v parametru a tutéž vrstvu také předtím z mapy odejme
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

    // updatuje obrázkovou vrstvu mapy (nastaví nové url a souřadnice pro aktuální zobrazení mapy)
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

    // získá url string s informacemi o výšce a šířce obrázku + souřadnice obrázku (vlevo nahoře + vpravo dole)
    getImageLayerLimitsUrlParameters: function () {

        var bounds = this.map.getBounds();
        var lefttop = this.map.project([bounds.getSouth(), bounds.getWest()], 0);
        var rightbottom = this.map.project([bounds.getNorth(), bounds.getEast()], 0);

        return "&BBOX=" + lefttop.x + "," + lefttop.y + "," + rightbottom.x + "," + rightbottom.y +
            "&HEIGHT=" + this.map.getSize().y +
            "&WIDTH=" + this.map.getSize().x;
    },

    // připraví mapu do úvodního stavu
    initMap: function (jsonMapSettings) {

        this.fitMapToWindow();



        this.map = L.map('map', { zoomControl: false });

        if (!this.setMapWithInfoFromUrl())
            this.map.setView([50.07905886, 14.43715096], 14);

        // update url a obrázkových vrstev po té, co se změní poloha mapy
        this.map.on("moveend", function (e) {
            document.getElementById("map").style.cursor = 'default';
            mapAPI.setUrlByMapInfo();
            mapAPI.updateImageLayers();

            setTimeout(() => {
                mapAPI.setIsDragging(false);
            }, 500);

        });

        // tohle je tu kvůli tomu, aby nevznikl dojem, že uživatel klikl myší, když jenom přesunul mapu o kousek vedle
        this.map.on("movestart", function (e) {
            document.getElementById("map").style.cursor = 'grab';
            mapAPI.setIsDragging(true);
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
            var layer = this.convertMapSettingsObjectToMapLayer(mapSettings[i]);
            var group = L.layerGroup(layer, { id: mapSettings[i].name + "_group" });
            group.addLayer(layer);
            group.setZIndex(mapSettings[i].zIndex);
            group.addTo(this.map);
        }
    },

    // konvertuje objekt s nastavením mapových vrstev do mapových vrstev leafletu
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
            var url = mapSettingsObject.baseUrl +
                "service=WMS&request=GetMap&version=1.3.0&srs=EPSG:3857&crs=EPSG:3857&map=" +
                mapSettingsObject.mapParameter +
                "&opacities=" + mapSettingsObject.opacities +
                "&format=image/png" +
                "&transparent=true" +
                "&layers=" + mapSettingsObject.layers.map(a => a.name).join();
            //console.log(url);
            return L.imageOverlay(url + this.getImageLayerLimitsUrlParameters(), this.map.getBounds(), { id: mapSettingsObject.name, type: "image", url: url });
        }

    },

    // konverze souřadnic z EPSG 4236 do EPSG 3857
    convertPoint: function (originalPoint) {
        return this.map.unproject(originalPoint, 0);
    },

    // přidá objekty (polygony atd.) na mapu podle jsonu
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

            //console.log("Your coordinate is: Lat: " + lat + " Long: " + long + " Accuracy: " + accuracy)
        }
    },

    getIsDragging: function () {
        return this.isDragging;
    },

    setIsDragging: function (value) {
        if (value)
            this.isDragging++;
        else if (this.isDragging > 0)
            this.isDragging--;
    }
}

window.addEventListener("resize", mapAPI.fitMapToWindow);
