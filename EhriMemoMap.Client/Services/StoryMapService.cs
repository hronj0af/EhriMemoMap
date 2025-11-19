using EhriMemoMap.Models;
using System.Net.NetworkInformation;

namespace EhriMemoMap.Client.Services
{

    /// <summary>
    /// Pomocné metody k příběhovým mapám
    /// </summary>
    public class StoryMapService(MapStateService mapState, MapLogicService mapLogic)
    {
        public async Task ShowNarrativeMap(long? id)
        {
            if (id == null)
                return;
            await mapState.SetMapType(MapTypeEnum.StoryMapWhole);
            await mapState.SetDialog(DialogTypeEnum.StoryMap, new DialogParameters { Id = id });
            await mapLogic.GetNarrativeMap(mapState.DialogParameters.Id, mapState.Map.InitialVariables?.City);
            await mapLogic.ShowNarrativeMapPlaces();
            mapState.NotifyStateChanged();
        }

        public async Task ShowAllStoryMaps()
        {
            mapState.DialogParameters.Id = null;
            mapState.NarrativeMap = null;
            await mapState.SetMapType(MapTypeEnum.Normal);
            mapState.AddToDialogHistory(DialogTypeEnum.StoryMap, new DialogParameters { Id = null });
            mapState.NotifyStateChanged();

        }

    }
}
