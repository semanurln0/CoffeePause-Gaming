using System.Collections;

namespace GameLauncher;

/// <summary>
/// Custom enumerator for game session collection
/// </summary>
public class GameSessionEnumerator : IEnumerator<GameSession>
{
    private readonly List<GameSession> _sessions;
    private int _position = -1;
    
    public GameSessionEnumerator(List<GameSession> sessions)
    {
        _sessions = sessions;
    }
    
    public GameSession Current
    {
        get
        {
            try
            {
                return _sessions[_position];
            }
            catch (IndexOutOfRangeException)
            {
                throw new InvalidOperationException();
            }
        }
    }
    
    object IEnumerator.Current => Current;
    
    public bool MoveNext()
    {
        _position++;
        return _position < _sessions.Count;
    }
    
    public void Reset()
    {
        _position = -1;
    }
    
    public void Dispose()
    {
        // Nothing to dispose
    }
}

/// <summary>
/// Collection of game sessions implementing IEnumerable with custom IEnumerator
/// </summary>
public class GameSessionCollection : IEnumerable<GameSession>
{
    private readonly List<GameSession> _sessions = new List<GameSession>();
    
    public event EventHandler<GameSessionEventArgs>? SessionStarted;
    public event EventHandler<GameSessionEventArgs>? SessionEnded;
    
    public int Count => _sessions.Count;
    
    public void AddSession(GameSession session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));
            
        _sessions.Add(session);
        OnSessionStarted(session);
    }
    
    public void EndSession(GameSession session)
    {
        if (session != null && _sessions.Contains(session))
        {
            session.EndTime = DateTime.Now;
            OnSessionEnded(session);
        }
    }
    
    public GameSession? GetSessionByGame(string gameName)
    {
        return _sessions.FirstOrDefault(s => s.GameName == gameName && s.EndTime == null);
    }
    
    // Custom IEnumerator implementation
    public IEnumerator<GameSession> GetEnumerator()
    {
        return new GameSessionEnumerator(_sessions);
    }
    
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    // Iterator methods using yield
    public IEnumerable<GameSession> GetActiveSessions()
    {
        foreach (var session in _sessions)
        {
            if (session.EndTime == null)
                yield return session;
        }
    }
    
    public IEnumerable<GameSession> GetSessionsByGame(string gameName)
    {
        foreach (var session in _sessions)
        {
            if (session.GameName.Equals(gameName, StringComparison.OrdinalIgnoreCase))
                yield return session;
        }
    }
    
    public IEnumerable<GameSession> GetSessionsInDateRange(DateTime start, DateTime end)
    {
        foreach (var session in _sessions)
        {
            if (session.StartTime >= start && session.StartTime <= end)
                yield return session;
        }
    }
    
    protected virtual void OnSessionStarted(GameSession session)
    {
        SessionStarted?.Invoke(this, new GameSessionEventArgs(session));
    }
    
    protected virtual void OnSessionEnded(GameSession session)
    {
        SessionEnded?.Invoke(this, new GameSessionEventArgs(session));
    }
}

/// <summary>
/// Represents a game playing session
/// </summary>
public class GameSession : ICloneable
{
    public string GameName { get; set; } = "";
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int Score { get; set; }
    public string PlayerName { get; set; } = "Player";
    
    public TimeSpan Duration => 
        (EndTime ?? DateTime.Now) - StartTime;
    
    public bool IsActive => EndTime == null;
    
    // ICloneable implementation
    public object Clone()
    {
        return new GameSession
        {
            GameName = this.GameName,
            StartTime = this.StartTime,
            EndTime = this.EndTime,
            Score = this.Score,
            PlayerName = this.PlayerName
        };
    }
    
    public GameSession CloneTyped()
    {
        return (GameSession)Clone();
    }
}

/// <summary>
/// Event arguments for game session events
/// </summary>
public class GameSessionEventArgs : EventArgs
{
    public GameSession Session { get; }
    
    public GameSessionEventArgs(GameSession session)
    {
        Session = session;
    }
}
