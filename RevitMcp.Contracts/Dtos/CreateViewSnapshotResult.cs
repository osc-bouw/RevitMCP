namespace RevitMcp.Contracts.Dtos
{
    public class CreateViewSnapshotResult
    {
        public bool    Success     { get; set; }
        public string  Message     { get; set; } = "";
        public string? Base64Image { get; set; }
        public string  MimeType    { get; set; } = "image/png";
    }
}
