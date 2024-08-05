using EhriMemoMap.Data;

namespace EhriMemoMap.Shared
{
    public class PlacesResult
    {
        public List<PragueIncidentsTimeline>? Incidents { get; set; }
        public List<PraguePlacesOfInterestTimeline>? PlacesOfInterest { get; set; }
        public List<PraguePlacesOfInterestTimeline>? InaccessiblePlaces { get; set; }
        public List<AddressWithVictimsWrappwer>? Addresses { get; set; }
        public List<PraguePlacesOfMemory>? PlacesOfMemory { get; set; }
    }
}
