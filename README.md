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
