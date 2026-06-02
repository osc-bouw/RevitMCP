using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Develoh.Bim.RVT25.Events;
using RevitMcp.Contracts.Dtos;
using System.Linq;
using System.Threading;

namespace RevitMcp.Plugin.Addin.ExternalEvents
{
    public class ListViews : ExternalEventBase, IExternalEventHandler
    {
        public static ListViewsResult? result { get; set; }
        public static readonly SemaphoreSlim Completed = new SemaphoreSlim(0, 1);

        public void Execute(UIApplication app)
        {
            var doc = app.ActiveUIDocument.Document;

            var views = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(v => !v.IsTemplate)
                .Select(v => new ViewDto
                {
                    Id        = (int)v.Id.Value,
                    Name      = v.Name,
                    ViewType  = v.ViewType.ToString(),
                    LevelName = v.GenLevel?.Name
                })
                .OrderBy(v => v.ViewType).ThenBy(v => v.Name)
                .ToList();

            result = new ListViewsResult
            {
                Success = true,
                Message = $"{views.Count} view(s) retrieved.",
                Views   = views
            };

            Completed.Release();
        }

        public override bool Register() { EventName = nameof(ListViews); return true; }
        public new string GetName() => nameof(ListViews);
    }
}
