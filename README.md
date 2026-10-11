# ASP.NET Core Identity Web API Authentication System
A robust and secure authentication system built using ASP.NET Core Identity for web APIs. This system provides user registration, login, password management, and role-based access control.

# Features
* User Registration (`Sign Up`): Secure account creation with custom fields (`FirstName`, `LastName`) and automated password hashing.
* Authentication (`Login`): Credential validation with lockout protection against brute-force attacks.
* Two-Factor Authentication (2FA): Automated email-based token generation and verification for secure login workflows.
* Password Management (`Forgot / Reset Password`): Secure token generation and validation to reset user credentials.
* Dynamic Role Management: Role seeding (`Admin`, `User`) on startup and dynamic role assignment to users.
* API Documentation: Fully integrated Swagger UI for interactive endpoint testing.

#  Tech Stack
* Framework: .NET 10 Web API
* Security & Auth: ASP.NET Core Identity
* ORM: Entity Framework Core (SQL Server / LocalDB)
* Email Service: Mailtrap / SMTP Integration
* Documentation: Swashbuckle Swagger

# Getting Started & Installation

# Prerequisites
* [.NET SDK](https://dotnet.microsoft.com/) installed on your machine.
* SQL Server LocalDB (comes packaged with Visual Studio).

# 1. Clone the Repository
```bash
git clone [https://github.com/YOUR-USERNAME/IdentityAuthSystem.git](https://github.com/YOUR-USERNAME/IdentityAuthSystem.git)
cd IdentityAuthSystem