# 📊 Project Management System

A comprehensive **enterprise-grade Project Management System** built with **ASP.NET Core MVC, C#, ADO.NET, and Microsoft SQL Server**.

The application provides a centralized platform for managing **employees, projects, teams, tasks, leaves, invoices, comments, budgets, and project profitability** with a database-driven architecture and reusable repository layer.

---

## 🚀 Overview

The **Project Management System** is designed to simulate real-world enterprise project operations.

It enables organizations to:

- Manage employees and employee types
- Create and monitor projects
- Build teams and assign members
- Assign and track tasks
- Manage employee leave requests
- Add project/task comments
- Generate and manage invoices
- Track project budgets and profitability
- Monitor operational statistics through a dashboard
- Enforce business rules directly at the database level

The project follows a structured **MVC + Repository Pattern** architecture with **Dependency Injection** and direct SQL Server access through **ADO.NET**.

---

## ✨ Key Features

### 📊 Dashboard
- Centralized project overview
- Employee statistics
- Team and task counts
- Financial metrics
- Quick navigation to major modules

### 👨‍💼 Employee Management
- Create, view, update, and delete employees
- Employee role/position management
- Support for full-time and hourly employees
- Salary and hourly-rate management
- Unique username and email validation

### 📁 Project Management
- Create and manage projects
- Project descriptions
- Budget management
- Deadline tracking
- Project status management
- Project cost monitoring

### 👥 Team Management
- Create project teams
- Assign employees to teams
- Manage team composition
- Track team-member assignments

### ✅ Task Management
- Assign tasks to teams
- Associate tasks with projects
- Track task status
- Monitor task assignments

### 💬 Comments & Collaboration
- Add comments and notes
- Associate comments with tasks/projects
- Automatic modification timestamps
- Supports team collaboration

### 🏖️ Leave Management
- Submit employee leave requests
- Track leave type and dates
- Approval status management
- Monitor leave-related records

### 💰 Invoice Management
- Create project invoices
- Calculate invoice amounts
- Track billing information
- Support hourly employee billing workflows

### 📈 Profit Tracking
- Monitor project financial performance
- Compare project budget and actual expenses
- Track profitability
- Database-level budget protection

---

## 🏗️ Architecture

The application uses a clean, modular architecture:

```text
Browser
   │
   ▼
ASP.NET Core MVC
   │
   ├── Controllers
   │      │
   │      ▼
   ├── Repository Interfaces
   │      │
   │      ▼
   ├── Repository Implementations
   │      │
   │      ▼
   ├── Database Helper / ADO.NET
   │      │
   ▼      ▼
Microsoft SQL Server
```

### Design Patterns & Practices

- **MVC (Model-View-Controller)**
- **Repository Pattern**
- **Dependency Injection**
- **Separation of Concerns**
- **ADO.NET Data Access**
- **Stored Procedures**
- **Database Transactions**
- **Database Triggers**
- **DataAnnotations Validation**
- **Centralized Error Handling**

---

## 🛠️ Technology Stack

### Backend

| Technology | Purpose |
|---|---|
| **C# 13** | Application programming language |
| **.NET 10** | Application framework |
| **ASP.NET Core MVC** | Web application framework |
| **ADO.NET** | Database access |
| **Microsoft.Data.SqlClient 5.2.2** | SQL Server connectivity |

### Frontend

| Technology | Purpose |
|---|---|
| **Razor Views** | Server-side UI rendering |
| **HTML5** | Page structure |
| **CSS3** | Styling |
| **Bootstrap 5** | Responsive UI |
| **Bootstrap Icons** | UI icons |

### Database

| Technology | Purpose |
|---|---|
| **Microsoft SQL Server** | Primary relational database |
| **Stored Procedures** | Business/database operations |
| **Triggers** | Automated database rules |
| **Transactions** | Data consistency |
| **Foreign Keys** | Referential integrity |
| **Unique Constraints** | Duplicate prevention |

---

## 🗄️ Database Design

The system uses a relational SQL Server database named:

```text
ProjectDB
```

### Main Tables

```text
Employee
├── FullTimeEmployee
└── HourlyEmployee

Project
Team
TeamMember
TaskAssignment
Comment
Leave
Invoice
Profit
DailyPerformance
```

### Important Database Capabilities

#### Employee Hierarchy
The database supports different employee types:

- Full-time employees
- Hourly employees

#### Transaction Safety
Critical insert/update operations are protected using SQL transactions.

#### Data Integrity
The database uses:

- Primary keys
- Foreign keys
- Unique constraints
- Validation rules

#### Automatic Timestamps
Triggers automatically maintain modification dates.

#### Budget Protection
A database trigger prevents project expenses from exceeding configured project budgets.

---

## ⚙️ Stored Procedures

Business operations are encapsulated in stored procedures.

Examples:

