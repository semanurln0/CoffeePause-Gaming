namespace GameLauncher;

public partial class MainForm : Form
{
    private const int InitialWidth = 1100; // Increased width for caffeine panel
    private const int InitialHeight = 600;
    private const int CaffeinePanelWidth = 280;
    private const int CaffeinePanelRightMargin = 20;
    private const int CaffeinePanelHeight = 520;
    private const int CaffeinePanelTopMargin = 40;
    private const int CaffeinePanelMinHeight = 400; // Minimum height to keep panel usable
    private CaffeineTracker caffeineTracker = new CaffeineTracker();
    private Label? caffeineStatusLabel;
    private Panel? caffeinePanel;
    private ListBox? caffeineHistoryList;
    private DateTimePicker? sleepTimePicker;
    
    public MainForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "CoffeePause - Game Launcher";
        this.ClientSize = new Size(InitialWidth, InitialHeight);
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
        catch (Exception ex)
        {
            // Log background image loading failure for debugging
            System.Diagnostics.Debug.WriteLine($"Failed to load background image: {ex.Message}");
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
        try
        {
            caffeinePanel = CreateCaffeineTrackerPanel();
            mainPanel.Controls.Add(caffeinePanel);
            caffeinePanel.BringToFront(); // Ensure it's visible on top
            System.Diagnostics.Debug.WriteLine("Caffeine panel created successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to create caffeine panel: {ex.Message}");
            // Continue without caffeine panel if creation fails
        }
        
        this.Controls.Add(mainPanel);
    }
    
    private Panel CreateCaffeineTrackerPanel()
    {
        // Calculate position from the right edge of the client area
        // Since we set ClientSize explicitly in InitializeComponent, it should be available
        int clientWidth = this.ClientSize.Width;
        int xPosition = clientWidth - CaffeinePanelWidth - CaffeinePanelRightMargin;
        
        var panel = new Panel
        {
            Name = "caffeinePanel",
            Location = new Point(xPosition, CaffeinePanelTopMargin),
            Size = new Size(CaffeinePanelWidth, CaffeinePanelHeight),
            BackColor = Color.FromArgb(240, 248, 255),
            BorderStyle = BorderStyle.FixedSingle
            // Removed conflicting Anchor property - using absolute positioning instead
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
            Location = new Point(10, 45),
            Size = new Size(130, 18),
            Font = new Font("Segoe UI", 8)
        };
        panel.Controls.Add(drinkLabel);
        
        var drinkCombo = new ComboBox
        {
            Location = new Point(10, 65),
            Size = new Size(105, 22),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 8)
        };
        // Load default drinks and custom drinks
        var allDrinks = new List<string>(CaffeineData.CaffeineContent.Keys);
        var customDrinks = caffeineTracker.LoadCustomDrinks();
        allDrinks.AddRange(customDrinks.Keys);
        drinkCombo.Items.AddRange(allDrinks.ToArray());
        drinkCombo.SelectedIndex = 0;
        panel.Controls.Add(drinkCombo);
        
