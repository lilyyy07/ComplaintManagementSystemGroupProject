The Complaint Management System (CMS) is a web-based application developed using ASP.NET MVC and SQL Server Management Studio (SSMS).
It streamlines the process of submitting, tracking, and resolving user complaints in an organized and efficient manner.
The backend handles database operations, role-based access, and complaint management workflows between Users, Staff, and Administrators.

#Project Structure

ComplaintManagementSystem.zip	- The complete backend project folder containing controllers, models, and views.
ComplaintDB.sql -	SQL script used to create the database, tables, and sample data. Can be executed in SSMS.
/Controllers/ComplaintsApiController.cs	- RESTful API endpoints for CRUD operations (GET, POST, PUT, DELETE). Tested using Postman.
/Controllers/ComplaintsController.cs - MVC controller that manages complaints logic and connects to SQL Server.
/Views/Complaints/	- Contains the system’s pages for complaint submission, editing, and dashboard views.
/Views/Home/	- Public-facing pages such as Home, About, and FAQ.
appsettings.json -	Stores database connection strings and environment configuration.
