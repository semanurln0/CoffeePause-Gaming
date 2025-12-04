namespace GameLauncher;

public partial class MainForm : Form
{
    private const int InitialWidth = 1100; // Increased width for caffeine panel
    private const int InitialHeight = 600;
    private CaffeineTracker caffeineTracker = new CaffeineTracker();
    private Label? caffeineStatusLabel;
    private Panel? caffeinePanel;
    
    public MainForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "CoffeePause - Game Launcher";
        this.Size = new Size(1100, 600);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.Sizable;
        this.KeyPreview = true;
        this.KeyDown += MainForm_KeyDown;
        this.Resize += MainForm_Resize;
        this.DoubleBuffered = true;
        
        // Set form icon from SVG
        try
        {
            var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "sprites", "project-logo.svg");
            var icon = IconConverter.LoadIconFromSvg(logoPath, 64);
            if (icon != null)
            {
                this.Icon = icon;
            }
        }
        catch
        {
            // Icon loading failed, continue without custom icon
        }
        
        // Create menu strip
        var menuStrip = new MenuStrip();
        var fileMenu = new ToolStripMenuItem("File");
        var exitMenuItem = new ToolStripMenuItem("Exit");
        exitMenuItem.Click += (s, e) => Application.Exit();
        fileMenu.DropDownItems.Add(exitMenuItem);
        
        var helpMenu = new ToolStripMenuItem("Help");
        var aboutMenuItem = new ToolStripMenuItem("About");
        aboutMenuItem.Click += ShowAbout;
        helpMenu.DropDownItems.Add(aboutMenuItem);
        
        menuStrip.Items.Add(fileMenu);
        menuStrip.Items.Add(helpMenu);
        this.MainMenuStrip = menuStrip;
        this.Controls.Add(menuStrip);
        
        // Create main panel
        var mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(245, 245, 245),
            Padding = new Padding(20)
        };
        
        // Load and set background image
        try
        {
            var backgroundImage = AssetManager.LoadImage("main-menu-background.jpg");
            mainPanel.BackgroundImage = backgroundImage;
            mainPanel.BackgroundImageLayout = ImageLayout.Stretch;
        }
        catch
        {
            // Keep default background color if image loading fails
        }
        
        // Calculate initial center position
        int formCenterX = InitialWidth / 2;
        
        // Title label with logo
        var logoImage = AssetManager.LoadSvgAsImage("project-logo.svg", 64, 64);
        if (logoImage != null)
        {
            var logoPictureBox = new PictureBox
            {
                Image = logoImage,
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(64, 64),
                Location = new Point(formCenterX - 150, 40),
                BackColor = Color.Transparent
            };
            mainPanel.Controls.Add(logoPictureBox);
        }
        
        var titleLabel = new Label
        {
            Text = "CoffeePause",
            Font = new Font("Segoe UI", 28, FontStyle.Bold),
            ForeColor = Color.FromArgb(74, 166, 186),
            AutoSize = true,
            Location = new Point(formCenterX - 80, 50),
            BackColor = Color.Transparent
        };
        mainPanel.Controls.Add(titleLabel);
        
        // Subtitle
        var subtitleLabel = new Label
        {
            Text = "Choose Game",
            Font = new Font("Segoe UI", 12),
            ForeColor = Color.Gray,
            AutoSize = true,
            Location = new Point(formCenterX - 50, 100),
            BackColor = Color.Transparent
        };
        mainPanel.Controls.Add(subtitleLabel);
        
        // Game buttons panel
        var gamesPanel = new TableLayoutPanel
        {
            Name = "gamesPanel",
            Location = new Point((InitialWidth - 500) / 2, 150),
            Size = new Size(500, 350),
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(10),
            CellBorderStyle = TableLayoutPanelCellBorderStyle.None
        };
        
        gamesPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        gamesPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        gamesPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        gamesPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        
        // Create game buttons
        var pacmanBtn = CreateGameButton("Pac-Man", Color.FromArgb(255, 213, 0));
        pacmanBtn.Click += (s, e) => LaunchGame("PacMan");
        
        var sudokuBtn = CreateGameButton("Sudoku", Color.FromArgb(76, 175, 80));
        sudokuBtn.Click += (s, e) => LaunchGame("Sudoku");
        
        var minesweeperBtn = CreateGameButton("Minesweeper", Color.FromArgb(33, 150, 243));
        minesweeperBtn.Click += (s, e) => LaunchGame("Minesweeper");
        
        var solitaireBtn = CreateGameButton("Spider Solitaire", Color.FromArgb(156, 39, 176));
        solitaireBtn.Click += (s, e) => LaunchGame("SpiderSolitaire");
        
        gamesPanel.Controls.Add(pacmanBtn, 0, 0);
        gamesPanel.Controls.Add(sudokuBtn, 1, 0);
        gamesPanel.Controls.Add(minesweeperBtn, 0, 1);
        gamesPanel.Controls.Add(solitaireBtn, 1, 1);
        
        mainPanel.Controls.Add(gamesPanel);
        
        // Add caffeine tracker panel on the right
        caffeinePanel = CreateCaffeineTrackerPanel();
        mainPanel.Controls.Add(caffeinePanel);
        caffeinePanel.BringToFront(); // Ensure it's visible on top
        
        this.Controls.Add(mainPanel);
    }
    
    private Panel CreateCaffeineTrackerPanel()
    {
        // Calculate position from the right edge of the client area
        // Panel width is 280, with 20 margin from right edge
        int panelWidth = 280;
        int rightMargin = 20;
        // Use actual client width instead of InitialWidth constant
        int xPosition = this.ClientSize.Width - panelWidth - rightMargin;
        
        var panel = new Panel
        {
            Location = new Point(xPosition, 40),
            Size = new Size(panelWidth, 520),
            BackColor = Color.FromArgb(240, 248, 255),
            BorderStyle = BorderStyle.FixedSingle,
            Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom
        };
        
        // Title
        var titleLabel = new Label
        {
            Text = "☕ Caffeine Tracker",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(74, 166, 186),
            Location = new Point(10, 10),
            Size = new Size(260, 30),
            TextAlign = ContentAlignment.MiddleCenter
        };
        panel.Controls.Add(titleLabel);
        
        // Drink type
        var drinkLabel = new Label
        {
            Text = "Choose Drink:",
            Location = new Point(10, 50),
            Size = new Size(260, 20),
            Font = new Font("Segoe UI", 9)
        };
        panel.Controls.Add(drinkLabel);
        
        var drinkCombo = new ComboBox
        {
            Location = new Point(10, 75),
            Size = new Size(260, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 9)
        };
        drinkCombo.Items.AddRange(CaffeineData.CaffeineContent.Keys.ToArray());
        drinkCombo.SelectedIndex = 0;
        panel.Controls.Add(drinkCombo);
        
        // Size
        var sizeLabel = new Label
        {
            Text = "Choose Size (ml):",
            Location = new Point(10, 110),
            Size = new Size(260, 20),
            Font = new Font("Segoe UI", 9)
        };
        panel.Controls.Add(sizeLabel);
        
        var sizeInput = new NumericUpDown
        {
            Location = new Point(10, 135),
            Size = new Size(260, 25),
            Minimum = 50,
            Maximum = 1000,
            Value = 250,
            Increment = 50,
            Font = new Font("Segoe UI", 9)
        };
        panel.Controls.Add(sizeInput);
        
        // Quantity
        var quantityLabel = new Label
        {
            Text = "Quantity:",
            Location = new Point(10, 170),
            Size = new Size(260, 20),
            Font = new Font("Segoe UI", 9)
        };
        panel.Controls.Add(quantityLabel);
        
        var quantityInput = new NumericUpDown
        {
            Location = new Point(10, 195),
            Size = new Size(260, 25),
            Minimum = 1,
            Maximum = 10,
            Value = 1,
            Font = new Font("Segoe UI", 9)
        };
        panel.Controls.Add(quantityInput);
        
        // When (time consumed)
        var whenLabel = new Label
        {
            Text = "When (day-time):",
            Location = new Point(10, 230),
            Size = new Size(260, 20),
            Font = new Font("Segoe UI", 9)
        };
        panel.Controls.Add(whenLabel);
        
        var whenPicker = new DateTimePicker
        {
            Location = new Point(10, 255),
            Size = new Size(260, 25),
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "MM/dd/yyyy hh:mm tt",
            Font = new Font("Segoe UI", 9)
        };
        panel.Controls.Add(whenPicker);
        
        // Sleep time
        var sleepLabel = new Label
        {
            Text = "Sleep Time Today:",
            Location = new Point(10, 290),
            Size = new Size(260, 20),
            Font = new Font("Segoe UI", 9)
        };
        panel.Controls.Add(sleepLabel);
        
        var sleepPicker = new DateTimePicker
        {
            Location = new Point(10, 315),
            Size = new Size(260, 25),
            Format = DateTimePickerFormat.Time,
            ShowUpDown = true,
            Value = DateTime.Today.AddHours(23),
            Font = new Font("Segoe UI", 9)
        };
        panel.Controls.Add(sleepPicker);
        
        // Add button
        var addButton = new Button
        {
            Text = "Add Caffeine",
            Location = new Point(10, 355),
            Size = new Size(260, 35),
            BackColor = Color.FromArgb(74, 166, 186),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        addButton.FlatAppearance.BorderSize = 0;
        addButton.Click += (s, e) =>
        {
            var drinkType = drinkCombo.SelectedItem?.ToString() ?? "";
            var size = (int)sizeInput.Value;
            var quantity = (int)quantityInput.Value;
            var when = whenPicker.Value;
            
            var caffeineAmount = CaffeineData.CalculateCaffeine(drinkType, size, quantity);
            
            var entry = new CaffeineEntry
            {
                DrinkType = drinkType,
                SizeMl = size,
                Quantity = quantity,
                CaffeineAmount = caffeineAmount,
                ConsumedAt = when
            };
            
            caffeineTracker.SaveEntry(entry);
            UpdateCaffeineStatus(sleepPicker.Value);
            
            MessageBox.Show($"Added {caffeineAmount:F0}mg of caffeine!", "Success", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        };
        panel.Controls.Add(addButton);
        
        // Status label
        caffeineStatusLabel = new Label
        {
            Location = new Point(10, 400),
            Size = new Size(260, 100),
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(70, 70, 70),
            TextAlign = ContentAlignment.TopLeft,
            BackColor = Color.FromArgb(255, 255, 240),
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(5)
        };
        panel.Controls.Add(caffeineStatusLabel);
        
        UpdateCaffeineStatus(sleepPicker.Value);
        
        return panel;
    }
    
    private void UpdateCaffeineStatus(DateTime sleepTime)
    {
        if (caffeineStatusLabel == null) return;
        
        var currentCaffeine = caffeineTracker.GetCurrentCaffeineLevel(sleepTime);
        
        string status;
        if (currentCaffeine < 100)
            status = "✅ Safe Level";
        else if (currentCaffeine < 200)
            status = "⚠️ Moderate Level";
        else if (currentCaffeine < 400)
            status = "⚠️ High Level";
        else
            status = "🚫 Very High!";
        
        caffeineStatusLabel.Text = $"Current Caffeine:\n{currentCaffeine:F0} mg\n\n{status}\n\nSafe limit: 400mg/day";
    }
    
    private Button CreateGameButton(string gameName, Color color)
    {
        var button = new Button
        {
            Text = gameName,
            Size = new Size(220, 150),
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            BackColor = color,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Margin = new Padding(10)
        };
        
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = ControlPaint.Light(color, 0.1f);
        button.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(color, 0.1f);
        
        return button;
    }
    
    private void LaunchGame(string gameName)
    {
        Form? gameForm = gameName switch
        {
            "PacMan" => new PacManForm(),
            "Sudoku" => new SudokuForm(),
            "Minesweeper" => new MinesweeperForm(),
            "SpiderSolitaire" => new SpiderSolitaireForm(),
            _ => null
        };
        
        if (gameForm != null)
        {
            gameForm.ShowDialog(this);
        }
    }
    
    private void ShowAbout(object? sender, EventArgs e)
    {
        MessageBox.Show(
            "CoffeePause - Game Library\n\n" +
            "A collection of classic games:\n" +
            "- Pac-Man\n" +
            "- Sudoku\n" +
            "- Minesweeper\n" +
            "- Spider Solitaire\n\n" +
            "Version 1.0\n\n" +
            "For help, contact: semanurln0.code@gmail.com",
            "About CoffeePause",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        );
    }
    
    private void MainForm_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            Application.Exit();
        }
    }
    
    private void MainForm_Resize(object? sender, EventArgs e)
    {
        // Calculate scale factors
        float scaleX = (float)this.ClientSize.Width / InitialWidth;
        float scaleY = (float)this.ClientSize.Height / InitialHeight;
        float scale = Math.Min(scaleX, scaleY); // Use uniform scaling
        
        // Find the main panel and games panel
        Panel? mainPanel = null;
        TableLayoutPanel? gamesPanel = null;
        PictureBox? logoPictureBox = null;
        Label? titleLabel = null;
        Label? subtitleLabel = null;
        
        foreach (Control control in this.Controls)
        {
            if (control is Panel panel && panel.BackgroundImage != null)
            {
                mainPanel = panel;
                break;
            }
        }
        
        if (mainPanel != null)
        {
            foreach (Control control in mainPanel.Controls)
            {
                if (control.Name == "gamesPanel" || control is TableLayoutPanel)
                {
                    gamesPanel = control as TableLayoutPanel;
                }
                else if (control is PictureBox)
                {
                    logoPictureBox = control as PictureBox;
                }
                else if (control is Label label)
                {
                    if (label.Text == "CoffeePause")
                    {
                        titleLabel = label;
                    }
                    else if (label.Text == "Choose Game")
                    {
                        subtitleLabel = label;
                    }
                }
            }
            
            // Scale and center the games panel
            if (gamesPanel != null)
            {
                int scaledWidth = (int)(500 * scale);
                int scaledHeight = (int)(350 * scale);
                gamesPanel.Size = new Size(scaledWidth, scaledHeight);
                
                int centerX = (this.ClientSize.Width - scaledWidth) / 2;
                int centerY = (int)((this.ClientSize.Height - scaledHeight) / 2 + 20 * scale);
                gamesPanel.Location = new Point(Math.Max(10, centerX), Math.Max((int)(150 * scale), centerY));
                
                // Scale button fonts
                foreach (Control ctrl in gamesPanel.Controls)
                {
                    if (ctrl is Button btn)
                    {
                        btn.Font = new Font("Segoe UI", 16 * scale, FontStyle.Bold);
                    }
                }
            }
            
            // Scale and reposition title elements based on new center
            int formCenterX = this.ClientSize.Width / 2;
            
            if (logoPictureBox != null)
            {
                int logoSize = (int)(64 * scale);
                logoPictureBox.Size = new Size(logoSize, logoSize);
                logoPictureBox.Location = new Point((int)(formCenterX - 150 * scale), (int)(40 * scale));
            }
            
            if (titleLabel != null)
            {
                titleLabel.Font = new Font("Segoe UI", 28 * scale, FontStyle.Bold);
                // Recalculate position after font change
                titleLabel.Location = new Point((int)(formCenterX - 80 * scale), (int)(50 * scale));
            }
            
            if (subtitleLabel != null)
            {
                subtitleLabel.Font = new Font("Segoe UI", 12 * scale);
                subtitleLabel.Location = new Point((int)(formCenterX - subtitleLabel.Width / 2), (int)(100 * scale));
            }
        }
    }
}
