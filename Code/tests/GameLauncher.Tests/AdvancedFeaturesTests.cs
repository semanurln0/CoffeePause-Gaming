using Xunit;
using System.Drawing;

namespace GameLauncher.Tests;

/// <summary>
/// Tests for advanced C# features: custom exceptions
/// </summary>
public class ExceptionTests
{
    [Fact]
    public void GameException_CanBeCreated()
    {
        // Act
        var exception = new GameException("Test message");
        
        // Assert
        Assert.NotNull(exception);
        Assert.Equal("Test message", exception.Message);
    }
    
    [Fact]
    public void ScoreValidationException_ContainsGameNameAndScore()
    {
        // Arrange
        var gameName = "TestGame";
        var score = -10;
        
        // Act
        var exception = new ScoreValidationException(gameName, score);
        
        // Assert
        Assert.Equal(gameName, exception.GameName);
        Assert.Equal(score, exception.InvalidScore);
        Assert.Contains(gameName, exception.Message);
    }
    
    [Fact]
    public void AssetLoadException_ContainsAssetPath()
    {
        // Arrange
        var assetPath = "test/path/to/asset.png";
        
        // Act
        var exception = new AssetLoadException(assetPath);
        
        // Assert
        Assert.Equal(assetPath, exception.AssetPath);
        Assert.Contains(assetPath, exception.Message);
    }
    
    [Fact]
    public void CaffeineDataException_CanBeCreatedWithInnerException()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner error");
        
        // Act
        var exception = new CaffeineDataException("Outer message", innerException);
        
        // Assert
        Assert.Equal(innerException, exception.InnerException);
    }
}

/// <summary>
/// Tests for ICloneable implementations
/// </summary>
public class CloneableTests
{
    [Fact]
    public void ScoreEntry_CloneCreatesDeepCopy()
    {
        // Arrange
        var original = new ScoreEntry
        {
            PlayerName = "TestPlayer",
            Score = 100,
            Date = DateTime.Now
        };
        
        // Act
        var clone = (ScoreEntry)original.Clone();
        
        // Assert
        Assert.NotSame(original, clone);
        Assert.Equal(original.PlayerName, clone.PlayerName);
        Assert.Equal(original.Score, clone.Score);
        Assert.Equal(original.Date, clone.Date);
    }
    
    [Fact]
    public void ScoreEntry_TypedCloneWorks()
    {
        // Arrange
        var original = new ScoreEntry
        {
            PlayerName = "TestPlayer",
            Score = 200,
            Date = DateTime.Now
        };
        
        // Act
        var clone = original.CloneTyped();
        
        // Assert
        Assert.NotSame(original, clone);
        Assert.Equal(original.PlayerName, clone.PlayerName);
    }
    
    [Fact]
    public void CaffeineEntry_CloneCreatesDeepCopy()
    {
        // Arrange
        var original = new CaffeineEntry
        {
            DrinkType = "Coffee",
            SizeMl = 250,
            Quantity = 2,
            CaffeineAmount = 100.0,
            ConsumedAt = DateTime.Now
        };
        
        // Act
        var clone = (CaffeineEntry)original.Clone();
        
        // Assert
        Assert.NotSame(original, clone);
        Assert.Equal(original.DrinkType, clone.DrinkType);
        Assert.Equal(original.SizeMl, clone.SizeMl);
        Assert.Equal(original.CaffeineAmount, clone.CaffeineAmount);
    }
    
    [Fact]
    public void GameSession_CloneCreatesDeepCopy()
    {
        // Arrange
        var original = new GameSession
        {
            GameName = "TestGame",
            StartTime = DateTime.Now,
            Score = 500,
            PlayerName = "Player1"
        };
        
        // Act
        var clone = (GameSession)original.Clone();
        
        // Assert
        Assert.NotSame(original, clone);
        Assert.Equal(original.GameName, clone.GameName);
        Assert.Equal(original.Score, clone.Score);
    }
}

/// <summary>
/// Tests for IEnumerable implementations
/// </summary>
public class EnumerableTests
{
    [Fact]
    public void HighScoreManager_CanBeEnumerated()
    {
        // Arrange
        var manager = new HighScoreManager();
        var testGameName = $"TestGame_{Guid.NewGuid()}";
        
        manager.SaveScore(testGameName, new ScoreEntry { PlayerName = "Player1", Score = 100 });
        manager.SaveScore(testGameName, new ScoreEntry { PlayerName = "Player2", Score = 200 });
        
        // Act
        int count = 0;
        foreach (var score in manager)
        {
            count++;
            Assert.NotNull(score);
        }
        
        // Assert
        Assert.True(count >= 2);
    }
    
