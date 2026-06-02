using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Develoh.Bim.RVT25.Events;
using RevitMcp.Contracts.Dtos;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;


namespace RevitMcp.Plugin.Addin.ExternalEvents
{

    public class GetColumnFamilies : ExternalEventBase, IExternalEventHandler
    {

        static string _filename = string.Empty;
        static ElementId _categoryId = new ElementId(BuiltInCategory.OST_GenericModel);
        public const string TroubleshootingUrl = "http://truevis.com/troubleshoot-revit-mesh-import";

        /// <summary>
        /// Can be executed with .Raise() if the external event is registered first.
        /// </summary>
        public static ExternalEvent FamilyPlacerEvent;

        public static List<ColumnFamilyDto> result { get; set; }

        public static readonly SemaphoreSlim Completed = new SemaphoreSlim(0, 1);


        public void Execute(UIApplication app)
        {

            app.Application.FailuresProcessing += FaliureProcessor;


            UIApplication uiapp = app;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            FilteredElementCollector collector = new FilteredElementCollector(doc);

            var families = collector
                .OfCategory(BuiltInCategory.OST_StructuralColumns)
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                .Select(fs => fs.Family)
                .GroupBy(f => f.Id)
                .Select(g => g.First())
                .ToList();

            result = new List<ColumnFamilyDto>();
            result = families.Select(f => new ColumnFamilyDto
            {
                Name = f.Name,
                Id = f.Id.ToString()
            }).ToList();
            Completed.Release();

        }





        private void FaliureProcessor(object sender, FailuresProcessingEventArgs e)
        {
            FailuresAccessor fas = e.GetFailuresAccessor();

            List<FailureMessageAccessor> fma = fas.GetFailureMessages().ToList();

            foreach (FailureMessageAccessor fa in fma)
            {
                if (fa.GetFailureDefinitionId() == BuiltInFailures.OverlapFailures.DuplicateInstances)
                {
                    fas.DeleteWarning(fa);
                }
            }
        }


        public override bool Register()
        {
            EventName = nameof(GetColumnFamilies);
            FamilyPlacerEvent = ExternalEvent.Create(this);

            return true;
        }

        public static void Raise()
        {
            FamilyPlacerEvent.Raise();
        }
    }
}
