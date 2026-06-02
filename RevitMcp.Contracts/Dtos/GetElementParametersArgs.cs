namespace RevitMcp.Contracts.Dtos
{
    public class GetElementParametersArgs
    {
        public int     ElementId     { get; set; }
        public string? ParameterName { get; set; }
    }
}
