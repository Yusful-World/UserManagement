# UserManagement

## Project Description

UserManagement is a demo project built with **.NET** and **PostgreSQL**. It performs **CRUD operations** on users with a simple structure that demonstrates clean architecture concepts. Minimal validations are included to make testing easier.

---

## Features

* **Get User by Id** (requires a GUID) ()
* **Get All Users**
* **Create User**
* **Update User**
* **Minimal Validations** for easier testing

---

## Project Structure

```
UserManagement
│
├── ApplicationFeatures
│   └── Users
│       ├── Commands
│       ├── DTOs
│       ├── Handlers
│       ├── Mappers
│
├── Extensions
├── Data
├── Domain
├── Infrastructure
│   ├── Services
│   ├── Utils
│   ├── Repository
│   └── SharedDTOs
```

---

## Technologies Used

* **.NET 8**
* **Entity Framework Core**
* **PostgreSQL**
* **CQRS Pattern**

---

## Environment Setup

Create an `.env` file in the root folder based on `.env.example`.

### Example `.env.example`

```env
DB_HOST=localhost
DB_PORT=5432
DB_NAME=usermanagement
DB_USER=postgres
DB_PASSWORD=yourpassword
```

---

## Cloning and Running the Project

1. **Clone the repository**

```bash
git clone https://github.com/Yusful-World/UserManagement.git
cd usermanagement
```

2. **Set up environment variables**

   * Copy `.env.example` → `.env`
   * Update values according to your PostgreSQL setup.

3. **Run database migrations**

```bash
dotnet ef database update
```

4. **Build and run the project**

```bash
dotnet build
dotnet run
```

---

## Seeding Demo Users

You can seed demo users for quick testing.

### Example Seeder (inside `DataSeeder.cs`):

```csharp
public static async Task SeedUsers(UserDbContext context)
{
    if (!context.Users.Any())
    {
        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), Email = "demo1@test.com", FirstName = "John" LastName = "Doe" Role = "User" },
            new User { Id = Guid.NewGuid(), Email = "demo2@test.com", FirstName = "John" LastName = "Bull" Role = "Admin" }
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();
    }
}
```

Call this seeder in your `Program.cs` after building the app:

```csharp
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    await DataSeeder.SeedUsers(dbContext);
}
```

---

## Testing Endpoints

You can test endpoints using **Swagger** (if enabled) or tools like **Postman** / **cURL**.

Example request:

```bash
GET https://localhost:5001/api/users/{id}
```

---

