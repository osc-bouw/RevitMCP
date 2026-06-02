using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Develoh.Bim.RVT25.Events;
using RevitMcp.Contracts.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace RevitMcp.Plugin.Addin.ExternalEvents
{
    public class PlaceColumn : ExternalEventBase, IExternalEventHandler
    {
        public static PlaceColumnArgs? args { get; set; }
        public static PlaceColumnResult? result { get; set; }
        public static readonly SemaphoreSlim Completed = new SemaphoreSlim(0, 1);

        public void Execute(UIApplication app)
        {
            var doc = app.ActiveUIDocument.Document;
            var input = args ?? new PlaceColumnArgs();

            try
            {
                if (input.FamilySymbolId == null)
                {
                    result = Failure("familySymbolId is required. Call ListFamilies and choose a symbol from Structural Columns or Architectural Columns.", GetColumnSymbolOptions(doc));
                    Completed.Release();
                    return;
                }

                var symbol = doc.GetElement(new ElementId(input.FamilySymbolId.Value)) as FamilySymbol;
                if (symbol == null || !IsColumnSymbol(symbol))
                {
                    result = Failure($"FamilySymbol {input.FamilySymbolId} was not found or is not a Structural Columns or Architectural Columns symbol.", GetColumnSymbolOptions(doc));
                    Completed.Release();
                    return;
                }

                if (input.Height <= 0)
                {
                    result = Failure("height must be greater than 0.", GetColumnSymbolOptions(doc));
                    Completed.Release();
                    return;
                }

                using var tx = new Transaction(doc, "MCP PlaceColumn");
                tx.Start();

                if (!symbol.IsActive)
                {
                    symbol.Activate();
                    doc.Regenerate();
                }

                var point = new XYZ(input.Base.X, input.Base.Y, input.Base.Z);
                var structuralType = symbol.Category?.Id.Value == (long)BuiltInCategory.OST_StructuralColumns
                    ? Autodesk.Revit.DB.Structure.StructuralType.Column
                    : Autodesk.Revit.DB.Structure.StructuralType.NonStructural;

                var instance = doc.Create.NewFamilyInstance(point, symbol, structuralType);
                SetHeight(instance, input.Height);

                tx.Commit();

                result = new PlaceColumnResult
                {
                    Success = true,
                    Guid = instance.UniqueId,
                    ObjectId = (int)instance.Id.Value,
                    Message = $"Placed column '{symbol.Family.Name}: {symbol.Name}'."
                };
            }
            catch (Exception ex)
            {
                result = new PlaceColumnResult { Success = false, Message = ex.Message };
            }

            Completed.Release();
        }

        private static bool IsColumnSymbol(FamilySymbol symbol)
        {
            var categoryId = symbol.Category?.Id.Value;
            return categoryId == (long)BuiltInCategory.OST_StructuralColumns ||
                   categoryId == (long)BuiltInCategory.OST_Columns;
        }

        private static void SetHeight(FamilyInstance instance, double height)
        {
            var parameter = instance.LookupParameter("Unconnected Height") ??
                            instance.get_Parameter(BuiltInParameter.FAMILY_HEIGHT_PARAM);

            if (parameter != null && !parameter.IsReadOnly)
                parameter.Set(height);
        }

        private static PlaceColumnResult Failure(string message, List<FamilySymbolOptionDto> options)
        {
            return new PlaceColumnResult
            {
                Success = false,
                Message = message,
                AvailableFamilySymbols = options
            };
        }

        private static List<FamilySymbolOptionDto> GetColumnSymbolOptions(Document doc)
        {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                .Where(IsColumnSymbol)
                .Select(symbol => new FamilySymbolOptionDto
                {
                    FamilyId = symbol.Family.Id.Value,
                    FamilyName = symbol.Family.Name,
                    SymbolId = symbol.Id.Value,
                    SymbolName = symbol.Name,
                    Category = symbol.Category?.Name
                })
                .OrderBy(symbol => symbol.Category)
                .ThenBy(symbol => symbol.FamilyName)
                .ThenBy(symbol => symbol.SymbolName)
                .ToList();
        }

        public override bool Register() { EventName = nameof(PlaceColumn); return true; }
        public new string GetName() => nameof(PlaceColumn);
    }
}
