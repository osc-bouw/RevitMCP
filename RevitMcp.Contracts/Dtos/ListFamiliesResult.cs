using System.Collections.Generic;

namespace RevitMcp.Contracts.Dtos
{
    public class FamilyDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Category { get; set; }
        public int TypeCount { get; set; }
        public List<FamilySymbolDto> Symbols { get; set; } = new List<FamilySymbolDto>();
    }

    public class FamilySymbolDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = "";
    }

    public class ListFamiliesResult
    {
        public bool            Success  { get; set; }
        public string          Message  { get; set; } = "";
        public List<FamilyDto> Families { get; set; } = new List<FamilyDto>();
    }
}
