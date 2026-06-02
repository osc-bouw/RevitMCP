using System.Collections.Generic;

namespace RevitMcp.Contracts.Dtos
{
    public class ViewDto
    {
        public int     Id        { get; set; }
        public string  Name      { get; set; } = "";
        public string  ViewType  { get; set; } = "";
        public string? LevelName { get; set; }
    }

    public class ListViewsResult
    {
        public bool          Success { get; set; }
        public string        Message { get; set; } = "";
        public List<ViewDto> Views   { get; set; } = new List<ViewDto>();
    }
}
