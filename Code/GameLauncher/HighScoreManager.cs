using Newtonsoft.Json;

namespace GameLauncher;

// Custom interface for game statistics
public interface IGameStatistics
{
    int Score { get; }
    DateTime Date { get; }
    string GetDisplayText();
}

// Abstract class for score management
public abstract class ScoreManagerBase
{
    protected abstract string GetStoragePath();
    
    public abstract bool ValidateScore(int score);
}

// Sealed class for score configuration
public sealed class ScoreConfiguration
{
    private static ScoreConfiguration? _instance;
    private static readonly object _lock = new object();
    
    public int MaxTopScores { get; set; } = 10;
    public int MinValidScore { get; set; } = 0;
    
    // Static constructor
    static ScoreConfiguration()
    {
        // Initialize default configuration
        Console.WriteLine("ScoreConfiguration static constructor initialized");
    }
    
    public static ScoreConfiguration Instance
    {
        get
        {
            lock (_lock)
            {
                return _instance ??= new ScoreConfiguration();
            }
        }
    }
    
    private ScoreConfiguration() { }
}

public class HighScoreManager : ScoreManagerBase
{
    private readonly string _appDataPath;
    private static int _totalScoresSaved;
    
    // Static constructor
    static HighScoreManager()
    {
        _totalScoresSaved = 0;
        Console.WriteLine("HighScoreManager static constructor initialized");
    }
    
    public HighScoreManager()
    {
        _appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CoffeePause"
        );
        Directory.CreateDirectory(_appDataPath);
    }
    
    protected override string GetStoragePath()
    {
        return _appDataPath;
    }
    
    public override bool ValidateScore(int score)
    {
        return score >= ScoreConfiguration.Instance.MinValidScore;
    }
    
    // Method with out parameter
    public bool TryGetHighScore(string gameName, out ScoreEntry? highestScore)
    {
        var scores = LoadScores(gameName);
        if (scores.Count > 0)
        {
            highestScore = scores[0];
            return true;
        }
        highestScore = null;
        return false;
    }
    
    // Method with params keyword
    public void SaveMultipleScores(string gameName, params ScoreEntry[] entries)
    {
        foreach (var entry in entries)
        {
            SaveScore(gameName, entry);
        }
    }
    
    // Method with default and named arguments
    public List<ScoreEntry> LoadScores(string gameName, int maxCount = 10, bool sortDescending = true)
    {
        var filePath = GetScoreFilePath(gameName);
        if (!File.Exists(filePath))
            return new List<ScoreEntry>();
            
        try
        {
            var json = File.ReadAllText(filePath);
            var scores = JsonConvert.DeserializeObject<List<ScoreEntry>>(json) ?? new List<ScoreEntry>();
            
            if (sortDescending)
                scores = scores.OrderByDescending(s => s.Score).Take(maxCount).ToList();
            else
                scores = scores.OrderBy(s => s.Score).Take(maxCount).ToList();
                
            return scores;
        }
        catch
        {
            return new List<ScoreEntry>();
        }
    }
    
    public void SaveScore(string gameName, ScoreEntry entry)
    {
        var scores = LoadScores(gameName);
        scores.Add(entry);
        scores = scores.OrderByDescending(s => s.Score).Take(ScoreConfiguration.Instance.MaxTopScores).ToList();
        
        var filePath = GetScoreFilePath(gameName);
        var tempPath = filePath + ".tmp";
        
        try
        {
            var json = JsonConvert.SerializeObject(scores, Formatting.Indented);
            File.WriteAllText(tempPath, json);
            
            if (File.Exists(filePath))
                File.Delete(filePath);
            
            File.Move(tempPath, filePath);
            _totalScoresSaved++;
        }
        catch
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
            throw;
        }
    }
    
    private string GetScoreFilePath(string gameName)
    {
        return Path.Combine(_appDataPath, $"{gameName}_scores.json");
    }
    
    public static int GetTotalScoresSaved() => _totalScoresSaved;
}

// ScoreEntry implementing IComparable, IEquatable, IFormattable, and custom interface
public class ScoreEntry : IComparable<ScoreEntry>, IEquatable<ScoreEntry>, IFormattable, IGameStatistics
{
    public string PlayerName { get; set; } = "Player";
    public int Score { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    
    // Deconstructor
    public void Deconstruct(out string playerName, out int score, out DateTime date)
    {
        playerName = PlayerName;
        score = Score;
        date = Date;
    }
    
    // IComparable<T> implementation
    public int CompareTo(ScoreEntry? other)
    {
        if (other == null) return 1;
        // Higher scores come first
        int scoreComparison = other.Score.CompareTo(this.Score);
        if (scoreComparison != 0) return scoreComparison;
        // If scores are equal, earlier dates come first
        return this.Date.CompareTo(other.Date);
    }
    
    // IEquatable<T> implementation
    public bool Equals(ScoreEntry? other)
    {
        if (other == null) return false;
        return this.PlayerName == other.PlayerName && 
               this.Score == other.Score && 
               this.Date == other.Date;
    }
    
    public override bool Equals(object? obj)
    {
        return Equals(obj as ScoreEntry);
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(PlayerName, Score, Date);
    }
    
    // IFormattable implementation
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (string.IsNullOrEmpty(format)) format = "G";
        
        return format.ToUpperInvariant() switch
        {
            "G" => $"{PlayerName}: {Score} ({Date:yyyy-MM-dd})",
            "S" => $"{PlayerName}: {Score}",
            "D" => $"{PlayerName} - {Score} points on {Date:yyyy-MM-dd HH:mm}",
            "L" => $"Player: {PlayerName}, Score: {Score}, Date: {Date:yyyy-MM-dd HH:mm:ss}",
            _ => $"{PlayerName}: {Score}"
        };
    }
    
    public override string ToString()
    {
        return ToString("G", null);
    }
    
    // IGameStatistics implementation
    public string GetDisplayText()
    {
        return $"{PlayerName}: {Score} points";
    }
    
    // Operator overloading
    public static bool operator ==(ScoreEntry? left, ScoreEntry? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }
    
    public static bool operator !=(ScoreEntry? left, ScoreEntry? right)
    {
        return !(left == right);
    }
    
    public static bool operator >(ScoreEntry? left, ScoreEntry? right)
    {
        if (left is null) return false;
        if (right is null) return true;
        return left.Score > right.Score;
    }
    
    public static bool operator <(ScoreEntry? left, ScoreEntry? right)
    {
        if (left is null) return right is not null;
        if (right is null) return false;
        return left.Score < right.Score;
    }
    
    public static ScoreEntry operator +(ScoreEntry entry, int bonus)
    {
        return new ScoreEntry 
        { 
            PlayerName = entry.PlayerName, 
            Score = entry.Score + bonus, 
            Date = entry.Date 
        };
    }
}
