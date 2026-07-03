@echo off
title Clean and Rebuild Function
color 0C

echo ========================================
echo   CLEANING BUILD FILES
echo ========================================
echo.

cd "D:\personal\Azure-leaaning\Azure-learning-hands-on\azure-notification\NotificationSystem\NotificationFunctionApp"

echo Killing any dotnet processes...
taskkill /F /IM dotnet.exe 2>nul
taskkill /F /IM func.exe 2>nul

echo.
echo Cleaning project...
dotnet clean

echo.
echo Deleting bin and obj folders...
if exist bin rmdir /s /q bin
if exist obj rmdir /s /q obj

echo.
echo Restoring packages...
dotnet restore

echo.
echo Building project...
dotnet build

echo.
echo ========================================
if errorlevel 1 (
    echo   BUILD FAILED!
    echo ========================================
    echo.
    echo Check the errors above.
    echo Common fixes:
    echo 1. Close Visual Studio
    echo 2. Run this script again
    echo 3. Check antivirus isn't blocking files
) else (
    echo   BUILD SUCCESSFUL!
    echo ========================================
    echo.
    echo Now run START-2-AzureFunction.bat
)
echo.
pause
