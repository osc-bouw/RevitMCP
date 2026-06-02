using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Develoh.Bim.RVT25.Events;
using RevitMcp.Contracts.Dtos;
using System;
using System.Linq;
using System.Threading;

namespace RevitMcp.Plugin.Addin.ExternalEvents
{
    public class GetElements : ExternalEventBase, IExternalEventHandler
    {
        public static GetElementsArgs?   args   { get; set; }
        public static GetElementsResult? result { get; set; }
        public static readonly SemaphoreSlim Completed = new SemaphoreSlim(0, 1);

        public void Execute(UIApplication app)
        {
            var doc   = app.ActiveUIDocument.Document;
            var input = args ?? new GetElementsArgs();

            try
            {
                var collector = new FilteredElementCollector(doc);

                if (!string.IsNullOrWhiteSpace(input.Category) &&
                    Enum.TryParse<BuiltInCategory>(input.Category, out var bic))
                    collector = collector.OfCategory(bic);

                if (!string.IsNullOrWhiteSpace(input.LevelName))
                {
                    var level = new FilteredElementCollector(doc)
                        .OfClass(typeof(Level))
                        .Cast<Level>()
                        .FirstOrDefault(l => l.Name == input.LevelName);
                    if (level != null)
                        collector = collector.WherePasses(new ElementLevelFilter(level.Id));
                }

                var limit = input.Limit > 0 ? input.Limit : 100;
                var elements = collector
                    .WhereElementIsNotElementType()
                    .Cast<Element>()
                    .Where(e => string.IsNullOrWhiteSpace(input.FamilyName) ||
                                (e is FamilyInstance fi && fi.Symbol?.Family?.Name == input.FamilyName))
                    .Take(limit)
                    .Select(e =>
                    {
                        var levelName = (e.LevelId != null && e.LevelId != ElementId.InvalidElementId)
                            ? (doc.GetElement(e.LevelId) as Level)?.Name
                            : null;
                        var familyName = (e is FamilyInstance fi2) ? fi2.Symbol?.Family?.Name : null;
                        return new ElementSummaryDto
                        {
                            Id         = (int)e.Id.Value,
                            Name       = e.Name,
                            Category   = e.Category?.Name,
                            LevelName  = levelName,
                            FamilyName = familyName
                        };
                    })
                    .ToList();

                result = new GetElementsResult
                {
                    Success  = true,
                    Message  = $"{elements.Count} element(s) retrieved.",
                    Elements = elements
                };
            }
            catch (Exception ex)
            {
                result = new GetElementsResult { Success = false, Message = ex.Message };
            }

            Completed.Release();
        }

        public override bool Register() { EventName = nameof(GetElements); return true; }
        public new string GetName() => nameof(GetElements);
    }
}
