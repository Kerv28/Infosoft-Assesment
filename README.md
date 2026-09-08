# Bogsy Video Store (BVS) Web App

A web-based video rental management application for **Bogsy Video Store** located in Buhangin, Davao City. Built with ASP.NET Core Razor Pages and backed by a SQLite database.

---

## Features

- **Customer Management**: Add and maintain a directory of customer names and contact numbers.
- **Video Inventory Management**: 
  - Add new titles with custom categories (**VCD** / **DVD**), rental limits, and stock quantities.
  - Delete items from the video catalog.
- **Rental & Return Module**:
  - Process movie rentals by linking customers, titles, and rental durations.
  - Process video returns with automatic calculation of overdue penalties.
- **Reporting System**:
  - **Alphabetical Video Inventory**: Visual breakdown of total inventory showing items currently checked in vs. checked out.
  - **Active Rentals**: Detailed list tracking current rentals, due dates, and real-time overdue statuses.
- **Dashboard Metrics**: Quick view of active inventory counts, customer counts, videos checked out, and overdue alerts.

---

## Tech Stack

* **Framework**: ASP.NET Core 10.0 (Razor Pages)
* **Database**: SQLite (`Microsoft.Data.Sqlite`)
* **Styling**: Bootstrap 5 + Custom CSS

---

## Getting Started

### Prerequisites

* [.NET 10.0 SDK](https://dotnet.microsoft.com/download) installed on your machine.

### Quick Start

1. **Clone the repository**:
   ```bash
   git clone [https://github.com/your-username/bvs-web-app.git](https://github.com/your-username/bvs-web-app.git)
   cd bvs-web-app
2. **Restore Dependecies**:
   ```bash
   dotnet restore
3. **Run the application**:
   ```bash
   dotnet run
4.  **Access the web appn**:
   Open your browser and navigate to http://localhost:5000 or the HTTPS URL provided in the terminal.

Project Structure
```bash
Plaintext
BVSWebApp/
├── Pages/
│   ├── Shared/
│   │   └── _Layout.cshtml          # Main application template & navigation layout
│   ├── Error.cshtml                # Error display view
│   ├── Error.cshtml.cs             # Error model logic & request ID handling
│   ├── Index.cshtml                # Store dashboard, rental forms & reports
│   ├── Index.cshtml.cs             # Index page handlers & service interactions
│   ├── Privacy.cshtml              # Privacy policy view
│   └── Privacy.cshtml.cs           # Privacy page model
├── Properties/
│   └── launchSettings.json         # Development environment configuration
├── .vscode/
│   └── launch.json                 # VS Code debugging settings
├── _Layout.cshtml.css              # Isolation CSS for site layout
└── BVSWebApp.csproj                # .NET project file & dependencies
