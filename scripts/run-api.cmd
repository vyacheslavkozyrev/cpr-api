@echo off
REM Wrapper to run the PowerShell script that generates a token, copies it to clipboard, and starts the API.
set SCRIPT_DIR=%~dp0
powershell -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT_DIR%run-api-with-token.ps1"
