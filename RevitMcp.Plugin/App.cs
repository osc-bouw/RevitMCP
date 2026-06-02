using Autodesk.Revit.UI;
using Develoh.Bim.RVT25.Extensions;
using Develoh.BIM.RVT25.Extensions;
using RevitMcp.Plugin.Addin.Commands;
using RevitMcp.Plugin.Addin.ExternalEvents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AW = Autodesk.Windows;


namespace RevitMcp.Plugin
{
    public class App : IExternalApplication
    {
        public static Dictionary<string, ExternalEvent> allEvents = new Dictionary<string, ExternalEvent>();

        public UIControlledApplication? UICapp { get; set; }
        public static UIApplication? uiapp { get; set; }

        // Existing events
        public static ExternalEvent? PlaceBeamEvent;
        public static ExternalEvent? PlaceColumnEvent;
        public static ExternalEvent? GetColumnFamiliesEvent;
        public static ExternalEvent? GetModelSummaryEvent;

        // New events
        public static ExternalEvent? ListLevelsEvent;
        public static ExternalEvent? ListViewsEvent;
        public static ExternalEvent? ListFamiliesEvent;
        public static ExternalEvent? GetElementsEvent;
        public static ExternalEvent? GetElementParametersEvent;
        public static ExternalEvent? SetParameterValueEvent;
        public static ExternalEvent? BatchSetParametersEvent;
        public static ExternalEvent? GetWarningsEvent;
        public static ExternalEvent? CreateViewSnapshotEvent;

        public Result OnStartup(UIControlledApplication application)
        {
            UICapp = application;
            RegisterRibbonPanel(application);
            RegisterExternalEvent();
            return Result.Succeeded;
        }

        private void RegisterRibbonPanel(UIControlledApplication application)
        {
            var tabName = "Develoh Tools";
            application.CreateRibbonTab(tabName);
            var tools = application.CreateRibbonPanel(tabName, "AI");
            var dict = new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/RevitMcp.Plugin;component/Resources/Icons.xaml")
            };
            var svg = (Geometry)dict["app.LogoIcon"];
            tools.AddRibbonButton("Start MCP server", "Create a MCP server you can use with claude code or gemini", nameof(McpCommand), "cmd_start_mcp", svg.ToBitmapImage(), Assembly.GetExecutingAssembly().Location);
        }

        private void RegisterExternalEvent()
        {
            PlaceBeamEvent            = ExternalEvent.Create(new PlaceBeam());
            PlaceColumnEvent          = ExternalEvent.Create(new PlaceColumn());
            GetColumnFamiliesEvent    = ExternalEvent.Create(new GetColumnFamilies());
            GetModelSummaryEvent      = ExternalEvent.Create(new GetModelSummary());

            ListLevelsEvent           = ExternalEvent.Create(new ListLevels());
            ListViewsEvent            = ExternalEvent.Create(new ListViews());
            ListFamiliesEvent         = ExternalEvent.Create(new ListFamilies());
            GetElementsEvent          = ExternalEvent.Create(new GetElements());
            GetElementParametersEvent = ExternalEvent.Create(new GetElementParameters());
            SetParameterValueEvent    = ExternalEvent.Create(new SetParameterValue());
            BatchSetParametersEvent   = ExternalEvent.Create(new BatchSetParameters());
            GetWarningsEvent          = ExternalEvent.Create(new GetWarnings());
            CreateViewSnapshotEvent   = ExternalEvent.Create(new CreateViewSnapshot());
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
