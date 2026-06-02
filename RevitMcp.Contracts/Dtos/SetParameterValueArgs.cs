namespace RevitMcp.Contracts.Dtos
{
    public class SetParameterValueArgs
    {
        public int    ElementId     { get; set; }
        public string ParameterName { get; set; } = "";
        public string Value         { get; set; } = "";
    }
}
