using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Develoh.Bim.RVT25.Events;
using RevitMcp.Contracts.Dtos;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace RevitMcp.Plugin.Addin.ExternalEvents
{
    public class GetModelSummary : ExternalEventBase, IExternalEventHandler
    {
        public static ModelSummaryResult result { get; set; }

        public static readonly SemaphoreSlim Completed = new SemaphoreSlim(0, 1);

        public void Execute(UIApplication app)
        {
            UIDocument uidoc = app.ActiveUIDocument;
            Document doc = uidoc.Document;

            var projectInfo = doc.ProjectInformation;

            var levelCount = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .GetElementCount();

            var categoriesToCount = new[]
            {
                BuiltInCategory.OST_StructuralColumns,
                BuiltInCategory.OST_StructuralFraming,
                BuiltInCategory.OST_Floors,
                BuiltInCategory.OST_Walls,
                BuiltInCategory.OST_Doors,
                BuiltInCategory.OST_Windows
            };

            var elementCounts = categoriesToCount.Select(cat => new ElementCountDto
            {
                Category = LabelUtils.GetLabelFor(cat),
                Count = new FilteredElementCollector(doc)
                    .OfCategory(cat)
                    .WhereElementIsNotElementType()
                    .GetElementCount()
            }).ToList();

            result = new ModelSummaryResult
            {
                Success       = true,
                ProjectName   = projectInfo.Name,
                ProjectNumber = projectInfo.Number,
                LevelCount    = levelCount,
                ElementCounts = elementCounts,
                Message       = "Model summary retrieved successfully."
            };

            Completed.Release();
        }

        public override bool Register()
        {
            EventName = nameof(GetModelSummary);
            return true;
        }

        public string GetName() => nameof(GetModelSummary);
    }
}
