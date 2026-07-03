@echo off
title [2] Azure Function App
color 0B
echo.
echo ========================================
echo   [2] AZURE FUNCTION APP
echo ========================================
echo.
echo Building and starting Azure Function on PORT 7071...
echo.

cd "D:\personal\Azure-leaaning\Azure-learning-hands-on\azure-notification\NotificationSystem\NotificationFunctionApp"

dotnet build
func start --port 7071

pause
