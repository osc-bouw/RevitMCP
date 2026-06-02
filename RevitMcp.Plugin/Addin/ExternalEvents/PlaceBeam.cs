using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Develoh.Bim.RVT25.Events;
using RevitMcp.Contracts.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace RevitMcp.Plugin.Addin.ExternalEvents
{
    public class PlaceBeam : ExternalEventBase, IExternalEventHandler
    {
        public static PlaceBeamArgs? args { get; set; }
        public static PlaceBeamResult? result { get; set; }
        public static readonly SemaphoreSlim Completed = new SemaphoreSlim(0, 1);

        public void Execute(UIApplication app)
        {
            var doc = app.ActiveUIDocument.Document;
            var input = args ?? new PlaceBeamArgs();

            try
            {
                if (input.FamilySymbolId == null)
                {
                    result = Failure("familySymbolId is required. Call ListFamilies and choose a symbol from Structural Framing.", GetBeamSymbolOptions(doc));
                    Completed.Release();
                    return;
                }

                var symbol = doc.GetElement(new ElementId(input.FamilySymbolId.Value)) as FamilySymbol;
                if (symbol == null || !IsBeamSymbol(symbol))
                {
                    result = Failure($"FamilySymbol {input.FamilySymbolId} was not found or is not a Structural Framing symbol.", GetBeamSymbolOptions(doc));
                    Completed.Release();
                    return;
                }

                var start = new XYZ(input.Start.X, input.Start.Y, input.Start.Z);
                var end = new XYZ(input.End.X, input.End.Y, input.End.Z);
                if (start.IsAlmostEqualTo(end))
                {
                    result = Failure("start and end must be different coordinates.", GetBeamSymbolOptions(doc));
                    Completed.Release();
                    return;
                }

                var level = FindNearestLevel(doc, start.Z);
                if (level == null)
                {
                    result = Failure("No level was found in the model. A level is required to place a beam.", GetBeamSymbolOptions(doc));
                    Completed.Release();
                    return;
                }

                using var tx = new Transaction(doc, "MCP PlaceBeam");
                tx.Start();

                if (!symbol.IsActive)
                {
                    symbol.Activate();
                    doc.Regenerate();
                }

                var line = Line.CreateBound(start, end);
                var instance = doc.Create.NewFamilyInstance(line, symbol, level, StructuralType.Beam);

                tx.Commit();

                result = new PlaceBeamResult
                {
                    Success = true,
                    Guid = instance.UniqueId,
                    ObjectId = (int)instance.Id.Value,
                    Message = $"Placed beam '{symbol.Family.Name}: {symbol.Name}' on level '{level.Name}'."
                };
            }
            catch (Exception ex)
            {
                result = new PlaceBeamResult { Success = false, Message = ex.Message };
            }

            Completed.Release();
        }

        private static bool IsBeamSymbol(FamilySymbol symbol)
        {
            return symbol.Category?.Id.Value == (long)BuiltInCategory.OST_StructuralFraming;
        }

        private static Level? FindNearestLevel(Document doc, double elevation)
        {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .OrderBy(level => Math.Abs(level.Elevation - elevation))
                .FirstOrDefault();
        }

        private static PlaceBeamResult Failure(string message, List<FamilySymbolOptionDto> options)
        {
            return new PlaceBeamResult
            {
                Success = false,
                Message = message,
                AvailableFamilySymbols = options
            };
        }

        private static List<FamilySymbolOptionDto> GetBeamSymbolOptions(Document doc)
        {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                .Where(IsBeamSymbol)
                .Select(symbol => new FamilySymbolOptionDto
                {
                    FamilyId = symbol.Family.Id.Value,
                    FamilyName = symbol.Family.Name,
                    SymbolId = symbol.Id.Value,
                    SymbolName = symbol.Name,
                    Category = symbol.Category?.Name
                })
                .OrderBy(symbol => symbol.FamilyName)
                .ThenBy(symbol => symbol.SymbolName)
                .ToList();
        }

        public override bool Register() { EventName = nameof(PlaceBeam); return true; }
        public new string GetName() => nameof(PlaceBeam);
    }
}