```text
sp_Hiring_AddFullTime
sp_Hiring_AddHourly
sp_Accounting_GenerateInvoice
```

Stored procedures help centralize important database operations and maintain transaction safety.

---

## 🔄 Database Triggers

The project includes automated triggers such as:

```text
trg_Employee_AutoUpdateDate
trg_Comment_AutoUpdateDate
trg_CheckBudgetOverflow
trg_AssignmentID_Constraint
```

### Example Responsibilities

- Automatically update modification timestamps
- Validate assignment rules
- Prevent project budget overflow
- Maintain database business rules

---

## 📂 Project Structure

```text
Project-Management-System/
│
├── Database/
│   ├── Tables.sql
│   ├── StoredProcedures.sql
│   └── Triggers.sql
│
├── DatabaseProject/
│   │
│   ├── Controllers/
│   │   ├── HomeController.cs
│   │   ├── EmployeesController.cs
│   │   ├── ProjectsController.cs
│   │   ├── TeamsController.cs
│   │   ├── TasksController.cs
│   │   ├── CommentsController.cs
│   │   ├── LeavesController.cs
│   │   ├── InvoicesController.cs
│   │   └── ProfitsController.cs
│   │
│   ├── Models/
│   │   ├── Employee.cs
│   │   ├── Project.cs
│   │   ├── Team.cs
│   │   ├── TeamMember.cs
│   │   ├── TaskAssignment.cs
│   │   ├── Comment.cs
│   │   ├── Leave.cs
│   │   ├── Invoice.cs
│   │   └── Profit.cs
│   │
│   ├── Repositories/
│   │   ├── Interfaces/
│   │   │   ├── IEmployeeRepository.cs
│   │   │   ├── IProjectRepository.cs
│   │   │   ├── ITeamRepository.cs
│   │   │   ├── ITeamMemberRepository.cs
│   │   │   ├── ITaskAssignmentRepository.cs
│   │   │   ├── ICommentRepository.cs
│   │   │   ├── ILeaveRepository.cs
│   │   │   ├── IInvoiceRepository.cs
│   │   │   └── IProfitRepository.cs
│   │   │
│   │   └── Implementations/
│   │       ├── EmployeeRepository.cs
│   │       ├── ProjectRepository.cs
│   │       ├── TeamRepository.cs
│   │       ├── TeamMemberRepository.cs
│   │       ├── TaskAssignmentRepository.cs
│   │       ├── CommentRepository.cs
│   │       ├── LeaveRepository.cs
│   │       ├── InvoiceRepository.cs
│   │       └── ProfitRepository.cs
│   │
│   ├── Data/
│   │   └── DatabaseHelper.cs
│   │
│   ├── Views/
│   │   ├── Home/
│   │   ├── Employees/
│   │   ├── Projects/
│   │   ├── Teams/
│   │   ├── Tasks/
│   │   ├── Comments/
│   │   ├── Leaves/
│   │   ├── Invoices/
│   │   ├── Profits/
│   │   └── Shared/
│   │
│   ├── wwwroot/
│   │   ├── css/
│   │   ├── js/
│   │   └── images/
│   │
│   ├── appsettings.json
│   └── Program.cs
│
└── README.md
```

---

## 📋 Prerequisites

Before running the application, install:

- **.NET 10 SDK**
- **Microsoft SQL Server 2019+**
- **SQL Server Management Studio (SSMS)**
- **Visual Studio 2026** or **Visual Studio Code**
- **Git**

Verify .NET installation:

```bash
dotnet --version
```

---

## 🚀 Installation

### 1. Clone the Repository

```bash
git clone https://github.com/Nuraddin0/Project-Management-System.git
```

Navigate into the project:

```bash
cd Project-Management-System
```

### 2. Restore Dependencies

```bash
cd DatabaseProject
dotnet restore
```

### 3. Build the Application

```bash
dotnet build
```

---

## 🗄️ Database Setup

### Step 1 — Create Database

Open **SQL Server Management Studio (SSMS)** and connect to your SQL Server instance.

Run:

```sql
CREATE DATABASE ProjectDB;
GO

USE ProjectDB;
GO
```

### Step 2 — Execute SQL Scripts

Inside the repository's `Database` folder, execute the scripts in this order:

```text
1. Tables.sql
2. StoredProcedures.sql
3. Triggers.sql
```

### Recommended Order

```text
Tables
  ↓
Stored Procedures
  ↓
Triggers
  ↓
Application
```

This order ensures that tables exist before stored procedures and triggers reference them.

---

## ⚙️ Configuration

Open:

```text
DatabaseProject/appsettings.json
```

