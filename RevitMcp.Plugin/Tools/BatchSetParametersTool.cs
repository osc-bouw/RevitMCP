using System;
using System.Text.Json;
using RevitMcp.Contracts.Dtos;
using RevitMcp.Plugin.Logging;
using RevitMcp.Plugin.Revit;

namespace RevitMcp.Plugin.Tools
{
    public class BatchSetParametersTool
    {
        private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        private readonly RevitModelFacade _facade;
        public BatchSetParametersTool(RevitModelFacade facade) => _facade = facade;

        public McpToolDefinition GetDefinition() => new McpToolDefinition
        {
            Name        = "BatchSetParameters",
            Description = "Sets multiple parameter values across one or more elements in a single Transaction. Per-update errors are captured without rolling back the entire batch.",
            InputSchema = new
            {
                type       = "object",
                required   = new[] { "updates" },
                properties = new
                {
                    updates = new
                    {
                        type  = "array",
                        items = new
                        {
                            type       = "object",
                            required   = new[] { "elementId", "parameterName", "value" },
                            properties = new
                            {
                                elementId     = new { type = "integer" },
                                parameterName = new { type = "string" },
                                value         = new { type = "string" }
                            }
                        }
                    }
                }
            }
        };

        public BatchSetParametersResult Invoke(JsonElement body)
        {
            var args = body.Deserialize<BatchSetParametersArgs>(JsonOpts) ?? new BatchSetParametersArgs();
            BatchSetParametersResult result;
            try   { result = _facade.BatchSetParameters(args); }
            catch (Exception ex) { result = new BatchSetParametersResult { Success = false, Message = ex.Message }; }
            McpAuditLog.Write("BatchSetParameters", args, result.Success, null, result.Message);
            return result;
        }
    }
}
