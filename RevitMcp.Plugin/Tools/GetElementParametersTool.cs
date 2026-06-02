using System;
using System.Text.Json;
using RevitMcp.Contracts.Dtos;
using RevitMcp.Plugin.Logging;
using RevitMcp.Plugin.Revit;

namespace RevitMcp.Plugin.Tools
{
    public class GetElementParametersTool
    {
        private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        private readonly RevitModelFacade _facade;
        public GetElementParametersTool(RevitModelFacade facade) => _facade = facade;

        public McpToolDefinition GetDefinition() => new McpToolDefinition
        {
            Name        = "GetElementParameters",
            Description = "Returns parameters of a Revit element. Optionally filter by parameter name. Omit parameterName to get all parameters.",
            InputSchema = new
            {
                type       = "object",
                required   = new[] { "elementId" },
                properties = new
                {
                    elementId     = new { type = "integer" },
                    parameterName = new { type = "string", description = "Optional: filter to a specific parameter name" }
                }
            }
        };

        public GetElementParametersResult Invoke(JsonElement body)
        {
            var args = body.Deserialize<GetElementParametersArgs>(JsonOpts) ?? new GetElementParametersArgs();
            GetElementParametersResult result;
            try   { result = _facade.GetElementParameters(args); }
            catch (Exception ex) { result = new GetElementParametersResult { Success = false, Message = ex.Message }; }
            McpAuditLog.Write("GetElementParameters", args, result.Success, null, result.Message);
            return result;
        }
    }
}
