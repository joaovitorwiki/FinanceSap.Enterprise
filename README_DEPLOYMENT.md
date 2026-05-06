# FinanceSap.Enterprise 🏦

[![Build and Deploy](https://github.com/<your-username>/<your-repo>/actions/workflows/main_financesap.yml/badge.svg)](https://github.com/<your-username>/<your-repo>/actions/workflows/main_financesap.yml)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple)](https://dotnet.microsoft.com/)
[![Azure](https://img.shields.io/badge/Azure-Ready-blue)](https://azure.microsoft.com/)
[![Tests](https://img.shields.io/badge/Tests-Passing-success)](./FinanceSap.Tests)

Enterprise-grade financial management system built with Clean Architecture, DDD, and CQRS patterns.

## 🚀 Quick Start

### Local Development
```bash
# Clone repository
git clone https://github.com/<your-username>/<your-repo>.git
cd FinanceSap.Enterprise

# Restore dependencies
dotnet restore

# Run tests
dotnet test

# Run API
dotnet run --project FinanceSap.Api
```

### Azure Deployment
See [AZURE_DEPLOYMENT.md](./AZURE_DEPLOYMENT.md) for complete deployment guide.

**Quick Deploy**:
1. Create Azure resources
2. Run `database-schema.sql` on Azure MySQL
3. Configure environment variables
4. Push to `main` branch → Auto-deploy via GitHub Actions

## 📋 Features

### ✅ Implemented Modules

#### Customer Management
- Create and manage customers
- CPF validation (Brazilian tax ID)
- Customer aggregate root with domain events

#### Account Management
- Create customer accounts
- Balance tracking
- Account number generation

#### Loan Management 🆕
- Request loans with compound interest calculation
- Automatic payment calculation
- Loan status tracking (Pending, Approved, Rejected)
- Domain-driven validation rules

#### Authentication & Authorization
- JWT-based authentication
- ASP.NET Core Identity integration
- Role-based access control

## 🏗️ Architecture

```
FinanceSap.Enterprise/
├── FinanceSap.Domain/          # Domain entities, value objects, interfaces
├── FinanceSap.Application/     # Use cases, commands, handlers, validators
├── FinanceSap.Infrastructure/  # Data access, repositories, Identity
├── FinanceSap.Api/            # REST API, controllers, middlewares
└── FinanceSap.Tests/          # Unit and integration tests
```

**Patterns Used**:
- Clean Architecture
- Domain-Driven Design (DDD)
- CQRS (Command Query Responsibility Segregation)
- Repository Pattern
- Unit of Work
- Strategy Pattern (Loan Calculator)
- Factory Pattern (Entity creation)

## 🛠️ Technology Stack

- **.NET 9.0** - Latest .NET framework
- **ASP.NET Core** - Web API framework
- **Entity Framework Core 9.0** - ORM
- **MySQL** - Database (Azure MySQL Flexible Server)
- **FluentValidation** - Input validation
- **MediatR** - CQRS implementation
- **xUnit** - Unit testing
- **FluentAssertions** - Test assertions
- **NSubstitute** - Mocking framework
- **Scalar** - API documentation

## 📊 API Endpoints

### Customers
- `POST /api/customers` - Create customer
- `GET /api/customers/{id}` - Get customer by ID

### Accounts
- `POST /api/accounts` - Create account
- `GET /api/accounts/{id}/balance` - Get account balance

### Loans
- `POST /api/loans` - Request loan
- `GET /api/loans/{id}` - Get loan details

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login and get JWT token

**API Documentation**: Available at `/scalar/v1` when running

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test FinanceSap.Tests

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

**Test Coverage**:
- ✅ Unit Tests: Domain entities, value objects, calculators
- ✅ Integration Tests: API endpoints, database operations
- ✅ Handler Tests: Use case handlers with mocked dependencies

## 🔐 Security Features

- JWT authentication with short-lived tokens (15 min)
- Password requirements (8+ chars, uppercase, lowercase, digit, special char)
- Account lockout after 5 failed attempts
- HTTPS enforcement
- Security headers middleware
- Rate limiting
- CORS configuration
- SQL injection protection (parameterized queries)

## 🌐 Deployment

### Environment Variables

```bash
# Required
MYSQL_CONNECTION_STRING="Server=<server>;Database=financesap_db;User=<user>;Password=<password>;SslMode=Required;"
Jwt__Key="<YourSecureJwtKey-MinimumLength32Characters>"
Jwt__Issuer="FinanceSap"
Jwt__Audience="FinanceSap"
ASPNETCORE_ENVIRONMENT="Production"
```

### GitHub Actions CI/CD

Automatic deployment on push to `main`:
1. ✅ Build application
2. ✅ Run all tests
3. ✅ Publish artifacts
4. ✅ Deploy to Azure Web App

See [.github/workflows/main_financesap.yml](./.github/workflows/main_financesap.yml)

## 📚 Documentation

- [Azure Deployment Guide](./AZURE_DEPLOYMENT.md) - Complete deployment instructions
- [Quick Reference](./QUICK_REFERENCE.md) - Quick commands and URLs
- [Deployment Summary](./DEPLOYMENT_SUMMARY.md) - What's been prepared
- [Database Schema](./database-schema.sql) - SQL migration script

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License.

## 👥 Authors

- **Your Name** - Initial work

## 🙏 Acknowledgments

- Clean Architecture by Robert C. Martin
- Domain-Driven Design by Eric Evans
- Microsoft .NET Documentation
- Azure Documentation

---

**Status**: ✅ Production Ready | 🚀 Azure Deployment Ready | ✅ All Tests Passing

**Live API**: `https://financesap-api.azurewebsites.net` (after deployment)
