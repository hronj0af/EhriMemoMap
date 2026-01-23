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
    let blazorMapObjects = [];
    let groups = [];
    let trackingInterval = null;
    let _isMobileView = null;
    let mapSettings = null;
    let applicationIsTrackingLocation = null;
    let actualLocation = null;
    let dialogWidth = null;
    let dialogHeight = null;
    let mobileDialogHeight = null;
    let isFullscreen = null;
    let polygonStrokeColor = "#222";
    let polygonColor = "#C5222C";
    let polygonColorSelected = "#000";
    let heatmapLayer = null;
    let heatmapData = null;
    let initialVariables = null;
    function initMap(jsonMapSettings) {
        mapSettings = JSON.parse(jsonMapSettings);
        mobileDialogHeight = mapSettings.initialVariables.heightOfDialog;
        wmsProxyUrl = mapSettings.initialVariables.wmsProxyUrl;
        initialVariables = mapSettings.initialVariables;
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
    mapAPI.initMap = initMap;
    function destroyMap() {
        if (trackingInterval != null) {
            clearInterval(trackingInterval);
            trackingInterval = null;
        }
        if (heatmapLayer != null && map != null) {
            map.removeLayer(heatmapLayer);
            heatmapLayer = null;
        }
        heatmapData = null;
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
        if (map != null) {
            map.off();
            map.remove();
            map = null;
        }
        applicationIsTrackingLocation = null;
        actualLocation = null;
        blazorMapObjects = [];
        initialVariables = null;
    }
    mapAPI.destroyMap = destroyMap;
    function resetMapViewToInitialState() {
        if (initialVariables == null)
            map.setView([50.07905886, 14.43715096], 14);
        else {
            var initialZoom = isMobileView() ? initialVariables.zoomMobile : initialVariables.zoom;
            var initialLat = isMobileView() ? initialVariables.latMobile : initialVariables.lat;
            var initialLng = isMobileView() ? initialVariables.lngMobile : initialVariables.lng;
            map.setView([initialLat, initialLng], initialZoom);
        }
    }
    mapAPI.resetMapViewToInitialState = resetMapViewToInitialState;
    function onResizeWindow() {
        fitMapToWindow();
        resizePhotoHeight();
        if (blazorMapObjects["Map"] != null)
            blazorMapObjects["Map"].invokeMethodAsync("SetMobileView", isMobileView());
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
        mapElement.style.width = mapWidth == null ? "100%" : mapWidth;
        const mapHeight = !mapAPI.isMobileView()
            ? pageHeight - 1
            : pageHeight - 44 - tempHeight;
        mapElement.style.height = mapHeight + "px";
        if (mapAPI.isMobileView())
            mapElement.style.marginTop = "44px";
        pageElement[0].style.height = !isMobileView() ? (pageHeight - 1) + "px" : (pageHeight - 44) + "px";
        if (map != null)
            map.invalidateSize();
    }
    mapAPI.fitMapToWindow = fitMapToWindow;
    function getMapInfoFromUrl() {
        const queryString = window.location.search;
        const urlParams = new URLSearchParams(queryString);
        const urlBounds = urlParams.get("bounds");
        var southWest = null, northEast = null, bounds = null, customCoordinates = null;
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
            zoom: zoom !== null && zoom !== void 0 ? zoom : "13",
            customCoordinates: customCoordinates
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
        history.replaceState({}, 'Mapa', window.location.pathname + "?" + urlParams);
    }
    mapAPI.setUrlParam = setUrlParam;
    function getUrlParam(paramName) {
        const urlParams = new URLSearchParams(window.location.search);
        return urlParams.get(paramName);
    }
    mapAPI.getUrlParam = getUrlParam;
    function setMapWithInfoFromUrl() {
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
    mapAPI.setMapWithInfoFromUrl = setMapWithInfoFromUrl;
    function convertMapSettingsObjectToMapLayer(mapSettingsObject) {
        if (mapSettingsObject.type == LayersTypeEnum.Base) {
            return new L.TileLayer(mapSettingsObject.url, {
                maxZoom: 18,
                attribution: mapSettingsObject.attribution,
            });
        }
        else if (mapSettingsObject.type == LayersTypeEnum.WMS) {
            return new L.TileLayer.WMS(mapSettingsObject.url, {
                tileSize: 512,
                layers: mapSettingsObject.layersParameter,
                className: "customWmsLayer"
            });
        }
        return null;
    }
    mapAPI.convertMapSettingsObjectToMapLayer = convertMapSettingsObjectToMapLayer;
    function initBlazorMapObject(dotNetObject, name) {
        blazorMapObjects[name] = dotNetObject;
    }
    mapAPI.initBlazorMapObject = initBlazorMapObject;
    function callBlazor_RefreshObjectsOnMap() {
        const polygonsGroup = groups.filter(a => a.options.type == "Polygons");
        const mapHasPolygonsYet = polygonsGroup.length > 0;
        blazorMapObjects["Map"].invokeMethodAsync("RefreshObjectsOnMap", !mapHasPolygonsYet, map.getZoom(), getMapBoundsForMapState());
    }
    mapAPI.callBlazor_RefreshObjectsOnMap = callBlazor_RefreshObjectsOnMap;
    function callBlazor_ShowPlaceInfo(event) {
        removeAdditionalObjects();
        unselectAllSelectedPoints();
        if (event.target._latlng == undefined)
            event.target.setStyle({ fillColor: polygonColorSelected });
        else if (event.target._icon != null) {
            event.target._icon.className = event.target._icon.className.replace('map-point-selected', 'map-point').replace('map-point', 'map-point-selected');
        }
        const point = event.target._latlng != undefined ? event.target._latlng : [lat, lng];
        const bbox = convertObjectPositionToBBoxParameter(point);
        blazorMapObjects["Map"].invokeMethodAsync("ShowPlaceInfo", map.getZoom(), bbox, coorx);
    }
    mapAPI.callBlazor_ShowPlaceInfo = callBlazor_ShowPlaceInfo;
    function refreshObjectsOnMap(objectJson) {
        if (groups == null || groups.length == 0)
            return;
        const objectsGroup = groups.find(a => a.options.id == "Objects_group");
        const polygonsGroup = groups.filter(a => a.options.type == "Polygons" || a.options.type == "WFS");
        objectsGroup.clearLayers();
        const objects = JSON.parse(objectJson);
        for (let i = 0; i < objects.length; i++) {
            let newObject;
            if (objects[i].mapPolygon != null) {
                objects[i].mapPolygonModel = JSON.parse(objects[i].mapPolygon);
                newObject = getPolygon(objects[i]);
                if (objects[i].layerName != null && objects[i].layerName.toLocaleLowerCase() != "statistics") {
                    var groupToAdd = polygonsGroup.find(a => a.options.name.toLocaleLowerCase() == objects[i].layerName.toLocaleLowerCase());
                    newObject.addTo(groupToAdd);
                }
                else
                    newObject.addTo(objectsGroup);
            }
            else if (objects[i].mapPoint != null && !objects[i].heatmap) {
                objects[i].mapPointModel = JSON.parse(objects[i].mapPoint);
                newObject = getPoint(objects[i], "map-point");
                newObject.options.guid = objects[i].guid;
                newObject.addTo(objectsGroup);
            }
        }
        for (let i = 0; i < polygonsGroup.length; i++) {
            polygonsGroup[i].bringToFront();
        }
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
    mapAPI.refreshObjectsOnMap = refreshObjectsOnMap;
    function setZIndexForGroups() {
        for (let i = 0; i < mapSettings.layers.length; i++) {
            var group = groups.find(a => a.options.name == mapSettings.layers[i].name);
            group.setZIndex(mapSettings.layers[i].zIndex);
        }
    }
    mapAPI.setZIndexForGroups = setZIndexForGroups;
    function seedHeatmapData(objects, heatmapIndex) {
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
                const markerObject = JSON.parse(objects[i].mapPoint);
                heatmapEntry.heatmapdata.data.push({
                    lat: markerObject.coordinates[1],
                    lng: markerObject.coordinates[0],
                    count: objects[i].citizens
                });
            }
        }
    }
    mapAPI.seedHeatmapData = seedHeatmapData;
    function getIndexOfHeatmapData(objects) {
        for (let i = 0; i < objects.length; i++) {
            if (objects[i].heatmap)
                return objects[i].placeType;
        }
        return null;
    }
    mapAPI.getIndexOfHeatmapData = getIndexOfHeatmapData;
    function showHeatmap(heatmapData) {
        if (heatmapData.data.length == 0)
            return;
        var cfg = {
            "radius": 20,
            "maxOpacity": 1,
            "scaleRadius": false,
            "useLocalExtrema": true,
            latField: 'lat',
            lngField: 'lng',
            valueField: 'count'
        };
        heatmapLayer = new HeatmapOverlay(cfg);
        heatmapLayer.setData(heatmapData);
        heatmapLayer.addTo(map);
    }
    mapAPI.showHeatmap = showHeatmap;
    function addObjectsFromJsonString(jsonInfo, group) {
        var groupName = group + "_group";
        const objectsGroup = groups.find(a => a.options.id == groupName);
        objectsGroup.clearLayers();
        const parsedObjects = JSON.parse(jsonInfo);
        for (let i = 0; i < parsedObjects.length; i++) {
            const parsedLocation = JSON.parse(parsedObjects[i].mapPoint);
            var newObject = null;
            if (parsedLocation == null)
                continue;
            if (parsedLocation.type == "Point") {
                parsedObjects[i].mapPointModel = parsedLocation;
                newObject = getPoint(parsedObjects[i], 'z-index-999', parsedObjects[i].placeType == NarrativeMapStopPlaceType.Main ? openStoryMapTimelineLabel : null);
            }
            else {
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
    mapAPI.addObjectsFromJsonString = addObjectsFromJsonString;
    function drawCurveBettwenPoints(groupName, shortenEndPixels = 10) {
        const objectsGroup = groups.find(a => a.options.id == groupName);
        if (!objectsGroup)
            return;
        const allLayers = objectsGroup.getLayers();
        allLayers.forEach(layer => {
            if (layer.options && layer.options.isTrajectoryLine) {
                objectsGroup.removeLayer(layer);
            }
        });
        const validMarkers = [];
        for (const layer of objectsGroup.getLayers()) {
            if (layer instanceof L.Marker && layer.options.type == NarrativeMapStopPlaceType.Trajectory) {
                validMarkers.push(layer);
            }
        }
        for (let i = 0; i < validMarkers.length - 1; i++) {
            const startMarker = validMarkers[i];
            const endMarker = validMarkers[i + 1];
            const startLatLng = startMarker.getLatLng();
            const endLatLng = endMarker.getLatLng();
            let currentShortenPixels = shortenEndPixels;
            const isLastSegment = (i === validMarkers.length - 2);
            const isPointingSouth = endLatLng.lat < startLatLng.lat;
            if (isLastSegment && isPointingSouth) {
                currentShortenPixels = shortenEndPixels * 3.5;
            }
            const curve = getCurve(startLatLng, endLatLng, currentShortenPixels);
            if (curve) {
                curve.addTo(objectsGroup);
            }
        }
    }
    mapAPI.drawCurveBettwenPoints = drawCurveBettwenPoints;
    const CURVE_CFG = {
        minArrowDist: 15,
        maxArrowDist: 25,
        maxShortenRatio: 0.4,
        shortDistThreshold: 100,
        arcHeightMin: 10,
        arcHeightRatio: 0.15,
        curveSegments: 100,
        fallbackShortenRatio: 0.3
    };
    function getVisualArcControlPoint(p1Pix, p2Pix, pixDist) {
        const midPix = p1Pix.add(p2Pix).divideBy(2);
        const vec = p2Pix.subtract(p1Pix);
        const perpX = -vec.y;
        const perpY = vec.x;
        const len = Math.sqrt(perpX * perpX + perpY * perpY);
        const arcHeight = Math.max(CURVE_CFG.arcHeightMin, pixDist * CURVE_CFG.arcHeightRatio);
        const normX = (perpX / len) * arcHeight;
        const normY = (perpY / len) * arcHeight;
        const ctrlPix = L.point(midPix.x + normX, midPix.y + normY);
        const ctrlLatLng = map.containerPointToLatLng(ctrlPix);
        return [ctrlLatLng.lat, ctrlLatLng.lng];
    }
    function getGeoCurveControlPoint(pointA, pointB) {
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
    function sampleQuadraticBezier(start, control, end, segments) {
        const points = [];
        for (let t = 0; t <= 1; t += 1 / segments) {
            const invT = 1 - t;
            const x = invT * invT * start[1] + 2 * invT * t * control[1] + t * t * end[1];
            const y = invT * invT * start[0] + 2 * invT * t * control[0] + t * t * end[0];
            points.push([y, x]);
        }
        return points;
    }
    function shortenCurveFromEndByPixels(points, targetPx) {
        if (points.length < 2 || targetPx <= 0)
            return points;
        let cumulativeDist = 0;
        for (let i = points.length - 1; i > 0; i--) {
            const curr = map.latLngToContainerPoint(L.latLng(points[i][0], points[i][1]));
            const prev = map.latLngToContainerPoint(L.latLng(points[i - 1][0], points[i - 1][1]));
            const segDist = curr.distanceTo(prev);
            cumulativeDist += segDist;
            if (cumulativeDist >= targetPx) {
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
        if (points.length >= 2 && cumulativeDist > 0) {
            const actualRatio = Math.min(targetPx / cumulativeDist, CURVE_CFG.fallbackShortenRatio);
            const keepCount = Math.max(2, Math.ceil(points.length * (1 - actualRatio)));
            return points.slice(0, keepCount);
        }
        return points;
    }
    function getCurve(pointA, pointB, shortenEndPixels) {
        const p1Pix = map.latLngToContainerPoint(pointA);
        const p2Pix = map.latLngToContainerPoint(pointB);
        const pixDist = p1Pix.distanceTo(p2Pix);
        if (pixDist < 1)
            return null;
        let finalShortenPx = Math.max(CURVE_CFG.minArrowDist, Math.min(CURVE_CFG.maxArrowDist, shortenEndPixels));
        if (finalShortenPx > pixDist * CURVE_CFG.maxShortenRatio) {
            finalShortenPx = pixDist * CURVE_CFG.maxShortenRatio;
        }
        const controlPoint = pixDist < CURVE_CFG.shortDistThreshold
            ? getVisualArcControlPoint(p1Pix, p2Pix, pixDist)
            : getGeoCurveControlPoint(pointA, pointB);
        let curvePoints = sampleQuadraticBezier([pointA.lat, pointA.lng], controlPoint, [pointB.lat, pointB.lng], CURVE_CFG.curveSegments);
        curvePoints = shortenCurveFromEndByPixels(curvePoints, finalShortenPx);
        if (curvePoints.length < 2)
            return null;
        return L.polyline(curvePoints, {
            color: '#771646',
            weight: 1,
            dashArray: '5, 5',
            dashOffset: '0',
            isTrajectoryLine: true
        }).arrowheads({
            frequency: 'endonly',
            fill: true,
            size: '15px',
            color: '#771646'
        });
    }
    mapAPI.getCurve = getCurve;
    function fitMapToGroup(groupName) {
        const objectsGroup = groups.find(a => a.options.id == groupName);
        var latlngs = [];
        objectsGroup.eachLayer(function (layer) {
            if (layer instanceof L.Marker) {
                latlngs.push(layer.getLatLng());
            }
        });
        map.fitBounds(latlngs, { padding: [100, 100] });
    }
    mapAPI.fitMapToGroup = fitMapToGroup;
    function getPoint(markerObject, className, clickFunction) {
        var _a;
        let iconOptions = null;
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
            };
        const result = new L.Marker([markerObject.mapPointModel.coordinates[1], markerObject.mapPointModel.coordinates[0]], iconOptions);
        var pos = map.latLngToLayerPoint(result.getLatLng()).round();
        var zIndex = ((_a = markerObject.priorityOnMap) !== null && _a !== void 0 ? _a : 0);
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
        map.on('click', function () {
            result.closeTooltip();
        });
        return result;
    }
    mapAPI.getPoint = getPoint;
    function getPolygon(polygonObject) {
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
            ? { className: polygonObject.customPolygonClass, color: null, weight: null, fillOpacity: null, opacity: null, fillColor: null }
            : { fillColor: polygonColor, color: polygonStrokeColor, weight: 0.5, fillOpacity: 0.40, opacity: 1 };
        const result = new L.Polygon(pointsArray, polygonOptions);
        if (polygonObject.clickable) {
            result.on('click', callBlazor_ShowPlaceInfo);
        }
        result.on('click', function () {
            result.openTooltip();
        });
        map.on('click', function () {
            result.closeTooltip();
        });
        if (polygonObject.label != undefined && polygonObject.label != null) {
            result.bindTooltip(polygonObject.label, { sticky: true, className: polygonObject.customTooltipClass != undefined && polygonObject.customTooltipClass != null ? polygonObject.customTooltipClass : null });
        }
        return result;
    }
    mapAPI.getPolygon = getPolygon;
    function removeObjects(group) {
        const objectsGroup = groups.find(a => a.options.id == group + "_group");
        objectsGroup.eachLayer(function (item) {
            if (item.options.type == undefined || item.options.type !== "bluepoint") {
                item.remove();
            }
        });
    }
    mapAPI.removeObjects = removeObjects;
    function removeAdditionalObjects() {
        removeObjects("AdditionalObjects");
        const polygonsGroups = groups.filter(a => a.options.type == "Polygons");
        for (var i = 0; i < polygonsGroups.length; i++) {
            polygonsGroups[i].eachLayer(function (item) {
                if (item.options.fillColor != undefined || item.options.fillColor == polygonColorSelected) {
                    item.setStyle({ fillColor: polygonColor });
                }
            });
        }
    }
    mapAPI.removeAdditionalObjects = removeAdditionalObjects;
    function unselectAllSelectedPoints() {
        const objectsGroup = groups.find(a => a.options.id == "Objects_group");
        objectsGroup.eachLayer((item) => {
            const element = item.getElement();
            if (element && element.className) {
                let className;
                if (typeof element.className === 'string') {
                    className = element.className;
                }
                else if (typeof element.className === 'object' && 'baseVal' in element.className) {
                    className = element.className.baseVal;
                }
                else {
                    return;
                }
                if (className.indexOf("map-point-selected") !== -1) {
                    element.className = className.replace('map-point-selected', 'map-point');
                    element.style.zIndex = (map.latLngToLayerPoint(item.getLatLng()).round().y +
                        item.options.zIndexOffset).toString();
                }
            }
        });
    }
    mapAPI.unselectAllSelectedPoints = unselectAllSelectedPoints;
    function selectPointOnMap(guidArrayJson) {
        const guidArray = JSON.parse(guidArrayJson);
        const objectsGroup = groups.find(a => a.options.id == "Objects_group");
        objectsGroup.eachLayer(function (item) {
            if (item.getElement() != null && item.options.guid !== undefined && guidArray.indexOf(item.options.guid) > -1) {
                item.getElement().className = item.getElement().className.replace('map-point-selected', 'map-point').replace('map-point', 'map-point-selected');
                item.getElement().style.zIndex = (map.latLngToLayerPoint(item.getLatLng()).round().y * 2 + item.options.zIndexOffset).toString();
                item.openTooltip();
            }
        });
    }
    mapAPI.selectPointOnMap = selectPointOnMap;
    function getGroups() {
        return groups;
    }
    mapAPI.getGroups = getGroups;
    function getMap() {
        return map;
    }
    mapAPI.getMap = getMap;
    function hideLayersForNormalMap() {
        groups.forEach(group => {
            if (group.options.type == LayersTypeEnum.Base || group.options.type == LayersTypeEnum.WMS)
                return;
            map.eachLayer(function (layer) {
                if (layer.options.id == group.options.id)
                    layer.remove();
            });
        });
    }
    mapAPI.hideLayersForNormalMap = hideLayersForNormalMap;
    function showLayersForNormalMap() {
        hideLayersForNormalMap();
        groups.forEach(group => {
            if (group.options.type == LayersTypeEnum.Base || group.options.type == LayersTypeEnum.Narration)
                return;
            if (group.options.selected)
                group.addTo(map);
        });
    }
    mapAPI.showLayersForNormalMap = showLayersForNormalMap;
    function toggleLayerGroup(name, selected) {
        const groupToAdd = groups.find(a => a.options.name == name);
        groupToAdd.options.selected = selected;
        map.eachLayer(function (layer) { if (layer.options.name == name)
            layer.remove(); });
        if (selected) {
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
    function getSelectedPlaceFromUrl() {
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
    mapAPI.getSelectedPlaceFromUrl = getSelectedPlaceFromUrl;
    function setUrlFromSelectedPlace(coordinatesJson) {
        const coordinates = JSON.parse(coordinatesJson);
        if (coordinates == null || coordinates.length != 2)
            return;
        setUrlParam("x1", coordinates[0].X);
        setUrlParam("y1", coordinates[0].Y);
        setUrlParam("x2", coordinates[1].X);
        setUrlParam("y2", coordinates[1].Y);
    }
    mapAPI.setUrlFromSelectedPlace = setUrlFromSelectedPlace;
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
    function toggleScaleVisibility(visible) {
        const scale = document.getElementsByClassName("leaflet-control-scale")[0];
        if (visible)
            scale.style.visibility = '';
        else
            scale.style.visibility = 'hidden';
    }
    mapAPI.toggleScaleVisibility = toggleScaleVisibility;
    function turnOnLocationTracking() {
        if (!navigator.geolocation) {
            console.log("Your browser doesn't support geolocation feature!");
        }
        else {
            navigator.geolocation.watchPosition(showMyLocation, null, {
                enableHighAccuracy: true,
                timeout: 5000,
                maximumAge: 0
            });
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
    function openStoryMapTimelineLabel(event) {
        const point = event.target;
        blazorMapObjects["Map"].invokeMethodAsync("OpenStoryMapTimelineLabel", point.options.stopId, point.options.narrativeMapId);
    }
    mapAPI.openStoryMapTimelineLabel = openStoryMapTimelineLabel;
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
var NarrativeMapStopPlaceType;
(function (NarrativeMapStopPlaceType) {
    NarrativeMapStopPlaceType["Main"] = "main point";
    NarrativeMapStopPlaceType["Context"] = "context point";
    NarrativeMapStopPlaceType["Trajectory"] = "trajectory point";
})(NarrativeMapStopPlaceType || (NarrativeMapStopPlaceType = {}));
var MapTypeEnum;
(function (MapTypeEnum) {
    MapTypeEnum[MapTypeEnum["Normal"] = 0] = "Normal";
    MapTypeEnum[MapTypeEnum["AllStoryMaps"] = 1] = "AllStoryMaps";
    MapTypeEnum[MapTypeEnum["StoryMapWhole"] = 2] = "StoryMapWhole";
    MapTypeEnum[MapTypeEnum["StoryMapOneStop"] = 3] = "StoryMapOneStop";
})(MapTypeEnum || (MapTypeEnum = {}));
var LayersTypeEnum;
(function (LayersTypeEnum) {
    LayersTypeEnum["Base"] = "Base";
    LayersTypeEnum["WMS"] = "WMS";
    LayersTypeEnum["Narration"] = "Narration";
})(LayersTypeEnum || (LayersTypeEnum = {}));
//# sourceMappingURL=site.js.map