// Převod dat z API na objekty pro Leaflet – náhrada konstruktorů MapObjectForLeafletModel
// (EhriMemoMap.Client/Models/MapObjectForLeafletModel.cs).
import type { MapObject, MapStatistic, Place, SolrPlace } from '../models/api'
import type { LayerModel, MapObjectForLeaflet } from '../models/map'
import { NarrativeMapStopPlaceType, PlaceType } from '../models/enums'
import { byLang, t } from '../i18n'

function empty(): MapObjectForLeaflet {
  return {
    clickable: false,
    placeType: null,
    layerName: null,
    citizens: null,
    citizensTotal: null,
    id: null,
    guid: null,
    label: null,
    customTooltipClass: null,
    customPolygonClass: null,
    customPolygonColor: null,
    customPolygonFillColor: null,
    customPolygonFillOpacity: null,
    customPolygonWeight: null,
    mapPoint: null,
    mapPolygon: null,
    htmlIcon: null,
    iconUrl: null,
    iconAnchor: null,
    iconSize: null,
    heatmap: null,
    stopId: null,
    narrativeMapId: null,
    priorityOnMap: null,
  }
}

/** Výsledek fulltextového hledání (MapObjectForLeafletModel(SolrPlace)). */
export function fromSolrPlace(place: SolrPlace): MapObjectForLeaflet {
  return {
    ...empty(),
    id: place.id,
    label: byLang(place.labelCs, place.labelEn),
    mapPoint: place.mapLocation,
    clickable: true,
    iconAnchor: [13, 45],
    htmlIcon: place.id.includes('victim_last_residence')
      ? "<img src='css/images/marker-icon-red.png' />"
      : "<img src='css/images/marker-icon.png' />",
  }
}

/** Místo příběhové mapy (MapObjectForLeafletModel(Place, defaultColor)). */
export function fromPlace(place: Place, defaultColor = true): MapObjectForLeaflet {
  const result: MapObjectForLeaflet = {
    ...empty(),
    id: String(place.id),
    label: byLang(place.labelCs, place.labelEn),
    mapPoint: place.mapPoint,
    placeType: place.type,
    stopId: place.stopId,
    narrativeMapId: place.narrativeMapId,
  }

  switch (place.type) {
    case NarrativeMapStopPlaceType.Main: {
      // C#: NarrativeMapId % 7 – pro null je výsledek null → "narrative-icon.png"
      const iconNumber = place.narrativeMapId != null ? place.narrativeMapId % 7 : ''
      result.htmlIcon = `<img src='css/images/narrative-icon${defaultColor ? '' : iconNumber}.png' />`
      result.clickable = true
      result.iconAnchor = [22, 55]
      break
    }
    case NarrativeMapStopPlaceType.Trajectory:
      result.htmlIcon = "<img src='css/images/trajectory-icon.png' />"
      result.iconAnchor = [10, 10]
      break
    default:
      result.htmlIcon = "<img src='css/images/marker-icon-wine-red.png' />"
      result.iconAnchor = [12, 41]
      break
  }
  return result
}

/** Ikona adresy podle podílu obětí: victims_{100|051|001|000}_{počet}.png */
function addressIconFile(citizens: number, citizensTotal: number | null): string {
  // C#: (decimal)Citizens / CitizensTotal – dělení null dává null → default větev "victims_100"
  if (citizensTotal == null || citizensTotal === 0) return 'victims_100'
  const percents = citizens / citizensTotal
  if (percents === 1) return 'victims_100'
  if (percents < 1 && percents >= 0.5) return 'victims_051'
  if (percents < 0.5 && percents > 0) return 'victims_001'
  if (percents === 0) return 'victims_000'
  return 'victims_100'
}

/**
 * Jednoznačný identifikátor objektu na mapě – podle něj se vybírají / zvýrazňují body.
 * (C# používal DateFrom.ToString("yyyy-mm-dd"), kde "mm" jsou omylem minuty; důležitá je jen konzistence,
 * Guid se vytváří i porovnává pouze na klientovi.)
 */
function objectGuid(mapObject: MapObject): string {
  return `${mapObject.placeType}_${mapObject.id ?? ''}_${mapObject.dateFrom ?? ''}`
}

