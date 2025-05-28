# VotingSystem

A modern web application for anonymous online voting, built with ASP.NET Core WebAPI, Blazor, and React.  
Developed as a university project for “Modern webalkalmazások fejlesztése .NET környezetben” (Modern Web Application Development in .NET).

---

## Features

- **Anonymous voting:** No UserId or identifying data stored with any vote, guaranteed by database structure and code.
- Admin dashboard (Blazor) for poll creation and result management.
- Visitor/client interface (React) for participating in polls.
- RESTful API with clean DTOs.
- Code-first database (EF Core), automatic migrations and seeding.
- JWT-based authentication.
- Extensive validation and error handling.
- Swagger UI for API exploration.

---

## Technologies

- **Backend:** ASP.NET Core WebAPI (.NET 8)
- **Frontend:** Blazor WebAssembly (admin), React (user client)
- **ORM:** Entity Framework Core (Code-first)
- **Authentication:** ASP.NET Identity, JWT
- **Testing:** xUnit, bUnit, Moq
- **Version control:** Git, GitHub

---
## Screenshots
React:
![image](https://github.com/user-attachments/assets/da3cff9a-16c6-4f84-acf9-2f91dc975db7)
![image](https://github.com/user-attachments/assets/8e0cecda-3843-4a68-8249-ac4645a56a27)
![image](https://github.com/user-attachments/assets/a40a9a75-8912-4684-9fbf-39707cd064da)

Blazor:
![image](https://github.com/user-attachments/assets/0a556a52-50f9-49bc-a553-debcc9453993)
![image](https://github.com/user-attachments/assets/ded2aff0-6cdd-4cda-a69e-babe6827a733)
![image](https://github.com/user-attachments/assets/c808bee5-fd27-42f4-a61f-6067ea95d1fb)

---
## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
- [Node.js and npm](https://nodejs.org/) (for React client)
- Visual Studio 2022+ or VS Code

### Running the Project

1. **Clone the repository:**
    ```sh
    git clone https://github.com/patakimatyas/VotingSystem.git
    cd VotingSystem
    ```

2. **Open `VotingSystem.sln` in Visual Studio**.

3. **Apply database migrations and seed data**  
   (Automatically runs on first start. See `DbInitializer.cs`.)

4. **Start the backend (WebAPI):**
    - Set `VotingSystem.WebAPI` as the startup project.
    - Run (`F5` or `dotnet run --project VotingSystem.WebAPI`)
    - The API will be at `https://localhost:xxxx` (see `launchSettings.json`).

5. **Access Swagger UI for API testing:**  
   `https://localhost:xxxx/swagger`

6. **Run the Blazor admin frontend:**
    - Set `VotingSystem.Blazor` as the startup project and run.

7. **Run the React client:**
    ```sh
    cd VotingSystem.React
    npm install
    npm run dev
    ```
    - The React client will be at `http://localhost:5173` (or your local port).

---

## Configuration

- Uses LocalDB for local development by default.
- JWT secret and test credentials are for development only.  
  **Never use these in production!**

---

## Project Status

This project is still being actively developed and improved.  
Watch this repository for future updates and new features!

