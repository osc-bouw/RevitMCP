namespace RevitMcp.Contracts.Dtos
{
    using System.Collections.Generic;

    public class PlaceColumnResult
    {
        public bool Success { get; set; }
        public string? Guid { get; set; }
        public int ObjectId { get; set; }
        public string? Message { get; set; }
        public List<FamilySymbolOptionDto> AvailableFamilySymbols { get; set; } = new List<FamilySymbolOptionDto>();
    }

    public class FamilySymbolOptionDto
    {
        public long FamilyId { get; set; }
        public string FamilyName { get; set; } = "";
        public long SymbolId { get; set; }
        public string SymbolName { get; set; } = "";
        public string? Category { get; set; }
    }
}
