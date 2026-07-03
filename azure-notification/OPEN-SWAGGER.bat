@echo off
title Open Swagger UI
color 0C
echo.
echo ========================================
echo   OPENING SWAGGER UI
echo ========================================
echo.
echo Waiting 5 seconds for Web API to start...
timeout /t 5 /nobreak > nul

echo Opening Swagger in your default browser...
start http://localhost:5000/swagger

echo.
echo ✅ Swagger UI should be open now!
echo.
echo If not, manually open: http://localhost:5000/swagger
echo.
pause
