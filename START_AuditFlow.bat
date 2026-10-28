@echo off
cd /d "%~dp0"
where docker >nul 2>nul || (echo Docker Desktop is required. & pause & exit /b 1)
docker compose up -d
if errorlevel 1 (echo Unable to start. Check Docker Desktop and .env. & pause & exit /b 1)
echo AuditFlow service started. Open the URL in docs\INSTALLATION.md (default http://localhost:8000).
pause
