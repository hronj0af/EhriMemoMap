using EhriMemoMap.Models;
using EhriMemoMap.Shared;

namespace EhriMemoMap.Client.Services
{

    /// <summary>
    /// Pomocné metody k příběhovým mapám
    /// </summary>
    public partial class MapService
    {
        public long? ExpandedStopId = null;
        public HashSet<string> ExpandedSections = [];

        public async Task ShowNarrativeMap(long? id)
        {
            if (id == null)
                return;
            await SetMapType(MapTypeEnum.StoryMapWhole);
            await GetNarrativeMap(id, Map.InitialVariables?.City);
            await SetDialog(DialogTypeEnum.StoryMap, new DialogParameters { Id = id });
            await ShowNarrativeMapPlaces();
            NotifyStateChanged();
        }

        public async Task ShowAllStoryMaps()
        {
            DialogParameters.Id = null;
            NarrativeMap = null;
            await SetMapType(MapTypeEnum.AllStoryMaps);
            await ShowAllNarrativeMapsPlaces();
            AddToDialogHistory(DialogTypeEnum.StoryMap, new DialogParameters { Id = null });
            NotifyStateChanged();

        }

        public async Task ShowNarrativeMapPlaces()
        {
            if (NarrativeMap == null)
                return;
            await ShowPlacesOnMap(NarrativeMap?.Stops?.SelectMany(a => a.Places!).Where(a => a.Type == "main point"));
        }

        public async Task ShowAllNarrativeMapsPlaces()
        {
            if (AllNarrativeMaps == null)
                return;
            await ShowPlacesOnMap(AllNarrativeMaps?.SelectMany(a => a.MainPoints!), false);
        }

        public async Task GetAllNarrativeMaps()
        {
            if ((Map.InitialVariables?.StoryMaps ?? false) == false)
                return;

            if (AllNarrativeMaps != null && AllNarrativeMaps.Count > 0)
                return;

            var result = await GetResultFromApiGet<List<NarrativeMap>>("getallnarrativemaps", "city=" + Map.InitialVariables?.City);
            AllNarrativeMaps = result;
        }

        public async Task GetNarrativeMap(long? id, string? city)
        {
            if (id == null)
                return;

            var result = await GetResultFromApiGet<NarrativeMap>("getnarrativemap", "id=" + id + "&city=" + city);
            NarrativeMap = result;
        }


        public async Task ShowStopPlacesOnMap(long stopId)
        {
            await SetMapType(MapTypeEnum.StoryMapOneStop);
            var stop = NarrativeMap?.Stops?.FirstOrDefault(a => a.Id == stopId);
            if (stop == null)
                return;
            await ShowPlacesOnMap(stop.Places);
            NotifyStateChanged();
        }


        public async Task SwitchStoryMap()
        {
            if (DialogType == DialogTypeEnum.StoryMap)
            {
                await SetMapType(MapTypeEnum.Normal);
                await SetDialog(DialogTypeEnum.None);
                NotifyStateChanged();
                return;
            }

            if (AllNarrativeMaps != null && AllNarrativeMaps.Count == 1)
            {
                DialogParameters.Id = AllNarrativeMaps.FirstOrDefault()?.Id;
                await ShowNarrativeMap(DialogParameters.Id);
            }
            else
            {
                DialogParameters.Id = null;
                NarrativeMap = null;
                await SetMapType(MapTypeEnum.AllStoryMaps);
                await ShowAllNarrativeMapsPlaces();
                await SetDialog(DialogTypeEnum.StoryMap);
            }

            NotifyStateChanged();
        }


        public async Task OpenStoryMapTimelineLabel(long stopId, long? narrativeMapId)
        {
            if (NarrativeMap == null || NarrativeMap.Id != narrativeMapId)
            {
                // Pokud není otevřená správná mapa, otevřeme ji.
                await GetNarrativeMap(narrativeMapId, Map.InitialVariables?.City);
                await ShowNarrativeMap(narrativeMapId);
            }

            if (ExpandedStopId == stopId)
            {
                // Pokud kliknete na již otevřený bod, bod se uzavře.
                ExpandedStopId = null;
                ExpandedSections.Clear(); // Zavřít všechny sekce
                await ShowNarrativeMapPlaces();
            }
            else
            {
                // Otevření nového bodu (předchozí bod se automaticky uzavře)
                ExpandedStopId = stopId;
                ExpandedSections.Clear(); // Zavřít všechny sekce
                await ShowStopPlacesOnMap(stopId);
            }
            NotifyStateChanged();
        }

    }
}