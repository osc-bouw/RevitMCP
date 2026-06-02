using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Develoh.BIM.RVT25.Extensions
{
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    public static class SvgRenderer
    {
        public static BitmapImage ToBitmapImage(this Geometry geometry, int size = 32, Color? foreground = null)
        {
           
            var color = foreground ?? Colors.White;
            var geometryClone = geometry.Clone();
            // Schaal de geometry naar het gewenste formaat
            var bounds = geometryClone.Bounds;
            double scale = size / Math.Max(bounds.Width, bounds.Height);
            var transform = new TransformGroup();
            transform.Children.Add(new TranslateTransform(-bounds.X, -bounds.Y));
            transform.Children.Add(new ScaleTransform(scale, scale));
            geometryClone.Transform = transform;

            var drawing = new GeometryDrawing
            {
                Geometry = geometryClone,
                Brush = new SolidColorBrush(color)
            };

            var drawingVisual = new DrawingVisual();
            using (var ctx = drawingVisual.RenderOpen())
            {
                ctx.DrawDrawing(drawing);
            }

            var rtb = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(drawingVisual);

            // Converteer RenderTargetBitmap → BitmapImage
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            using var ms = new System.IO.MemoryStream();
            encoder.Save(ms);
            ms.Position = 0;

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = ms;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }
    }
}
