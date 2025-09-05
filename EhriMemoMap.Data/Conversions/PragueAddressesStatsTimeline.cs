
namespace EhriMemoMap.Data;

public partial class PragueAddressesStatsTimeline
{
    public Shared.PragueAddressesStatsTimeline ConvertToPragueAddressesStatsTimelineShared()
    {
        return new Shared.PragueAddressesStatsTimeline
        {
            Id = Id,
            AddressCs = AddressCs,
            AddressEn = AddressEn,
            AddressDe = AddressDe,
            Count = Count,
            Present19420101 = Present19420101,
            Deported19420101 = Deported19420101,
            Present19420101Perc = Present19420101Perc,
            Present19420401 = Present19420401,
            Deported19420401 = Deported19420401,
            Present19420401Perc = Present19420401Perc,
            Present19420701 = Present19420701,
            Deported19420701 = Deported19420701,
            Present19420701Perc = Present19420701Perc,
            Present19421001 = Present19421001,
            Deported19421001 = Deported19421001,
            Present19421001Perc = Present19421001Perc,
            Present19430101 = Present19430101,
            Deported19430101 = Deported19430101,
            Present19430101Perc = Present19430101Perc,
            Present19430401 = Present19430401,
            Deported19430401 = Deported19430401,
            Present19430401Perc = Present19430401Perc,
            Present19430701 = Present19430701,
            Deported19430701 = Deported19430701,
            Present19430701Perc = Present19430701Perc,
            Present19431001 = Present19431001,
            Deported19431001 = Deported19431001,
            Present19431001Perc = Present19431001Perc,
            Present19440101 = Present19440101,
            Deported19440101 = Deported19440101,
            Present19440101Perc = Present19440101Perc,
            Present19440401 = Present19440401,
            Deported19440401 = Deported19440401,
            Present19440401Perc = Present19440401Perc,
            Present19440701 = Present19440701,
            Deported19440701 = Deported19440701,
            Present19440701Perc = Present19440701Perc,
            Present19441001 = Present19441001,
            Deported19441001 = Deported19441001,
            Present19441001Perc = Present19441001Perc,
            Present19450101 = Present19450101,
            Deported19450101 = Deported19450101,
            Present19450101Perc = Present19450101Perc,
            Present19450401 = Present19450401,
            Deported19450401 = Deported19450401,
            Present19450401Perc = Present19450401Perc,
            Present19450508 = Present19450508,
            Deported19450508 = Deported19450508,
            Present19450508Perc = Present19450508Perc,
            Geography = Geography,
            Murdered = Murdered,
            Survived = Survived,
            FateUnknown = FateUnknown,
            AddressCurrentCs = AddressCurrentCs,
            AddressCurrentEn = AddressCurrentEn,
            AddressCurrentDe = AddressCurrentDe,
        };

    }

}
