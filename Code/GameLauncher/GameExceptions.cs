namespace GameLauncher;

// Custom exception types for the game launcher application

/// <summary>
/// Base exception class for all game-related exceptions
/// </summary>
public class GameException : Exception
{
    public GameException() : base() { }
    
    public GameException(string message) : base(message) { }
    
    public GameException(string message, Exception innerException) 
        : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when score validation fails
/// </summary>
public class ScoreValidationException : GameException
{
    public int InvalidScore { get; }
    public string GameName { get; }
    
    public ScoreValidationException(string gameName, int score) 
        : base($"Invalid score {score} for game {gameName}")
    {
        GameName = gameName;
        InvalidScore = score;
    }
    
    public ScoreValidationException(string gameName, int score, string message) 
        : base(message)
    {
        GameName = gameName;
        InvalidScore = score;
    }
}

/// <summary>
/// Exception thrown when asset loading fails
/// </summary>
public class AssetLoadException : GameException
{
    public string AssetPath { get; }
    
    public AssetLoadException(string assetPath) 
        : base($"Failed to load asset: {assetPath}")
    {
        AssetPath = assetPath;
    }
    
    public AssetLoadException(string assetPath, Exception innerException) 
        : base($"Failed to load asset: {assetPath}", innerException)
    {
        AssetPath = assetPath;
    }
}

/// <summary>
/// Exception thrown when caffeine data operations fail
/// </summary>
public class CaffeineDataException : GameException
{
    public CaffeineDataException(string message) : base(message) { }
    
    public CaffeineDataException(string message, Exception innerException) 
        : base(message, innerException) { }
}
