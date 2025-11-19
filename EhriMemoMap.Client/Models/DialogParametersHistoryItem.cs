using EhriMemoMap.Shared;

namespace EhriMemoMap.Models
{
    public class DialogParametersHistoryItem
    {
        public DialogTypeEnum DialogType { get; set; }
        public MapTypeEnum MapType { get; set; }
        public VictimLongInfoModel? VictimInfo { get; set; }
        public DialogParameters Parameters { get; set; } = new DialogParameters();
    }

}
