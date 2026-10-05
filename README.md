# Arniston Letting

| Thomas Dennis | ST10391450
| Dwayne Prins  | ST10032544 

Hosted At https://arniston.duckdns.org/Login 

This is hosted on an old laptop temporarily since existing hosting service did not have support for .Net 10 nor Asp.Net Core

Arniston Letting provides a centralised system for managing properties, owners, bookings, cleaners, tasks, breakages, notifications and reports.

## Overview

The system consists of two ASP.NET Core applications:

* **Arniston Letting Front** — Razor Pages frontend
* **Arniston Letting API** — REST API and database layer

The frontend communicates with the API, while the API uses Entity Framework Core to access the MySQL database.

### Main Features

* User registration and login
* Property management
* Owner management
* Booking management
* Cleaner management
* Cleaner tasks
* Breakage reporting
* Notifications
* Reports
* Dashboard summaries

## Technology Stack

| Component         | Technology                       |
| ----------------- | -------------------------------- |
| Framework         | .NET 10                          |
| Frontend          | ASP.NET Core Razor Pages         |
| Backend           | ASP.NET Core Web API             |
| UI                | HTML, CSS, JavaScript, Bootstrap |
| Database          | MySQL                            |
| ORM               | Entity Framework Core            |
| MySQL Provider    | Pomelo.EntityFrameworkCore.MySql |
| Password Hashing  | BCrypt.Net-Next                  |
| Authentication    | Cookie Authentication            |
| API Documentation | Swagger / Swashbuckle            |
| Source Control    | Git / GitHub                     |
| CI/CD             | GitHub Actions                   |
| Web Server        | Nginx                            |
| Process Manager   | systemd                          |

## System Architecture

```text
Browser
   |
   v
Razor Pages Frontend
   |
   v
ASP.NET Core API
   |
   v
MySQL Database
```

## Project Structure

```text
Arniston_Letting/
│
├── Arniston_Letting.slnx
│
├── Arniston_Letting_Api/
│   ├── Controllers/
│   ├── Data/
│   ├── DTOs/
│   ├── Models/
│   ├── Services/
│   └── Program.cs
│
├── Arniston_Letting_Front/
│   ├── Pages/
│   ├── Services/
│   ├── wwwroot/
│   └── Program.cs
│
├── .github/
│   └── workflows/
│       └── build.yml
│
└── README.md
```

## Local Setup

### Requirements

* .NET 10 SDK
* MySQL Server
* Git
* Visual Studio or another compatible .NET IDE

### Clone

```powershell
git clone https://github.com/ST10391450-1/Arniston_Letting.git
cd Arniston_Letting
```

### Restore and Build

```powershell
dotnet restore Arniston_Letting.slnx
dotnet build Arniston_Letting.slnx
```

### Database

Create the database:

```sql
CREATE DATABASE arniston_letting;
```

Set the connection string:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=localhost;Port=3306;Database=arniston_letting;User=YOUR_DB_USER;Password=YOUR_DB_PASSWORD;"
```

Do not commit database credentials to GitHub.

### Run the API

```powershell
dotnet dev-certs https --trust
dotnet run --project Arniston_Letting_Api/Arniston_Letting_Api.csproj --launch-profile https
```

API:

```text
https://localhost:7169
```

Swagger:

```text
https://localhost:7169/swagger
```

### Run the Frontend

Open another terminal:

```powershell
$env:ApiSettings__BaseUrl = "https://localhost:7169/"
dotnet run --project Arniston_Letting_Front/Arniston_Letting_Front.csproj --launch-profile https
```

Frontend:

```text
https://localhost:7106
```

## Development Account

When the Users table is empty, a demonstration administrator account is created:

| Field    | Value                         |
| -------- | ----------------------------- |
| Email    | `admin@arnistonletting.co.za` |
| Password | `Admin123!`                   |

This account is for development and demonstration only and must not be used in production.

## API

The API provides endpoints for:

| Resource       | Route                |
| -------------- | -------------------- |
| Authentication | `/api/Auth`          |
| Properties     | `/api/Properties`    |
| Owners         | `/api/Owners`        |
| Bookings       | `/api/Bookings`      |
| Cleaners       | `/api/Cleaners`      |
| Tasks          | `/api/Tasks`         |
| Breakages      | `/api/Breakages`     |
| Notifications  | `/api/Notifications` |
| Reports        | `/api/Reports`       |
| Dashboard      | `/api/Dashboard`     |

Swagger provides the full API documentation when running in Development.

## Deployment

Production uses:

```text
Internet
   |
   v
Nginx
   |
   +---- Frontend
   |
   +---- API
          |
          v
        MySQL
```

### Production Environment

| Item            | Value                           |
| --------------- | ------------------------------- |
| Server          | Ubuntu Linux                    |
| Database        | MySQL                           |
| Web Server      | Nginx                           |
| Process Manager | systemd                         |
| API             | `https://arniston.duckdns.org/` |
| HTTPS           | Enabled                         |

### Publish

```powershell
dotnet publish Arniston_Letting_Api/Arniston_Letting_Api.csproj -c Release -o publish/api
dotnet publish Arniston_Letting_Front/Arniston_Letting_Front.csproj -c Release -o publish/frontend
```

Production configuration uses:

```text
ConnectionStrings__DefaultConnection
ApiSettings__BaseUrl
ASPNETCORE_ENVIRONMENT=Production
```

Production credentials should be stored outside the repository.

## GitHub Actions

The project uses:

```text
.github/workflows/build.yml
```

The workflow runs on pushes and pull requests to `master`.

It:

1. Restores the solution
2. Builds the applications
3. Runs tests
4. Publishes the API
5. Publishes the frontend
6. Creates build artifacts

Artifacts:

```text
Arniston-Letting-API
Arniston-Letting-Frontend
```

Production deployment is handled separately.

## Task 2 Submission

| Item                 | Status                                             |
| -------------------- | -------------------------------------------------- |
| GitHub Repository    | `https://github.com/ST10391450-1/Arniston_Letting` |
| Website              | `https://arniston.duckdns.org/`                    |
| Database             | MySQL                                              |
| CI/CD                | GitHub Actions                                     |
| Presentation / Video | To be added                                        |
| Deployment Evidence  | To be added                                        |
| Hosting Rationale    | To be added                                        |

## Team Contributions

| Member        | Student Number 
| ------------- | -------------- 
| Dwayne Prins  | ST10032544     
| Thomas Dennis | ST10391450     

## Repository

**GitHub:** https://github.com/ST10391450-1/Arniston_Letting
