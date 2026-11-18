@echo off
cd /d "%~dp0"
set "Logging__LogLevel__Microsoft.EntityFrameworkCore.Database.Command=Information"
powershell -NoProfile -ExecutionPolicy Bypass -File "scripts\Start.ps1"
pause
