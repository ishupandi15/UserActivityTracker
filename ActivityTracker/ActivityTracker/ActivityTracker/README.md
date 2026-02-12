# User Activity Tracking System

## About This Project

This project was developed as part of the technical assessment.  
The objective was to design and implement a user activity tracking system that logs user interactions and generates analytical reports based on user behavior.

The solution is built using C#, Entity Framework Core, and SQL Server.  
I focused on making the system functional, structured, and scalable.

---

## Features Implemented

The console application provides the following operations:

1. Log a user activity (creates user if not exists)
2. View most active users (Top 10)
3. Calculate average activities per session
4. Calculate Daily Active Users (DAU)
5. Calculate Weekly Active Users (WAU)
6. Calculate session duration
7. View recent session durations
8. Seed bulk test data for realistic testing

---

## Database Design

The system uses two main tables:

### Users
- ID
- Username
- Email

### UserActivities
- ID
- UserID (Foreign Key)
- ActivityType
- ActivityTimestamp
- SessionID
- Metadata

---

## Implementation Approach

- Timestamps are stored as `datetimeoffset` to handle time filtering correctly.
- Proper indexing is applied on:
  - `UserID`
  - `SessionID`
  - `ActivityTimestamp`
  - Composite index (`UserID + ActivityTimestamp`)
- Logging uses background batching to improve performance.
- `SqlBulkCopy` is used for seeding large volumes of test data.
- All reports are implemented using LINQ with grouping and aggregation.

---

## Reporting Logic Summary

- **Most Active Users** → Group by `UserID` and count activities.
- **Average per Session** → Group by `SessionID` and calculate average.
- **DAU** → Count distinct users within a selected date range.
- **WAU** → Dynamically calculate week boundaries and count distinct users.
- **Session Duration** → Difference between first and last activity timestamp per session.

---

## How to Run

1. Ensure SQL Server is running.
2. Update the connection string in `Program.cs` if required.
3. Run the application.
4. Use the console menu to test each feature.
5. Use Option 8 to seed bulk data before testing reports.

---

## SQL Verification

All reporting features have been verified using direct SQL queries.  
These queries are included in the file:

`SQL_Verification_Scripts.sql`

---

## Author

Ishwariya Pandi