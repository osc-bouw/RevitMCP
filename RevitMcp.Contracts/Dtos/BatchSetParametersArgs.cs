using System.Collections.Generic;

namespace RevitMcp.Contracts.Dtos
{
    public class ParameterUpdateDto
    {
        public int    ElementId     { get; set; }
        public string ParameterName { get; set; } = "";
        public string Value         { get; set; } = "";
    }

    public class BatchSetParametersArgs
    {
        public List<ParameterUpdateDto> Updates { get; set; } = new List<ParameterUpdateDto>();
    }
}