        // Add Custom Drink button
        var addCustomBtn = new Button
        {
            Text = "+",
            Location = new Point(120, 65),
            Size = new Size(20, 22),
            BackColor = Color.FromArgb(100, 200, 100),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        addCustomBtn.FlatAppearance.BorderSize = 0;
        addCustomBtn.Click += (s, e) => ShowAddCustomDrinkDialog(drinkCombo, customDrinks);
        panel.Controls.Add(addCustomBtn);
        
        // Size
        var sizeLabel = new Label
        {
            Text = "Size (ml):",
            Location = new Point(145, 45),
            Size = new Size(125, 18),
            Font = new Font("Segoe UI", 8)
        };
        panel.Controls.Add(sizeLabel);
        
        var sizeInput = new NumericUpDown
        {
            Location = new Point(145, 65),
            Size = new Size(125, 22),
            Minimum = 50,
            Maximum = 1000,
            Value = 250,
            Increment = 50,
            Font = new Font("Segoe UI", 8)
        };
        panel.Controls.Add(sizeInput);
        
        // Quantity
        var quantityLabel = new Label
        {
            Text = "Qty:",
            Location = new Point(10, 92),
            Size = new Size(60, 18),
            Font = new Font("Segoe UI", 8)
        };
        panel.Controls.Add(quantityLabel);
        
        var quantityInput = new NumericUpDown
        {
            Location = new Point(10, 112),
            Size = new Size(60, 22),
            Minimum = 1,
            Maximum = 10,
            Value = 1,
            Font = new Font("Segoe UI", 8)
        };
        panel.Controls.Add(quantityInput);
        
        // When (time consumed)
        var whenLabel = new Label
        {
            Text = "When:",
            Location = new Point(75, 92),
            Size = new Size(195, 18),
            Font = new Font("Segoe UI", 8)
        };
        panel.Controls.Add(whenLabel);
        
        var whenPicker = new DateTimePicker
        {
            Location = new Point(75, 112),
            Size = new Size(195, 22),
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "MM/dd hh:mm tt",
            Font = new Font("Segoe UI", 8)
        };
        panel.Controls.Add(whenPicker);
        
        // Sleep time
        var sleepLabel = new Label
        {
            Text = "Sleep Time:",
            Location = new Point(10, 139),
            Size = new Size(130, 18),
            Font = new Font("Segoe UI", 8)
        };
        panel.Controls.Add(sleepLabel);
        
        sleepTimePicker = new DateTimePicker
        {
            Location = new Point(10, 159),
            Size = new Size(130, 22),
            Format = DateTimePickerFormat.Time,
            ShowUpDown = true,
            Value = DateTime.Today.AddHours(23),
            Font = new Font("Segoe UI", 8)
        };
        panel.Controls.Add(sleepTimePicker);
        
        // Add button
        var addButton = new Button
        {
            Text = "Add",
            Location = new Point(145, 139),
            Size = new Size(125, 42),
            BackColor = Color.FromArgb(74, 166, 186),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        addButton.FlatAppearance.BorderSize = 0;
        addButton.Click += (s, e) =>
        {
            try
            {
                var drinkType = drinkCombo.SelectedItem?.ToString() ?? "";
                var size = (int)sizeInput.Value;
                var quantity = (int)quantityInput.Value;
                var when = whenPicker.Value;
                
                // Calculate caffeine considering custom drinks
                double caffeineAmount;
                if (CaffeineData.CaffeineContent.ContainsKey(drinkType))
                {
                    caffeineAmount = CaffeineData.CalculateCaffeine(drinkType, size, quantity);
                }
                else
                {
                    // It's a custom drink
                    var customDrinksLocal = caffeineTracker.LoadCustomDrinks();
                    if (customDrinksLocal.TryGetValue(drinkType, out double contentPer100ml))
                    {
                        caffeineAmount = (contentPer100ml * size / 100.0) * quantity;
                    }
                    else
                    {
                        throw new Exception("Drink type not found");
                    }
                }
                
                var entry = new CaffeineEntry
                {
                    DrinkType = drinkType,
                    SizeMl = size,
                    Quantity = quantity,
                    CaffeineAmount = caffeineAmount,
                    ConsumedAt = when
                };
                
                caffeineTracker.SaveEntry(entry);
                UpdateCaffeineStatus(sleepTimePicker.Value);
                RefreshHistoryList();
                
                MessageBox.Show($"Added {caffeineAmount:F0}mg of caffeine!", "Success", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // Log detailed error for debugging
                System.Diagnostics.Debug.WriteLine($"Error saving caffeine entry: {ex.Message}");
                // Show user-friendly error message
                MessageBox.Show("Failed to save caffeine entry. Please try again.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };
        panel.Controls.Add(addButton);
        
        // Status label (smaller)
        caffeineStatusLabel = new Label
        {
            Location = new Point(10, 190),
            Size = new Size(260, 60),
            Font = new Font("Segoe UI", 8),
            ForeColor = Color.FromArgb(70, 70, 70),
            TextAlign = ContentAlignment.TopLeft,
            BackColor = Color.FromArgb(255, 255, 240),
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(3)
        };
        panel.Controls.Add(caffeineStatusLabel);
        
        // History section
        var historyLabel = new Label
        {
            Text = "Today's History:",
            Location = new Point(10, 258),
            Size = new Size(260, 18),
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(74, 166, 186)
        };
        panel.Controls.Add(historyLabel);
        
        // History list
        caffeineHistoryList = new ListBox
        {
            Location = new Point(10, 280),
            Size = new Size(260, 190),
            Font = new Font("Segoe UI", 8),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        panel.Controls.Add(caffeineHistoryList);
        
        // Delete and Reset buttons
        var deleteButton = new Button
        {
            Text = "Remove Selected",
            Location = new Point(10, 475),
            Size = new Size(125, 30),
            BackColor = Color.FromArgb(220, 50, 50),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 8, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        deleteButton.FlatAppearance.BorderSize = 0;
        deleteButton.Click += (s, e) =>
        {
            if (caffeineHistoryList.SelectedItem != null)
            {
                var selectedEntry = caffeineHistoryList.SelectedItem as CaffeineEntry;
                if (selectedEntry != null)
                {
                    caffeineTracker.DeleteEntry(selectedEntry);
                    UpdateCaffeineStatus(sleepTimePicker.Value);
                    RefreshHistoryList();
                }
            }
        };
        panel.Controls.Add(deleteButton);
        
        var resetButton = new Button
        {
            Text = "Reset All",
            Location = new Point(145, 475),
            Size = new Size(125, 30),
            BackColor = Color.FromArgb(180, 180, 180),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 8, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        resetButton.FlatAppearance.BorderSize = 0;
        resetButton.Click += (s, e) =>
        {
            var result = MessageBox.Show("Are you sure you want to reset all caffeine history?", 
                "Confirm Reset", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                caffeineTracker.ResetAllEntries();
                UpdateCaffeineStatus(sleepTimePicker.Value);
                RefreshHistoryList();
            }
        };
        panel.Controls.Add(resetButton);
        
        UpdateCaffeineStatus(sleepTimePicker.Value);
        RefreshHistoryList();
        
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
        
        caffeineStatusLabel.Text = $"Current: {currentCaffeine:F0}mg\n{status}\nLimit: 400mg/day";
    }
    
    private void RefreshHistoryList()
    {
        if (caffeineHistoryList == null) return;
        
        caffeineHistoryList.Items.Clear();
        caffeineHistoryList.DisplayMember = "DisplayText";
        
        var entries = caffeineTracker.LoadEntries();
        var today = DateTime.Today;
        var todayEntries = entries.Where(e => e.ConsumedAt.Date == today).OrderByDescending(e => e.ConsumedAt);
        
        foreach (var entry in todayEntries)
        {
            caffeineHistoryList.Items.Add(entry);
        }
        
        if (caffeineHistoryList.Items.Count == 0)
        {
            caffeineHistoryList.Items.Add("No entries for today");
        }
    }
    
    private void ShowAddCustomDrinkDialog(ComboBox drinkCombo, Dictionary<string, double> customDrinks)
    {
        var dialog = new Form
        {
            Text = "Add Custom Drink",
            Size = new Size(350, 200),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false
        };
        
        var nameLabel = new Label
        {
            Text = "Drink Name:",
            Location = new Point(20, 20),
            Size = new Size(100, 20),
            Font = new Font("Segoe UI", 9)
        };
        dialog.Controls.Add(nameLabel);
        
        var nameInput = new TextBox
        {
            Location = new Point(130, 18),
            Size = new Size(180, 22),
            Font = new Font("Segoe UI", 9)
        };
        dialog.Controls.Add(nameInput);
        
        var caffeineLabel = new Label
        {
            Text = "Caffeine (mg per 100ml):",
            Location = new Point(20, 60),
            Size = new Size(150, 20),
            Font = new Font("Segoe UI", 9)
        };
        dialog.Controls.Add(caffeineLabel);
        
        var caffeineInput = new NumericUpDown
        {
            Location = new Point(180, 58),
            Size = new Size(130, 22),
            Minimum = 0,
            Maximum = 500,
            Value = 40,
            DecimalPlaces = 1,
            Font = new Font("Segoe UI", 9)
        };
        dialog.Controls.Add(caffeineInput);
        
        var saveButton = new Button
        {
            Text = "Save",
            Location = new Point(100, 110),
            Size = new Size(100, 30),
            BackColor = Color.FromArgb(74, 166, 186),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        saveButton.FlatAppearance.BorderSize = 0;
        saveButton.Click += (s, e) =>
        {
            var name = nameInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Please enter a drink name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            try
            {
                double caffeineContent = (double)caffeineInput.Value;
                caffeineTracker.SaveCustomDrink(name, caffeineContent);
                
                // Update the combo box
                if (!drinkCombo.Items.Contains(name))
                {
                    drinkCombo.Items.Add(name);
                }
                drinkCombo.SelectedItem = name;
                
                MessageBox.Show($"Custom drink '{name}' saved!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dialog.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save custom drink: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };
        dialog.Controls.Add(saveButton);
        
        var cancelButton = new Button
        {
            Text = "Cancel",
            Location = new Point(210, 110),
            Size = new Size(100, 30),
            BackColor = Color.Gray,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9)
        };
        cancelButton.FlatAppearance.BorderSize = 0;
        cancelButton.Click += (s, e) => dialog.Close();
        dialog.Controls.Add(cancelButton);
        
        dialog.ShowDialog(this);
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
            
            // Reposition and scale caffeine panel to stay on the right side
            if (caffeinePanel != null)
            {
                // Calculate new position to keep panel on right side with proper margin
                int newXPosition = this.ClientSize.Width - CaffeinePanelWidth - CaffeinePanelRightMargin;
                
                // Scale the panel height to fit the window, but maintain a minimum size
                int scaledHeight = (int)(CaffeinePanelHeight * scaleY);
                int maxAllowedHeight = this.ClientSize.Height - (int)(CaffeinePanelTopMargin * 2);
                scaledHeight = Math.Max(CaffeinePanelMinHeight, Math.Min(scaledHeight, maxAllowedHeight));
                
                caffeinePanel.Location = new Point(newXPosition, (int)(CaffeinePanelTopMargin * scaleY));
                caffeinePanel.Size = new Size(CaffeinePanelWidth, scaledHeight);
                
                // Ensure panel stays visible and on top
                caffeinePanel.BringToFront();
                
                System.Diagnostics.Debug.WriteLine($"Caffeine panel resized to: {caffeinePanel.Location}, {caffeinePanel.Size}");
            }
        }
    }
}
