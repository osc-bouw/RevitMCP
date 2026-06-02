using System.Collections.Generic;

namespace RevitMcp.Contracts.Dtos
{
    public class BatchResultDto
    {
        public int     ElementId     { get; set; }
        public string  ParameterName { get; set; } = "";
        public bool    Success       { get; set; }
        public string? Error         { get; set; }
    }

    public class BatchSetParametersResult
    {
        public bool                 Success { get; set; }
        public string               Message { get; set; } = "";
        public List<BatchResultDto> Results { get; set; } = new List<BatchResultDto>();
    }
}
