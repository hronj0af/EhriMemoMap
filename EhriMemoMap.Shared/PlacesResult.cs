﻿using EhriMemoMap.Data;

namespace EhriMemoMap.Shared
{
    public class PlacesResult
    {
        public List<PragueIncidentsTimeline>? Incidents { get; set; }
        public List<PlaceInterest>? PlacesOfInterest { get; set; }
        public List<PlaceInterest>? InaccessiblePlaces { get; set; }
        public List<AddressWithVictims>? Addresses { get; set; }
        public List<PraguePlacesOfMemory>? PlacesOfMemory { get; set; }
    }
}
