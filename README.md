# R3M.Financas.Back.Api

R3M.Financas.Back.Api is a RESTful API for managing financial data, including institutions, categories, transactions (movimentações), and periods. The project is built with ASP.NET Core, Dapper, and PostgreSQL, and follows modern development practices such as automated testing and versioning.

## Features

- **Institutions Management**: List and retrieve financial institutions, including their balances and credit status.
- **Categories Management**: List, search, and retrieve categories, including hierarchical relationships (parent/child categories).
- **Transactions (Movimentações)**: List and add financial transactions, associating them with institutions, categories, and periods. The API ensures data consistency by validating related entities before adding a transaction.
- **Periods**: List and retrieve periods (e.g., months) for a given year, supporting financial planning and reporting.
- **OpenAPI/Swagger**: Interactive API documentation and testing via Swagger UI.
- **CORS Support**: Configured to allow cross-origin requests for integration with front-end applications.

## Automated Testing

The project includes a comprehensive suite of unit tests for controllers, ensuring the reliability of core business logic. Tests are written using xUnit and Moq, and can be run with the standard .NET test tooling:

```powershell
dotnet test
```

## Automated Versioning

This project uses [GitVersion](https://gitversion.net/) for automatic semantic versioning. The CI pipeline (see `.github/workflows/build-versioning.yml`) determines the version based on commit messages and branch strategy, creates tags, and publishes GitHub releases automatically.

## Getting Started

1. **Clone the repository**
2. **Configure the database connection** in `appsettings.json` (default is PostgreSQL)
3. **Run the API**:
   ```powershell
   dotnet run --project R3M.Financas.Back.Api
   ```
4. **Access Swagger UI** at `https://localhost:7267/swagger` (or the URL shown in the console)

## Project Structure

- `R3M.Financas.Back.Api/` - Main API project
- `R3M.Financas.Back.Api.UnitTests/` - Unit tests for controllers
- `R3M.Financas.Back.IntegrationTests/` - (Reserved for future integration tests)

## Technologies Used

- ASP.NET Core 9
- Dapper
- PostgreSQL
- xUnit & Moq (testing)
- GitVersion (versioning)
- Swagger/OpenAPI

## License

This project is licensed under the MIT License.
