@echo off
REM Script to run API with manager user token
REM Uses Jane Smith (Manager) - User ID: c6874b28-e2fa-4835-8e8f-159bd5067091
REM Employee ID: 0353f880-f993-4b3a-a7c2-41e7c58f0aa6

echo Starting API with Manager user token...
echo User: Henry Wilson (Manager)
echo User ID: 977f4f1f-b3ce-4244-98fc-2c0d0248de88
echo Employee ID: 0c8b8b8b-8b8b-4b8b-8b8b-8b8b8b8b8b8b
echo.

REM Run API with manager user token
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-api-with-token.ps1" -UserId "977f4f1f-b3ce-4244-98fc-2c0d0248de88"