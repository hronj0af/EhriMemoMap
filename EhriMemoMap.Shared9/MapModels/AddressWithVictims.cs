namespace EhriMemoMap.Shared
{
    public class AddressWithVictims
    {
        public AddressInfo? Address { get; set; }
        public PragueAddressesStatsTimeline? PragueAddress { get; set; }
        public List<VictimShortInfoModel>? Victims { get; set; }

        private List<AddressStatistics>? statistics;
        public List<AddressStatistics>? Statistics
        {
            get
            {
                if (PragueAddress == null)
                    statistics = [];

                if (statistics != null)
                    return statistics;

                statistics =
                    [
                        new() {
                            Date = new DateTime(1942, 1, 1),
                            NumberPresent = PragueAddress.Present19420101,
                            NumberAbsent = PragueAddress.Deported19420101,
                        },
                        new()
                        {
                            Date = new DateTime(1942, 04, 01),
                            NumberPresent = PragueAddress.Present19420401,
                            NumberAbsent = PragueAddress.Deported19420401,
                        },
                        new()
                        {
                            Date = new DateTime(1942, 07, 01),
                            NumberPresent = PragueAddress.Present19420701,
                            NumberAbsent = PragueAddress.Deported19420701,
                        },
                        new()
                        {
                            Date = new DateTime(1942, 10, 01),
                            NumberPresent = PragueAddress.Present19421001,
                            NumberAbsent = PragueAddress.Deported19421001,
                        },
                        new()
                        {
                            Date = new DateTime(1943, 01, 01),
                            NumberPresent = PragueAddress.Present19430101,
                            NumberAbsent = PragueAddress.Deported19430101,
                        },
                        new()
                        {
                            Date = new DateTime(1943, 04, 01),
                            NumberPresent = PragueAddress.Present19430401,
                            NumberAbsent = PragueAddress.Deported19430401,
                        },
                        new()
                        {
                            Date = new DateTime(1943, 07, 01),
                            NumberPresent = PragueAddress.Present19430701,
                            NumberAbsent = PragueAddress.Deported19430701,
                        },
                        new()
                        {
                            Date = new DateTime(1943, 10, 01),
                            NumberPresent = PragueAddress.Present19431001,
                            NumberAbsent = PragueAddress.Deported19431001,
                        },
                        new()
                        {
                            Date = new DateTime(1944, 01, 01),
                            NumberPresent = PragueAddress.Present19440101,
                            NumberAbsent = PragueAddress.Deported19440101,
                        },
                        new()
                        {
                            Date = new DateTime(1944, 04, 01),
                            NumberPresent = PragueAddress.Present19440401,
                            NumberAbsent = PragueAddress.Deported19440401,
                        },
                        new()
                        {
                            Date = new DateTime(1944, 07, 01),
                            NumberPresent = PragueAddress.Present19440701,
                            NumberAbsent = PragueAddress.Deported19440701,
                        },
                        new()
                        {
                            Date = new DateTime(1944, 10, 01),
                            NumberPresent = PragueAddress.Present19441001,
                            NumberAbsent = PragueAddress.Deported19441001,
                        },
                        new()
                        {
                            Date = new DateTime(1945, 01, 01),
                            NumberPresent = PragueAddress.Present19450101,
                            NumberAbsent = PragueAddress.Deported19450101,
                        },
                        new()
                        {
                            Date = new DateTime(1945, 04, 01),
                            NumberPresent = PragueAddress.Present19450401,
                            NumberAbsent = PragueAddress.Deported19450401,
                        },
                        new()
                        {
                            Date = new DateTime(1945, 05, 08),
                            NumberPresent = PragueAddress.Present19450508,
                            NumberAbsent = PragueAddress.Deported19450508,
                        }
                    ];
                return statistics;
            }
        }

    }
}
