@echo off
REM Script to run API with employee user token
REM Uses John Doe (Employee) - User ID: 679add6e-6c29-4e00-b6a5-b69c8e0f3445
REM Employee ID: 004e1f8b-1ea3-4e27-a373-ed82f85147cc

echo Starting API with Employee user token...
echo User: John Doe (Employee)
echo User ID: 5950a2be-bdfb-4dcb-9913-1e3e0e022a5c
echo Employee ID: 16161616-1616-4161-6161-616161616161
echo.

REM Run API with employee user token
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-api-with-token.ps1" -UserId "5950a2be-bdfb-4dcb-9913-1e3e0e022a5c"