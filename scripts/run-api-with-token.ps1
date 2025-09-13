# run-api-with-token.ps1
# Generates a token, copies it to the clipboard, sets required env vars, and runs the API project.
# Usage: powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\run-api-with-token.ps1

param(
    [string]$SigningKey = 'test-signing-key-12345',
    [string]$UserId = '33333333-3333-3333-3333-333333333333',
    [string]$Url = 'http://localhost:5000'
)

Write-Host "Generating token for user $UserId"

# Create HMACSHA256 of user id with signing key
$hmac = [System.Security.Cryptography.HMACSHA256]::new([System.Text.Encoding]::UTF8.GetBytes($SigningKey))
$sig = $hmac.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($UserId))
$token = "$UserId.$([Convert]::ToBase64String($sig))"

# Copy to clipboard
Set-Clipboard $token
Write-Host "Token generated and copied to clipboard (length=$($token.Length))"
Write-Host "Token (truncated): $($token.Substring(0,[Math]::Min(60,$token.Length)))..."

# Export required env vars for the API process
$env:JWT_SIGNING_KEY = $SigningKey
$env:ASPNETCORE_URLS = $Url

Write-Host "Starting CPR API at $Url (JWT signing key set)."

# Run API project (this process will run in the current shell)
# Using --project to point at the web project
dotnet run --project "$PSScriptRoot\..\src\CPR.Api\CPR.Api.csproj" --configuration Debug
