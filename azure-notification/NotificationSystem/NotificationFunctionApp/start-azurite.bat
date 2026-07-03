@echo off
REM Start Azurite (Azure Storage Emulator)
REM This script must be run in a separate terminal/window

title Azurite - Storage Emulator
echo.
echo ========================================
echo   AZURITE - Azure Storage Emulator
echo ========================================
echo.
echo Starting Azurite on port 10000...
echo.

azurite --silent --location ./azurite-data

echo.
echo If you see the message "Azurite (all services) server started"
echo then Azurite is running successfully!
echo.
pause
