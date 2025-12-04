using Xunit;

namespace GameLauncher.Tests;

public class HighScoreManagerTests
{
    [Fact]
    public void HighScoreManager_CanCreateInstance()
    {
        // Arrange & Act
        var manager = new HighScoreManager();
        
        // Assert
        Assert.NotNull(manager);
    }
    
    [Fact]
    public void HighScoreManager_LoadScoresReturnsEmptyListForNewGame()
    {
        // Arrange
        var manager = new HighScoreManager();
        var testGameName = $"TestGame_{Guid.NewGuid()}";
        
        // Act
        var scores = manager.LoadScores(testGameName);
        
        // Assert
        Assert.NotNull(scores);
        Assert.Empty(scores);
    }
    
    [Fact]
    public void HighScoreManager_SaveAndLoadScore()
    {
        // Arrange
        var manager = new HighScoreManager();
        var testGameName = $"TestGame_{Guid.NewGuid()}";
        var entry = new ScoreEntry 
        { 
            PlayerName = "TestPlayer", 
            Score = 100,
            Date = DateTime.Now
        };
        
        // Act
        manager.SaveScore(testGameName, entry);
        var scores = manager.LoadScores(testGameName);
        
        // Assert
        Assert.NotNull(scores);
        Assert.Single(scores);
        Assert.Equal("TestPlayer", scores[0].PlayerName);
        Assert.Equal(100, scores[0].Score);
    }
    
    [Fact]
    public void HighScoreManager_KeepsTop10Scores()
    {
        // Arrange
        var manager = new HighScoreManager();
        var testGameName = $"TestGame_{Guid.NewGuid()}";
        
        // Act - Add 15 scores
        for (int i = 0; i < 15; i++)
        {
            manager.SaveScore(testGameName, new ScoreEntry 
            { 
                PlayerName = $"Player{i}", 
                Score = i * 10,
                Date = DateTime.Now
            });
        }
        
        var scores = manager.LoadScores(testGameName);
        
        // Assert
        Assert.Equal(10, scores.Count);
        Assert.Equal(140, scores[0].Score); // Highest score
        Assert.Equal(50, scores[9].Score);  // 10th highest
    }
}
