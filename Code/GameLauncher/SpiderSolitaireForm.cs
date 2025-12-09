namespace GameLauncher;

public partial class SpiderSolitaireForm : Form
{
    private const int CardWidth = 85; // Increased from 80 for better text display
    private const int CardHeight = 120;
    private const int CardOffset = 25;
    private const int CardSpacing = 5; // Space between cards
    private const int FoundationSpacing = 10; // Space between foundation piles
    
    // Layout constants for new design
    private const int ButtonPanelHeight = 60;
    private const int StockAreaY = 70;
    private const int TableauAreaY = 200;
    private const int CardsPerStockDeal = 10;
    
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
    private const int AnimationFrames = 12; // Smooth animation with more frames (was 2)
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
    
    // Time update timer for continuous display
    private System.Windows.Forms.Timer? timeUpdateTimer;
    private int lastDisplayedSeconds = -1; // Track last displayed time to avoid unnecessary updates
    
    private int suitCount = 1; // 1, 2, or 4 suits
    private int score = 0;
    private int moves = 0;
    private DateTime startTime;
    private TimeSpan pausedDuration = TimeSpan.Zero; // Track total paused time
    private DateTime pauseStartTime; // Track when pause started
    private bool isPaused = false;
    private Panel? gamePanel;
    private Label? scoreLabel;
    private Label? pauseLabel;
    private HighScoreManager scoreManager = new HighScoreManager();
    
    // Card image caching
    private Dictionary<string, Image> cardImageCache = new Dictionary<string, Image>();
    private Image? cardBackImage = null;
    private Image? backgroundImage = null;
    
    public SpiderSolitaireForm()
    {
        InitializeComponent();
        InitializeGame();
    }
    
