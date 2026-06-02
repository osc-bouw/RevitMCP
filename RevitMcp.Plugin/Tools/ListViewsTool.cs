using System;
using System.Text.Json;
using RevitMcp.Contracts.Dtos;
using RevitMcp.Plugin.Logging;
using RevitMcp.Plugin.Revit;

namespace RevitMcp.Plugin.Tools
{
    public class ListViewsTool
    {
        private readonly RevitModelFacade _facade;
        public ListViewsTool(RevitModelFacade facade) => _facade = facade;

        public McpToolDefinition GetDefinition() => new McpToolDefinition
        {
            Name        = "ListViews",
            Description = "Returns all non-template views in the Revit model with their id, name, type, and associated level.",
            InputSchema = new { type = "object", properties = new { } }
        };

        public ListViewsResult Invoke(JsonElement body)
        {
            ListViewsResult result;
            try   { result = _facade.ListViews(); }
            catch (Exception ex) { result = new ListViewsResult { Success = false, Message = ex.Message }; }
            McpAuditLog.Write("ListViews", null, result.Success, null, result.Message);
            return result;
        }
    }
}
