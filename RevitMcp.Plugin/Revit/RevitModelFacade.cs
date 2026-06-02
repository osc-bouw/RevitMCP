using System;
using System.Collections.Generic;
using System.Threading;
using RevitMcp.Contracts.Dtos;
using EE = RevitMcp.Plugin.Addin.ExternalEvents;

namespace RevitMcp.Plugin.Revit
{
    public class RevitModelFacade
    {
        public RevitModelFacade() { }

        private static void DrainSemaphore(SemaphoreSlim sem)
        {
            while (sem.CurrentCount > 0) sem.Wait(0);
        }

        // ── Existing tools ───────────────────────────────────────────────────

        public PlaceBeamResult InsertBeam(PlaceBeamArgs args)
        {
            EE.PlaceBeam.args = args;
            EE.PlaceBeam.result = null;
            DrainSemaphore(EE.PlaceBeam.Completed);
            App.PlaceBeamEvent!.Raise();
            EE.PlaceBeam.Completed.Wait(TimeSpan.FromSeconds(15));
            return EE.PlaceBeam.result ?? new PlaceBeamResult { Success = false, Message = "Timeout waiting for Revit." };
        }

        public PlaceColumnResult InsertColumn(PlaceColumnArgs args)
        {
            EE.PlaceColumn.args = args;
            EE.PlaceColumn.result = null;
            DrainSemaphore(EE.PlaceColumn.Completed);
            App.PlaceColumnEvent!.Raise();
            EE.PlaceColumn.Completed.Wait(TimeSpan.FromSeconds(15));
            return EE.PlaceColumn.result ?? new PlaceColumnResult { Success = false, Message = "Timeout waiting for Revit." };
        }

        public GetColumnFamiliesResult GetColumnFamilyList()
        {
            EE.GetColumnFamilies.result = null!;
            DrainSemaphore(EE.GetColumnFamilies.Completed);
            App.GetColumnFamiliesEvent!.Raise();
            EE.GetColumnFamilies.Completed.Wait(TimeSpan.FromSeconds(10));

            if (EE.GetColumnFamilies.result == null)
                return new GetColumnFamiliesResult { Success = false, Families = new List<ColumnFamilyDto>(), Message = "Failed to retrieve column families from Revit." };

            return new GetColumnFamiliesResult { Success = true, Families = EE.GetColumnFamilies.result, Message = "Retrieved column families." };
        }

        public ModelSummaryResult GetModelSummaryInfo()
        {
            EE.GetModelSummary.result = null!;
            DrainSemaphore(EE.GetModelSummary.Completed);
            App.GetModelSummaryEvent!.Raise();
            EE.GetModelSummary.Completed.Wait(TimeSpan.FromSeconds(10));

            return EE.GetModelSummary.result ?? new ModelSummaryResult { Success = false, Message = "Failed to retrieve model summary from Revit." };
        }

        // ── New tools ────────────────────────────────────────────────────────

        public ListLevelsResult ListLevels()
        {
            EE.ListLevels.result = null;
            DrainSemaphore(EE.ListLevels.Completed);
            App.ListLevelsEvent!.Raise();
            EE.ListLevels.Completed.Wait(TimeSpan.FromSeconds(10));
            return EE.ListLevels.result ?? new ListLevelsResult { Success = false, Message = "Timeout waiting for Revit." };
        }

        public ListViewsResult ListViews()
        {
            EE.ListViews.result = null;
            DrainSemaphore(EE.ListViews.Completed);
            App.ListViewsEvent!.Raise();
            EE.ListViews.Completed.Wait(TimeSpan.FromSeconds(10));
            return EE.ListViews.result ?? new ListViewsResult { Success = false, Message = "Timeout waiting for Revit." };
        }

        public ListFamiliesResult ListFamilies()
        {
            EE.ListFamilies.result = null;
            DrainSemaphore(EE.ListFamilies.Completed);
            App.ListFamiliesEvent!.Raise();
            EE.ListFamilies.Completed.Wait(TimeSpan.FromSeconds(10));
            return EE.ListFamilies.result ?? new ListFamiliesResult { Success = false, Message = "Timeout waiting for Revit." };
        }

        public GetElementsResult GetElements(GetElementsArgs args)
        {
            EE.GetElements.args   = args;
            EE.GetElements.result = null;
            DrainSemaphore(EE.GetElements.Completed);
            App.GetElementsEvent!.Raise();
            EE.GetElements.Completed.Wait(TimeSpan.FromSeconds(15));
            return EE.GetElements.result ?? new GetElementsResult { Success = false, Message = "Timeout waiting for Revit." };
        }

        public GetElementParametersResult GetElementParameters(GetElementParametersArgs args)
        {
            EE.GetElementParameters.args   = args;
            EE.GetElementParameters.result = null;
            DrainSemaphore(EE.GetElementParameters.Completed);
            App.GetElementParametersEvent!.Raise();
            EE.GetElementParameters.Completed.Wait(TimeSpan.FromSeconds(10));
            return EE.GetElementParameters.result ?? new GetElementParametersResult { Success = false, Message = "Timeout waiting for Revit." };
        }

        public SetParameterValueResult SetParameterValue(SetParameterValueArgs args)
        {
            EE.SetParameterValue.args   = args;
            EE.SetParameterValue.result = null;
            DrainSemaphore(EE.SetParameterValue.Completed);
            App.SetParameterValueEvent!.Raise();
            EE.SetParameterValue.Completed.Wait(TimeSpan.FromSeconds(10));
            return EE.SetParameterValue.result ?? new SetParameterValueResult { Success = false, Message = "Timeout waiting for Revit." };
        }

        public BatchSetParametersResult BatchSetParameters(BatchSetParametersArgs args)
        {
            EE.BatchSetParameters.args   = args;
            EE.BatchSetParameters.result = null;
            DrainSemaphore(EE.BatchSetParameters.Completed);
            App.BatchSetParametersEvent!.Raise();
            EE.BatchSetParameters.Completed.Wait(TimeSpan.FromSeconds(30));
            return EE.BatchSetParameters.result ?? new BatchSetParametersResult { Success = false, Message = "Timeout waiting for Revit." };
        }

        public GetWarningsResult GetWarnings()
        {
            EE.GetWarnings.result = null;
            DrainSemaphore(EE.GetWarnings.Completed);
            App.GetWarningsEvent!.Raise();
            EE.GetWarnings.Completed.Wait(TimeSpan.FromSeconds(10));
            return EE.GetWarnings.result ?? new GetWarningsResult { Success = false, Message = "Timeout waiting for Revit." };
        }

        public CreateViewSnapshotResult CreateViewSnapshot(CreateViewSnapshotArgs args)
        {
            EE.CreateViewSnapshot.args   = args;
            EE.CreateViewSnapshot.result = null;
            DrainSemaphore(EE.CreateViewSnapshot.Completed);
            App.CreateViewSnapshotEvent!.Raise();
            EE.CreateViewSnapshot.Completed.Wait(TimeSpan.FromSeconds(60));
            return EE.CreateViewSnapshot.result ?? new CreateViewSnapshotResult { Success = false, Message = "Timeout waiting for Revit." };
        }
    }
}
