using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Develoh.Bim.RVT25.Interfaces
{
    public interface UIAppConnectorInterface
    {
        UIControlledApplication UICApp { get; set; }
        UIApplication UIApp { get; set; }
    }
}
