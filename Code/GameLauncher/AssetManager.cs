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
    
    public static Image LoadCardImage(string suit, int rank)
    {
        try
        {
            // Map suit names to folder names
            string suitFolder = suit switch
            {
                "Spades" => "Spades",
                "Hearts" => "Hearts",
                "Diamonds" => "Diamonds",
                "Clubs" => "Clubs",
                _ => "Spades"
            };
            
            // Map rank to file prefix
            string prefix = suitFolder.ToLower()[0].ToString(); // s, h, d, c
            string rankStr = rank switch
            {
                1 => "1",    // Ace
                11 => "j",   // Jack
                12 => "q",   // Queen
                13 => "k",   // King
                _ => rank.ToString()
            };
            
            string fileName = $"{prefix}{rankStr}.png";
            string cardPath = Path.Combine(AssetPath, "Playing Cards", suitFolder, fileName);
            
            if (!File.Exists(cardPath))
            {
                System.Diagnostics.Debug.WriteLine($"Card not found: {cardPath}");
                return CreatePlaceholderCard(100, 140);
            }
            
            // Load image from stream to avoid file locking
            using (var stream = new FileStream(cardPath, FileMode.Open, FileAccess.Read))
            {
                return Image.FromStream(stream);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading card: {ex.Message}");
            return CreatePlaceholderCard(100, 140);
        }
    }
    
    public static Image LoadCardBackImage()
    {
        try
        {
            string cardBackPath = Path.Combine(AssetPath, "Playing Cards", "ReversedCard.png");
            
            if (!File.Exists(cardBackPath))
            {
                System.Diagnostics.Debug.WriteLine($"Card back not found: {cardBackPath}");
                return CreatePlaceholderCard(100, 140);
            }
            
            // Load image from stream to avoid file locking
            using (var stream = new FileStream(cardBackPath, FileMode.Open, FileAccess.Read))
            {
                return Image.FromStream(stream);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading card back: {ex.Message}");
            return CreatePlaceholderCard(100, 140);
        }
    }
    
    public static Image? LoadButtonImage(string buttonName)
    {
        try
        {
            string imagePath = Path.Combine(AssetPath, "Playing Cards", $"{buttonName}.png");
            
            if (!File.Exists(imagePath))
            {
                System.Diagnostics.Debug.WriteLine($"Button image not found: {imagePath}");
                return null;
            }
            
            // Load image from stream to avoid file locking
            using (var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
            {
                return Image.FromStream(stream);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading button image: {ex.Message}");
            return null;
        }
    }
    
    public static Image? LoadBackgroundImage()
    {
        try
        {
            string backgroundPath = Path.Combine(AssetPath, "Playing Cards", "background.png");
            
            if (!File.Exists(backgroundPath))
            {
                System.Diagnostics.Debug.WriteLine($"Background image not found: {backgroundPath}");
                return null;
            }
            
            // Load image from stream to avoid file locking
            using (var stream = new FileStream(backgroundPath, FileMode.Open, FileAccess.Read))
            {
                return Image.FromStream(stream);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading background image: {ex.Message}");
            return null;
        }
    }
    
    private static Image CreatePlaceholderCard(int width, int height)
    {
        var placeholder = new Bitmap(width, height);
        using (var g = Graphics.FromImage(placeholder))
        using (var font = new Font("Arial", 24, FontStyle.Bold))
        {
            g.Clear(Color.LightGray);
            g.DrawRectangle(Pens.Black, 0, 0, width - 1, height - 1);
            g.DrawString("?", font, Brushes.Black, 
                new PointF(width / 2 - 15, height / 2 - 15));
        }
        return placeholder;
    }
}
