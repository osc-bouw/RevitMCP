namespace RevitMcp.Contracts.Dtos
{
    using System.Collections.Generic;

    public class PlaceBeamResult
    {
        public bool Success { get; set; }
        public string? Guid { get; set; }
        public int ObjectId { get; set; }
        public string? Message { get; set; }
        public List<FamilySymbolOptionDto> AvailableFamilySymbols { get; set; } = new List<FamilySymbolOptionDto>();
    }
}
