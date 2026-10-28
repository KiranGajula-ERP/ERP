@echo off
cd /d "%~dp0"
echo Rebuilding the application container. Database data is not deleted.
docker compose up -d --build
if errorlevel 1 (echo Rebuild failed. See Docker output. & pause & exit /b 1)
echo Rebuild command completed. Check service health and docs\TEST_PLAN.md.
pause
