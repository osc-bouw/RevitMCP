using System;
using System.Text.Json;
using RevitMcp.Contracts.Dtos;
using RevitMcp.Plugin.Logging;
using RevitMcp.Plugin.Revit;

namespace RevitMcp.Plugin.Tools
{
    public class SetParameterValueTool
    {
        private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        private readonly RevitModelFacade _facade;
        public SetParameterValueTool(RevitModelFacade facade) => _facade = facade;

        public McpToolDefinition GetDefinition() => new McpToolDefinition
        {
            Name        = "SetParameterValue",
            Description = "Sets a single parameter value on a Revit element. Doubles are in Revit internal units (feet for lengths). Use a Transaction internally.",
            InputSchema = new
            {
                type       = "object",
                required   = new[] { "elementId", "parameterName", "value" },
                properties = new
                {
                    elementId     = new { type = "integer" },
                    parameterName = new { type = "string" },
                    value         = new { type = "string", description = "Value as string; converted to the parameter's storage type" }
                }
            }
        };

        public SetParameterValueResult Invoke(JsonElement body)
        {
            var args = body.Deserialize<SetParameterValueArgs>(JsonOpts) ?? new SetParameterValueArgs();
            SetParameterValueResult result;
            try   { result = _facade.SetParameterValue(args); }
            catch (Exception ex) { result = new SetParameterValueResult { Success = false, Message = ex.Message }; }
            McpAuditLog.Write("SetParameterValue", args, result.Success, null, result.Message);
            return result;
        }
    }
}
