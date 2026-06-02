using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Develoh.Bim.RVT25.Extensions
{
    public static class GetUIAppExtension
    {
        public static UIApplication GetUIApp(this UIControlledApplication application)
        {
            var versionNumber = application.ControlledApplication.VersionNumber;
            string fieldName;
            switch (versionNumber)
            {
                case "2020":
                    fieldName = "m_uiapplication";
                    break;
                default:
                    fieldName = "m_uiapplication";
                    break;
            }
            return (UIApplication)(application.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)).GetValue(application);
        }


        public static void AddRibbonButton(this RibbonPanel rp, string Label, string tooltip, string _Class, string uniCommand, BitmapImage image, string assemblyPath)
        {
            // create push button
            PushButtonData b2Data = new PushButtonData(uniCommand, Label, assemblyPath, "RevitMcp.Plugin.Addin.Commands." + _Class);

            PushButton? pb2 = rp.AddItem(b2Data) as PushButton;
            if (pb2 != null)
            {
                pb2.ToolTip = tooltip;
                if (image != null)
                {
                    pb2.LargeImage = image;
                }
            }

        }

    }
}

