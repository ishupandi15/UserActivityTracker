# 📊 User Activity Tracking System

**Assessment:** Technical Evaluation – KAAR Infotech  
**Technology Stack:** C#, .NET, Entity Framework Core, SQL Server  
**Author:** Ishwariya Pandi  

---

## 🧭 Project Overview

This project implements a **Complex User Activity Tracking System** that logs user interactions on a website or application and generates analytical insights based on user behavior.

The system is designed to:

- Log user activities (page views, clicks, navigation, etc.)
- Analyze usage patterns
- Generate behavioral metrics such as:
  - Most Active Users
  - Average Activity per Session
  - Daily Active Users (DAU)
  - Weekly Active Users (WAU)
  - Session Duration

The solution focuses on scalability, performance optimization, and clean architecture.

---

## 🧩 Core Functionalities

### 🔹 Activity Logging
- Automatically creates users if they do not exist
- Logs activity with:
  - Activity Type
  - Timestamp
  - Session ID
  - Optional Metadata

### 🔹 Reporting & Analytics

The console interface provides:

1. Log User Activity  
2. View Most Active Users (Top 10)  
3. Calculate Average Activities per Session  
4. Calculate Daily Active Users (DAU)  
5. Calculate Weekly Active Users (WAU)  
6. Calculate Session Duration  
7. View Recent Session Durations  
8. Seed Bulk Test Data (High-Volume Simulation)  

---

## 🗄️ Database Design

### Users Table

| Column | Type | Description |
|--------|------|-------------|
| ID | bigint (PK) | Unique User Identifier |
| Username | nvarchar | User Name |
| Email | nvarchar | User Email |

---

### UserActivities Table

| Column | Type | Description |
|--------|------|-------------|
| ID | bigint (PK) | Activity Identifier |
| UserID | bigint (FK) | References Users(ID) |
| ActivityType | nvarchar | Type of activity |
| ActivityTimestamp | datetimeoffset | Timestamp of event |
| SessionID | nvarchar | Logical session grouping |
| Metadata | nvarchar | Additional activity data |

---

## ⚙️ Implementation Approach

### ✔ SQL Layer
- Normalized relational schema
- Foreign key constraint enforced
- Indexed columns:
  - `UserID`
  - `ActivityTimestamp`
  - `SessionID`
  - Composite index (`UserID`, `ActivityTimestamp`)

### ✔ C# & Entity Framework Core
- Code-first design approach
- LINQ-based aggregation queries
- Async database operations
- Structured service layer for separation of concerns

---

## 📈 Reporting Logic Summary

| Feature | LINQ Strategy Used |
|----------|-------------------|
| Most Active Users | `GroupBy + Count + OrderByDescending` |
| Avg Activity per Session | `GroupBy(SessionID) + Average` |
| DAU | `Where + Distinct + Count` |
| WAU | `Date range filtering + Distinct` |
| Session Duration | `Max(timestamp) - Min(timestamp)` |
| Users in Date Range | `Where + Distinct` |

---

## 🚀 High-Volume Optimization

To ensure scalability for high traffic systems:

- Implemented `SqlBulkCopy` for efficient batch inserts
- Used asynchronous database calls
- Applied index optimization for query speed
- Designed system ready for partitioning strategy (by ActivityTimestamp)
- Structured for horizontal scaling via read replicas (future expansion)

---

## 🧪 Bulk Data Simulation

The system includes a bulk seeding feature:

- Dynamically generates test users
- Generates multiple activities per user
- Uses `SqlBulkCopy` for performance
- Allows testing with thousands of records

---

## 🖥️ Console Interface

The application uses an interactive console menu for:

- Executing analytics
- Logging events
- Running performance tests
- Validating system functionality

---

## 📌 How to Run

1. Ensure SQL Server is running  
2. Update the connection string in `Program.cs` if required  
3. Build and run the project  
4. Use the console menu to test features  
5. Use Option 8 to seed bulk data before testing reports  

---

## 🎯 Key Concepts Demonstrated

- SQL schema design  
- EF Core configuration  
- LINQ aggregation and grouping  
- Foreign key enforcement  
- Bulk data handling  
- Performance optimization strategy  
- Console-based system architecture  

---

## 🏆 Summary

This project demonstrates the ability to:

- Design scalable backend systems  
- Implement structured data logging  
- Build analytical reporting pipelines  
- Optimize for high-volume traffic  
- Translate business requirements into working production-ready code  

---

## 👩‍💻 Author

**Ishwariya Pandi**  
Technical Assessment Submission – KAAR Infotech  


