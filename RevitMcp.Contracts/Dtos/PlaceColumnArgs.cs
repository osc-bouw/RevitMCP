namespace RevitMcp.Contracts.Dtos
{
    public class PlaceColumnArgs
    {
        public Point3 Base { get; set; } = new Point3();
        public double Height { get; set; }
        public long? FamilySymbolId { get; set; }
    }
}
