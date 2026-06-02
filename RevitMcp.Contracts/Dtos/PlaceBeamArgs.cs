namespace RevitMcp.Contracts.Dtos
{
    public class PlaceBeamArgs
    {
        public Point3 Start { get; set; } = new Point3();
        public Point3 End { get; set; } = new Point3();
        public long? FamilySymbolId { get; set; }
    }
}
