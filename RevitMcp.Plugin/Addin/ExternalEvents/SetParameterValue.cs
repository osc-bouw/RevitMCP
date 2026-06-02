using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Develoh.Bim.RVT25.Events;
using RevitMcp.Contracts.Dtos;
using System;
using System.Globalization;
using System.Threading;

namespace RevitMcp.Plugin.Addin.ExternalEvents
{
    public class SetParameterValue : ExternalEventBase, IExternalEventHandler
    {
        public static SetParameterValueArgs?   args   { get; set; }
        public static SetParameterValueResult? result { get; set; }
        public static readonly SemaphoreSlim Completed = new SemaphoreSlim(0, 1);

        public void Execute(UIApplication app)
        {
            var doc   = app.ActiveUIDocument.Document;
            var input = args ?? new SetParameterValueArgs();

            try
            {
                var element = doc.GetElement(new ElementId((long)input.ElementId));
                if (element == null)
                {
                    result = new SetParameterValueResult { Success = false, Message = $"Element {input.ElementId} not found." };
                    Completed.Release();
                    return;
                }

                var param = element.LookupParameter(input.ParameterName);
                if (param == null)
                {
                    result = new SetParameterValueResult { Success = false, Message = $"Parameter '{input.ParameterName}' not found on element {input.ElementId}." };
                    Completed.Release();
                    return;
                }

                if (param.IsReadOnly)
                {
                    result = new SetParameterValueResult { Success = false, Message = $"Parameter '{input.ParameterName}' is read-only." };
                    Completed.Release();
                    return;
                }

                using var tx = new Transaction(doc, "MCP SetParameterValue");
                tx.Start();

                switch (param.StorageType)
                {
                    case StorageType.String:
                        param.Set(input.Value);
                        break;
                    case StorageType.Integer:
                        param.Set(int.Parse(input.Value, CultureInfo.InvariantCulture));
                        break;
                    case StorageType.Double:
                        param.Set(double.Parse(input.Value, CultureInfo.InvariantCulture));
                        break;
                    case StorageType.ElementId:
                        param.Set(new ElementId(long.Parse(input.Value, CultureInfo.InvariantCulture)));
                        break;
                    default:
                        tx.RollBack();
                        result = new SetParameterValueResult { Success = false, Message = $"Unsupported StorageType: {param.StorageType}." };
                        Completed.Release();
                        return;
                }

                tx.Commit();
                result = new SetParameterValueResult { Success = true, Message = $"Parameter '{input.ParameterName}' set to '{input.Value}'." };
            }
            catch (Exception ex)
            {
                result = new SetParameterValueResult { Success = false, Message = ex.Message };
            }

            Completed.Release();
        }

        public override bool Register() { EventName = nameof(SetParameterValue); return true; }
        public new string GetName() => nameof(SetParameterValue);
    }
}
