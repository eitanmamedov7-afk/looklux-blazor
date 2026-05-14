# Setup on a New Computer

## 1. Clone the repository
```powershell
git clone https://github.com/eitanmamedov7-afk/looklux-blazor.git
cd looklux-blazor
```

## 2. Install prerequisites
- .NET SDK 8 for the Web project
- .NET SDK 9 with MAUI workload for the Mobile project
- MySQL Server

## 3. Create or connect a MySQL database
Create a database named `eitan_project12` or use your own name.

If you use a different database name or credentials, update the connection string in:
- `ConsoleApp1/DB.cs`

Current connection string is hard coded in `MySqlConnSTR`.

## 4. Apply SQL updates
Run these files in MySQL:
- `docs/sql/2026-01-29-matching.sql`
- `docs/sql/2026-02-09-outfit-metadata-and-features.sql`
- `docs/sql/2026-02-09-filter-indexes.sql`

## 5. Configure optional external services
For AI image analysis and outfit scoring:
- set `GEMINI_API_KEY` in environment variables

For password reset email:
- configure SMTP values in `gadifff/appsettings.json`
- or provide values with environment variables

## 6. Run Web app
```powershell
dotnet restore
dotnet run --project gadifff/gadifff.csproj
```

## 7. Run MAUI app
```powershell
dotnet build gadifff.Mobile/gadifff.Mobile.csproj
```

MAUI opens the same backend in a WebView.
You can change server URL inside the app if needed.
