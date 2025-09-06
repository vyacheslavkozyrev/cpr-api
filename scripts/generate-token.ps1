<#
Generate a stub HMAC token for local development.
Usage:
  # use env var JWT_SIGNING_KEY or pass -SigningKey
  .\generate-token.ps1 -SigningKey 'test-signing-key-12345' -UserId '00000000-0000-0000-0000-000000000123' -Copy

Outputs the token to stdout and optionally copies it to the clipboard with -Copy.
Token format: {userId}.{base64Signature} where signature = HMACSHA256(UTF8Bytes(signingKey), UTF8Bytes(userId))
#>
param(
    [string]$SigningKey = $env:JWT_SIGNING_KEY,
    [string]$UserId = '00000000-0000-0000-0000-000000000123',
    [switch]$Copy
)

if (-not $SigningKey) {
    Write-Error "Signing key not provided. Set the JWT_SIGNING_KEY environment variable or pass -SigningKey."
    exit 1
}

try {
    $keyBytes = [System.Text.Encoding]::UTF8.GetBytes($SigningKey)
    $dataBytes = [System.Text.Encoding]::UTF8.GetBytes($UserId)
    $hmac = [System.Security.Cryptography.HMACSHA256]::new($keyBytes)
    $sig = $hmac.ComputeHash($dataBytes)
    $token = $UserId + '.' + [Convert]::ToBase64String($sig)
    if ($Copy) { Set-Clipboard $token }
    Write-Output $token
} catch {
    Write-Error "Failed to generate token: $($_.Exception.Message)"
    exit 1
}
