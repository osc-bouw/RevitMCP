namespace RevitMcp.Contracts.Dtos
{
    public class PlaceBeamArgs
    {
        public Point3 Start    { get; set; } = new Point3();
        public Point3 End      { get; set; } = new Point3();
        public string Profile  { get; set; } = "";
        public string? Material { get; set; }
        public int?    Class   { get; set; }
        public string? Name    { get; set; }
    }
}
