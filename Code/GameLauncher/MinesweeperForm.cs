namespace GameLauncher;

public partial class MinesweeperForm : Form
{
    private const int CellSize = 30;
    private int gridWidth = 10;
    private int gridHeight = 10;
    private int mineCount = 10;
    
    private Cell[,]? cells;
    private Panel? gamePanel;
    private Label? statusLabel;
    private Button? settingsBtn;
    private Button? newGameBtn;
    private Button? scoreboardBtn;
    private bool gameOver = false;
    private int flagsPlaced = 0;
    private int cellsRevealed = 0;
    private HighScoreManager scoreManager = new HighScoreManager();
    private int difficulty = 0; // 0=easy, 1=medium, 2=hard
    
    public MinesweeperForm()
    {
        InitializeComponent();
        InitializeGame();
    }
    
    private void InitializeComponent()
    {
        this.Text = "Minesweeper";
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.Sizable;
        this.KeyPreview = true;
        this.KeyDown += MinesweeperForm_KeyDown;
        this.Resize += MinesweeperForm_Resize;
        this.DoubleBuffered = true;
        
        UpdateFormSize();
        
        // Status label
        statusLabel = new Label
        {
            Location = new Point(10, 10),
            Size = new Size(300, 25),
            Font = new Font("Arial", 12, FontStyle.Bold)
        };
        this.Controls.Add(statusLabel);
        
        // Game panel - will be created in UpdateFormSize
        
        // Settings button
        settingsBtn = new Button
        {
            Text = "Settings",
            Location = new Point(gridWidth * CellSize + 30, 40),
            Size = new Size(150, 30)
        };
        settingsBtn.Click += ShowSettings;
        this.Controls.Add(settingsBtn);
        
        // New game button
        newGameBtn = new Button
        {
            Text = "New Game",
            Location = new Point(gridWidth * CellSize + 30, 80),
            Size = new Size(150, 30)
        };
        newGameBtn.Click += (s, e) => InitializeGame();
        this.Controls.Add(newGameBtn);
        
        // Scoreboard button
        scoreboardBtn = new Button
        {
            Text = "Scoreboard",
            Location = new Point(gridWidth * CellSize + 30, 120),
            Size = new Size(150, 30)
        };
        scoreboardBtn.Click += ShowScoreboard;
        this.Controls.Add(scoreboardBtn);
    }
    
    private void UpdateFormSize()
    {
        this.ClientSize = new Size(gridWidth * CellSize + 210, Math.Max(gridHeight * CellSize + 60, 250));
        this.MinimumSize = new Size(gridWidth * CellSize + 210, Math.Max(gridHeight * CellSize + 60, 250));
        
        if (gamePanel != null)
        {
            this.Controls.Remove(gamePanel);
        }
        
        gamePanel = new Panel
        {
            Location = new Point(10, 40),
            Size = new Size(gridWidth * CellSize, gridHeight * CellSize),
            BackColor = Color.LightGray,
            BorderStyle = BorderStyle.FixedSingle,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left // Don't anchor right
        };
        gamePanel.Paint += GamePanel_Paint;
        gamePanel.MouseClick += GamePanel_MouseClick;
        gamePanel.Resize += (s, e) => gamePanel.Invalidate();
        typeof(Panel).InvokeMember("DoubleBuffered",
            System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
            null, gamePanel, new object[] { true });
        this.Controls.Add(gamePanel);
        
        // Update button positions with anchoring - buttons stay on right
        if (settingsBtn != null)
        {
            settingsBtn.Location = new Point(gridWidth * CellSize + 30, 40);
            settingsBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        }
        if (newGameBtn != null)
        {
            newGameBtn.Location = new Point(gridWidth * CellSize + 30, 80);
            newGameBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        }
        if (scoreboardBtn != null)
        {
            scoreboardBtn.Location = new Point(gridWidth * CellSize + 30, 120);
            scoreboardBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        }
    }
    