    private void InitializeComponent()
    {
        this.Text = "Spider Solitaire";
        this.Size = new Size(1100, 850);
        this.MinimumSize = new Size(1100, 850);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.Sizable;
        this.KeyPreview = true;
        this.KeyDown += SpiderSolitaireForm_KeyDown;
        this.Resize += SpiderSolitaireForm_Resize;
        this.DoubleBuffered = true;
        
        // Top panel for score display
        var topPanel = new Panel
        {
            Location = new Point(0, 0),
            Size = new Size(1100, 50),
            BackColor = Color.White,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        this.Controls.Add(topPanel);
        
        // Score label with larger font
        scoreLabel = new Label
        {
            Location = new Point(20, 8),
            Size = new Size(700, 35),
            Font = new Font("Arial", 16, FontStyle.Bold),
            Text = "Moves: 0 | Time: 0s | Score: 0",
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };
        topPanel.Controls.Add(scoreLabel);
        
        // Pause button
        var pauseBtn = new Button
        {
            Text = "⏸ Pause",
            Location = new Point(750, 10),
            Size = new Size(100, 30),
            Font = new Font("Arial", 12, FontStyle.Bold),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        topPanel.Controls.Add(pauseBtn);
        
        // Settings button
        var settingsBtn = new Button
        {
            Text = "⚙ Settings",
            Location = new Point(860, 10),
            Size = new Size(110, 30),
            Font = new Font("Arial", 12, FontStyle.Bold),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        settingsBtn.Click += ShowSettings;
        topPanel.Controls.Add(settingsBtn);
        
        // Scoreboard button
        var scoreboardBtn = new Button
        {
            Text = "📊 Scores",
            Location = new Point(980, 10),
            Size = new Size(110, 30),
            Font = new Font("Arial", 12, FontStyle.Bold),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        scoreboardBtn.Click += ShowScoreboard;
        topPanel.Controls.Add(scoreboardBtn);
        
        // Game panel (green background)
        gamePanel = new Panel
        {
            Location = new Point(0, 50),
            Size = new Size(1100, 800),
            BackColor = Color.DarkGreen,
            BorderStyle = BorderStyle.None,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
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
        
        // Buttons panel inside game panel (below score, above cards)
        var buttonsPanel = new Panel
        {
            Location = new Point(10, 10),
            Size = new Size(1080, 50),
            BackColor = Color.Transparent,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        gamePanel.Controls.Add(buttonsPanel);
        
        // Hint button with icon
        var hintBtn = new Button
        {
            Location = new Point(10, 10),
            Size = new Size(50, 40),
            BackColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        var hintIcon = AssetManager.LoadButtonImage("hint");
        if (hintIcon != null)
        {
            hintBtn.Image = new Bitmap(hintIcon, new Size(35, 35));
            hintIcon.Dispose();
        }
        else
        {
            hintBtn.Text = "💡";
            hintBtn.Font = new Font("Arial", 16);
        }
        hintBtn.Click += ShowHint;
        buttonsPanel.Controls.Add(hintBtn);
        
        // Undo button with icon
        var undoBtn = new Button
        {
            Location = new Point(70, 10),
            Size = new Size(50, 40),
            BackColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        var undoIcon = AssetManager.LoadButtonImage("undo");
        if (undoIcon != null)
        {
            undoBtn.Image = new Bitmap(undoIcon, new Size(35, 35));
            undoIcon.Dispose();
        }
        else
        {
            undoBtn.Text = "↶";
            undoBtn.Font = new Font("Arial", 16);
        }
        undoBtn.Click += UndoMove;
        buttonsPanel.Controls.Add(undoBtn);
        
        // Restart button with icon
        var restartBtn = new Button
        {
            Location = new Point(130, 10),
            Size = new Size(50, 40),
            BackColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        var restartIcon = AssetManager.LoadButtonImage("restart");
        if (restartIcon != null)
        {
            restartBtn.Image = new Bitmap(restartIcon, new Size(35, 35));
            restartIcon.Dispose();
        }
        else
        {
            restartBtn.Text = "🔄";
            restartBtn.Font = new Font("Arial", 16);
        }
        restartBtn.Click += (s, e) => InitializeGame();
        buttonsPanel.Controls.Add(restartBtn);
        
        // Home button with icon
        var homeBtn = new Button
        {
            Location = new Point(190, 10),
            Size = new Size(50, 40),
            BackColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        var homeIcon = AssetManager.LoadButtonImage("home");
        if (homeIcon != null)
        {
            homeBtn.Image = new Bitmap(homeIcon, new Size(35, 35));
            homeIcon.Dispose();
        }
        else
        {
            homeBtn.Text = "🏠";
            homeBtn.Font = new Font("Arial", 16);
        }
        homeBtn.Click += (s, e) => this.Close();
        buttonsPanel.Controls.Add(homeBtn);
        
        // Pause label (centered on game panel)
        pauseLabel = new Label
        {
            Text = "PAUSED\nPress P to Resume",
            Font = new Font("Arial", 32, FontStyle.Bold),
            ForeColor = Color.Yellow,
            BackColor = Color.FromArgb(180, 0, 0, 0), // Semi-transparent black
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize = false,
            Size = new Size(400, 120),
            Visible = false,
            Anchor = AnchorStyles.None
        };
        gamePanel.Controls.Add(pauseLabel);
        pauseLabel.BringToFront();
    }
    
    private void LoadCardImages()
    {
        try
        {
            // Load the card back image
            cardBackImage = AssetManager.LoadCardBackImage();
            System.Diagnostics.Debug.WriteLine("Card back image loaded successfully");
            
            // Load the background image
            backgroundImage = AssetManager.LoadBackgroundImage();
            if (backgroundImage != null)
            {
                System.Diagnostics.Debug.WriteLine("Background image loaded successfully");
            }
            
            // Preload all card images for better performance
            string[] suits = { "Spades", "Hearts", "Diamonds", "Clubs" };
            foreach (var suit in suits)
            {
                for (int rank = 1; rank <= 13; rank++)
                {
                    string key = $"{suit}_{rank}";
                    cardImageCache[key] = AssetManager.LoadCardImage(suit, rank);
                }
            }
            System.Diagnostics.Debug.WriteLine("Card images loaded successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load card images: {ex.Message}");
            // Will fall back to drawing cards programmatically
        }
    }
    
    private void InitializeGame()
    {
        // Load card images if not already loaded
        if (cardImageCache.Count == 0)
        {
            LoadCardImages();
        }
        
        tableau.Clear();
        stock.Clear();
        foundation.Clear();
        undoStack.Clear();
        score = 0;
        moves = 0;
        startTime = DateTime.Now;
        pausedDuration = TimeSpan.Zero; // Reset paused time
        isPaused = false;
        if (pauseLabel != null) pauseLabel.Visible = false;
        lastDisplayedSeconds = -1; // Reset time tracking
        
        // Start/restart continuous time update timer
        if (timeUpdateTimer == null)
        {
            timeUpdateTimer = new System.Windows.Forms.Timer();
            timeUpdateTimer.Interval = 100; // Check every 100ms but only update UI when seconds change
            timeUpdateTimer.Tick += TimeUpdateTimer_Tick;
        }
        timeUpdateTimer.Start();
        
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
        
        // Draw background image if available
        if (backgroundImage != null && gamePanel != null)
        {
            g.DrawImage(backgroundImage, 0, 0, gamePanel.Width, gamePanel.Height);
        }
        
        // Layout: buttons at top, stock/foundation, then play area
        int stockY = StockAreaY;
        int tableauY = TableauAreaY;
        
        // Draw stock area with card back image
        if (stock.Count > 0)
        {
            if (cardBackImage != null)
            {
                // Draw card back image for stock
                g.DrawImage(cardBackImage, 10, stockY, CardWidth, CardHeight);
                // Draw stock count on top
                using (var font = new Font("Arial", 20, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.FromArgb(200, 255, 255, 255)))
                using (var backgroundBrush = new SolidBrush(Color.FromArgb(150, 0, 0, 0)))
                {
                    string stockText = $"{stock.Count / CardsPerStockDeal}";
                    var textSize = g.MeasureString(stockText, font);
                    float textX = 10 + (CardWidth - textSize.Width) / 2;
                    float textY = stockY + (CardHeight - textSize.Height) / 2;
                    g.FillRectangle(backgroundBrush, textX - 5, textY - 2, textSize.Width + 10, textSize.Height + 4);
                    g.DrawString(stockText, font, brush, textX, textY);
                }
            }
            else
            {
                g.FillRectangle(Brushes.Gray, 10, stockY, CardWidth, CardHeight);
                using (var font = new Font("Arial", 16, FontStyle.Bold))
                {
                    g.DrawString($"Stock\n{stock.Count}", font, Brushes.White, 15, stockY + 20);
                }
            }
        }
        
        // Draw foundation (completed decks)
        for (int i = 0; i < foundation.Count; i++)
        {
            int x = 200 + i * (CardWidth + FoundationSpacing);
            g.FillRectangle(Brushes.DarkGray, x, stockY, CardWidth, CardHeight);
            using (var font = new Font("Arial", 14, FontStyle.Bold))
            {
                g.DrawString($"Complete\n{i + 1}", font, Brushes.White, x + 5, stockY + 40);
            }
        }
        
        // Draw tableau columns
        for (int col = 0; col < tableau.Count; col++)
        {
            int x = col * (CardWidth + CardSpacing) + 10;
            
            // Draw column placeholder
            g.DrawRectangle(Pens.White, x, tableauY, CardWidth, CardHeight);
            
            // Draw cards in column
            for (int i = 0; i < tableau[col].Count; i++)
            {
                var card = tableau[col][i];
                int cardY = tableauY + i * CardOffset;
                
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
        
        // Highlight hint target column
        if (hintTargetColumn >= 0 && (hintBlinkCount % 2 == 0))
        {
            int x = hintTargetColumn * (CardWidth + CardSpacing) + 10;
            int y = tableauY + tableau[hintTargetColumn].Count * CardOffset;
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
            // Apply ease-out cubic easing for smoother animation
            t = 1 - (float)Math.Pow(1 - t, 3);
            
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
            // Draw card back using PNG image if available
            if (cardBackImage != null)
            {
                g.DrawImage(cardBackImage, x, y, CardWidth, CardHeight);
            }
            else
            {
                // Fallback to blue rectangle
                g.FillRectangle(Brushes.Blue, x, y, CardWidth, CardHeight);
                g.DrawRectangle(Pens.White, x, y, CardWidth, CardHeight);
            }
        }
        else
        {
            // Draw card face using cached PNG image if available
            string key = $"{card.Suit}_{card.Rank}";
            if (cardImageCache.TryGetValue(key, out var cardImage))
            {
                g.DrawImage(cardImage, x, y, CardWidth, CardHeight);
            }
            else
            {
                // Fallback to programmatic drawing
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
    }
    
    private void GamePanel_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;
        
        mouseDownPos = e.Location;
        isDragging = false;
        
        int stockY = StockAreaY;
        int tableauY = TableauAreaY;
        
        // Check if clicking on stock
        if (e.X >= 10 && e.X <= 10 + CardWidth && e.Y >= stockY && e.Y <= stockY + CardHeight && stock.Count > 0)
        {
            DealFromStock();
            return;
        }
        
        // Check if clicking on a card in tableau
        for (int col = 0; col < tableau.Count; col++)
        {
            int x = col * (CardWidth + CardSpacing) + 10;
            
            for (int i = tableau[col].Count - 1; i >= 0; i--)
            {
                int cardY = tableauY + i * CardOffset;
                
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
                int x = col * (CardWidth + CardSpacing) + 10;
                
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
        
        moves++; // Count each card move as a move
        
        // Check for completed sequences
        CheckForCompletedSequences();
        
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
        int tableauY = TableauAreaY;
        
        // Calculate start and end positions for animation
        int startX = fromCol * (CardWidth + CardSpacing) + 10;
        int startY = tableauY + cardIndex * CardOffset;
        
        int endX = toCol * (CardWidth + CardSpacing) + 10;
        int endY = tableauY + tableau[toCol].Count * CardOffset;
        
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
                        int elapsedSeconds = (int)(DateTime.Now - startTime).TotalSeconds;
                        score = moves + elapsedSeconds;
                        MessageBox.Show($"Congratulations! You won!\nMoves: {moves}\nTime: {elapsedSeconds}s\nFinal Score: {score}\n(Lower is better)", "Victory!");
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
        moves++; // Count dealing from stock as a move
        
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
            Score = score,
            Moves = moves
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
        moves = state.Moves;
        
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
    
    private void TimeUpdateTimer_Tick(object? sender, EventArgs e)
    {
        // Only update display when seconds actually change to reduce CPU usage
        int elapsedSeconds = (int)(DateTime.Now - startTime).TotalSeconds;
        if (elapsedSeconds != lastDisplayedSeconds)
        {
            lastDisplayedSeconds = elapsedSeconds;
            UpdateScore();
        }
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
            TimeSpan currentPausedTime = isPaused ? (DateTime.Now - pauseStartTime) : TimeSpan.Zero;
            int elapsedSeconds = (int)((DateTime.Now - startTime) - pausedDuration - currentPausedTime).TotalSeconds;
            score = moves + elapsedSeconds; // Score = moves + seconds (lower is better)
            scoreLabel.Text = $"Moves: {moves} | Time: {elapsedSeconds}s | Score: {score} | Complete: {foundation.Count}/8";
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
        else if (e.KeyCode == Keys.P)
        {
            TogglePause();
        }
    }
    
    private void TogglePause()
    {
        isPaused = !isPaused;
        
        if (isPaused)
        {
            pauseStartTime = DateTime.Now;
            timeUpdateTimer?.Stop();
            if (pauseLabel != null)
            {
                pauseLabel.Visible = true;
                pauseLabel.BringToFront();
            }
        }
        else
        {
            // Add elapsed pause time to total paused duration
            pausedDuration += DateTime.Now - pauseStartTime;
            timeUpdateTimer?.Start();
            if (pauseLabel != null)
            {
                pauseLabel.Visible = false;
            }
        }
        
        gamePanel?.Invalidate();
    }
    
    private void SpiderSolitaireForm_Resize(object? sender, EventArgs e)
    {
        // Game panel automatically resizes via Anchor property
        gamePanel?.Invalidate();
        
        // Center pause label
        if (pauseLabel != null && gamePanel != null)
        {
            pauseLabel.Location = new Point(
                (gamePanel.Width - pauseLabel.Width) / 2,
                (gamePanel.Height - pauseLabel.Height) / 2
            );
        }
    }
    
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        animationTimer?.Stop();
        animationTimer?.Dispose();
        hintTimer?.Stop();
        hintTimer?.Dispose();
        timeUpdateTimer?.Stop();
        timeUpdateTimer?.Dispose();
        
        // Dispose cached card images to prevent memory leaks
        foreach (var image in cardImageCache.Values)
        {
            image.Dispose();
        }
        cardImageCache.Clear();
        
        cardBackImage?.Dispose();
        backgroundImage?.Dispose();
        
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
    public int Moves { get; set; }
}
