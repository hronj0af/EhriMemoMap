﻿
namespace EhriMemoMap.Shared
{
    /// <summary>
    /// Informace o mapě
    /// </summary>
    public class MapModel
    {

        public InitialVariables? InitialVariables { get; set; }
        public List<LayerModel>? Layers { get; set; }
        public List<TimelinePointModel>? Timeline { get; set; }
    }

    public class InitialVariables
    {
        public int Zoom { get; set; }
        public int ZoomMobile { get; set; }
        public int MinZoom { get; set; }
        public int MaxZoom { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string? WmsProxyUrl { get; set; }
    }

}