    [Fact]
    public void HighScoreManager_GetScoresForGameIterator()
    {
        // Arrange
        var manager = new HighScoreManager();
        var testGameName = $"TestGame_{Guid.NewGuid()}";
        
        manager.SaveScore(testGameName, new ScoreEntry { PlayerName = "Player1", Score = 100 });
        manager.SaveScore(testGameName, new ScoreEntry { PlayerName = "Player2", Score = 200 });
        
        // Act
        var scores = manager.GetScoresForGame(testGameName).ToList();
        
        // Assert
        Assert.Equal(2, scores.Count);
    }
    
    [Fact]
    public void GameSessionCollection_CanBeEnumerated()
    {
        // Arrange
        var collection = new GameSessionCollection();
        collection.AddSession(new GameSession { GameName = "Game1", StartTime = DateTime.Now });
        collection.AddSession(new GameSession { GameName = "Game2", StartTime = DateTime.Now });
        
        // Act
        int count = 0;
        foreach (var session in collection)
        {
            count++;
            Assert.NotNull(session);
        }
        
        // Assert
        Assert.Equal(2, count);
    }
    
    [Fact]
    public void GameSessionCollection_GetActiveSessionsIterator()
    {
        // Arrange
        var collection = new GameSessionCollection();
        var session1 = new GameSession { GameName = "Game1", StartTime = DateTime.Now };
        var session2 = new GameSession { GameName = "Game2", StartTime = DateTime.Now };
        
        collection.AddSession(session1);
        collection.AddSession(session2);
        collection.EndSession(session1);
        
        // Act
        var activeSessions = collection.GetActiveSessions().ToList();
        
        // Assert
        Assert.Single(activeSessions);
        Assert.Equal("Game2", activeSessions[0].GameName);
    }
    
    [Fact]
    public void GenericRepository_SupportsIteration()
    {
        // Arrange
        var repo = new GenericRepository<ScoreEntry>("test");
        repo.Add(new ScoreEntry { PlayerName = "Player1", Score = 100 });
        repo.Add(new ScoreEntry { PlayerName = "Player2", Score = 200 });
        
        // Act
        var items = repo.ToList();
        
        // Assert
        Assert.Equal(2, items.Count);
    }
    
    [Fact]
    public void GenericRepository_GetItemsReverseIterator()
    {
        // Arrange
        var repo = new GenericRepository<ScoreEntry>("test");
        repo.Add(new ScoreEntry { PlayerName = "Player1", Score = 100 });
        repo.Add(new ScoreEntry { PlayerName = "Player2", Score = 200 });
        
        // Act
        var reversed = repo.GetItemsReverse().ToList();
        
        // Assert
        Assert.Equal(2, reversed.Count);
        Assert.Equal(200, reversed[0].Score);
        Assert.Equal(100, reversed[1].Score);
    }
}

/// <summary>
/// Tests for extension methods
/// </summary>
public class ExtensionMethodTests
{
    [Fact]
    public void DateTime_DeconstructToDateComponents()
    {
        // Arrange
        var date = new DateTime(2024, 12, 8);
        
        // Act
        var (year, month, day) = date;
        
        // Assert
        Assert.Equal(2024, year);
        Assert.Equal(12, month);
        Assert.Equal(8, day);
    }
    
    [Fact]
    public void String_IsValidPlayerName()
    {
        // Act & Assert
        Assert.True("Player123".IsValidPlayerName());
        Assert.True("Player_One".IsValidPlayerName());
        Assert.False("A".IsValidPlayerName()); // Too short
        Assert.False("".IsValidPlayerName());
        Assert.False("Player With Spaces".IsValidPlayerName());
    }
    
    [Fact]
    public void String_TruncateWithEllipsis()
    {
        // Arrange
        var text = "This is a long text that needs truncation";
        
        // Act
        var truncated = text.TruncateWithEllipsis(20);
        
        // Assert
        Assert.Equal(20, truncated.Length);
        Assert.EndsWith("...", truncated);
    }
    
