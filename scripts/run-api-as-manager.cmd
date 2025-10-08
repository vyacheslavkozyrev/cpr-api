@echo off
REM Script to run API with manager user token
REM Uses Henry Wilson (Director of Technical Support)
REM User ID: 977f4f1f-b3ce-4244-98fc-2c0d0248de88
REM Employee ID: 00000000-0000-0000-0000-00000000000a

echo Starting API with Manager user token...
echo User: Henry Wilson (Director of Technical Support)
echo User ID: 977f4f1f-b3ce-4244-98fc-2c0d0248de88
echo Employee ID: 00000000-0000-0000-0000-00000000000a
echo.

REM Run API with manager user token
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-api-with-token.ps1" -UserId "977f4f1f-b3ce-4244-98fc-2c0d0248de88"