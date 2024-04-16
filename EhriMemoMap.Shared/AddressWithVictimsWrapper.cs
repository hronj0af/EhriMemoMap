using EhriMemoMap.Data;

namespace EhriMemoMap.Shared
{
    public class AddressWithVictimsWrappwer
    {
        public PragueAddressesStatsTimeline? Address { get; set; }
        public List<PragueVictimsTimeline>? Victims { get; set; }

        private List<AddressStatistics>? statistics;
        public List<AddressStatistics>? Statistics
        {
            get
            {
                if (Address == null)
                    statistics = new List<AddressStatistics>();

                if (statistics != null)
                    return statistics;

                statistics = new List<AddressStatistics>
                    {
                        new AddressStatistics
                        {
                            Date = new DateTime(1942, 1, 1),
                            NumberPresent = Address.Present19420101,
                            NumberAbsent = Address.Deported19420101,
                        },
                        new AddressStatistics
                        {
                            Date = new DateTime(1942, 04, 01),
                            NumberPresent = Address.Present19420401,
                            NumberAbsent = Address.Deported19420401,
                        },
                        new AddressStatistics
                        {
                            Date = new DateTime(1942, 07, 01),
                            NumberPresent = Address.Present19420701,
                            NumberAbsent = Address.Deported19420701,
                        },
                        new AddressStatistics
                        {
                            Date = new DateTime(1942, 10, 01),
                            NumberPresent = Address.Present19421001,
                            NumberAbsent = Address.Deported19421001,
                        },
                        new AddressStatistics
                        {
                            Date = new DateTime(1943, 01, 01),
                            NumberPresent = Address.Present19430101,
                            NumberAbsent = Address.Deported19430101,
                        },
                        new AddressStatistics
                        {
                            Date = new DateTime(1943, 04, 01),
                            NumberPresent = Address.Present19430401,
                            NumberAbsent = Address.Deported19430401,
                        },
                        new AddressStatistics
                        {
                            Date = new DateTime(1943, 07, 01),
                            NumberPresent = Address.Present19430701,
                            NumberAbsent = Address.Deported19430701,
                        },
                        new AddressStatistics
                        {
                            Date = new DateTime(1943, 10, 01),
                            NumberPresent = Address.Present19431001,
                            NumberAbsent = Address.Deported19431001,
                        },
                        new AddressStatistics
                        {
                            Date = new DateTime(1944, 01, 01),
                            NumberPresent = Address.Present19440101,
                            NumberAbsent = Address.Deported19440101,
                        },
                        new AddressStatistics
                        {
                            Date = new DateTime(1944, 04, 01),
                            NumberPresent = Address.Present19440401,
                            NumberAbsent = Address.Deported19440401,
                        },
                        new AddressStatistics
                        {
                            Date = new DateTime(1944, 07, 01),
                            NumberPresent = Address.Present19440701,
                            NumberAbsent = Address.Deported19440701,
                        },
                        new AddressStatistics
                        {
                            Date = new DateTime(1944, 10, 01),
                            NumberPresent = Address.Present19441001,
                            NumberAbsent = Address.Deported19441001,
                        },
                        new AddressStatistics
                        {
                            Date = new DateTime(1945, 01, 01),
                            NumberPresent = Address.Present19450101,
                            NumberAbsent = Address.Deported19450101,
                        },
                        new AddressStatistics
                        {
                            Date = new DateTime(1945, 04, 01),
                            NumberPresent = Address.Present19450401,
                            NumberAbsent = Address.Deported19450401,
                        },
                        new AddressStatistics
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