    [Fact]
    public void Color_GetColorBrightness()
    {
        // Arrange
        var white = Color.White;
        var black = Color.Black;
        
        // Act
        var whiteBrightness = white.GetColorBrightness();
        var blackBrightness = black.GetColorBrightness();
        
        // Assert
        Assert.True(whiteBrightness > 0.9);
        Assert.True(blackBrightness < 0.1);
    }
    
    [Fact]
    public void Int_GetScoreRank()
    {
        // Act & Assert
        Assert.Equal("Beginner", 50.GetScoreRank());
        Assert.Equal("Intermediate", 200.GetScoreRank());
        Assert.Equal("Advanced", 400.GetScoreRank());
        Assert.Equal("Expert", 700.GetScoreRank());
        Assert.Equal("Master", 1000.GetScoreRank());
    }
    
    [Fact]
    public void IEnumerable_ChunkBy()
    {
        // Arrange
        var numbers = Enumerable.Range(1, 10);
        
        // Act
        var chunks = numbers.ChunkBy(3).ToList();
        
        // Assert
        Assert.Equal(4, chunks.Count); // 3, 3, 3, 1
        Assert.Equal(3, chunks[0].Count);
        Assert.Single(chunks[3]);
    }
}

/// <summary>
/// Tests for generic types with constraints
/// </summary>
public class GenericTypeTests
{
    [Fact]
    public void GenericRepository_WorksWithConstrainedTypes()
    {
        // Arrange
        var repo = new GenericRepository<ScoreEntry>("test");
        
        // Act
        repo.Add(new ScoreEntry { PlayerName = "Player1", Score = 100 });
        
        // Assert
        Assert.Equal(1, repo.Count);
    }
    
    [Fact]
    public void GenericRepository_FindWorks()
    {
        // Arrange
        var repo = new GenericRepository<ScoreEntry>("test");
        repo.Add(new ScoreEntry { PlayerName = "Player1", Score = 100 });
        repo.Add(new ScoreEntry { PlayerName = "Player2", Score = 200 });
        
        // Act
        var found = repo.Find(s => s.Score == 200);
        
        // Assert
        Assert.NotNull(found);
        Assert.Equal("Player2", found.PlayerName);
    }
    
    [Fact]
    public void GenericRepository_GetItemsWhereIterator()
    {
        // Arrange
        var repo = new GenericRepository<ScoreEntry>("test");
        repo.Add(new ScoreEntry { PlayerName = "Player1", Score = 100 });
        repo.Add(new ScoreEntry { PlayerName = "Player2", Score = 200 });
        repo.Add(new ScoreEntry { PlayerName = "Player3", Score = 150 });
        
        // Act
        var highScores = repo.GetItemsWhere(s => s.Score >= 150).ToList();
        
        // Assert
        Assert.Equal(2, highScores.Count);
    }
}

/// <summary>
/// Tests for event handling
/// </summary>
public class EventTests
{
    [Fact]
    public void HighScoreManager_RaisesScoreSavedEvent()
    {
        // Arrange
        var manager = new HighScoreManager();
        var testGameName = $"TestGame_{Guid.NewGuid()}";
        bool eventRaised = false;
        
        manager.ScoreSaved += (sender, args) =>
        {
            eventRaised = true;
            Assert.Equal(testGameName, args.GameName);
        };
        
        // Act
        manager.SaveScore(testGameName, new ScoreEntry { PlayerName = "Player1", Score = 100 });
        
        // Assert
        Assert.True(eventRaised);
    }
    
    [Fact]
    public void GameSessionCollection_RaisesSessionStartedEvent()
    {
        // Arrange
        var collection = new GameSessionCollection();
        bool eventRaised = false;
        
        collection.SessionStarted += (sender, args) =>
        {
            eventRaised = true;
            Assert.NotNull(args.Session);
        };
        
        // Act
        collection.AddSession(new GameSession { GameName = "TestGame", StartTime = DateTime.Now });
        
        // Assert
        Assert.True(eventRaised);
    }
    
    [Fact]
    public void GenericRepository_RaisesItemAddedEvent()
    {
        // Arrange
        var repo = new GenericRepository<ScoreEntry>("test");
        bool eventRaised = false;
        
        repo.ItemAdded += (sender, args) =>
        {
            eventRaised = true;
            Assert.NotNull(args.Item);
        };
        
        // Act
        repo.Add(new ScoreEntry { PlayerName = "Player1", Score = 100 });
        
        // Assert
        Assert.True(eventRaised);
    }
}
