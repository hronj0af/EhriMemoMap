using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace EhriMemoMap.Models
{
    public enum PlaceType
    {
        Inaccessible,
        
        Address,
        
        Interest,
        
        Incident,

        Statistics
    }
}
