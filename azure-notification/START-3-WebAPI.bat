@echo off
title [3] Web API
color 0E
echo.
echo ========================================
echo   [3] WEB API (Swagger UI)
echo ========================================
echo.
echo Starting Web API on port 5000...
echo.
echo Once started, Swagger will open automatically!
echo.

cd "D:\personal\Azure-leaaning\Azure-learning-hands-on\azure-notification\NotificationSystem\NotificationWebApi"

dotnet run --urls "http://localhost:5000"

pause
