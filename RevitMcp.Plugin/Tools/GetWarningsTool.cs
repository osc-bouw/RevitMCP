using System;
using System.Text.Json;
using RevitMcp.Contracts.Dtos;
using RevitMcp.Plugin.Logging;
using RevitMcp.Plugin.Revit;

namespace RevitMcp.Plugin.Tools
{
    public class GetWarningsTool
    {
        private readonly RevitModelFacade _facade;
        public GetWarningsTool(RevitModelFacade facade) => _facade = facade;

        public McpToolDefinition GetDefinition() => new McpToolDefinition
        {
            Name        = "GetWarnings",
            Description = "Returns all current Revit model warnings with their description, failure definition id, and affected element ids.",
            InputSchema = new { type = "object", properties = new { } }
        };

        public GetWarningsResult Invoke(JsonElement body)
        {
            GetWarningsResult result;
            try   { result = _facade.GetWarnings(); }
            catch (Exception ex) { result = new GetWarningsResult { Success = false, Message = ex.Message }; }
            McpAuditLog.Write("GetWarnings", null, result.Success, null, result.Message);
            return result;
        }
    }
}
