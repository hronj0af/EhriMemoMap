using EhriMemoMap.Data;

namespace EhriMemoMap.Shared
{
    public class PlacesResult
    {
        public List<PlaceIncident>? Incidents { get; set; }
        public List<PlaceInterest>? PlacesOfInterest { get; set; }
        public List<PlaceInterest>? InaccessiblePlaces { get; set; }
        public List<AddressWithVictims>? Addresses { get; set; }
        public List<AddressWithVictims>? AddressesLastResidence { get; set; }
        public List<PlaceMemory>? PlacesOfMemory { get; set; }
    }
}
