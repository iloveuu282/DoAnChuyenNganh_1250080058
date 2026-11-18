@echo off
cd /d "%~dp0"
echo SQL Server test uses Windows Authentication on .\SQLEXPRESS.
echo It creates new databases ending in _Test. Application data is not reset.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\ReviewChecks.ps1" -SqlServer
set "result=%ERRORLEVEL%"
pause
exit /b %result%
