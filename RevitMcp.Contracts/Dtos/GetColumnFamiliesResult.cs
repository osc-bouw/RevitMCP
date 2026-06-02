using System.Collections.Generic;

namespace RevitMcp.Contracts.Dtos
{
    public class ColumnFamilyDto
    {
        public string Name { get; set; } = "";
        public string Id { get; set; } = "";
    }

    public class GetColumnFamiliesResult
    {
        public bool Success { get; set; }
        public List<ColumnFamilyDto> Families { get; set; } = new List<ColumnFamilyDto>();
        public string? Message { get; set; }
    }
}
