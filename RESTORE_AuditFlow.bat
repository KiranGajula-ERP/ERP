@echo off
cd /d "%~dp0"
echo WARNING: Database restore replaces data in the current AuditFlow database.
echo Restore only after verifying the backup file and coordinating with all users.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\Restore.ps1"
if errorlevel 1 (echo Restore reported an error. Review the output. & pause & exit /b 1)
pause
