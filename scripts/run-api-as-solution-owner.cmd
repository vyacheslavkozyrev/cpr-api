@echo off
REM Script to run API with Solution Owner user token
REM Uses John Doe (Solution Owner)
REM User ID: 679add6e-6c29-4e00-b6a5-b69c8e0f3445
REM Note: Solution Owner role must be assigned in database

echo Starting API with Solution Owner user token...
echo User: John Doe (Solution Owner)
echo User ID: 679add6e-6c29-4e00-b6a5-b69c8e0f3445
echo.
echo NOTE: Ensure john.doe has Solution Owner role assigned in database
echo.

REM Run API with Solution Owner user token
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-api-with-token.ps1" -UserId "679add6e-6c29-4e00-b6a5-b69c8e0f3445"
