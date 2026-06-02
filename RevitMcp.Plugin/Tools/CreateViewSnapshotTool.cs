using System;
using System.Text.Json;
using RevitMcp.Contracts.Dtos;
using RevitMcp.Plugin.Logging;
using RevitMcp.Plugin.Revit;

namespace RevitMcp.Plugin.Tools
{
    public class CreateViewSnapshotTool
    {
        private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        private readonly RevitModelFacade _facade;
        public CreateViewSnapshotTool(RevitModelFacade facade) => _facade = facade;

        public McpToolDefinition GetDefinition() => new McpToolDefinition
        {
            Name        = "CreateViewSnapshot",
            Description = "Exports a Revit view as a PNG image and returns it as a base64-encoded string.",
            InputSchema = new
            {
                type       = "object",
                required   = new[] { "viewId" },
                properties = new
                {
                    viewId  = new { type = "integer" },
                    widthPx = new { type = "integer", description = "Export width in pixels (default 1920)" }
                }
            }
        };

        public CreateViewSnapshotResult Invoke(JsonElement body)
        {
            var args = body.Deserialize<CreateViewSnapshotArgs>(JsonOpts) ?? new CreateViewSnapshotArgs();
            CreateViewSnapshotResult result;
            try   { result = _facade.CreateViewSnapshot(args); }
            catch (Exception ex) { result = new CreateViewSnapshotResult { Success = false, Message = ex.Message }; }
            McpAuditLog.Write("CreateViewSnapshot", args, result.Success, null, result.Message);
            return result;
        }
    }
}
