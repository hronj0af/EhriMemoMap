namespace EhriMemoMap.Shared
{
    public class PlacesParameters
    {
        public string? City { get; set; }
        public long?[]? IncidentsIds { get; set; }
        public long?[]? PlacesOfInterestIds { get; set; }
        public long?[]? PlacesOfMemoryIds { get; set; }
        public long?[]? InaccessiblePlacesIds { get; set; }
        public long?[]? AddressesIds { get; set; }
    }
}
