@echo off
REM Start Azure Functions locally

title Azure Function App - NotificationFunctionApp
echo.
echo ========================================
echo   Azure Function App
echo   NotificationFunctionApp
echo ========================================
echo.
echo Prerequisites:
echo - Azurite must be running in another terminal
echo - Use: start-azurite.bat
echo.
echo Building and starting function app...
echo.

cd /d "%~dp0"

REM Restore NuGet packages
echo [Step 1/3] Restoring NuGet packages...
dotnet restore

REM Build the project
echo [Step 2/3] Building project...
dotnet build

REM Start the function app
echo [Step 3/3] Starting function app...
func start

pause
