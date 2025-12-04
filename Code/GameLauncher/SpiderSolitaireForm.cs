namespace GameLauncher;

public partial class SpiderSolitaireForm : Form
{
    private const int CardWidth = 80;
    private const int CardHeight = 120;
    private const int CardOffset = 25;
    
    private List<List<Card>> tableau = new List<List<Card>>();
    private List<Card> stock = new List<Card>();
    private List<List<Card>> foundation = new List<List<Card>>();
    private Stack<GameState> undoStack = new Stack<GameState>();
    
    private Card? draggedCard = null;
    private Point dragOffset;
    private int dragSourceColumn = -1;
    private int dragCardIndex = -1;
    private bool isDragging = false;
    private Point mouseDownPos;
    
    // Animation fields
    private System.Windows.Forms.Timer? animationTimer;
    private List<Card>? animatingCards = null;
    private Point animationStart;
    private Point animationEnd;
    private int animationFrame = 0;
    private const int AnimationFrames = 15;
    private int animationTargetColumn = -1;
    private int animationSourceColumn = -1;
    private int animationSourceCardIndex = -1;
    
    // Hint animation fields
    private System.Windows.Forms.Timer? hintTimer;
    private int hintSourceColumn = -1;
    private int hintSourceCardIndex = -1;
    private int hintTargetColumn = -1;
    private int hintBlinkCount = 0;
    private const int HintBlinkTotal = 6; // 3 blinks (on/off cycles)
    
    private int suitCount = 1; // 1, 2, or 4 suits
    private int score = 0;
    private Panel? gamePanel;
    private Label? scoreLabel;
    private HighScoreManager scoreManager = new HighScoreManager();
    
    public SpiderSolitaireForm()
    {
        InitializeComponent();
        InitializeGame();
    }
    
    private void InitializeComponent()
    {
        this.Text = "Spider Solitaire";
        this.Size = new Size(900, 700);
        this.MinimumSize = new Size(900, 700);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.Sizable;
        this.KeyPreview = true;
        this.KeyDown += SpiderSolitaireForm_KeyDown;
        this.Resize += SpiderSolitaireForm_Resize;
        this.DoubleBuffered = true;
        
        // Score label
        scoreLabel = new Label
        {
            Location = new Point(20, 10),
            Size = new Size(200, 25),
            Font = new Font("Arial", 12, FontStyle.Bold),
            Text = "Score: 0",
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };
        this.Controls.Add(scoreLabel);
        
        // Game panel
        gamePanel = new Panel
        {
            Location = new Point(10, 50),
            Size = new Size(860, 600),
            BackColor = Color.DarkGreen,
            BorderStyle = BorderStyle.FixedSingle,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right // Solitaire can expand in all directions
        };
        gamePanel.Paint += GamePanel_Paint;
        gamePanel.MouseDown += GamePanel_MouseDown;
        gamePanel.MouseMove += GamePanel_MouseMove;
        gamePanel.MouseUp += GamePanel_MouseUp;
        gamePanel.Resize += (s, e) => gamePanel.Invalidate();
        typeof(Panel).InvokeMember("DoubleBuffered",
            System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
            null, gamePanel, new object[] { true });
        this.Controls.Add(gamePanel);
        
        // Settings button
        var settingsBtn = new Button
        {
            Text = "Settings",
            Location = new Point(250, 10),
            Size = new Size(100, 30),
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };
        settingsBtn.Click += ShowSettings;
        this.Controls.Add(settingsBtn);
        
        // Hint button
        var hintBtn = new Button
        {
            Text = "Hint",
            Location = new Point(360, 10),
            Size = new Size(100, 30),
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };
        hintBtn.Click += ShowHint;
        this.Controls.Add(hintBtn);
        
        // Undo button
        var undoBtn = new Button
        {
            Text = "Undo",
            Location = new Point(470, 10),
            Size = new Size(100, 30),
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };
        undoBtn.Click += UndoMove;
        this.Controls.Add(undoBtn);
        
        // New game button
        var newGameBtn = new Button
        {
            Text = "New Game",
            Location = new Point(580, 10),
            Size = new Size(100, 30),
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };
        newGameBtn.Click += (s, e) => InitializeGame();
        this.Controls.Add(newGameBtn);
        
        // Scoreboard button
        var scoreboardBtn = new Button
        {
            Text = "Scoreboard",
            Location = new Point(690, 10),
            Size = new Size(100, 30),
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };
        scoreboardBtn.Click += ShowScoreboard;
        this.Controls.Add(scoreboardBtn);
    }
    
