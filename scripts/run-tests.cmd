@echo off
REM Wrapper to run PowerShell test script without requiring the user to change ExecutionPolicy.
REM Usage: .\scripts\run-tests.cmd

REM Get directory of this script and call the PowerShell script with ExecutionPolicy bypass
set SCRIPT_DIR=%~dp0
powershell -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT_DIR%run-tests.ps1"
