# CoffeePause - Game Library

A collection of classic games built with .NET WinForms.

## Games Included

### 1. Pac-Man
- Two food types: regular (1pt) and supreme (5pt)
- Eating supreme food makes ghosts vulnerable
- Eating vulnerable ghosts gives 10 points
- Difficulty settings: Easy, Medium, Hard
- Ghost AI: Chase when normal, flee when vulnerable

### 2. Sudoku
- Random puzzle generation
- Draft mode (Ctrl + Number) for notes in red
- Submit mode (Number) for answers in blue
- Solution validation
- High score tracking

### 3. Minesweeper
- Left click to reveal cells
- Right click to place flags
- Difficulty presets and custom grid sizes
- Mine counter display
- Score based on difficulty and grid size

### 4. Spider Solitaire
- Suit options: 1, 2, or 4 suits
- Drag and drop cards
- Hint system for valid moves
- Undo last 10 moves
- Complete 13-card sequences to win

## Features

- **High Score System**: Each game tracks top 10 scores with player names and dates
- **Persistent Storage**: Scores saved to `%APPDATA%/CoffeePause` in JSON format
  - Windows: `C:\Users\<YourName>\AppData\Roaming\CoffeePause\`
  - Files: `PacMan_scores.json`, `Sudoku_scores.json`, `Minesweeper_scores.json`, `SpiderSolitaire_scores.json`
- **Settings**: Per-game difficulty and customization options
- **Clean UI**: Modern, colorful main menu with easy game selection
- **SVG Assets**: High-quality scalable vector graphics for game elements

## Building and Running

### Prerequisites
- .NET 9.0 SDK or later
- Windows OS (for WinForms support)

### Quick Start (Windows)

1. **Clone the repository**
   ```bash
   git clone https://github.com/semanurln0/CoffeePause.git
   cd CoffeePause
   ```

2. **Build the application**
   - Double-click `Build.bat`, or
   - Run manually:
   ```bash
   dotnet publish Code/GameLauncher/GameLauncher.csproj -c Release -r win-x64 --self-contained false -o .
   ```

3. **Run the application**
   - Double-click `GameLauncher.exe` in the main folder

### Development Build

```bash
# Build the solution
dotnet build CoffeePause.sln

# Run the application
dotnet run --project Code/GameLauncher/GameLauncher.csproj
```

### Running Tests

```bash
# Run all tests (Windows only - requires Windows Desktop runtime)
dotnet test Code/tests/GameLauncher.Tests/GameLauncher.Tests.csproj
```

**Note**: Tests require Windows Desktop runtime and will not run on Linux/macOS build servers.

### Creating a Self-Contained Executable

If you want to run the application without installing .NET:

```bash
dotnet publish Code/GameLauncher/GameLauncher.csproj -c Release -r win-x64 --self-contained true -o .
```

## Project Structure

```
CoffeePause/
├── Build.bat                      # Build script (outputs to main folder)
├── GameLauncher.exe               # Game executable (after build)
├── CoffeePause.sln                # Solution file
├── Code/
│   ├── GameLauncher/              # Game source code
│   │   ├── Program.cs             # Application entry point
│   │   ├── MainForm.cs            # Main menu
│   │   ├── AssetManager.cs        # SVG asset loading
│   │   ├── HighScoreManager.cs    # Score persistence
│   │   ├── PacManForm.cs          # Pac-Man game
│   │   ├── SudokuForm.cs          # Sudoku game
│   │   ├── MinesweeperForm.cs     # Minesweeper game
│   │   └── SpiderSolitaireForm.cs # Spider Solitaire game
│   └── tests/
│       └── GameLauncher.Tests/
│           └── HighScoreManagerTests.cs  # Score persistence tests
├── Assets/
│   └── sprites/                   # SVG and image assets
└── Documentation/
    └── README.md                  # This file
```

## Testing

The project includes unit tests for core functionality:
- **HighScoreManager**: Score persistence, atomic saves, top-10 tracking
- Tests are located in `tests/GameLauncher.Tests/`
- Run tests on Windows with: `dotnet test`

## Controls

### Pac-Man
- Arrow keys or WASD: Move Pac-Man
- Eat all food to win
- Avoid ghosts (unless they're vulnerable)

### Sudoku
- Click a cell to select it
- Number keys (1-9): Enter value (blue)
- Ctrl + Number keys: Add draft note (red)
- Backspace: Clear cell

### Minesweeper
- Left click: Reveal cell
- Right click: Place/remove flag
- Reveal all non-mine cells to win

### Spider Solitaire
- Drag and drop cards between columns
- Click stock to deal more cards
- Hint button: Suggests a move
- Undo button: Undo last move (up to 10)

## License

This is a personal project for educational and entertainment purposes.

## Credits

Built with:
- .NET 9.0 WinForms
- Newtonsoft.Json for score persistence
- Svg library for SVG rendering
