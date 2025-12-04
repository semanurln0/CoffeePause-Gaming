namespace GameLauncher;

public partial class PacManForm : Form
{
    private const int CellSize = 30;
    private const int GridWidth = 20;
    private const int GridHeight = 20;
    
    private System.Windows.Forms.Timer? gameTimer;
    private int[,] grid = new int[GridHeight, GridWidth];
    private Point pacmanPos = new Point(1, 1);
    private Direction pacmanDir = Direction.Right;
    private List<Ghost> ghosts = new List<Ghost>();
    private int score = 0;
    private bool ghostsVulnerable = false;
    private int vulnerableTimer = 0;
    private int difficulty = 1; // 0=easy, 1=medium, 2=hard
    private HighScoreManager scoreManager = new HighScoreManager();
    private Panel? gamePanel;
    private Label? scoreLabel;
    private Button? settingsBtn;
    private Button? scoreboardBtn;
    private Button? newGameBtn;
    
    // SVG Images
    private Image? pacmanImage;
    private Image? foodDotImage;
    private Image? powerPelletImage;
    private Image? ghostRedImage;
    private Image? ghostVulnerableImage;
    
    // Grid values: 0=empty, 1=food, 2=supreme food, 3=wall
    
    public PacManForm()
    {
        InitializeComponent();
        LoadAssets();
        InitializeGame();
    }
    
    private void LoadAssets()
    {
        try
        {
            pacmanImage = AssetManager.LoadSvgAsImage("pacman-character.svg", CellSize - 6, CellSize - 6);
            foodDotImage = AssetManager.LoadSvgAsImage("pacman-food_dot.svg", 6, 6);
            powerPelletImage = AssetManager.LoadSvgAsImage("pacman-strawberry.svg", 14, 14);
            ghostRedImage = AssetManager.LoadSvgAsImage("pacman-ghost_red_normal.svg", CellSize - 6, CellSize - 6);
            ghostVulnerableImage = AssetManager.LoadSvgAsImage("pacman-ghost_blue_vulnerable.svg", CellSize - 6, CellSize - 6);
        }
        catch
        {
            // Assets will fall back to default rendering if loading fails
        }
    }
    
