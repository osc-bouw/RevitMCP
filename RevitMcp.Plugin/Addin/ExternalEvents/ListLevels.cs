using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Develoh.Bim.RVT25.Events;
using RevitMcp.Contracts.Dtos;
using System.Linq;
using System.Threading;

namespace RevitMcp.Plugin.Addin.ExternalEvents
{
    public class ListLevels : ExternalEventBase, IExternalEventHandler
    {
        public static ListLevelsResult? result { get; set; }
        public static readonly SemaphoreSlim Completed = new SemaphoreSlim(0, 1);

        public void Execute(UIApplication app)
        {
            var doc = app.ActiveUIDocument.Document;

            var levels = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .OrderBy(l => l.Elevation)
                .Select(l => new LevelDto
                {
                    Id          = (int)l.Id.Value,
                    Name        = l.Name,
                    ElevationMm = UnitUtils.ConvertFromInternalUnits(l.Elevation, UnitTypeId.Millimeters)
                })
                .ToList();

            result = new ListLevelsResult
            {
                Success = true,
                Message = $"{levels.Count} level(s) retrieved.",
                Levels  = levels
            };

            Completed.Release();
        }

        public override bool Register() { EventName = nameof(ListLevels); return true; }
        public new string GetName() => nameof(ListLevels);
    }
}
