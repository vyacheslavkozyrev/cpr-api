@echo off
REM Script to run API with employee user token
REM Uses Eve Adams (Director of Security Engineering)
REM User ID: c7746e91-a5e8-4f8b-9f22-f48374ffa2a4
REM Employee ID: 00000000-0000-0000-0000-000000000007

echo Starting API with Employee user token...
echo User: Eve Adams (Director of Security Engineering)
echo User ID: c7746e91-a5e8-4f8b-9f22-f48374ffa2a4
echo Employee ID: 00000000-0000-0000-0000-000000000007
echo.

REM Run API with employee user token
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-api-with-token.ps1" -UserId "c7746e91-a5e8-4f8b-9f22-f48374ffa2a4"