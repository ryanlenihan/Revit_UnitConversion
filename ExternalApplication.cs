using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;
using UnitsConverter.Properties;

namespace UnitsConverter
{
    internal class ExternalApplication : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public BitmapImage Convert(Image img)
        {
            using (var memory = new MemoryStream())
            {
                img.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        public Result OnStartup(UIControlledApplication application)
        {
            //get the file path of the current assembly
            var path = Assembly.GetExecutingAssembly().Location;
            var tabName = "Revit.AU";

            //create ribbon tab
            application.CreateRibbonTab(tabName);

            //create button for metric for current document
            var metricDocButton = new PushButtonData("metricDocButton", "Metric", path,
                "UnitsConverter.ConvertCurrentDocumetToMetric");
            //create button for imperialfor current document
            var imperialDocButton = new PushButtonData("imperialDocButton", "Imperial", path,
                "UnitsConverter.ConvertCurrentDocumetToImperial");

            var batchDocButton =
                new PushButtonData("batchDocButton", "Batch Convert", path, "UnitsConverter.BatchConverter");


            //create ribbon panel
            var panel = application.CreateRibbonPanel(tabName, "Unit Conversion Tools");

            Image bResource = Resources.batch_32;
            Image mResource = Resources.metric_16;
            Image iResource = Resources.imperial_16;

            metricDocButton.Image = Convert(mResource);
            imperialDocButton.Image = Convert(iResource);
            batchDocButton.LargeImage = Convert(bResource);


            var unitsButtons = new List<RibbonItem>();
            unitsButtons.AddRange(panel.AddStackedItems(metricDocButton, imperialDocButton));

            var bDocButton = panel.AddItem(batchDocButton) as PushButton;


            return Result.Succeeded;
        }
    }
}