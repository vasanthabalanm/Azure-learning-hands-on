@echo off
title Rebuild and Check Function
color 0E

echo ========================================
echo   REBUILD FUNCTION APP
echo ========================================
echo.

cd "D:\personal\Azure-leaaning\Azure-learning-hands-on\azure-notification\NotificationSystem\NotificationFunctionApp"

echo Cleaning previous build...
dotnet clean

echo.
echo Building with latest changes...
dotnet build

echo.
echo ========================================
echo   BUILD COMPLETE!
echo ========================================
echo.
echo Next steps:
echo 1. Close this window
echo 2. Run START-2-AzureFunction.bat
echo 3. Wait for "Worker process started"
echo 4. Look for BOTH functions listed:
echo    - SendNotification
echo    - ProcessNotificationQueue
echo.
pause
