# 🍽️ Food Safety Inspection Tracker

ASP.NET Core MVC application for tracking food premises inspections across multiple towns.

## Credentials
- **Admin:** assignment@dorset.ie / Dorset123!
- **Inspector:** inspector@dorset.ie / Dorset123!
- **Viewer:** viewer@dorset.ie / Dorset123!

## Features
- Premises management with risk ratings and consecutive fails detection
- Inspections with automatic follow-up creation when score is below 50
- Follow-ups with overdue highlighting
- Dashboard with Pass vs Fail chart and filtering by Town/Risk Rating
- Role-based access control (Admin, Inspector, Viewer)
- Serilog logging to console and rolling file
- User management page for Admin

## Tech Stack
- ASP.NET Core MVC (.NET 10)
- Entity Framework Core + SQLite
- Identity with Roles
- Serilog logging
- xUnit tests (8 tests)
- GitHub Actions CI

## How to Run
1. Clone the repo
2. Open `FoodSafetyTracker.sln` in Visual Studio
3. Run `Update-Database` in Package Manager Console
4. Press F5
