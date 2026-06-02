using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Reflection;

namespace Develoh.Bim.RVT25
{
    public static class RevitExtensions
    {
        public static List<Element> GetElements(this Selection selection, Document doc)
        {
            List<Element> elements = new List<Element>();

            ICollection<ElementId> ids = selection.GetElementIds();

            foreach (ElementId id in ids)
            {
                Element element = doc.GetElement(id);

                if (element != null)
                {
                    elements.Add(element);
                }
            }

            return elements;
        }

        public static Parameter GetParameterByName(this Element element, BuiltInParameter bip)
        {
            return element.get_Parameter(bip);
        }


        public static Dictionary<string, object> ToDictionary(this List<Parameter> parameters)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            foreach (var parameter in parameters)
            {
                if (parameter.HasValue)
                {
                    dictionary.Add(parameter.Definition.Name, parameter.AsString());
                }
            }

            return dictionary;
        }

       

    }
}