/** Objekt z API getmapobjects / getheatmap / getwfsobjects (MapObjectForLeafletModel(MapObject, heatmap, layers)). */
export function fromMapObject(mapObject: MapObject, heatmap: boolean, layers?: LayerModel[] | null): MapObjectForLeaflet {
  const layerName = mapObject.layerName?.toLowerCase()
  const layer = layers?.find((a) => !!a.name && a.name.toLowerCase() === layerName)

  const result: MapObjectForLeaflet = {
    ...empty(),
    clickable: !heatmap,
    citizens: mapObject.citizens,
    mapPoint: mapObject.mapPoint,
    placeType: mapObject.placeType,
    heatmap,
    layerName: mapObject.layerName,
    priorityOnMap: layer?.priorityOnMap ?? null,
    customPolygonColor: layer?.customPolygonColor ?? null,
    customPolygonFillColor: layer?.customPolygonFillColor ?? null,
    customPolygonFillOpacity: layer?.customPolygonFillOpacity ?? null,
    customPolygonWeight: layer?.customPolygonWeight ?? null,
  }

  if (heatmap) return result

  result.citizensTotal = mapObject.citizensTotal
  result.id = mapObject.id != null ? String(mapObject.id) : ''
  result.guid = objectGuid(mapObject)
  result.label = layer?.labelCode ? t(layer.labelCode) : byLang(mapObject.labelCs, mapObject.labelEn)
  result.mapPolygon = mapObject.mapPolygon

  switch (mapObject.placeType) {
    case PlaceType.Incident:
      result.htmlIcon = "<img src='images/incident-map.png' />"
      result.iconAnchor = [12, 12]
      break
    case PlaceType.Interest:
      result.htmlIcon = "<img src='images/interest-map.png' />"
      result.iconAnchor = [12, 12]
      break
    case PlaceType.Memory:
      result.htmlIcon = "<img src='images/memory-map.png' />"
      result.iconAnchor = [20, 20]
      break
    case PlaceType.Memorial:
      result.htmlIcon = "<img src='images/memorial-map.png' />"
      result.iconAnchor = [12, 12]
      break
    case PlaceType.Address:
    case PlaceType.AddressLastResidence: {
      const invert = mapObject.placeType === PlaceType.AddressLastResidence ? 'filter:invert(1);' : ''
      const file =
        mapObject.citizens == null
          ? 'victims_100'
          : `${addressIconFile(mapObject.citizens, mapObject.citizensTotal)}_${mapObject.citizens}`
      result.htmlIcon = `<div style='${invert}position:relative'><img src='images/addresses/${file}.png' /></div>`
      result.iconAnchor = [12, 12]
      break
    }
  }
  return result
}

/** Statistika jedné části města – polygon s tabulkou v tooltipu (MapObjectForLeafletModel(List<MapStatistic>)). */
export function fromStatistics(statistics: MapStatistic[]): MapObjectForLeaflet | null {
  if (!statistics || statistics.length === 0) return null
  const first = statistics[0]
  const count = (type: string) => statistics.find((a) => a.type?.includes(type))?.count ?? 0

  const row = (labelKey: string, value: number) => `
                <tr>
                    <td class='statistics-table-firstcell'>
                        <span>${t(labelKey)}</span>
                    </td>
                    <td class='statistics-table-secondcell'>
                        ${value}
                    </td>
                </tr>`

  const label = `<h2 class='rz-mb-2'>${byLang(first.quarterCs, first.quarterEn) ?? ''}</h2>
            <table class='statistics-table'>${row('countJews', count('victims'))}${row('countIncidents', count('incidents'))}${row('countPointsOfInterest', count('pois_points'))}${row('countInaccessiblePlaces', count('pois_polygons'))}${row('countPlacesOfMemory', count('places_of_memory'))}
                <tr>
                    <td colspan=2>
                        ${t('forDetailsZoom')}
                    </td>
                </tr>
            </table>`

  return {
    ...empty(),
    placeType: PlaceType.Statistics,
    id: first.id != null ? String(first.id) : null,
    guid: `${PlaceType.Statistics}_${first.id ?? ''}_`,
    clickable: false,
    mapPolygon: statistics.find((a) => !!a.mapPolygon)?.mapPolygon ?? null,
    mapPoint: statistics.find((a) => !!a.mapPoint)?.mapPoint ?? null,
    customTooltipClass: 'statistics-tooltip',
    customPolygonClass: 'statistics-polygon',
    label,
  }
}
