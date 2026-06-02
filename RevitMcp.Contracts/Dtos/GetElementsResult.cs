using System.Collections.Generic;

namespace RevitMcp.Contracts.Dtos
{
    public class ElementSummaryDto
    {
        public int     Id         { get; set; }
        public string  Name       { get; set; } = "";
        public string? Category   { get; set; }
        public string? LevelName  { get; set; }
        public string? FamilyName { get; set; }
    }

    public class GetElementsResult
    {
        public bool                   Success  { get; set; }
        public string                 Message  { get; set; } = "";
        public List<ElementSummaryDto> Elements { get; set; } = new List<ElementSummaryDto>();
    }
}