    private void InitializeGame()
    {
        tableau.Clear();
        stock.Clear();
        foundation.Clear();
        undoStack.Clear();
        score = 0;
        
        // Create deck(s)
        var deck = new List<Card>();
        string[] suits = suitCount switch
        {
            1 => new[] { "Spades", "Spades", "Spades", "Spades", "Spades", "Spades", "Spades", "Spades" },
            2 => new[] { "Spades", "Spades", "Spades", "Spades", "Hearts", "Hearts", "Hearts", "Hearts" },
            _ => new[] { "Spades", "Spades", "Hearts", "Hearts", "Diamonds", "Diamonds", "Clubs", "Clubs" }
        };
        
        foreach (var suit in suits)
        {
            for (int rank = 1; rank <= 13; rank++)
            {
                deck.Add(new Card { Suit = suit, Rank = rank, FaceUp = false });
            }
        }
        
        // Shuffle
        Random rand = new Random();
        deck = deck.OrderBy(c => rand.Next()).ToList();
        
        // Deal to tableau (10 columns)
        for (int i = 0; i < 10; i++)
        {
            tableau.Add(new List<Card>());
        }
        
        int cardIndex = 0;
        // First 4 columns get 6 cards, rest get 5
        for (int col = 0; col < 10; col++)
        {
            int cardsInColumn = col < 4 ? 6 : 5;
            for (int i = 0; i < cardsInColumn; i++)
            {
                if (cardIndex < deck.Count)
                {
                    var card = deck[cardIndex++];
                    if (i == cardsInColumn - 1)
                        card.FaceUp = true;
                    tableau[col].Add(card);
                }
            }
        }
        
        // Remaining cards go to stock
        while (cardIndex < deck.Count)
        {
            stock.Add(deck[cardIndex++]);
        }
        
        UpdateScore();
        gamePanel?.Invalidate();
    }
    
    private void GamePanel_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        
        // Draw tableau columns
        for (int col = 0; col < tableau.Count; col++)
        {
            int x = col * (CardWidth + 5) + 10;
            int y = 10;
            
            // Draw column placeholder
            g.DrawRectangle(Pens.White, x, y, CardWidth, CardHeight);
            
            // Draw cards in column
            for (int i = 0; i < tableau[col].Count; i++)
            {
                var card = tableau[col][i];
                int cardY = y + i * CardOffset;
                
                // Skip if this is the dragged card during dragging
                if (draggedCard != null && isDragging && col == dragSourceColumn && i >= dragCardIndex)
                    continue;
                
                // Skip if animating and this is from the source column
                if (animatingCards != null && col == animationSourceColumn && i >= animationSourceCardIndex)
                    continue;
                
                DrawCard(g, card, x, cardY);
                
                // Highlight selected cards in click-to-move mode
                if (draggedCard != null && !isDragging && col == dragSourceColumn && i >= dragCardIndex)
                {
                    using (var pen = new Pen(Color.Yellow, 3))
                    {
                        g.DrawRectangle(pen, x, cardY, CardWidth, CardHeight);
                    }
                }
                
                // Highlight hint cards (blink effect)
                if (hintSourceColumn >= 0 && col == hintSourceColumn && i >= hintSourceCardIndex && (hintBlinkCount % 2 == 0))
                {
                    using (var pen = new Pen(Color.LimeGreen, 4))
                    {
                        g.DrawRectangle(pen, x - 2, cardY - 2, CardWidth + 4, CardHeight + 4);
                    }
                }
            }
        }
        
        // Draw stock
        if (stock.Count > 0)
        {
            g.FillRectangle(Brushes.Gray, 10, 500, CardWidth, CardHeight / 2);
            g.DrawString($"Stock: {stock.Count}", new Font("Arial", 10), Brushes.White, 15, 510);
        }
        
        // Draw foundation
        for (int i = 0; i < foundation.Count; i++)
        {
            int x = 200 + i * (CardWidth + 5);
            g.FillRectangle(Brushes.DarkGray, x, 500, CardWidth, CardHeight / 2);
            g.DrawString($"Complete {i + 1}", new Font("Arial", 8), Brushes.White, x + 5, 510);
        }
        
