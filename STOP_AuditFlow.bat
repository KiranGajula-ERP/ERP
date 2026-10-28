@echo off
cd /d "%~dp0"
docker compose stop
echo AuditFlow services stopped. Database volume and document files have been retained.
pause
