using System.Collections.Generic;

namespace RevitMcp.Contracts.Dtos
{
    public class LevelDto
    {
        public int    Id          { get; set; }
        public string Name        { get; set; } = "";
        public double ElevationMm { get; set; }
    }

    public class ListLevelsResult
    {
        public bool           Success { get; set; }
        public string         Message { get; set; } = "";
        public List<LevelDto> Levels  { get; set; } = new List<LevelDto>();
    }
}
