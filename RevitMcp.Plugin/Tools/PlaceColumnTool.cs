using System;
using System.Text.Json;
using RevitMcp.Contracts.Dtos;
using RevitMcp.Plugin.Logging;
using RevitMcp.Plugin.Revit;

namespace RevitMcp.Plugin.Tools
{
    public class PlaceColumnTool
    {
        private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly RevitModelFacade _facade;

        public PlaceColumnTool(RevitModelFacade facade) => _facade = facade;

        public McpToolDefinition GetDefinition() => new McpToolDefinition
        {
            Name        = "PlaceColumn",
            Description = "Places a column in Revit at a base coordinate with a height.",
            InputSchema = new
            {
                type     = "object",
                required = new[] { "base", "height", "profile" },
                properties = new
                {
                    @base    = new { type = "object", properties = new { x = new { type = "number" }, y = new { type = "number" }, z = new { type = "number" } } },
                    height   = new { type = "number" },
                    profile  = new { type = "string" },
                    material = new { type = "string" },
                    @class   = new { type = "integer" },
                    name     = new { type = "string" }
                }
            }
        };

        public PlaceColumnResult Invoke(JsonElement body)
        {
            var args = body.Deserialize<PlaceColumnArgs>(JsonOpts) ??
                       throw new ArgumentException("Invalid PlaceColumn arguments.");

            PlaceColumnResult result;
            try
            {
                result = _facade.InsertColumn(args);
            }
            catch (Exception ex)
            {
                result = new PlaceColumnResult { Success = false, Message = ex.Message };
            }

            McpAuditLog.Write("PlaceColumn", args, result.Success, result.Guid, result.Message);
            return result;
        }
    }
}
