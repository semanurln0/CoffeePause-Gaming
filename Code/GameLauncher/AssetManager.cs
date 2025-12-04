using Svg;
using System.Drawing.Imaging;

namespace GameLauncher;

public static class AssetManager
{
    private static readonly string AssetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "sprites");
    
    public static Image LoadSvgAsImage(string fileName, int width, int height)
    {
        try
        {
            var svgPath = Path.Combine(AssetPath, fileName);
            if (!File.Exists(svgPath))
            {
                // Return a placeholder image
                var placeholder = new Bitmap(width, height);
                using (var g = Graphics.FromImage(placeholder))
                {
                    g.Clear(Color.Gray);
                    g.DrawString("?", new Font("Arial", 16), Brushes.White, new PointF(width/2 - 10, height/2 - 10));
                }
                return placeholder;
            }
            
            var svgDocument = SvgDocument.Open(svgPath);
            svgDocument.Width = width;
            svgDocument.Height = height;
            
            var bitmap = new Bitmap(width, height);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                svgDocument.Draw(graphics);
            }
            
            return bitmap;
        }
        catch
        {
            var errorBitmap = new Bitmap(width, height);
            using (var g = Graphics.FromImage(errorBitmap))
            {
                g.Clear(Color.Red);
            }
            return errorBitmap;
        }
    }
    
    public static Image LoadImage(string fileName)
    {
        try
        {
            var imagePath = Path.Combine(AssetPath, fileName);
            if (!File.Exists(imagePath))
            {
                var placeholder = new Bitmap(100, 100);
                using (var g = Graphics.FromImage(placeholder))
                {
                    g.Clear(Color.LightGray);
                }
                return placeholder;
            }
            
            return Image.FromFile(imagePath);
        }
        catch
        {
            var errorBitmap = new Bitmap(100, 100);
            using (var g = Graphics.FromImage(errorBitmap))
            {
                g.Clear(Color.Red);
            }
            return errorBitmap;
        }
    }
}
