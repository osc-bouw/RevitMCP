using System;
using System.Text.Json;
using RevitMcp.Contracts.Dtos;
using RevitMcp.Plugin.Logging;
using RevitMcp.Plugin.Revit;

namespace RevitMcp.Plugin.Tools
{
    public class PlaceBeamTool
    {
        private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly RevitModelFacade _facade;

        public PlaceBeamTool(RevitModelFacade facade) => _facade = facade;

        public McpToolDefinition GetDefinition() => new McpToolDefinition
        {
            Name        = "PlaceBeam",
            Description = "Places a beam in Revit between two coordinates.",
            InputSchema = new
            {
                type     = "object",
                required = new[] { "start", "end", "profile" },
                properties = new
                {
                    start    = new { type = "object", properties = new { x = new { type = "number" }, y = new { type = "number" }, z = new { type = "number" } } },
                    end      = new { type = "object", properties = new { x = new { type = "number" }, y = new { type = "number" }, z = new { type = "number" } } },
                    profile  = new { type = "string" },
                    material = new { type = "string" },
                    @class   = new { type = "integer" },
                    name     = new { type = "string" }
                }
            }
        };

        public PlaceBeamResult Invoke(JsonElement body)
        {
            var args = body.Deserialize<PlaceBeamArgs>(JsonOpts) ??
                       throw new ArgumentException("Invalid PlaceBeam arguments.");

            Validate(args);

            PlaceBeamResult result;
            try
            {
                result = _facade.InsertBeam(args);
            }
            catch (Exception ex)
            {
                result = new PlaceBeamResult { Success = false, Message = ex.Message };
            }

            McpAuditLog.Write("PlaceBeam", args, result.Success, result.Guid, result.Message);
            return result;
        }

        private static void Validate(PlaceBeamArgs args)
        {
            if (string.IsNullOrWhiteSpace(args.Profile))
                throw new ArgumentException("profile is required and must not be empty.");

            var s = args.Start;
            var e = args.End;
            if (s.X == e.X && s.Y == e.Y && s.Z == e.Z)
                throw new ArgumentException("start and end must be different points.");
        }
    }
}
