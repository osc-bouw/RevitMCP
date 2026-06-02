using System;
using System.Text.Json;
using RevitMcp.Contracts.Dtos;
using RevitMcp.Plugin.Logging;
using RevitMcp.Plugin.Revit;

namespace RevitMcp.Plugin.Tools
{
    public class GetElementsTool
    {
        private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        private readonly RevitModelFacade _facade;
        public GetElementsTool(RevitModelFacade facade) => _facade = facade;

        public McpToolDefinition GetDefinition() => new McpToolDefinition
        {
            Name        = "GetElements",
            Description = "Queries Revit elements by optional category (e.g. OST_StructuralColumns), level name, or family name. Returns element id, name, category, level and family.",
            InputSchema = new
            {
                type       = "object",
                properties = new
                {
                    category   = new { type = "string", description = "BuiltInCategory name, e.g. OST_StructuralColumns" },
                    levelName  = new { type = "string" },
                    familyName = new { type = "string" },
                    limit      = new { type = "integer", description = "Max elements to return (default 100)" }
                }
            }
        };

        public GetElementsResult Invoke(JsonElement body)
        {
            var args = body.Deserialize<GetElementsArgs>(JsonOpts) ?? new GetElementsArgs();
            GetElementsResult result;
            try   { result = _facade.GetElements(args); }
            catch (Exception ex) { result = new GetElementsResult { Success = false, Message = ex.Message }; }
            McpAuditLog.Write("GetElements", args, result.Success, null, result.Message);
            return result;
        }
    }
}
