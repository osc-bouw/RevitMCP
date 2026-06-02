using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Develoh.Bim.RVT25.Events;
using RevitMcp.Contracts.Dtos;
using System.Linq;
using System.Threading;

namespace RevitMcp.Plugin.Addin.ExternalEvents
{
    public class ListFamilies : ExternalEventBase, IExternalEventHandler
    {
        public static ListFamiliesResult? result { get; set; }
        public static readonly SemaphoreSlim Completed = new SemaphoreSlim(0, 1);

        public void Execute(UIApplication app)
        {
            var doc = app.ActiveUIDocument.Document;

            var families = new FilteredElementCollector(doc)
                .OfClass(typeof(Family))
                .Cast<Family>()
                .Select(f => new FamilyDto
                {
                    Id        = (int)f.Id.Value,
                    Name      = f.Name,
                    Category  = f.FamilyCategory?.Name,
                    TypeCount = f.GetFamilySymbolIds().Count
                })
                .OrderBy(f => f.Category).ThenBy(f => f.Name)
                .ToList();

            result = new ListFamiliesResult
            {
                Success  = true,
                Message  = $"{families.Count} famil(ies) retrieved.",
                Families = families
            };

            Completed.Release();
        }

        public override bool Register() { EventName = nameof(ListFamilies); return true; }
        public new string GetName() => nameof(ListFamilies);
    }
}
