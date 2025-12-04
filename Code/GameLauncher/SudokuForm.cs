namespace GameLauncher;

public partial class SudokuForm : Form
{
    private const int GridSize = 9;
    private const int CellSize = 50;
    
    private int[,] puzzle = new int[GridSize, GridSize];
    private int[,] solution = new int[GridSize, GridSize];
    private bool[,] isFixed = new bool[GridSize, GridSize];
    private HashSet<string>[,] drafts = new HashSet<string>[GridSize, GridSize];
    
    private Panel? gamePanel;
    private Point? selectedCell = null;
    private HighScoreManager scoreManager = new HighScoreManager();
    private Label? instructLabel;
    private Button? newPuzzleBtn;
    private Button? checkBtn;
    private Button? scoreboardBtn;
    
    public SudokuForm()
    {
        InitializeComponent();
        GenerateNewPuzzle();
    }
    
    private void InitializeComponent()
    {
        this.Text = "Sudoku";
        this.Size = new Size(GridSize * CellSize + 250, GridSize * CellSize + 100);
        this.MinimumSize = new Size(GridSize * CellSize + 250, GridSize * CellSize + 100);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.Sizable;
        this.KeyPreview = true;
        this.KeyDown += SudokuForm_KeyDown;
        this.Resize += SudokuForm_Resize;
        this.DoubleBuffered = true;
        
        // Game panel
        gamePanel = new Panel
        {
            Location = new Point(10, 40),
            Size = new Size(GridSize * CellSize, GridSize * CellSize),
            BackColor = Color.White,
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
        
        // Instructions
        instructLabel = new Label
        {
            Location = new Point(GridSize * CellSize + 30, 40),
            Size = new Size(200, 100),
            Text = "Click cell, then:\nNumber: Enter value\n(blue)\n\nCtrl + Number:\nDraft (red)\n\nBackspace: Clear",
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        this.Controls.Add(instructLabel);
        
        // New puzzle button
        newPuzzleBtn = new Button
        {
            Text = "New Puzzle",
            Location = new Point(GridSize * CellSize + 30, 150),
            Size = new Size(150, 30),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        newPuzzleBtn.Click += (s, e) => GenerateNewPuzzle();
        this.Controls.Add(newPuzzleBtn);
        
        // Check button
        checkBtn = new Button
        {
            Text = "Check Solution",
            Location = new Point(GridSize * CellSize + 30, 190),
            Size = new Size(150, 30),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        checkBtn.Click += CheckSolution;
        this.Controls.Add(checkBtn);
        
        // Scoreboard button
        scoreboardBtn = new Button
        {
            Text = "Scoreboard",
            Location = new Point(GridSize * CellSize + 30, 230),
            Size = new Size(150, 30),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        scoreboardBtn.Click += ShowScoreboard;
        this.Controls.Add(scoreboardBtn);
    }
    
    private void GenerateNewPuzzle()
    {
        // Initialize arrays
        for (int i = 0; i < GridSize; i++)
        {
            for (int j = 0; j < GridSize; j++)
            {
                drafts[i, j] = new HashSet<string>();
            }
        }
        
        // Generate a valid solved Sudoku
        Array.Clear(solution, 0, solution.Length);
        FillDiagonal();
        SolveSudoku(solution);
        
        // Copy to puzzle and remove some numbers
        Array.Copy(solution, puzzle, solution.Length);
        Array.Clear(isFixed, 0, isFixed.Length);
        
        Random rand = new Random();
        int numbersToRemove = 40; // Adjust for difficulty
        
        for (int i = 0; i < numbersToRemove; i++)
        {
            int row = rand.Next(GridSize);
            int col = rand.Next(GridSize);
            
            if (puzzle[row, col] != 0)
            {
                puzzle[row, col] = 0;
            }
            else
            {
                i--;
            }
        }
        
        // Mark fixed cells
        for (int i = 0; i < GridSize; i++)
        {
            for (int j = 0; j < GridSize; j++)
            {
                isFixed[i, j] = puzzle[i, j] != 0;
            }
        }
        
        selectedCell = null;
        gamePanel?.Invalidate();
    }
    
    private void FillDiagonal()
    {
        for (int box = 0; box < GridSize; box += 3)
        {
            FillBox(box, box);
        }
    }
    
    private void FillBox(int row, int col)
    {
        Random rand = new Random();
        List<int> numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int idx = rand.Next(numbers.Count);
                solution[row + i, col + j] = numbers[idx];
                numbers.RemoveAt(idx);
            }
        }
    }
    
    private bool SolveSudoku(int[,] grid)
    {
        for (int row = 0; row < GridSize; row++)
        {
            for (int col = 0; col < GridSize; col++)
            {
                if (grid[row, col] == 0)
                {
                    for (int num = 1; num <= 9; num++)
                    {
                        if (IsSafe(grid, row, col, num))
                        {
                            grid[row, col] = num;
                            
                            if (SolveSudoku(grid))
                                return true;
                            
                            grid[row, col] = 0;
                        }
                    }
                    return false;
                }
            }
        }
        return true;
    }
    
    private bool IsSafe(int[,] grid, int row, int col, int num)
    {
        // Check row
        for (int x = 0; x < GridSize; x++)
            if (grid[row, x] == num)
                return false;
        
        // Check column
        for (int x = 0; x < GridSize; x++)
            if (grid[x, col] == num)
                return false;
        
        // Check 3x3 box
        int startRow = row - row % 3;
        int startCol = col - col % 3;
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                if (grid[i + startRow, j + startCol] == num)
                    return false;
        
        return true;
    }
    
    private void GamePanel_MouseClick(object? sender, MouseEventArgs e)
    {
        int col = e.X / CellSize;
        int row = e.Y / CellSize;
        
        if (row >= 0 && row < GridSize && col >= 0 && col < GridSize)
        {
            if (!isFixed[row, col])
            {
                selectedCell = new Point(col, row);
                gamePanel?.Invalidate();
            }
        }
    }
    
    private void SudokuForm_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            this.Close();
            return;
        }
        
        if (selectedCell == null) return;
        
        int row = selectedCell.Value.Y;
        int col = selectedCell.Value.X;
        
        if (isFixed[row, col]) return;
        
        // Number keys
        if (e.KeyCode >= Keys.D1 && e.KeyCode <= Keys.D9)
        {
            int num = e.KeyCode - Keys.D0;
            
            if (e.Control)
            {
                // Draft mode (red)
                drafts[row, col].Add(num.ToString());
            }
            else
            {
                // Submit mode (blue)
                puzzle[row, col] = num;
                drafts[row, col].Clear();
            }
            
            gamePanel?.Invalidate();
        }
        else if (e.KeyCode >= Keys.NumPad1 && e.KeyCode <= Keys.NumPad9)
        {
            int num = e.KeyCode - Keys.NumPad0;
            
            if (e.Control)
            {
                // Draft mode (red)
                drafts[row, col].Add(num.ToString());
            }
            else
            {
                // Submit mode (blue)
                puzzle[row, col] = num;
                drafts[row, col].Clear();
            }
            
            gamePanel?.Invalidate();
        }
        else if (e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete)
        {
            puzzle[row, col] = 0;
            drafts[row, col].Clear();
            gamePanel?.Invalidate();
        }
    }
    
    private void GamePanel_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        
        // Draw cells
        for (int row = 0; row < GridSize; row++)
        {
            for (int col = 0; col < GridSize; col++)
            {
                var rect = new Rectangle(col * CellSize, row * CellSize, CellSize, CellSize);
                
                // Background
                if (selectedCell.HasValue && selectedCell.Value.X == col && selectedCell.Value.Y == row)
                {
                    g.FillRectangle(Brushes.LightYellow, rect);
                }
                else if (isFixed[row, col])
                {
                    g.FillRectangle(Brushes.LightGray, rect);
                }
                
                // Draw number or drafts
                if (puzzle[row, col] != 0)
                {
                    var font = new Font("Arial", 20, FontStyle.Bold);
                    var brush = isFixed[row, col] ? Brushes.Black : Brushes.Blue;
                    var text = puzzle[row, col].ToString();
                    var size = g.MeasureString(text, font);
                    g.DrawString(text, font, brush, 
                        col * CellSize + (CellSize - size.Width) / 2,
                        row * CellSize + (CellSize - size.Height) / 2);
                }
                else if (drafts[row, col].Count > 0)
                {
                    var font = new Font("Arial", 8);
                    var draftText = string.Join(",", drafts[row, col].OrderBy(d => d));
                    g.DrawString(draftText, font, Brushes.Red, col * CellSize + 2, row * CellSize + 2);
                }
                
                // Grid lines
                g.DrawRectangle(Pens.Gray, rect);
            }
        }
        
        // Draw thick lines for 3x3 boxes
        using (var thickPen = new Pen(Color.Black, 3))
        {
            for (int i = 0; i <= GridSize; i += 3)
            {
                g.DrawLine(thickPen, 0, i * CellSize, GridSize * CellSize, i * CellSize);
                g.DrawLine(thickPen, i * CellSize, 0, i * CellSize, GridSize * CellSize);
            }
        }
    }
    
