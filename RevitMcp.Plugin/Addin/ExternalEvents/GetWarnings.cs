using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Develoh.Bim.RVT25.Events;
using RevitMcp.Contracts.Dtos;
using System;
using System.Linq;
using System.Threading;

namespace RevitMcp.Plugin.Addin.ExternalEvents
{
    public class GetWarnings : ExternalEventBase, IExternalEventHandler
    {
        public static GetWarningsResult? result { get; set; }
        public static readonly SemaphoreSlim Completed = new SemaphoreSlim(0, 1);

        public void Execute(UIApplication app)
        {
            var doc = app.ActiveUIDocument.Document;

            try
            {
                var warnings = doc.GetWarnings()
                    .Select(w => new WarningDto
                    {
                        Description        = w.GetDescriptionText(),
                        FailureId          = w.GetFailureDefinitionId().Guid.ToString(),
                        AffectedElementIds = w.GetFailingElements().Select(id => (int)id.Value).ToList()
                    })
                    .ToList();

                result = new GetWarningsResult
                {
                    Success  = true,
                    Message  = $"{warnings.Count} warning(s) found.",
                    Warnings = warnings
                };
            }
            catch (Exception ex)
            {
                result = new GetWarningsResult { Success = false, Message = ex.Message };
            }

            Completed.Release();
        }

        public override bool Register() { EventName = nameof(GetWarnings); return true; }
        public new string GetName() => nameof(GetWarnings);
    }
}
