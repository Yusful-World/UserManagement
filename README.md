# UserManagement

## Project Description

UserManagement is a demo project built with **.NET** and **PostgreSQL**. It performs **CRUD operations** on users with a simple structure that demonstrates clean architecture concepts. Minimal validations are included to make testing easier.

---

## Features

* **Get User by Id** (requires a GUID)
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
DbCredentials__Server=localhost
DbCredentials__Database=
DbCredentials__Port=5432
DbCredentials__Username=
DbCredentials__Password=

DefaultConnection="Host=localhost;Port=5432;Database=;Username=;Password=;"

# === CLOUDINARY SETTINGS ===
CLOUDINARY_CLOUD_NAME=
CLOUDINARY_API_KEY=
CLOUDINARY_API_SECRET=

# === JWT / AUTH SETTINGS ===
JWT__SECRETKEY=
JWT__ISSUER=GazeOnTradeAuth
JWT__AUDIENCE=
JWT__EXPIREINMINUTES=60
```

---

## Cloning and Running the Project

1. **Clone the repository**

```bash
git clone https://github.com/Yusful-World/UserManagement.git
cd usermanagement```

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
## Testing Endpoints

You can test endpoints using **Swagger** (if enabled) or tools like **Postman**

Example request:

```bash
GET https://localhost:5001/api/users/{id}
```

---

