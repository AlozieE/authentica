````markdown
# Authentica

Authentica is a password manager built with ASP.NET Core MVC.  
The idea behind the project is simple: create personal vaults and store sensitive information like passwords, credit cards and secure notes in a safer way.

This project started as a school project, but I wanted to build something with a stronger focus on security instead of just another CRUD application.

## What it does

With Authentica, users can:

- Create and manage personal vaults
- Add different types of vault items:
  - Passwords
  - Credit cards
  - Secure notes
- Generate strong passwords
- Enable two-factor authentication
- Update account information
- Store sensitive vault item data encrypted instead of plaintext

## Security

Security is the main focus of this project.

Vault item data is not stored as readable text in the database.  
Instead, the dynamic fields of an item are collected, converted to JSON, encrypted, and then stored together with an IV.

For example, a password item can contain fields like:

```text
Website
Username
Password
````

A credit card item can contain different fields like:

```text
CardNumber
ExpiryDate
CVC
```

Because these item types do not all share the same fields, Authentica stores the sensitive content as encrypted JSON while keeping important metadata relational.

Metadata such as title, vault id, item type and timestamps stays separate, while the actual sensitive content is encrypted.

The application also uses ownership checks, so users can only access vaults and items that belong to their own account.

## Authentication

Authentica uses ASP.NET Identity for authentication-related features such as:

* Password hashing
* Lockout after failed login attempts
* Security stamps
* Two-factor authentication support

Entity Framework was not used in this project.
Instead, I built a custom UserStore so ASP.NET Identity can work with my own SQL-based data access layer.

This way, I could still use the security features of Identity while keeping full control over the database queries.

## Architecture

The project uses a layered structure:

```text
Controllers
→ Services
→ Repositories
→ Database
```

The goal of this structure is to keep responsibilities separated:

* Controllers handle requests, responses and view logic
* Services contain the main application logic
* Repositories handle SQL/database communication
* DTOs are used for input between layers
* ViewModels are used for data that is shown in views

## Tech Stack

* C#
* ASP.NET Core MVC
* ASP.NET Core Identity
* SQL Server
* ADO.NET / SqlCommand
* Razor Views
* MSTest
* Moq

## Testing

The project includes unit tests for important parts of the application, such as:

* Account security settings
* Password hashing behavior
* Vault service logic
* Vault controller ownership flow
* Vault item service behavior

Mocks are used in the tests so services and controllers can be tested without relying on a real database.

## Things I would improve

Some things I would like to improve in the future:

* Move some item type mapping logic out of the controller
* Add stricter whitelisting for dynamic fields per vault item type
* Add more integration tests
* Improve logging
* Store encryption keys through environment variables or a secrets manager
* Polish the UI further

## Author

Built by Alozie Ekentason.

```
```
