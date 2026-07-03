@echo off
title [1] Azure Storage Emulator - GUI Instructions
color 0A
echo.
echo ========================================
echo   [1] AZURE STORAGE EMULATOR (GUI)
echo ========================================
echo.
echo No CLI needed! Use Azure Storage Explorer GUI instead.
echo.
echo STEPS:
echo 1. Open Azure Storage Explorer (you already have it!)
echo 2. Look for "Emulator & Attached" in left panel
echo 3. If not green, right-click and "Start Storage Emulator"
echo 4. You should see:
echo    - (Emulator - Default Ports) (Key)
echo    - Blob Containers
echo    - Queues        ^<-- Click here to see messages!
echo    - Tables
echo.
echo ✅ If you see "Queues" folder, you're ready!
echo.
echo Next: Run START-2-AzureFunction.bat
echo.
echo For detailed GUI guide, see: STORAGE-EXPLORER-GUIDE.md
echo.
pause
