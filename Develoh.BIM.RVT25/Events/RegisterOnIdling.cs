using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Develoh.Bim.RVT25.Events
{
    public class RegisterOnIdling
    {
        private OnIdlingMonitor onIdlingMonitor;
 

        public RegisterOnIdling()
        {
            onIdlingMonitor = new OnIdlingMonitor();
        }

        public void Register()
        {
          
            onIdlingMonitor.SelectionChanged += OnIdlingMonitor_SelectionChanged;

        }

        private void OnIdlingMonitor_SelectionChanged(object sender, EventArgs e)
        {
    
              var selectedElements = (sender as OnIdlingMonitor).SelectedElementIds;

        }
    }
}
