@echo off
cd /d "%~dp0"
docker compose ps
echo.
echo Recent web logs:
docker compose logs --tail=80 web
echo.
pause
