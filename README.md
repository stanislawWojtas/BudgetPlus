# BudgetPlus - Expense Sharing Application

BudgetPlus is a web application that helps users manage and split expenses with friends. Built with ASP.NET Core MVC, it provides an easy way to track shared expenses and settle debts between friends.

## Features

- **User Management**
  - User registration and authentication
  - Admin panel for user management
  - Friend system with request/accept functionality

- **Expense Management**
  - Create and track expenses
  - Split expenses among multiple friends
  - Categorize expenses
  - View expense details and history

- **Payment Tracking**
  - Track who owes what to whom
  - Mark expenses as paid
  - View balance with each friend
  - Settle up feature for clearing multiple debts at once

## Technology Stack

- **Backend**
  - ASP.NET Core MVC 7.0
  - Entity Framework Core
  - SQL Server
  - C# 10

- **Frontend**
  - Bootstrap 5
  - HTML5/CSS3
  - JavaScript

## Getting Started

### Prerequisites

- .NET 7.0 SDK
- SQL Server
- Visual Studio Code or Visual Studio 2022

### Installation

1. Clone the repository
```powershell
git clone https://github.com/yourusername/BudgetPlus.git
```

2. Navigate to the project directory
```powershell
cd BudgetPlus
```

3. Restore dependencies
```powershell
dotnet restore
```

4. Update the database
```powershell
dotnet ef database update
```

5. Run the application
```powershell
dotnet run
```

## Project Structure

```
BudgetPlus/
├── Controllers/               # MVC Controllers
├── Models/                   # Data models
├── Views/                    # MVC Views
├── Data/                    # Database context and migrations
├── Filters/                # Custom action filters
├── wwwroot/               # Static files (CSS, JS)
└── Program.cs            # Application entry point
```

## Key Features Explained

### Admin User
- Created automatically on first run
- Credentials: username: "admin", password: "admin123"
- Can manage users and categories

### Expense Sharing
1. Create an expense
2. Select friends to split with
3. Amount is automatically divided
4. Each user sees their share in "To Pay" section

### Friend System
- Send friend requests
- Accept/reject requests
- View friend balances
- Settle up with friends

## Database Schema

### Main Tables
- Users
- Expenses
- Categories
- Shares
- Friends

## Security Features

- Password hashing using MD5
- Session-based authentication
- Admin role protection
- CSRF protection

## Acknowledgments

- Bootstrap for UI components
- Entity Framework Core for ORM
- ASP.NET Core team for the framework