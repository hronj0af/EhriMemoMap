using EhriMemoMap.Data;

namespace EhriMemoMap.Models
{
    public class AddressWithVictimsWrappwer
    {
        public PragueAddressesStatsTimeline? Address { get; set; }
        public List<PragueVictimsTimeline>? Victims { get; set; }

        private List<WMSFeatureInfoStatistics>? statistics;
        public List<WMSFeatureInfoStatistics>? Statistics
        {
            get
            {
                if (Address == null)
                    statistics = new List<WMSFeatureInfoStatistics>();

                if (statistics != null)
                    return statistics;

                statistics = new List<WMSFeatureInfoStatistics>
                    {
                        new WMSFeatureInfoStatistics
                        {
                            Date = new DateTime(1941, 10, 01),
                            NumberPresent = Address.Survived + Address.Murdered + Address.FateUnknown,
                            NumberAbsent = 0,
                        },
                        new WMSFeatureInfoStatistics
                        {
                            Date = new DateTime(1941, 12, 31),
                            NumberPresent = Address.Present19411231,
                            NumberAbsent = Address.Deported19411231,
                        },
                        new WMSFeatureInfoStatistics
                        {
                            Date = new DateTime(1942, 03, 31),
                            NumberPresent = Address.Present19420331,
                            NumberAbsent = Address.Deported19420331,
                        },
                        new WMSFeatureInfoStatistics
                        {
                            Date = new DateTime(1942, 06, 30),
                            NumberPresent = Address.Present19420630,
                            NumberAbsent = Address.Deported19420630,
                        },
                        new WMSFeatureInfoStatistics
                        {
                            Date = new DateTime(1942, 09, 30),
                            NumberPresent = Address.Present19420930,
                            NumberAbsent = Address.Deported19420930,
                        },
                        new WMSFeatureInfoStatistics
                        {
                            Date = new DateTime(1942, 12, 31),
                            NumberPresent = Address.Present19421231,
                            NumberAbsent = Address.Deported19421231,
                        },
                        new WMSFeatureInfoStatistics
                        {
                            Date = new DateTime(1943, 03, 31),
                            NumberPresent = Address.Present19430331,
                            NumberAbsent = Address.Deported19430331,
                        },
                        new WMSFeatureInfoStatistics
                        {
                            Date = new DateTime(1943, 06, 30),
                            NumberPresent = Address.Present19430630,
                            NumberAbsent = Address.Deported19430630,
                        },
                        new WMSFeatureInfoStatistics
                        {
                            Date = new DateTime(1943, 09, 30),
                            NumberPresent = Address.Present19430930,
                            NumberAbsent = Address.Deported19430930,
                        },
                        new WMSFeatureInfoStatistics
                        {
                            Date = new DateTime(1943, 12, 31),
                            NumberPresent = Address.Present19431231,
                            NumberAbsent = Address.Deported19431231,
                        },
                        new WMSFeatureInfoStatistics
                        {
                            Date = new DateTime(1944, 03, 31),
                            NumberPresent = Address.Present19440331,
                            NumberAbsent = Address.Deported19440331,
                        },
                        new WMSFeatureInfoStatistics
                        {
                            Date = new DateTime(1944, 06, 30),
                            NumberPresent = Address.Present19440630,
                            NumberAbsent = Address.Deported19440630,
                        },
                        new WMSFeatureInfoStatistics
                        {
                            Date = new DateTime(1944, 09, 30),
                            NumberPresent = Address.Present19440930,
                            NumberAbsent = Address.Deported19440930,
                        },
                        new WMSFeatureInfoStatistics
                        {
                            Date = new DateTime(1944, 12, 31),
                            NumberPresent = Address.Present19441231,
                            NumberAbsent = Address.Deported19441231,
                        },
                        new WMSFeatureInfoStatistics
                        {
                            Date = new DateTime(1945, 03, 31),
                            NumberPresent = Address.Present19450331,
                            NumberAbsent = Address.Deported19450331,
                        },
                        new WMSFeatureInfoStatistics
                        {
                            Date = new DateTime(1945, 05, 08),
                            NumberPresent = Address.Present19450508,
                            NumberAbsent = Address.Deported19450508,
                        }
                    };
                return statistics;
            }
        }

    }
}
