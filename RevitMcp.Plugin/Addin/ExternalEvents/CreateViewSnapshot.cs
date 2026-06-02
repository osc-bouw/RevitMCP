using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Develoh.Bim.RVT25.Events;
using RevitMcp.Contracts.Dtos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace RevitMcp.Plugin.Addin.ExternalEvents
{
    public class CreateViewSnapshot : ExternalEventBase, IExternalEventHandler
    {
        public static CreateViewSnapshotArgs?   args   { get; set; }
        public static CreateViewSnapshotResult? result { get; set; }
        public static readonly SemaphoreSlim Completed = new SemaphoreSlim(0, 1);

        public void Execute(UIApplication app)
        {
            var doc     = app.ActiveUIDocument.Document;
            var input   = args ?? new CreateViewSnapshotArgs();
            var tempDir = Path.Combine(Path.GetTempPath(), "revitmcp_" + Guid.NewGuid().ToString("N"));

            try
            {
                var view = doc.GetElement(new ElementId((long)input.ViewId)) as View;
                if (view == null)
                {
                    result = new CreateViewSnapshotResult { Success = false, Message = $"View {input.ViewId} not found." };
                    Completed.Release();
                    return;
                }

                Directory.CreateDirectory(tempDir);

                var exportOpts = new ImageExportOptions
                {
                    FilePath              = Path.Combine(tempDir, "snapshot"),
                    FitDirection          = FitDirectionType.Horizontal,
                    PixelSize             = input.WidthPx > 0 ? input.WidthPx : 1920,
                    ImageResolution       = ImageResolution.DPI_150,
                    ExportRange           = ExportRange.VisibleRegionOfCurrentView,
                    HLRandWFViewsFileType = ImageFileType.JPEGMedium
                };
                exportOpts.SetViewsAndSheets(new List<ElementId> { view.Id });

                doc.ExportImage(exportOpts);

                var pngFiles = Directory.GetFiles(tempDir, "*.jpg");
                if (pngFiles.Length == 0)
                {
                    result = new CreateViewSnapshotResult { Success = false, Message = "Export produced no PNG file." };
                    Completed.Release();
                    return;
                }

                var bytes = File.ReadAllBytes(pngFiles[0]);
                result = new CreateViewSnapshotResult
                {
                    Success     = true,
                    Message     = $"Snapshot of view '{view.Name}' captured ({bytes.Length} bytes).",
                    Base64Image = Convert.ToBase64String(bytes),
                    MimeType    = "image/jpg"
                };
            }
            catch (Exception ex)
            {
                result = new CreateViewSnapshotResult { Success = false, Message = ex.Message };
            }
            finally
            {
                try { if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true); } catch { }
            }

            Completed.Release();
        }

        public override bool Register() { EventName = nameof(CreateViewSnapshot); return true; }
        public new string GetName() => nameof(CreateViewSnapshot);
    }
}
