using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Develoh.Bim.RVT25.Events
{
    public abstract class ExternalEventBase
    {
        public string EventName { get; set; } = "EventRegisterHandler";
        /// <summary>
        /// Implement the Register event for the External Event
        /// </summary>
        /// <returns>true or false</returns>
        public abstract bool Register();

        /// <summary>
        /// Return the name needed for the external event
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            return EventName;
        }

        
    }
}
