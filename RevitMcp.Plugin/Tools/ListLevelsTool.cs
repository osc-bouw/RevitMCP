using System;
using System.Text.Json;
using RevitMcp.Contracts.Dtos;
using RevitMcp.Plugin.Logging;
using RevitMcp.Plugin.Revit;

namespace RevitMcp.Plugin.Tools
{
    public class ListLevelsTool
    {
        private readonly RevitModelFacade _facade;
        public ListLevelsTool(RevitModelFacade facade) => _facade = facade;

        public McpToolDefinition GetDefinition() => new McpToolDefinition
        {
            Name        = "ListLevels",
            Description = "Returns all levels in the Revit model with their name, id, and elevation in millimeters.",
            InputSchema = new { type = "object", properties = new { } }
        };

        public ListLevelsResult Invoke(JsonElement body)
        {
            ListLevelsResult result;
            try   { result = _facade.ListLevels(); }
            catch (Exception ex) { result = new ListLevelsResult { Success = false, Message = ex.Message }; }
            McpAuditLog.Write("ListLevels", null, result.Success, null, result.Message);
            return result;
        }
    }
}