    private void InitializeGame()
    {
        cells = new Cell[gridHeight, gridWidth];
        gameOver = false;
        flagsPlaced = 0;
        cellsRevealed = 0;
        
        // Initialize cells
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                cells[y, x] = new Cell();
            }
        }
        
        // Place mines
        Random rand = new Random();
        int minesPlaced = 0;
        
        while (minesPlaced < mineCount)
        {
            int x = rand.Next(gridWidth);
            int y = rand.Next(gridHeight);
            
            if (!cells[y, x].IsMine)
            {
                cells[y, x].IsMine = true;
                minesPlaced++;
            }
        }
        
        // Calculate adjacent mine counts
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (!cells[y, x].IsMine)
                {
                    cells[y, x].AdjacentMines = CountAdjacentMines(x, y);
                }
            }
        }
        
        UpdateStatus();
        gamePanel?.Invalidate();
    }
    
    private int CountAdjacentMines(int x, int y)
    {
        int count = 0;
        
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0) continue;
                
                int nx = x + dx;
                int ny = y + dy;
                
                if (nx >= 0 && nx < gridWidth && ny >= 0 && ny < gridHeight)
                {
                    if (cells![ny, nx].IsMine)
                        count++;
                }
            }
        }
        
        return count;
    }
    
    private void GamePanel_MouseClick(object? sender, MouseEventArgs e)
    {
        if (gameOver || cells == null || gamePanel == null) return;
        
        // Calculate scale
        float scaleX = (float)gamePanel.Width / (gridWidth * CellSize);
        float scaleY = (float)gamePanel.Height / (gridHeight * CellSize);
        float scale = Math.Min(scaleX, scaleY);
        float scaledCellSize = CellSize * scale;
        
        int x = (int)(e.X / scaledCellSize);
        int y = (int)(e.Y / scaledCellSize);
        
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight) return;
        
        if (e.Button == MouseButtons.Left)
        {
            // Reveal cell
            RevealCell(x, y);
        }
        else if (e.Button == MouseButtons.Right)
        {
            // Flag cell
            if (!cells[y, x].IsRevealed)
            {
                cells[y, x].IsFlagged = !cells[y, x].IsFlagged;
                flagsPlaced += cells[y, x].IsFlagged ? 1 : -1;
                UpdateStatus();
            }
        }
        
        gamePanel?.Invalidate();
    }
    
    private void RevealCell(int x, int y)
    {
        if (cells == null) return;
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight) return;
        if (cells[y, x].IsRevealed || cells[y, x].IsFlagged) return;
        
        cells[y, x].IsRevealed = true;
        cellsRevealed++;
        
        if (cells[y, x].IsMine)
        {
            // Game over
            gameOver = true;
            RevealAllMines();
            MessageBox.Show("Game Over! You hit a mine!", "Game Over");
            return;
        }
        
        // If no adjacent mines, reveal neighbors
        if (cells[y, x].AdjacentMines == 0)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue;
                    RevealCell(x + dx, y + dy);
                }
            }
        }
        
        // Check win condition
        int totalCells = gridWidth * gridHeight;
        if (cellsRevealed == totalCells - mineCount)
        {
            gameOver = true;
            MessageBox.Show("Congratulations! You won!", "Victory!");
            
            var name = Microsoft.VisualBasic.Interaction.InputBox("Enter your name:", "High Score", "Player");
            if (!string.IsNullOrWhiteSpace(name))
            {
                int score = (totalCells - mineCount) * (difficulty + 1) * 10;
                scoreManager.SaveScore("Minesweeper", new ScoreEntry { PlayerName = name, Score = score });
            }
        }
    }
    
    private void RevealAllMines()
    {
        if (cells == null) return;
        
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (cells[y, x].IsMine)
                {
                    cells[y, x].IsRevealed = true;
                }
            }
        }
    }
    
    private void GamePanel_Paint(object? sender, PaintEventArgs e)
    {
        if (cells == null || gamePanel == null) return;
        
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        
        // Calculate scale to fit the panel
        float scaleX = (float)gamePanel.Width / (gridWidth * CellSize);
        float scaleY = (float)gamePanel.Height / (gridHeight * CellSize);
        float scale = Math.Min(scaleX, scaleY);
        
        // Calculate scaled cell size
        float scaledCellSize = CellSize * scale;
        
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                var rect = new RectangleF(x * scaledCellSize, y * scaledCellSize, scaledCellSize, scaledCellSize);
                var cell = cells[y, x];
                
                if (cell.IsRevealed)
                {
                    g.FillRectangle(Brushes.White, rect);
                    
                    if (cell.IsMine)
                    {
                        g.FillEllipse(Brushes.Red, x * scaledCellSize + 5 * scale, y * scaledCellSize + 5 * scale, 
                            scaledCellSize - 10 * scale, scaledCellSize - 10 * scale);
                    }
                    else if (cell.AdjacentMines > 0)
                    {
                        var fontSize = Math.Max(8, 12 * scale);
                        var font = new Font("Arial", fontSize, FontStyle.Bold);
                        var color = cell.AdjacentMines switch
                        {
                            1 => Brushes.Blue,
                            2 => Brushes.Green,
                            3 => Brushes.Red,
                            4 => Brushes.DarkBlue,
                            5 => Brushes.DarkRed,
                            _ => Brushes.Black
                        };
                        
                        var text = cell.AdjacentMines.ToString();
                        var size = g.MeasureString(text, font);
                        g.DrawString(text, font, color,
                            x * scaledCellSize + (scaledCellSize - size.Width) / 2,
                            y * scaledCellSize + (scaledCellSize - size.Height) / 2);
                    }
                }
                else
                {
                    g.FillRectangle(Brushes.LightGray, rect);
                    
                    if (cell.IsFlagged)
                    {
                        var flagPoints = new PointF[]
                        {
                            new PointF(x * scaledCellSize + 8 * scale, y * scaledCellSize + 8 * scale),
                            new PointF(x * scaledCellSize + 22 * scale, y * scaledCellSize + 15 * scale),
                            new PointF(x * scaledCellSize + 8 * scale, y * scaledCellSize + 22 * scale)
                        };
                        g.FillPolygon(Brushes.Red, flagPoints);
                    }
                }
                
                g.DrawRectangle(Pens.Gray, rect.X, rect.Y, rect.Width, rect.Height);
            }
        }
    }
    
    private void UpdateStatus()
    {
        if (statusLabel != null)
        {
            statusLabel.Text = $"Mines: {mineCount} | Flags: {flagsPlaced}";
        }
    }
    
    private void ShowSettings(object? sender, EventArgs e)
    {
        var settingsForm = new Form
        {
            Text = "Settings",
            Size = new Size(350, 250),
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
        diffCombo.Items.AddRange(new[] { "Easy (10x10, 10 mines)", "Medium (15x15, 30 mines)", "Hard (20x20, 60 mines)" });
        diffCombo.SelectedIndex = difficulty;
        settingsForm.Controls.Add(diffCombo);
        
        var sizeLabel = new Label
        {
            Text = "Custom Grid Size:",
            Location = new Point(20, 90),
            AutoSize = true
        };
        settingsForm.Controls.Add(sizeLabel);
        
        var widthInput = new NumericUpDown
        {
            Location = new Point(20, 120),
            Size = new Size(80, 25),
            Minimum = 5,
            Maximum = 30,
            Value = gridWidth
        };
        settingsForm.Controls.Add(widthInput);
        
        var xLabel = new Label
        {
            Text = "x",
            Location = new Point(105, 120),
            AutoSize = true
        };
        settingsForm.Controls.Add(xLabel);
        
        var heightInput = new NumericUpDown
        {
            Location = new Point(120, 120),
            Size = new Size(80, 25),
            Minimum = 5,
            Maximum = 30,
            Value = gridHeight
        };
        settingsForm.Controls.Add(heightInput);
        
        var okBtn = new Button
        {
            Text = "OK",
            Location = new Point(120, 170),
            DialogResult = DialogResult.OK
        };
        settingsForm.Controls.Add(okBtn);
        settingsForm.AcceptButton = okBtn;
        
        if (settingsForm.ShowDialog(this) == DialogResult.OK)
        {
            difficulty = diffCombo.SelectedIndex;
            
            switch (difficulty)
            {
                case 0: // Easy
                    gridWidth = 10;
                    gridHeight = 10;
                    mineCount = 10;
                    break;
                case 1: // Medium
                    gridWidth = 15;
                    gridHeight = 15;
                    mineCount = 30;
                    break;
                case 2: // Hard
                    gridWidth = 20;
                    gridHeight = 20;
                    mineCount = 60;
                    break;
            }
            
            UpdateFormSize();
            InitializeGame();
        }
    }
    
    private void ShowScoreboard(object? sender, EventArgs e)
    {
        var scores = scoreManager.LoadScores("Minesweeper");
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
    
    private void MinesweeperForm_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            this.Close();
        }
    }
    
    private void MinesweeperForm_Resize(object? sender, EventArgs e)
    {
        // Game panel automatically resizes via Anchor property
        // Just refresh the display
        gamePanel?.Invalidate();
    }
}

public class Cell
{
    public bool IsMine { get; set; }
    public bool IsRevealed { get; set; }
    public bool IsFlagged { get; set; }
    public int AdjacentMines { get; set; }
}
