# CPR — Career & Performance Review

Local dev

1. Start local Postgres and pgAdmin for dev:
   docker-compose -f docker-compose.dev.yml up -d

2. Open the API project and run:
   dotnet restore
   dotnet build
   dotnet run --project src\CPR.Api

3. Run tests:
   dotnet test src\CPR.sln

Conventions
- See `conventions.md` for rules about config, secrets and coding conventions.

Authentication: local stub token

This project includes a development-only authentication stub that accepts HMAC-signed tokens.
The token format is: `{userId}.{base64Signature}`
where signature = `HMACSHA256(UTF8Bytes(JWT_SIGNING_KEY), UTF8Bytes(userId))`.

1) Start the API with a known signing key (PowerShell):

```powershell
$env:JWT_SIGNING_KEY = 'test-signing-key-12345'
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet run --project src\CPR.Api --urls "http://localhost:5000"
```

2) Generate a token (PowerShell):

```powershell
# signingKey must match the server's JWT_SIGNING_KEY string
$signingKey = 'test-signing-key-12345'
$userId = '00000000-0000-0000-0000-000000000123'
$hmac = [System.Security.Cryptography.HMACSHA256]::new([System.Text.Encoding]::UTF8.GetBytes($signingKey))
$sig = $hmac.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($userId))
$token = $userId + '.' + [Convert]::ToBase64String($sig)
Set-Clipboard $token
Write-Host "Token generated and copied to clipboard (length=$($token.Length))"
```

3) Authorize in Swagger (open http://localhost:5000/swagger):

- Click "Authorize". In the input field paste the raw token only (the value produced by the generation step), e.g. `00000000-0000-0000-0000-000000000123.<base64signature>` — do NOT include the `Bearer ` prefix (Swagger UI will add it automatically).
- Click "Authorize" then close the dialog and call protected endpoints like `/Me`.

4) Call `/Me` from PowerShell (no Swagger):

```powershell
# assumes $token variable is available or re-run generation snippet
Invoke-RestMethod -Uri http://localhost:5000/Me -Headers @{ Authorization = "Bearer $token" } -Method Get
```

## Running integration tests locally with Postgres

A minimal test Postgres instance is provided in `docker-compose.test.yml`.

Start the database:

```powershell
docker-compose -f docker-compose.test.yml up -d
```

Set the `DATABASE_URL` environment variable for the test run (PowerShell):

```powershell
$env:DATABASE_URL = "Host=localhost;Port=5432;Database=cpr_test;Username=postgres;Password=postgres"
```

Run the integration tests:

```powershell
dotnet test tests/CPR.IntegrationTests/CPR.IntegrationTests.csproj
```

When finished, bring down the test database:

```powershell
docker-compose -f docker-compose.test.yml down
```

Notes
- The stub signing key stored in the `JWT_SIGNING_KEY` environment variable is treated as a plain UTF-8 string (do not base64-decode it).
- This auth stub is for local development and testing only. Do not use it in production.

Scripts
-------
This repo includes convenience scripts in the `scripts/` folder for running tests and starting the API with a generated token.

Windows (PowerShell):

```powershell
# run all tests (unit, contract, integration)
.\scripts\run-tests.cmd

# start API and generate a token (copied to clipboard)
.\scripts\run-api.cmd
```

Unix / CI (bash):

```bash
# make sure script is executable once on your machine or CI job
chmod +x ./scripts/run-tests.sh
./scripts/run-tests.sh    # default: runs unit, contract, integration
./scripts/run-tests.sh integration
```

If you need to override the test database name set `DATABASE_NAME` before invoking the scripts.

