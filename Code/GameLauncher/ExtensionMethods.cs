using System.Drawing;

namespace GameLauncher;

/// <summary>
/// Extension methods for various types in the game launcher
/// </summary>
public static class ExtensionMethods
{
    /// <summary>
    /// Extension deconstructor for DateTime to extract date components
    /// </summary>
    public static void Deconstruct(this DateTime dateTime, 
        out int year, out int month, out int day)
    {
        year = dateTime.Year;
        month = dateTime.Month;
        day = dateTime.Day;
    }
    
    /// <summary>
    /// Extension deconstructor for DateTime to extract time components
    /// </summary>
    public static void Deconstruct(this DateTime dateTime, 
        out int hour, out int minute, out int second, out int millisecond)
    {
        hour = dateTime.Hour;
        minute = dateTime.Minute;
        second = dateTime.Second;
        millisecond = dateTime.Millisecond;
    }
    
    /// <summary>
    /// Extension method to check if a string is a valid player name
    /// </summary>
    public static bool IsValidPlayerName(this string playerName)
    {
        if (string.IsNullOrWhiteSpace(playerName))
            return false;
            
        if (playerName.Length < 2 || playerName.Length > 20)
            return false;
            
        return playerName.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '-');
    }
    
    /// <summary>
    /// Extension method to truncate string with ellipsis
    /// </summary>
    public static string TruncateWithEllipsis(this string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;
            
        return text.Substring(0, maxLength - 3) + "...";
    }
    
    /// <summary>
    /// Extension method to calculate brightness of a color
    /// </summary>
    public static double GetColorBrightness(this Color color)
    {
        return (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255.0;
    }
    
    /// <summary>
    /// Extension method to check if a color is dark
    /// </summary>
    public static bool IsDark(this Color color)
    {
        return color.GetColorBrightness() < 0.5;
    }
    
    /// <summary>
    /// Extension method to calculate score ranking category
    /// </summary>
    public static string GetScoreRank(this int score)
    {
        return score switch
        {
            < 100 => "Beginner",
            < 300 => "Intermediate",
            < 500 => "Advanced",
            < 800 => "Expert",
            _ => "Master"
        };
    }
    
    /// <summary>
    /// Extension method to chunk a list into smaller lists
    /// </summary>
    public static IEnumerable<List<T>> ChunkBy<T>(this IEnumerable<T> source, int chunkSize)
    {
        if (chunkSize <= 0)
            throw new ArgumentException("Chunk size must be greater than 0", nameof(chunkSize));
            
        var chunk = new List<T>(chunkSize);
        foreach (var item in source)
        {
            chunk.Add(item);
            if (chunk.Count == chunkSize)
            {
                yield return chunk;
                chunk = new List<T>(chunkSize);
            }
        }
        
        if (chunk.Count > 0)
            yield return chunk;
    }
    
    /// <summary>
    /// Extension method to safely get value from dictionary
    /// </summary>
    public static TValue? GetValueOrDefault<TKey, TValue>(
        this Dictionary<TKey, TValue> dictionary, 
        TKey key, 
        TValue? defaultValue = default) where TKey : notnull
    {
        return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
    }
}
