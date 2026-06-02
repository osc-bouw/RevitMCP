using System;
using System.Text.Json;
using RevitMcp.Contracts.Dtos;
using RevitMcp.Plugin.Logging;
using RevitMcp.Plugin.Revit;

namespace RevitMcp.Plugin.Tools
{
    public class ListFamiliesTool
    {
        private readonly RevitModelFacade _facade;
        public ListFamiliesTool(RevitModelFacade facade) => _facade = facade;

        public McpToolDefinition GetDefinition() => new McpToolDefinition
        {
            Name        = "ListFamilies",
            Description = "Returns all loaded families in the Revit model with their id, name, category, and type count.",
            InputSchema = new { type = "object", properties = new { } }
        };

        public ListFamiliesResult Invoke(JsonElement body)
        {
            ListFamiliesResult result;
            try   { result = _facade.ListFamilies(); }
            catch (Exception ex) { result = new ListFamiliesResult { Success = false, Message = ex.Message }; }
            McpAuditLog.Write("ListFamilies", null, result.Success, null, result.Message);
            return result;
        }
    }
}
