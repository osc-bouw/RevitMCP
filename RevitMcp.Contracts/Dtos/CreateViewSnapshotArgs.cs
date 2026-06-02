namespace RevitMcp.Contracts.Dtos
{
    public class CreateViewSnapshotArgs
    {
        public int ViewId  { get; set; }
        public int WidthPx { get; set; } = 1920;
    }
}
