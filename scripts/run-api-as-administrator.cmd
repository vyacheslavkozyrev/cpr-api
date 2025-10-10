@echo off
REM Script to run API with administrator user token
REM Uses Administrator user
REM User ID: 679add6e-6c29-4e00-b6a5-b69c8e0f3445

echo Starting API with Administrator user token...
echo User: Administrator
echo User ID: 679add6e-6c29-4e00-b6a5-b69c8e0f3445
echo.

REM Run API with administrator user token
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-api-with-token.ps1" -UserId "679add6e-6c29-4e00-b6a5-b69c8e0f3445"
