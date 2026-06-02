using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Develoh.Bim.RVT25.Events
{
    public class OnIdlingMonitor
    {

        UIApplication UIApplication { get; set; }

        private List<long> _lastSelIds;



        public event EventHandler SelectionChanged;



        public List<ElementId> SelectedElementIds
        {
            get;
            set;
        }


        public void OnIdlingEvent(object sender, IdlingEventArgs e)
        {
            if (UIApplication.ActiveUIDocument != null)
            {
                try
                {
                    ICollection<ElementId> latestSelection = UIApplication.ActiveUIDocument.Selection.GetElementIds();

                    if (latestSelection.Count == 0)
                    {
                        if (SelectedElementIds != null && SelectedElementIds.Count > 0)

                        {
                            HandleSelectionChange(latestSelection);
                        }
                    }
                    else
                    {
                        if (SelectedElementIds == null)
                        {
                            HandleSelectionChange(latestSelection);
                        }
                        else
                        {
                            if (SelectedElementIds.Count != latestSelection.Count)
                            {
                                HandleSelectionChange(latestSelection);
                            }
                            else
                            {
                                if (SelectionHasChanged(latestSelection))
                                {
                                    HandleSelectionChange(latestSelection);
                                }
                            }
                        }
                    }

                }
                catch
                {

                }
            }
        }

        private void HandleSelectionChange(IEnumerable<ElementId> elementIds)
        {
            SelectedElementIds = new List<ElementId>();
            _lastSelIds = new List<long>();

            foreach (var elementId in elementIds)
            {
                SelectedElementIds.Add(elementId);
                _lastSelIds.Add(elementId.Value);
            }

            InvokeSelectionChangedEvent();
        }


        private void InvokeSelectionChangedEvent()
        {
            SelectionChanged?.Invoke(this, new EventArgs());
        }


        private bool SelectionHasChanged(IEnumerable<ElementId> elementIds)
        {
            var i = 0;

            foreach (var elementId in elementIds)
            {
                if (_lastSelIds[i] != elementId.Value)
                {
                    return true;
                }

                ++i;
            }

            return false;
        }


    }
}
