namespace GameLauncher;

// Sealed utility class with bitwise operations
public sealed class GameUtils
{
    // Bitwise flags for game features
    [Flags]
    public enum GameFeatures
    {
        None = 0,
        HighScores = 1 << 0,    // 1
        Settings = 1 << 1,       // 2
        Pause = 1 << 2,          // 4
        Timer = 1 << 3,          // 8
        Multiplayer = 1 << 4,    // 16
        Achievements = 1 << 5    // 32
    }
    
    // Method using bitwise operations
    public static bool HasFeature(GameFeatures features, GameFeatures checkFeature)
    {
        return (features & checkFeature) == checkFeature;
    }
    
    public static GameFeatures AddFeature(GameFeatures features, GameFeatures newFeature)
    {
        return features | newFeature;
    }
    
    public static GameFeatures RemoveFeature(GameFeatures features, GameFeatures removeFeature)
    {
        return features & ~removeFeature;
    }
    
    public static GameFeatures ToggleFeature(GameFeatures features, GameFeatures toggleFeature)
    {
        return features ^ toggleFeature;
    }
    
    // Method using switch with when keyword and pattern matching
    public static string GetDifficultyMessage(int score, string gameName) => (score, gameName) switch
    {
        (var s, "Minesweeper") when s < 50 => "Beginner level - Keep trying!",
        (var s, "Minesweeper") when s >= 50 && s < 150 => "Intermediate level - Good job!",
        (var s, "Minesweeper") when s >= 150 => "Expert level - Amazing!",
        
        (var s, "Sudoku") when s < 50 => "Novice solver",
        (var s, "Sudoku") when s >= 50 && s < 100 => "Competent solver",
        (var s, "Sudoku") when s >= 100 => "Master solver",
        
        (var s, "PacMan") when s < 100 => "Beginner ghost hunter",
        (var s, "PacMan") when s >= 100 && s < 500 => "Skilled player",
        (var s, "PacMan") when s >= 500 => "Pac-Man champion!",
        
        (var s, "SpiderSolitaire") when s < 200 => "Learning the ropes",
        (var s, "SpiderSolitaire") when s >= 200 && s < 500 => "Card master",
        (var s, "SpiderSolitaire") when s >= 500 => "Solitaire legend!",
        
        _ => "Keep playing!"
    };
    
    // Method with bitwise shift operations
    public static int CalculateBonus(int baseScore, int multiplier)
    {
        // Using bitwise shift for multiplication by powers of 2
        return baseScore << multiplier; // Equivalent to baseScore * 2^multiplier
    }
    
    // Method using out parameters for color calculation
    public static bool TryParseColorComponents(int colorValue, out int red, out int green, out int blue)
    {
        if (colorValue < 0 || colorValue > 0xFFFFFF)
        {
            red = green = blue = 0;
            return false;
        }
        
        // Bitwise operations to extract RGB components
        red = (colorValue >> 16) & 0xFF;
        green = (colorValue >> 8) & 0xFF;
        blue = colorValue & 0xFF;
        return true;
    }
    
    // Method demonstrating named arguments usage
    public static ScoreEntry CreateScoreEntry(string playerName = "Anonymous", int score = 0, DateTime? date = null)
    {
        return new ScoreEntry
        {
            PlayerName = playerName,
            Score = score,
            Date = date ?? DateTime.Now
        };
    }
}
