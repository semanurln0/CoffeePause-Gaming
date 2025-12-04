@echo off
echo Building CoffeePause Game Library...
echo.

dotnet publish Code/GameLauncher/GameLauncher.csproj -c Release -r win-x64 --self-contained false -o Build

if %ERRORLEVEL% EQU 0 (
    echo.
    echo Build successful!
    echo.
    echo All build files are in: Build\
    echo You can run the game by:
    echo   1. Running Build\GameLauncher.exe
    echo   2. Or create a shortcut to Build\GameLauncher.exe in the main folder
    echo.
    echo Note: The executable requires DLL files in the Build folder to run.
    echo       Do not move the .exe file without all dependencies.
    echo.
    pause
) else (
    echo.
    echo Build failed! Make sure .NET 9.0 SDK is installed.
    echo.
    pause
)
