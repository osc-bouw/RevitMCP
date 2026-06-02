using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Develoh.Bim.RVT25.Events;
using RevitMcp.Contracts.Dtos;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace RevitMcp.Plugin.Addin.ExternalEvents
{
    public class BatchSetParameters : ExternalEventBase, IExternalEventHandler
    {
        public static BatchSetParametersArgs?   args   { get; set; }
        public static BatchSetParametersResult? result { get; set; }
        public static readonly SemaphoreSlim Completed = new SemaphoreSlim(0, 1);

        public void Execute(UIApplication app)
        {
            var doc   = app.ActiveUIDocument.Document;
            var input = args ?? new BatchSetParametersArgs();
            var results = new List<BatchResultDto>();

            try
            {
                using var tx = new Transaction(doc, "MCP BatchSetParameters");
                tx.Start();

                foreach (var update in input.Updates)
                {
                    try
                    {
                        var element = doc.GetElement(new ElementId((long)update.ElementId));
                        if (element == null)
                            throw new Exception($"Element {update.ElementId} not found.");

                        var param = element.LookupParameter(update.ParameterName);
                        if (param == null)
                            throw new Exception($"Parameter '{update.ParameterName}' not found.");
                        if (param.IsReadOnly)
                            throw new Exception($"Parameter '{update.ParameterName}' is read-only.");

                        switch (param.StorageType)
                        {
                            case StorageType.String:
                                param.Set(update.Value);
                                break;
                            case StorageType.Integer:
                                param.Set(int.Parse(update.Value, CultureInfo.InvariantCulture));
                                break;
                            case StorageType.Double:
                                param.Set(double.Parse(update.Value, CultureInfo.InvariantCulture));
                                break;
                            case StorageType.ElementId:
                                param.Set(new ElementId(long.Parse(update.Value, CultureInfo.InvariantCulture)));
                                break;
                            default:
                                throw new Exception($"Unsupported StorageType: {param.StorageType}.");
                        }

                        results.Add(new BatchResultDto { ElementId = update.ElementId, ParameterName = update.ParameterName, Success = true });
                    }
                    catch (Exception ex)
                    {
                        results.Add(new BatchResultDto { ElementId = update.ElementId, ParameterName = update.ParameterName, Success = false, Error = ex.Message });
                    }
                }

                tx.Commit();
                result = new BatchSetParametersResult { Success = true, Message = $"{results.Count} update(s) processed.", Results = results };
            }
            catch (Exception ex)
            {
                result = new BatchSetParametersResult { Success = false, Message = ex.Message, Results = results };
            }

            Completed.Release();
        }

        public override bool Register() { EventName = nameof(BatchSetParameters); return true; }
        public new string GetName() => nameof(BatchSetParameters);
    }
}
