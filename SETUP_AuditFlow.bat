@echo off
setlocal
cd /d "%~dp0"
echo ======================================================
echo AuditFlow - First-time Windows setup (.NET + PostgreSQL)
echo ======================================================
where docker >nul 2>nul
if errorlevel 1 (
  echo Docker Desktop is not installed or is not on PATH.
  echo Install Docker Desktop for Windows, start it, then run this file again.
  echo https://docs.docker.com/desktop/setup/install/windows-install/
  pause
  exit /b 1
)
docker info >nul 2>nul
if errorlevel 1 (
  echo Docker Desktop is installed but is not running yet.
  echo Start Docker Desktop, wait until it is ready, then rerun this file.
  pause
  exit /b 1
)
if not exist .env (
  copy .env.example .env >nul
  echo Created .env from the example.
  echo Please edit .env and replace BOTH CHANGE_ME password values with unique long passwords.
  start notepad .env
  echo Save .env, close Notepad, then double-click SETUP_AuditFlow.bat again.
  pause
  exit /b 0
)
findstr /C:"CHANGE_ME" .env >nul
if not errorlevel 1 (
  echo The .env file still contains CHANGE_ME placeholders.
  start notepad .env
  echo Replace both values, save the file and run SETUP_AuditFlow.bat again.
  pause
  exit /b 1
)
echo Validating Docker Compose configuration...
docker compose config -q
if errorlevel 1 (
  echo Docker Compose configuration is invalid. Check your .env and docker-compose.yml.
  pause
  exit /b 1
)
echo Building and starting AuditFlow. The first run downloads the .NET and PostgreSQL images and NuGet packages.
docker compose up -d --build
if errorlevel 1 (
  echo Setup failed. Review the messages above and see docs\INSTALLATION.md.
  pause
  exit /b 1
)
echo Waiting briefly for the application to start...
timeout /t 8 /nobreak >nul
docker compose ps
echo.
echo Open http://localhost:8000 by default, or the port set by AUDITFLOW_HTTP_PORT in .env
echo Your initial administrator username/password are the values in .env.
echo After the first successful start, remove AUDITFLOW_BOOTSTRAP_ADMIN_PASSWORD from .env and restart the web service.
echo Important: change your administrator password after signing in. Use only trusted office LAN networks.
echo.
pause
