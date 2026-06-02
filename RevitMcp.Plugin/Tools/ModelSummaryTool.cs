using System;
using System.Text.Json;
using RevitMcp.Contracts.Dtos;
using RevitMcp.Plugin.Logging;
using RevitMcp.Plugin.Revit;

namespace RevitMcp.Plugin.Tools
{
    public class ModelSummaryTool
    {
        private readonly RevitModelFacade _facade;

        public ModelSummaryTool(RevitModelFacade facade) => _facade = facade;

        public McpToolDefinition GetDefinition() => new McpToolDefinition
        {
            Name        = "ModelSummary",
            Description = "Returns a summary of the current Revit model: project name, project number, level count, and element counts per structural category.",
            InputSchema = new
            {
                type       = "object",
                properties = new { }
            }
        };

        public ModelSummaryResult Invoke(JsonElement body)
        {
            ModelSummaryResult result;
            try
            {
                result = _facade.GetModelSummaryInfo();
            }
            catch (Exception ex)
            {
                result = new ModelSummaryResult { Success = false, Message = ex.Message };
            }

            McpAuditLog.Write("ModelSummary", null, result.Success, null, result.Message);
            return result;
        }
    }
}
