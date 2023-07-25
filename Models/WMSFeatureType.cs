using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace EhriMemoMap.Models
{
    public enum WMSFeatureType : byte
    {
        [Display(Name = "Nepřístupná místa")]
        Neprisupna_mista,
        
        [Display(Name = "Oběti holokaustu z Prahy")]
        Obeti_holokaustu_z_Prahy,
        
        [Display(Name = "Bydliště Židů")]
        Bydliste_Zidu_v_Praze_v_dobe_okupace,
        
        [Display(Name = "Body zájmu")]
        Body_zajmu,
        
        [Display(Name = "Incidenty v Praze")]
        Incidenty_v_prostoru_mesta_Prahy,
    }
}
