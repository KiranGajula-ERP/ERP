@echo off
cd /d "%~dp0"
where docker >nul 2>nul || (echo Docker Desktop is required. & pause & exit /b 1)
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\Backup.ps1"
if errorlevel 1 (echo Backup reported an error. Check the output above. & pause & exit /b 1)
echo Backup finished. Keep an additional copy on an external drive.
pause
