using System.Collections.Generic;

namespace RevitMcp.Contracts.Dtos
{
    public class WarningDto
    {
        public string      Description        { get; set; } = "";
        public string      FailureId          { get; set; } = "";
        public List<int>   AffectedElementIds { get; set; } = new List<int>();
    }

    public class GetWarningsResult
    {
        public bool             Success  { get; set; }
        public string           Message  { get; set; } = "";
        public List<WarningDto> Warnings { get; set; } = new List<WarningDto>();
    }
}
