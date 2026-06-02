using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitMcp.Plugin.Addin.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public partial class McpCommand : IExternalCommand
    {



        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                new MainWindow().Show();
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }

            return Result.Succeeded;

        }

    }
}