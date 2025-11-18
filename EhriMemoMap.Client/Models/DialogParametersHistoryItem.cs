namespace EhriMemoMap.Models
{
    public class DialogParametersHistoryItem
    {
        public DialogTypeEnum DialogType { get; set; }
        public DialogParameters Parameters { get; set; } = new DialogParameters();
    }

}