    private void CheckSolution(object? sender, EventArgs e)
    {
        bool correct = true;
        int emptyCells = 0;
        
        for (int row = 0; row < GridSize; row++)
        {
            for (int col = 0; col < GridSize; col++)
            {
                if (puzzle[row, col] == 0)
                {
                    emptyCells++;
                }
                else if (puzzle[row, col] != solution[row, col])
                {
                    correct = false;
                }
            }
        }
        
        if (emptyCells > 0)
        {
            MessageBox.Show("Puzzle is not complete yet!", "Incomplete");
        }
        else if (correct)
        {
            MessageBox.Show("Congratulations! Solution is correct!", "Victory!");
            var name = Microsoft.VisualBasic.Interaction.InputBox("Enter your name:", "High Score", "Player");
            if (!string.IsNullOrWhiteSpace(name))
            {
                scoreManager.SaveScore("Sudoku", new ScoreEntry { PlayerName = name, Score = 100 });
            }
            GenerateNewPuzzle();
        }
        else
        {
            MessageBox.Show("Solution is incorrect. Keep trying!", "Incorrect");
        }
    }
    
    private void ShowScoreboard(object? sender, EventArgs e)
    {
        var scores = scoreManager.LoadScores("Sudoku");
        var scoreText = "Top Scores:\n\n";
        
        for (int i = 0; i < scores.Count; i++)
        {
            scoreText += $"{i + 1}. {scores[i].PlayerName} ({scores[i].Date:yyyy-MM-dd})\n";
        }
        
        if (scores.Count == 0)
        {
            scoreText += "No scores yet!";
        }
        
        MessageBox.Show(scoreText, "Scoreboard");
    }
    
    private void SudokuForm_Resize(object? sender, EventArgs e)
    {
        // Game panel automatically resizes via Anchor property
        gamePanel?.Invalidate();
    }
}
