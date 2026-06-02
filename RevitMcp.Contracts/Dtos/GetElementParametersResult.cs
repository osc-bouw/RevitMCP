using System.Collections.Generic;

namespace RevitMcp.Contracts.Dtos
{
    public class ParameterDto
    {
        public string  Name        { get; set; } = "";
        public string  StorageType { get; set; } = "";
        public string? Value       { get; set; }
        public bool    IsReadOnly  { get; set; }
        public string? GroupName   { get; set; }
    }

    public class GetElementParametersResult
    {
        public bool               Success    { get; set; }
        public string             Message    { get; set; } = "";
        public List<ParameterDto> Parameters { get; set; } = new List<ParameterDto>();
    }
}
