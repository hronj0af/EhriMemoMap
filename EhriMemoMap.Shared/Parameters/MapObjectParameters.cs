﻿namespace EhriMemoMap.Shared
{
    public class MapObjectParameters
    {
        public string? City { get; set; }
        public PointModel[]? CustomCoordinates { get; set; }
        public PointModel? MapSouthWestPoint { get; set; }
        public PointModel? MapNorthEastPoint { get; set;}
        public List<string?>? SelectedLayerNames { get; set; }
        public TimelinePointModel? SelectedTimeLinePoint { get; set; }
    }
}