    private void InitializeComponent()
    {
        this.Text = "Pac-Man";
        this.Size = new Size(GridWidth * CellSize + 250, GridHeight * CellSize + 100);
        this.MinimumSize = new Size(GridWidth * CellSize + 250, GridHeight * CellSize + 100);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.Sizable;
        this.KeyPreview = true;
        this.KeyDown += PacManForm_KeyDown;
        this.Resize += PacManForm_Resize;
        this.DoubleBuffered = true;
        
        // Game panel
        gamePanel = new Panel
        {
            Location = new Point(10, 40),
            Size = new Size(GridWidth * CellSize, GridHeight * CellSize),
            BackColor = Color.Black,
            BorderStyle = BorderStyle.FixedSingle,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left // Don't anchor right
        };
        gamePanel.Paint += GamePanel_Paint;
        gamePanel.Resize += (s, e) => gamePanel.Invalidate();
        typeof(Panel).InvokeMember("DoubleBuffered",
            System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
            null, gamePanel, new object[] { true });
        this.Controls.Add(gamePanel);
        
        // Score label
        scoreLabel = new Label
        {
            Location = new Point(GridWidth * CellSize + 30, 40),
            Size = new Size(200, 30),
            Font = new Font("Arial", 14, FontStyle.Bold),
            Text = "Score: 0",
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        this.Controls.Add(scoreLabel);
        
        // Settings button
        settingsBtn = new Button
        {
            Text = "Settings",
            Location = new Point(GridWidth * CellSize + 30, 80),
            Size = new Size(150, 30),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        settingsBtn.Click += ShowSettings;
        this.Controls.Add(settingsBtn);
        
        // Scoreboard button
        scoreboardBtn = new Button
        {
            Text = "Scoreboard",
            Location = new Point(GridWidth * CellSize + 30, 120),
            Size = new Size(150, 30),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        scoreboardBtn.Click += ShowScoreboard;
        this.Controls.Add(scoreboardBtn);
        
        // New game button
        newGameBtn = new Button
        {
            Text = "New Game",
            Location = new Point(GridWidth * CellSize + 30, 160),
            Size = new Size(150, 30),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        newGameBtn.Click += (s, e) => InitializeGame();
        this.Controls.Add(newGameBtn);
    }
    
    private void InitializeGame()
    {
        // Create maze
        for (int y = 0; y < GridHeight; y++)
        {
            for (int x = 0; x < GridWidth; x++)
            {
                // Walls on borders
                if (x == 0 || x == GridWidth - 1 || y == 0 || y == GridHeight - 1)
                {
                    grid[y, x] = 3;
                }
                // Some internal walls
                else if ((x % 5 == 0 && y % 3 == 0) || (x % 7 == 0 && y % 5 == 0))
                {
                    grid[y, x] = 3;
                }
                // Supreme food (5 points) - less common
                else if ((x + y) % 13 == 0)
                {
                    grid[y, x] = 2;
                }
                // Regular food (1 point)
                else
                {
                    grid[y, x] = 1;
                }
            }
        }
        
        // Clear spawn area
        pacmanPos = new Point(1, 1);
        grid[1, 1] = 0;
        grid[1, 2] = 0;
        grid[2, 1] = 0;
        
        // Initialize ghosts
        ghosts.Clear();
        ghosts.Add(new Ghost { Position = new Point(GridWidth - 2, 1), Color = Color.Red });
        ghosts.Add(new Ghost { Position = new Point(1, GridHeight - 2), Color = Color.Cyan });
        ghosts.Add(new Ghost { Position = new Point(GridWidth - 2, GridHeight - 2), Color = Color.Orange });
        
        score = 0;
        ghostsVulnerable = false;
        vulnerableTimer = 0;
        pacmanDir = Direction.Right;
        
        UpdateScore();
        
        // Start game timer
        if (gameTimer != null)
        {
            gameTimer.Stop();
            gameTimer.Dispose();
        }
        
        gameTimer = new System.Windows.Forms.Timer();
        gameTimer.Interval = difficulty == 0 ? 200 : difficulty == 1 ? 150 : 100;
        gameTimer.Tick += GameTimer_Tick;
        gameTimer.Start();
        
        gamePanel?.Invalidate();
    }
    
    private void GameTimer_Tick(object? sender, EventArgs e)
    {
        // Move Pac-Man
        var newPos = GetNextPosition(pacmanPos, pacmanDir);
        if (grid[newPos.Y, newPos.X] != 3)
        {
            pacmanPos = newPos;
            
            // Check for food
            if (grid[pacmanPos.Y, pacmanPos.X] == 1)
            {
                score += 1;
                grid[pacmanPos.Y, pacmanPos.X] = 0;
                UpdateScore();
            }
            else if (grid[pacmanPos.Y, pacmanPos.X] == 2)
            {
                score += 5;
                grid[pacmanPos.Y, pacmanPos.X] = 0;
                ghostsVulnerable = true;
                vulnerableTimer = 50; // 50 ticks
                UpdateScore();
            }
        }
        
        // Update vulnerable timer
        if (ghostsVulnerable)
        {
            vulnerableTimer--;
            if (vulnerableTimer <= 0)
            {
                ghostsVulnerable = false;
            }
        }
        
        // Move ghosts
        foreach (var ghost in ghosts)
        {
            MoveGhost(ghost);
        }
        
        // Check collision with ghosts
        foreach (var ghost in ghosts)
        {
            if (ghost.Position == pacmanPos)
            {
                if (ghostsVulnerable)
                {
                    // Eat ghost
                    score += 10;
                    ghost.Position = new Point(GridWidth / 2, GridHeight / 2);
                    UpdateScore();
                }
                else
                {
                    // Game over
                    GameOver();
                    return;
                }
            }
        }
        
        // Check win condition
        bool hasFood = false;
        for (int y = 0; y < GridHeight; y++)
        {
            for (int x = 0; x < GridWidth; x++)
            {
                if (grid[y, x] == 1 || grid[y, x] == 2)
                {
                    hasFood = true;
                    break;
                }
            }
            if (hasFood) break;
        }
        
        if (!hasFood)
        {
            GameWon();
            return;
        }
        
        gamePanel?.Invalidate();
    }
    
    private void MoveGhost(Ghost ghost)
    {
        // Simple AI: if vulnerable, run away from Pac-Man, else chase Pac-Man
        Direction bestDir = Direction.Up;
        int bestScore = int.MaxValue;
        
        if (ghostsVulnerable)
        {
            // Run away - maximize distance
            bestScore = int.MinValue;
        }
        
        foreach (Direction dir in Enum.GetValues(typeof(Direction)))
        {
            var testPos = GetNextPosition(ghost.Position, dir);
            if (grid[testPos.Y, testPos.X] != 3)
            {
                int dist = Math.Abs(testPos.X - pacmanPos.X) + Math.Abs(testPos.Y - pacmanPos.Y);
                
                if (ghostsVulnerable)
                {
                    if (dist > bestScore)
                    {
                        bestScore = dist;
                        bestDir = dir;
                    }
                }
                else
                {
                    if (dist < bestScore)
                    {
                        bestScore = dist;
                        bestDir = dir;
                    }
                }
            }
        }
        
        ghost.Position = GetNextPosition(ghost.Position, bestDir);
    }
    
    private Point GetNextPosition(Point pos, Direction dir)
    {
        return dir switch
        {
            Direction.Up => new Point(pos.X, Math.Max(0, pos.Y - 1)),
            Direction.Down => new Point(pos.X, Math.Min(GridHeight - 1, pos.Y + 1)),
            Direction.Left => new Point(Math.Max(0, pos.X - 1), pos.Y),
            Direction.Right => new Point(Math.Min(GridWidth - 1, pos.X + 1), pos.Y),
            _ => pos
        };
    }
    
    private void PacManForm_KeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Escape:
                this.Close();
                break;
            case Keys.Up:
            case Keys.W:
                pacmanDir = Direction.Up;
                break;
            case Keys.Down:
            case Keys.S:
                pacmanDir = Direction.Down;
                break;
            case Keys.Left:
            case Keys.A:
                pacmanDir = Direction.Left;
                break;
            case Keys.Right:
            case Keys.D:
                pacmanDir = Direction.Right;
                break;
        }
    }
    
    private void GamePanel_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        
        // Draw grid
        for (int y = 0; y < GridHeight; y++)
        {
            for (int x = 0; x < GridWidth; x++)
            {
                var rect = new Rectangle(x * CellSize, y * CellSize, CellSize, CellSize);
                
                switch (grid[y, x])
                {
                    case 3: // Wall
                        g.FillRectangle(Brushes.Blue, rect);
                        break;
                    case 1: // Food
                        if (foodDotImage != null)
                        {
                            g.DrawImage(foodDotImage, x * CellSize + 12, y * CellSize + 12, 6, 6);
                        }
                        else
                        {
                            g.FillEllipse(Brushes.White, x * CellSize + 12, y * CellSize + 12, 6, 6);
                        }
                        break;
                    case 2: // Supreme food
                        if (powerPelletImage != null)
                        {
                            g.DrawImage(powerPelletImage, x * CellSize + 8, y * CellSize + 8, 14, 14);
                        }
                        else
                        {
                            g.FillEllipse(Brushes.Yellow, x * CellSize + 8, y * CellSize + 8, 14, 14);
                        }
                        break;
                }
            }
        }
        
        // Draw Pac-Man
        var pacmanRect = new Rectangle(pacmanPos.X * CellSize + 3, pacmanPos.Y * CellSize + 3, CellSize - 6, CellSize - 6);
        if (pacmanImage != null)
        {
            g.DrawImage(pacmanImage, pacmanRect);
        }
        else
        {
            g.FillEllipse(Brushes.Yellow, pacmanRect);
        }
        
        // Draw ghosts
        foreach (var ghost in ghosts)
        {
            var ghostRect = new Rectangle(ghost.Position.X * CellSize + 3, ghost.Position.Y * CellSize + 3, CellSize - 6, CellSize - 6);
            if (ghostsVulnerable && ghostVulnerableImage != null)
            {
                g.DrawImage(ghostVulnerableImage, ghostRect);
            }
            else if (!ghostsVulnerable && ghostRedImage != null)
            {
                g.DrawImage(ghostRedImage, ghostRect);
            }
            else
            {
                var ghostColor = ghostsVulnerable ? Color.Blue : ghost.Color;
                using (var brush = new SolidBrush(ghostColor))
                {
                    g.FillEllipse(brush, ghostRect);
                }
            }
        }
    }
    
    private void UpdateScore()
    {
        if (scoreLabel != null)
        {
            scoreLabel.Text = $"Score: {score}";
        }
    }
    
    private void GameOver()
    {
        gameTimer?.Stop();
        MessageBox.Show($"Game Over! Final Score: {score}", "Game Over");
        
        var name = Microsoft.VisualBasic.Interaction.InputBox("Enter your name:", "High Score", "Player");
        if (!string.IsNullOrWhiteSpace(name))
        {
            scoreManager.SaveScore("PacMan", new ScoreEntry { PlayerName = name, Score = score });
        }
        
        InitializeGame();
    }
    
    private void GameWon()
    {
        gameTimer?.Stop();
        MessageBox.Show($"You Won! Final Score: {score}", "Victory!");
        
        var name = Microsoft.VisualBasic.Interaction.InputBox("Enter your name:", "High Score", "Player");
        if (!string.IsNullOrWhiteSpace(name))
        {
            scoreManager.SaveScore("PacMan", new ScoreEntry { PlayerName = name, Score = score });
        }
        
        InitializeGame();
    }
    
    private void ShowSettings(object? sender, EventArgs e)
    {
        var settingsForm = new Form
        {
            Text = "Settings",
            Size = new Size(300, 200),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false
        };
        
        var diffLabel = new Label
        {
            Text = "Difficulty:",
            Location = new Point(20, 20),
            AutoSize = true
        };
        settingsForm.Controls.Add(diffLabel);
        
        var diffCombo = new ComboBox
        {
            Location = new Point(20, 50),
            Size = new Size(200, 25),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        diffCombo.Items.AddRange(new[] { "Easy", "Medium", "Hard" });
        diffCombo.SelectedIndex = difficulty;
        settingsForm.Controls.Add(diffCombo);
        
        var okBtn = new Button
        {
            Text = "OK",
            Location = new Point(100, 100),
            DialogResult = DialogResult.OK
        };
        settingsForm.Controls.Add(okBtn);
        settingsForm.AcceptButton = okBtn;
        
        if (settingsForm.ShowDialog(this) == DialogResult.OK)
        {
            difficulty = diffCombo.SelectedIndex;
            InitializeGame();
        }
    }
    
    private void ShowScoreboard(object? sender, EventArgs e)
    {
        var scores = scoreManager.LoadScores("PacMan");
        var scoreText = "Top Scores:\n\n";
        
        for (int i = 0; i < scores.Count; i++)
        {
            scoreText += $"{i + 1}. {scores[i].PlayerName}: {scores[i].Score} ({scores[i].Date:yyyy-MM-dd})\n";
        }
        
        if (scores.Count == 0)
        {
            scoreText += "No scores yet!";
        }
        
        MessageBox.Show(scoreText, "Scoreboard");
    }
    
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        gameTimer?.Stop();
        gameTimer?.Dispose();
        base.OnFormClosing(e);
    }
    
    private void PacManForm_Resize(object? sender, EventArgs e)
    {
        // Game panel automatically resizes via Anchor property
        gamePanel?.Invalidate();
    }
}

public class Ghost
{
    public Point Position { get; set; }
    public Color Color { get; set; }
}

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}
