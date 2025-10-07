@echo off
REM Script to run API with employee user token
REM Uses Eve Adams (Senior Frontend Engineer)
REM User ID: c7746e91-a5e8-4f8b-9f22-f48374ffa2a4
REM Employee ID: 32323232-3232-4323-2323-323232323232

echo Starting API with Employee user token...
echo User: Eve Adams (Senior Frontend Engineer)
echo User ID: c7746e91-a5e8-4f8b-9f22-f48374ffa2a4
echo Employee ID: 32323232-3232-4323-2323-323232323232
echo.

REM Run API with employee user token
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-api-with-token.ps1" -UserId "c7746e91-a5e8-4f8b-9f22-f48374ffa2a4"