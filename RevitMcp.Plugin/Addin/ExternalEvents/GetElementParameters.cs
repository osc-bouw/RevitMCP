using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Develoh.Bim.RVT25.Events;
using RevitMcp.Contracts.Dtos;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace RevitMcp.Plugin.Addin.ExternalEvents
{
    public class GetElementParameters : ExternalEventBase, IExternalEventHandler
    {
        public static GetElementParametersArgs?   args   { get; set; }
        public static GetElementParametersResult? result { get; set; }
        public static readonly SemaphoreSlim Completed = new SemaphoreSlim(0, 1);

        public void Execute(UIApplication app)
        {
            var doc   = app.ActiveUIDocument.Document;
            var input = args ?? new GetElementParametersArgs();

            try
            {
                var element = doc.GetElement(new ElementId((long)input.ElementId));
                if (element == null)
                {
                    result = new GetElementParametersResult { Success = false, Message = $"Element {input.ElementId} not found." };
                    Completed.Release();
                    return;
                }

                var parameters = new List<ParameterDto>();
                foreach (Parameter p in element.Parameters)
                {
                    if (!string.IsNullOrWhiteSpace(input.ParameterName) &&
                        !p.Definition.Name.Equals(input.ParameterName, StringComparison.OrdinalIgnoreCase))
                        continue;

                    string? value = p.StorageType switch
                    {
                        StorageType.String    => p.AsString(),
                        StorageType.Integer   => p.AsInteger().ToString(),
                        StorageType.Double    => p.AsDouble().ToString(CultureInfo.InvariantCulture),
                        StorageType.ElementId => p.AsElementId()?.Value.ToString(),
                        _                     => null
                    };

                    string groupName = "";
                    try
                    {
                        if (p.Definition is InternalDefinition internalDef)
                            groupName = internalDef.GetGroupTypeId()?.TypeId ?? "";
                    }
                    catch { }

                    parameters.Add(new ParameterDto
                    {
                        Name        = p.Definition.Name,
                        StorageType = p.StorageType.ToString(),
                        Value       = value,
                        IsReadOnly  = p.IsReadOnly,
                        GroupName   = groupName
                    });
                }

                result = new GetElementParametersResult
                {
                    Success    = true,
                    Message    = $"{parameters.Count} parameter(s) retrieved.",
                    Parameters = parameters.OrderBy(p => p.GroupName).ThenBy(p => p.Name).ToList()
                };
            }
            catch (Exception ex)
            {
                result = new GetElementParametersResult { Success = false, Message = ex.Message };
            }

            Completed.Release();
        }

        public override bool Register() { EventName = nameof(GetElementParameters); return true; }
        public new string GetName() => nameof(GetElementParameters);
    }
}