        // Highlight hint target column
        if (hintTargetColumn >= 0 && (hintBlinkCount % 2 == 0))
        {
            int x = hintTargetColumn * (CardWidth + 5) + 10;
            int y = 10 + tableau[hintTargetColumn].Count * CardOffset;
            using (var pen = new Pen(Color.LimeGreen, 4))
            {
                g.DrawRectangle(pen, x - 2, y - 2, CardWidth + 4, CardHeight + 4);
            }
        }
        
        // Draw dragged card(s) - only during dragging
        if (draggedCard != null && isDragging && dragSourceColumn >= 0 && dragCardIndex >= 0)
        {
            int offsetY = 0;
            for (int i = dragCardIndex; i < tableau[dragSourceColumn].Count; i++)
            {
                var card = tableau[dragSourceColumn][i];
                DrawCard(g, card, Cursor.Position.X - dragOffset.X - gamePanel!.Left - this.Left,
                    Cursor.Position.Y - dragOffset.Y - gamePanel.Top - this.Top + offsetY);
                offsetY += CardOffset;
            }
        }
        
        // Draw animating cards
        if (animatingCards != null && animationFrame > 0)
        {
            float t = (float)animationFrame / AnimationFrames;
            int currentX = (int)(animationStart.X + (animationEnd.X - animationStart.X) * t);
            int currentY = (int)(animationStart.Y + (animationEnd.Y - animationStart.Y) * t);
            
            int offsetY = 0;
            foreach (var card in animatingCards)
            {
                DrawCard(g, card, currentX, currentY + offsetY);
                offsetY += CardOffset;
            }
        }
    }
    
    private void DrawCard(Graphics g, Card card, int x, int y)
    {
        if (!card.FaceUp)
        {
            g.FillRectangle(Brushes.Blue, x, y, CardWidth, CardHeight);
            g.DrawRectangle(Pens.White, x, y, CardWidth, CardHeight);
        }
        else
        {
            g.FillRectangle(Brushes.White, x, y, CardWidth, CardHeight);
            g.DrawRectangle(Pens.Black, x, y, CardWidth, CardHeight);
            
            var color = card.Suit == "Hearts" || card.Suit == "Diamonds" ? Brushes.Red : Brushes.Black;
            
            string rankStr = card.Rank switch
            {
                1 => "A",
                11 => "J",
                12 => "Q",
                13 => "K",
                _ => card.Rank.ToString()
            };
            
            g.DrawString(rankStr, new Font("Arial", 16, FontStyle.Bold), color, x + 5, y + 5);
            
            string suitSymbol = card.Suit switch
            {
                "Spades" => "♠",
                "Hearts" => "♥",
                "Diamonds" => "♦",
                "Clubs" => "♣",
                _ => ""
            };
            
            g.DrawString(suitSymbol, new Font("Arial", 20), color, x + 30, y + 40);
        }
    }
    
    private void GamePanel_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;
        
        mouseDownPos = e.Location;
        isDragging = false;
        
        // Check if clicking on stock
        if (e.X >= 10 && e.X <= 10 + CardWidth && e.Y >= 500 && e.Y <= 500 + CardHeight / 2 && stock.Count > 0)
        {
            DealFromStock();
            return;
        }
        
        // Check if clicking on a card in tableau
        for (int col = 0; col < tableau.Count; col++)
        {
            int x = col * (CardWidth + 5) + 10;
            int y = 10;
            
            for (int i = tableau[col].Count - 1; i >= 0; i--)
            {
                int cardY = y + i * CardOffset;
                
                if (e.X >= x && e.X <= x + CardWidth && e.Y >= cardY && e.Y <= cardY + CardHeight)
                {
                    var card = tableau[col][i];
                    if (card.FaceUp && CanMoveSequence(col, i))
                    {
                        // Check if this is a click on an already selected card
                        if (draggedCard != null && dragSourceColumn == col && dragCardIndex == i)
                        {
                            // Deselect
                            draggedCard = null;
                            dragSourceColumn = -1;
                            dragCardIndex = -1;
                            gamePanel?.Invalidate();
                        }
                        else
                        {
                            // Select card for click-to-move or drag
                            draggedCard = card;
                            dragSourceColumn = col;
                            dragCardIndex = i;
                            dragOffset = new Point(e.X - x, e.Y - cardY);
                            if (draggedCard != null)
                                SaveGameState();
                        }
                    }
                    return;
                }
            }
        }
    }
    
    private bool CanMoveSequence(int col, int startIndex)
    {
        // Check if cards from startIndex to end form a valid descending sequence
        for (int i = startIndex; i < tableau[col].Count - 1; i++)
        {
            if (!tableau[col][i].FaceUp) return false;
            if (tableau[col][i].Rank != tableau[col][i + 1].Rank + 1) return false;
            if (suitCount > 1 && tableau[col][i].Suit != tableau[col][i + 1].Suit) return false;
        }
        return true;
    }
    
    private void GamePanel_MouseMove(object? sender, MouseEventArgs e)
    {
        if (draggedCard != null && e.Button == MouseButtons.Left)
        {
            // Check if mouse has moved enough to initiate drag
            if (!isDragging && Math.Abs(e.X - mouseDownPos.X) + Math.Abs(e.Y - mouseDownPos.Y) > 5)
            {
                isDragging = true;
            }
            
            if (isDragging)
            {
                gamePanel?.Invalidate();
            }
        }
    }
    
    private void GamePanel_MouseUp(object? sender, MouseEventArgs e)
    {
        if (draggedCard == null) return;
        
        // If we were dragging, handle as drag-and-drop with animation
        if (isDragging)
        {
            // Find target column
            for (int col = 0; col < tableau.Count; col++)
            {
                int x = col * (CardWidth + 5) + 10;
                
                if (e.X >= x && e.X <= x + CardWidth)
                {
                    if (CanPlaceOnColumn(col, dragCardIndex))
                    {
                        AnimateMoveCards(dragSourceColumn, dragCardIndex, col);
                    }
                    break;
                }
            }
            
            draggedCard = null;
            dragSourceColumn = -1;
            dragCardIndex = -1;
            isDragging = false;
            gamePanel?.Invalidate();
        }
        else
        {
            // Handle as auto-move: find the best valid destination
            int bestColumn = FindBestMoveColumn(dragSourceColumn, dragCardIndex);
            
            if (bestColumn >= 0)
            {
                AnimateMoveCards(dragSourceColumn, dragCardIndex, bestColumn);
                draggedCard = null;
                dragSourceColumn = -1;
                dragCardIndex = -1;
            }
            
            gamePanel?.Invalidate();
        }
    }
    
    private void MoveCards(int fromCol, int cardIndex, int toCol)
    {
        // Move cards
        var cardsToMove = tableau[fromCol].GetRange(cardIndex,
            tableau[fromCol].Count - cardIndex);
        tableau[fromCol].RemoveRange(cardIndex,
            tableau[fromCol].Count - cardIndex);
        tableau[toCol].AddRange(cardsToMove);
        
        // Flip top card if needed
        if (tableau[fromCol].Count > 0 &&
            !tableau[fromCol][tableau[fromCol].Count - 1].FaceUp)
        {
            tableau[fromCol][tableau[fromCol].Count - 1].FaceUp = true;
        }
        
        // Check for completed sequences
        CheckForCompletedSequences();
        
        score += 5;
        UpdateScore();
    }
    
    private int FindBestMoveColumn(int fromCol, int cardIndex)
    {
        // Find the first valid column that can accept the cards
        var movingCard = tableau[fromCol][cardIndex];
        
        for (int col = 0; col < tableau.Count; col++)
        {
            if (col == fromCol) continue;
            
            if (tableau[col].Count == 0)
            {
                // Prefer non-empty columns over empty ones
                continue;
            }
            
            var topCard = tableau[col][tableau[col].Count - 1];
            if (topCard.Rank == movingCard.Rank + 1)
            {
                return col;
            }
        }
        
        // If no non-empty column works, try empty columns
        for (int col = 0; col < tableau.Count; col++)
        {
            if (col == fromCol) continue;
            if (tableau[col].Count == 0)
            {
                return col;
            }
        }
        
        return -1; // No valid move found
    }
    
    private void AnimateMoveCards(int fromCol, int cardIndex, int toCol)
    {
        // Calculate start and end positions for animation
        int startX = fromCol * (CardWidth + 5) + 10;
        int startY = 10 + cardIndex * CardOffset;
        
        int endX = toCol * (CardWidth + 5) + 10;
        int endY = 10 + tableau[toCol].Count * CardOffset;
        
        animationStart = new Point(startX, startY);
        animationEnd = new Point(endX, endY);
        animationFrame = 0;
        animationTargetColumn = toCol;
        animationSourceColumn = fromCol;
        animationSourceCardIndex = cardIndex;
        
        // Store cards to animate
        animatingCards = tableau[fromCol].GetRange(cardIndex, tableau[fromCol].Count - cardIndex);
        
        // Start animation timer
        if (animationTimer == null)
        {
            animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = 16; // ~60 FPS
            animationTimer.Tick += AnimationTimer_Tick;
        }
        animationTimer.Start();
    }
    
    private void AnimationTimer_Tick(object? sender, EventArgs e)
    {
        animationFrame++;
        
        if (animationFrame >= AnimationFrames)
        {
            // Animation complete, perform the actual move
            animationTimer?.Stop();
            
            if (animatingCards != null && animationTargetColumn >= 0 && animationSourceColumn >= 0 && animationSourceCardIndex >= 0)
            {
                MoveCards(animationSourceColumn, animationSourceCardIndex, animationTargetColumn);
            }
            
            animatingCards = null;
            animationTargetColumn = -1;
            animationSourceColumn = -1;
            animationSourceCardIndex = -1;
        }
        
        gamePanel?.Invalidate();
    }
    
    private bool CanPlaceOnColumn(int col, int cardIndex)
    {
        if (col == dragSourceColumn) return false;
        
        if (tableau[col].Count == 0)
        {
            return true; // Can place any card on empty column
        }
        
        var topCard = tableau[col][tableau[col].Count - 1];
        var movingCard = tableau[dragSourceColumn][cardIndex];
        
        return topCard.Rank == movingCard.Rank + 1;
    }
    
    private void CheckForCompletedSequences()
    {
        for (int col = 0; col < tableau.Count; col++)
        {
            if (tableau[col].Count >= 13)
            {
                // Check last 13 cards
                bool isComplete = true;
                int startIndex = tableau[col].Count - 13;
                string suit = tableau[col][startIndex].Suit;
                
                for (int i = 0; i < 13; i++)
                {
                    var card = tableau[col][startIndex + i];
                    if (!card.FaceUp || card.Suit != suit || card.Rank != 13 - i)
                    {
                        isComplete = false;
                        break;
                    }
                }
                
                if (isComplete)
                {
                    // Remove completed sequence
                    var completed = tableau[col].GetRange(startIndex, 13);
                    tableau[col].RemoveRange(startIndex, 13);
                    foundation.Add(completed);
                    score += 100;
                    UpdateScore();
                    
                    // Flip top card if needed
                    if (tableau[col].Count > 0 &&
                        !tableau[col][tableau[col].Count - 1].FaceUp)
                    {
                        tableau[col][tableau[col].Count - 1].FaceUp = true;
                    }
                    
                    // Check win condition
                    if (foundation.Count == 8)
                    {
                        MessageBox.Show($"Congratulations! You won with a score of {score}!", "Victory!");
                        var name = Microsoft.VisualBasic.Interaction.InputBox("Enter your name:", "High Score", "Player");
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            scoreManager.SaveScore("SpiderSolitaire", new ScoreEntry { PlayerName = name, Score = score });
                        }
                    }
                }
            }
        }
    }
    
    private void DealFromStock()
    {
        if (stock.Count < 10) return;
        
        SaveGameState();
        
        for (int col = 0; col < 10; col++)
        {
            if (stock.Count > 0)
            {
                var card = stock[0];
                stock.RemoveAt(0);
                card.FaceUp = true;
                tableau[col].Add(card);
            }
        }
        
        gamePanel?.Invalidate();
    }
    
    private void SaveGameState()
    {
        // Keep only last 10 moves
        if (undoStack.Count >= 10)
        {
            var temp = undoStack.ToList();
            temp.RemoveAt(temp.Count - 1);
            undoStack = new Stack<GameState>(temp.Reverse<GameState>());
        }
        
        var state = new GameState
        {
            Tableau = tableau.Select(col => col.Select(c => c.Clone()).ToList()).ToList(),
            Stock = stock.Select(c => c.Clone()).ToList(),
            Foundation = foundation.Select(seq => seq.Select(c => c.Clone()).ToList()).ToList(),
            Score = score
        };
        
        undoStack.Push(state);
    }
    
    private void UndoMove(object? sender, EventArgs e)
    {
        if (undoStack.Count == 0)
        {
            MessageBox.Show("No moves to undo!", "Undo");
            return;
        }
        
        var state = undoStack.Pop();
        tableau = state.Tableau;
        stock = state.Stock;
        foundation = state.Foundation;
        score = state.Score;
        
        UpdateScore();
        gamePanel?.Invalidate();
    }
    
    private void ShowHint(object? sender, EventArgs e)
    {
        // If a hint is already showing, stop it
        if (hintTimer != null && hintTimer.Enabled)
        {
            hintTimer.Stop();
            hintSourceColumn = -1;
            hintSourceCardIndex = -1;
            hintTargetColumn = -1;
            hintBlinkCount = 0;
            gamePanel?.Invalidate();
            return;
        }
        
        // Find any valid move
        for (int srcCol = 0; srcCol < tableau.Count; srcCol++)
        {
            if (tableau[srcCol].Count == 0) continue;
            
            for (int i = 0; i < tableau[srcCol].Count; i++)
            {
                if (tableau[srcCol][i].FaceUp && CanMoveSequence(srcCol, i))
                {
                    var movingCard = tableau[srcCol][i];
                    
                    for (int destCol = 0; destCol < tableau.Count; destCol++)
                    {
                        if (destCol == srcCol) continue;
                        
                        if (tableau[destCol].Count == 0 ||
                            tableau[destCol][tableau[destCol].Count - 1].Rank == movingCard.Rank + 1)
                        {
                            // Start hint animation
                            hintSourceColumn = srcCol;
                            hintSourceCardIndex = i;
                            hintTargetColumn = destCol;
                            hintBlinkCount = 0;
                            
                            // Create and start hint timer
                            if (hintTimer == null)
                            {
                                hintTimer = new System.Windows.Forms.Timer();
                                hintTimer.Interval = 300; // Blink every 300ms
                                hintTimer.Tick += HintTimer_Tick;
                            }
                            hintTimer.Start();
                            return;
                        }
                    }
                }
            }
        }
        
        // No moves found - do nothing (silent)
    }
    
    private void HintTimer_Tick(object? sender, EventArgs e)
    {
        hintBlinkCount++;
        
        if (hintBlinkCount >= HintBlinkTotal)
        {
            // Stop hint animation
            hintTimer?.Stop();
            hintSourceColumn = -1;
            hintSourceCardIndex = -1;
            hintTargetColumn = -1;
            hintBlinkCount = 0;
        }
        
        gamePanel?.Invalidate();
    }
    
    private string GetCardName(Card card)
    {
        string rank = card.Rank switch
        {
            1 => "Ace",
            11 => "Jack",
            12 => "Queen",
            13 => "King",
            _ => card.Rank.ToString()
        };
        return $"{rank} of {card.Suit}";
    }
    
    private void UpdateScore()
    {
        if (scoreLabel != null)
        {
            scoreLabel.Text = $"Score: {score} | Complete: {foundation.Count}/8";
        }
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
        
        var suitLabel = new Label
        {
            Text = "Number of Suits:",
            Location = new Point(20, 20),
            AutoSize = true
        };
        settingsForm.Controls.Add(suitLabel);
        
        var suitCombo = new ComboBox
        {
            Location = new Point(20, 50),
            Size = new Size(200, 25),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        suitCombo.Items.AddRange(new[] { "1 Suit (Easy)", "2 Suits (Medium)", "4 Suits (Hard)" });
        suitCombo.SelectedIndex = suitCount == 1 ? 0 : suitCount == 2 ? 1 : 2;
        settingsForm.Controls.Add(suitCombo);
        
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
            suitCount = suitCombo.SelectedIndex == 0 ? 1 : suitCombo.SelectedIndex == 1 ? 2 : 4;
            InitializeGame();
        }
    }
    
    private void ShowScoreboard(object? sender, EventArgs e)
    {
        var scores = scoreManager.LoadScores("SpiderSolitaire");
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
    
    private void SpiderSolitaireForm_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            this.Close();
        }
    }
    
    private void SpiderSolitaireForm_Resize(object? sender, EventArgs e)
    {
        // Game panel automatically resizes via Anchor property
        gamePanel?.Invalidate();
    }
    
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        animationTimer?.Stop();
        animationTimer?.Dispose();
        hintTimer?.Stop();
        hintTimer?.Dispose();
        base.OnFormClosing(e);
    }
}

public class Card
{
    public string Suit { get; set; } = "";
    public int Rank { get; set; }
    public bool FaceUp { get; set; }
    
    public Card Clone()
    {
        return new Card { Suit = Suit, Rank = Rank, FaceUp = FaceUp };
    }
}

public class GameState
{
    public List<List<Card>> Tableau { get; set; } = new List<List<Card>>();
    public List<Card> Stock { get; set; } = new List<Card>();
    public List<List<Card>> Foundation { get; set; } = new List<List<Card>>();
    public int Score { get; set; }
}
