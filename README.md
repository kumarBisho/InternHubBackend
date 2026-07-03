# InternHub Project

# Project Description
-> It is a web application, designed to manage the complete lifecycle of interns within an organization.
-> It provides secure authentication, role-based access control, and structured modules for managing users, projects, tasks, profiles and real-time notifications.
-> It provides secure authentication, role-based access control, and structured modules for managing users, projects, tasks, profiles, and real-time notifications.


# Tech Stack (Backend)

ASP.NET Core Web API

Entity Framework Core + Npgsql.EntityFrameworkCore.PostgreSQL

JWT Authentication (with role-based authorization)

AutoMapper (for mapping entities ↔ DTOs)

FluentValidation (for validating DTOs)

SignalR (for real-time notifications)

# Create Backend Project

dotnet new solution -n InternMS

dotnet new webapi -n InternMS.Api --> Creates a new ASP.NET Core Web API project named InternMS.Api.
dotnet new classlib -n InternMS.Domain --> Creates a Class Library Project named InternMS.Domain.
dotnet new classlib -n InternMS.Infrastructure --> Creates a Class Library Project named InternMS.Infrastructure.

# Add the Project into the solution

dotnet sln InternMS.slnx add InternMS.Api/InternMS.Api.csproj
dotnet sln InternMS.slnx add InternMS.Domain/InternMS.Domain.csproj
dotnet sln InternMS.slnx add InternMS.Infrastructure/InternMS.Infrastructure.csproj

# Add Project References

dotnet add InternMS.Api/InternMS.Api.csproj reference InternMS.Domain/InternMS.Domain.csproj
dotnet add InternMS.Api/InternMS.Api.csproj reference InternMS.Infrastructure/InternMS.Infrastructure.csproj

# Install Required NuGet Packages

dotnet add InternMS.Api package Microsoft.EntityFrameworkCore
dotnet add InternMS.Api package Microsoft.EntityFrameworkCore.Design
dotnet add InternMS.Api package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add InternMS.Api package Swashbuckle.AspNetCore
dotnet add InternMS.Api package Microsoft.AspNetCore.Authentication.JwtBearer

# add EFCore to the Infrastructure project

dotnet add InternMS.Infrastructure package Microsoft.EntityFrameworkCore
dotnet add InternMS.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL

# some additional dependency installation

dotnet add InternMS.Api package BCrypt.Net-Next

# Initialize Backend

Create ASP.NET Core Web API project.
Add EF Core & Npgsql.
Configure AppDbContext and connection string to Postgres.
Add migration and update database.
Implement entities & DbContext.
Implement JWT auth & role-based authorization.
Implement controllers for auth, users, projects, notifications.
I am using "PostgreSQL" Database for this application, Because it's more powerful, more flexible, and more standards-compliant than MySQL. 


# Core data model & database
Create domain entities in InternMS.Domain
Add packages:
1. dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
2. dotnet add package Microsoft.EntityFrameworkCore.Design

Add AppDbContext in Infrastructure project and configure PostgreSQL connection string.
Create migration:

# Migrate DataBase using below command
if you are in Backend folder:
# Create migration
1. dotnet ef migrations add InitialCreate --project InternMS.Infrastructure --startup-project InternMS.Api
# Apply migration
2. dotnet ef database update --project InternMS.Infrastructure --startup-project InternMS.Api

if you are in Backend/InternMS.Infrastructure
# Create migration
1. dotnet ef migrations add MigrationName --project . --startup-project ../InternMS.Api
# Apply migration
2. dotnet ef database update --project . --startup-project ../InternMS.Api


# Run the backend
cd InternMS.Api
dotnet restore -> Downloads and installs NuGet packages (dependencies) that's project needs.
dotnet clean -> Deletes compiled files and temporary build folders.
dotnet build -> Compiles C# code into DLL/EXE files.
dotnet run -> Builds + Runs the application.

# DataBase Modeling 
Go inside Entities
->cd InternMS.Domain/Entities
Create all required business models file and initialize model property.
There are Enums folder for fix Property.

# Database Infrastructure
This is for Migrating our local data into online server using psql.

InternMS.Infrastructure/Data/AppDbContext.cs this is for Model creation on server.
InternMS.Infrastructure/Configrations Here we have define the property of each model, which is created online.
Then call Migration script to create Model. 
After running Migration script "Migration" folder will be automatically created.
Now you can see Your Database/Tables on online server.

# Code for Migration/Update of DataBase 
Go to Root folder /Backend , where .slnx file present.
Run Below Command
1. dotnet ef migrations add <MigrationName> -p InternMS.Infrastructure -s InternMS.Api
2. dotnet ef database update -p InternMS.Infrastructure -s InternMS.Api

-p => MigrationName
-s => Startup project (Program.cs)

# Business Logic
cd InternMS.Api
Controllers -> for creating routes
Services -> for implementing logic
DTOs -> for transfer data between different layers or components of an application
Hubs -> To handle Real time notification
Middleware -> For security handling
appsettings.json -> Configuration (DB, JWT, etc.)
Program.cs -> Entry point of the application.


InternMS.slnx -> It is a container that holds multiple projects, That will be combined to work together.
README.md -> Documentation of Backend part of This project.
Summary_Of_folder_Structure.txt -> Folder structure of Project.

<!-- password for smtp 
vjyqjktpsfyedhvk -->