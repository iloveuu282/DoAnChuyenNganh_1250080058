@echo off
cd /d "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\ReviewChecks.ps1"
set "result=%ERRORLEVEL%"
pause
exit /b %result%
