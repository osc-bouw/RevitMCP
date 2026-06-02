using System;
using System.Text.Json;
using RevitMcp.Contracts.Dtos;
using RevitMcp.Plugin.Logging;
using RevitMcp.Plugin.Revit;

namespace RevitMcp.Plugin.Tools
{
    public class GetColumnFamiliesTool
    {
        private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly RevitModelFacade _facade;

        public GetColumnFamiliesTool(RevitModelFacade facade) => _facade = facade;

        public McpToolDefinition GetDefinition() => new McpToolDefinition
        {
            Name        = "GetColumnFamilies",
            Description = "Lists all column families available in the current Revit model.",
            InputSchema = new
            {
                type       = "object",
                properties = new { } // No arguments needed
            }
        };

        public GetColumnFamiliesResult Invoke(JsonElement body)
        {
            GetColumnFamiliesResult result;
            try
            {
                result = _facade.GetColumnFamilyList();
            }
            catch (Exception ex)
            {
                result = new GetColumnFamiliesResult { Success = false, Message = ex.Message };
            }

            McpAuditLog.Write("GetColumnFamilies", null, result.Success, null, result.Message);
            return result;
        }
    }
}
