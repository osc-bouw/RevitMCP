namespace RevitMcp.Contracts.Dtos
{
    public class GetElementsArgs
    {
        public string? Category   { get; set; }
        public string? LevelName  { get; set; }
        public string? FamilyName { get; set; }
        public int     Limit      { get; set; } = 100;
    }
}
