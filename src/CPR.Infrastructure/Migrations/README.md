Migrations will be created using `dotnet ef migrations add <Name> -p src/CPR.Infrastructure -s src/CPR.Api` and applied with `dotnet ef database update -p src/CPR.Infrastructure -s src/CPR.Api`.

Ensure the connection string is available via environment variable or appsettings when running migrations.
