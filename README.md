# **myshop - eCommerce Web Application**

**myshop** is a scalable **ASP.NET Core MVC** eCommerce application using **Entity Framework Core** with **N-Tier Architecture**, **Repository Pattern**, and **Unit of Work** for maintainability and efficiency.

## **Features**
- 🛒 **Product & Category Management**  
- 🔐 **Authentication & Authorization** (Role-based access)  
- 📦 **Cart & Order Processing**  
- 📊 **Admin Dashboard**  
- 🎨 **Responsive UI** (Bootstrap + Razor Views)  

## **Tech Stack**
- **Backend:** ASP.NET Core MVC, Entity Framework Core, SQL Server  
- **Frontend:** Razor Views, Bootstrap  
- **Architecture:** N-Tier, Repository & Unit of Work Pattern  
- **Database:** SQL Server with EF Core Migrations  

## **Getting Started**
### **1. Clone the Repository**
```sh
git clone https://github.com/yourusername/myshop.git
cd myshop
```

### **2. Set Up the Database**
- Update the **Connection String** in `appsettings.json`
- Run migrations:
```sh
dotnet ef database update
```

### **3. Run the Application**
```sh
dotnet run
```
Access the app at **http://localhost:5000** (or as configured).

## **License**
This project is open-source under the **MIT License**.
