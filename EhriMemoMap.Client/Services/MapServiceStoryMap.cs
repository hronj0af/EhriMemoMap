using EhriMemoMap.Models;
using EhriMemoMap.Shared;

namespace EhriMemoMap.Client.Services
{

    /// <summary>
    /// Pomocné metody k příběhovým mapám
    /// </summary>
    public partial class MapService
    {
        public async Task ShowNarrativeMap(long? id)
        {
            if (id == null)
                return;
            await SetMapType(MapTypeEnum.StoryMapWhole);
            await SetDialog(DialogTypeEnum.StoryMap, new DialogParameters { Id = id });
            await GetNarrativeMap(DialogParameters.Id, Map.InitialVariables?.City);
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

    }
}
