using Svg;
using System.Drawing.Imaging;

namespace GameLauncher;

public static class IconConverter
{
    public static Icon? LoadIconFromSvg(string svgPath, int size = 256)
    {
        try
        {
            if (!File.Exists(svgPath))
                return null;

            var svgDocument = SvgDocument.Open(svgPath);
            svgDocument.Width = size;
            svgDocument.Height = size;

            using var bitmap = new Bitmap(size, size);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.Transparent);
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                svgDocument.Draw(graphics);
            }

            // Convert bitmap to icon
            using var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);
            ms.Seek(0, SeekOrigin.Begin);
            
            return new Icon(Icon.FromHandle(bitmap.GetHicon()), size, size);
        }
        catch
        {
            return null;
        }
    }

    public static void CreateIcoFile(string svgPath, string icoPath)
    {
        try
        {
            var svgDocument = SvgDocument.Open(svgPath);
            
            // Create multiple sizes for better quality
            int[] sizes = { 16, 32, 48, 256 };
            var bitmaps = new List<Bitmap>();

            foreach (var size in sizes)
            {
                svgDocument.Width = size;
                svgDocument.Height = size;
                
                var bitmap = new Bitmap(size, size);
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.Clear(Color.Transparent);
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    svgDocument.Draw(graphics);
                }
                bitmaps.Add(bitmap);
            }

            // Save as ICO
            using var fs = new FileStream(icoPath, FileMode.Create);
            using var icon = Icon.FromHandle(bitmaps[^1].GetHicon());
            icon.Save(fs);

            foreach (var bmp in bitmaps)
            {
                bmp.Dispose();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating ICO file: {ex.Message}");
        }
    }
}