Update the SQL Server connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=YOUR_SERVER_NAME;Initial Catalog=ProjectDB;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False;Command Timeout=30"
  }
}
```

Replace:

```text
YOUR_SERVER_NAME
```

with your SQL Server instance.

### Common Examples

For a local SQL Server:

```text
localhost
```

For LocalDB:

```text
(localdb)\MSSQLLocalDB
```

For a named SQL Server instance:

```text
YOUR-PC-NAME\SQLEXPRESS
```

> **Security Note:** Do not commit production passwords, API keys, or other secrets to GitHub.

---

## ▶️ Run the Application

From the `DatabaseProject` directory:

```bash
dotnet run
```

The application will normally be available at:

```text
https://localhost:5001
```

or:

```text
http://localhost:5000
```

> The exact port may vary depending on your ASP.NET Core launch configuration.

---

## 🧭 Application Modules

### Dashboard

Provides a high-level overview of:

- Total employees
- Total projects
- Teams
- Tasks
- Financial information
- Project performance

### Employees

```text
Employees → Create New
```

Manage:

- Employee information
- Employee type
- Position
- Salary
- Hourly rate
- Contact information

### Projects

Create projects with:

```text
Project Name
Description
Budget
Deadline
Status
```

### Teams

Create teams and assign employees as members.

### Tasks

Assign tasks to teams and associate them with projects.

### Comments

Add project/task comments for collaboration and notes.

### Leaves

Manage employee leave requests and approval status.

### Invoices

Create and track project invoices.

### Profits

Monitor project financial performance and profitability.

---

## 🧪 Validation & Error Handling

The application includes:

- Model-level DataAnnotations validation
- SQL constraints
- Foreign key validation
- Unique field validation
- Transaction-based database operations
- Exception handling
- Database-level business rules

These mechanisms help maintain data consistency and improve application reliability.

---

## 🎯 Learning Outcomes

This project demonstrates practical understanding of:

- ASP.NET Core MVC development
- C# application development
- SQL Server database design
- ADO.NET
- Repository Pattern
- Dependency Injection
- CRUD operations
- Stored Procedures
- SQL Triggers
- Database Transactions
- Relational database relationships
- MVC separation of concerns
- Bootstrap responsive UI
- Enterprise-style project structure

---

## 🔐 Security Considerations

For production deployment, consider adding:

- ASP.NET Core Identity
- Role-based authorization
- Authentication
- Anti-forgery protection
- Secure secret management
- Input sanitization
- HTTPS enforcement
- Audit logging
- Production-grade exception logging

---

## 🔮 Future Enhancements

Potential improvements include:

- 🔐 Authentication & role-based authorization
- 📧 Email notifications
- 📅 Calendar-based project scheduling
- 📊 Advanced analytics and charts
- 🔔 Real-time notifications
- 📎 Document/file attachments
- 🔎 Advanced search and filtering
- 📱 Improved mobile responsiveness
- 📤 Excel/PDF report generation
- ☁️ Cloud deployment
- 🧪 Automated unit/integration testing
- 🐳 Docker support
- ⚡ Web API layer for frontend/mobile integration

---

## 📸 Screenshots

Add application screenshots here to showcase the UI.

### Dashboard

```text
Add dashboard screenshot
```

### Project Management

```text
Add project management screenshot
```

### Team Management

```text
Add team management screenshot
```

### Task Assignment

```text
Add task assignment screenshot
```

### Invoice Management

```text
Add invoice screenshot
```

### Profit Tracking

```text
Add financial/profit screenshot
```

> Tip: Create a `Screenshots/` folder in the repository and reference images using relative paths.

Example:

```markdown
![Dashboard](Screenshots/dashboard.png)
```

---

## 📝 Resume Description

You can describe this project on your resume as:

**Project Management System | ASP.NET Core MVC, C#, ADO.NET, SQL Server**

> Developed an enterprise-style Project Management System using ASP.NET Core MVC, C#, ADO.NET, and SQL Server to manage employees, projects, teams, tasks, leaves, invoices, and project profitability. Implemented Repository Pattern, Dependency Injection, stored procedures, database transactions, triggers, validation, and responsive Bootstrap-based UI.

### Resume Highlights

- Built a modular **ASP.NET Core MVC** application for enterprise project and workforce management.
- Implemented **Repository Pattern and Dependency Injection** for maintainable and testable data access.
- Developed SQL Server database operations using **ADO.NET, stored procedures, transactions, and triggers**.
- Implemented project **budget and profitability tracking** with database-level business rules.
- Created CRUD workflows for employees, projects, teams, tasks, leaves, invoices, comments, and financial records.

---

## 👨‍💻 Author

**Ansari Aman**

MCA — 2026  
Ahmedabad, Gujarat, India

---

## 📄 License

This project is intended for **educational and portfolio purposes**.

If you use or modify this project, please provide appropriate attribution to the original author.

---

## ⭐ Support

If you find this project useful:

- ⭐ Star the repository
- 🍴 Fork the repository
- 🐛 Report issues
- 💡 Suggest improvements
---
### Built With ❤️ Using ASP.NET Core, C#, ADO.NET & Microsoft SQL Server
