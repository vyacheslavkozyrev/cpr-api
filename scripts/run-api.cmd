@echo off
REM Script to run API with Microsoft Entra External ID authentication
REM Uses real JWT tokens from Microsoft Entra (no stub tokens)
REM UI application will provide the bearer token

echo ========================================
echo   CPR API - Entra External ID Mode
echo ========================================
echo.
echo Authentication: Microsoft Entra External ID
echo Mode: Production-like (real JWT tokens)
echo API Endpoint: http://localhost:5000
echo.
echo IMPORTANT:
echo - This mode requires real JWT tokens from Microsoft Entra
echo - UI application must authenticate users and provide bearer tokens
echo - No stub tokens will be generated or accepted
echo.
echo Starting API...
echo.

REM Set environment variables for Entra External ID mode
set AUTHENTICATION_MODE=EntraExternalId
set ASPNETCORE_URLS=http://localhost:5000

REM Run API project
cd /d "%~dp0..\src\CPR.Api"
dotnet run --configuration Debug

pause
