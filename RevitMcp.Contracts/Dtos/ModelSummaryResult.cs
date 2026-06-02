using System.Collections.Generic;

namespace RevitMcp.Contracts.Dtos
{
    public class ElementCountDto
    {
        public string Category { get; set; } = "";
        public int Count { get; set; }
    }

    public class ModelSummaryResult
    {
        public bool Success { get; set; }
        public string ProjectName { get; set; } = "";
        public string ProjectNumber { get; set; } = "";
        public int LevelCount { get; set; }
        public List<ElementCountDto> ElementCounts { get; set; } = new List<ElementCountDto>();
        public string? Message { get; set; }
    }
}
