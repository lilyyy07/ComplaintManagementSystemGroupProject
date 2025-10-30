using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data.SqlClient;
using ComplaintManagementSystem.Models;
using Microsoft.Extensions.Configuration;

namespace ComplaintManagementSystem.Controllers
{
    public class ComplaintsController : Controller
    {
        private readonly IConfiguration _configuration;

        // Change between "Admin", "Staff", or "User" to test roles
        private string currentUserRole = "Admin";

        public ComplaintsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ===================== INDEX (Dashboard + Search) =====================
        public IActionResult Index(string searchQuery)
        {
            List<Complaint> complaints = new List<Complaint>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            int pendingCount = 0;
            int inProgressCount = 0;
            int resolvedCount = 0;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd;

                    if (!string.IsNullOrEmpty(searchQuery))
                    {
                        cmd = new SqlCommand(
                            "SELECT * FROM Complaints WHERE Title LIKE @search OR Description LIKE @search ORDER BY DateCreated DESC",
                            conn);
                        cmd.Parameters.AddWithValue("@search", "%" + searchQuery + "%");
                    }
                    else
                    {
                        cmd = new SqlCommand("SELECT * FROM Complaints ORDER BY DateCreated DESC", conn);
                    }

                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        string status = reader["Status"].ToString();

                        complaints.Add(new Complaint
                        {
                            ComplaintID = (int)reader["ComplaintID"],
                            UserID = (int)reader["UserID"],
                            Title = reader["Title"].ToString(),
                            Description = reader["Description"].ToString(),
                            Status = status,
                            DateCreated = (System.DateTime)reader["DateCreated"]
                        });

                        // 🧮 Count each type of complaint
                        if (status == "Pending") pendingCount++;
                        else if (status == "In Progress") inProgressCount++;
                        else if (status == "Resolved") resolvedCount++;
                    }
                }
            }
            catch (SqlException ex)
            {
                ViewBag.Error = "Error loading complaints: " + ex.Message;
            }

            // 📊 Pass totals to the view
            ViewBag.PendingCount = pendingCount;
            ViewBag.InProgressCount = inProgressCount;
            ViewBag.ResolvedCount = resolvedCount;
            ViewBag.UserRole = currentUserRole;
            ViewBag.SearchQuery = searchQuery;

            return View(complaints);
        }

        // ===================== CREATE (Form Page) =====================
        public IActionResult Create()
        {
            return View();
        }

        // ===================== CREATE (Handle Submission) =====================
        [HttpPost]
        public IActionResult Create(Complaint complaint)
        {
            if (!ModelState.IsValid)
            {
                return View(complaint);
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(
                        "INSERT INTO Complaints (UserID, Title, Description, Status, DateCreated) VALUES (@UserID, @Title, @Description, 'Pending', GETDATE())",
                        conn);
                    cmd.Parameters.AddWithValue("@UserID", 3); // Example: Lily’s UserID
                    cmd.Parameters.AddWithValue("@Title", complaint.Title);
                    cmd.Parameters.AddWithValue("@Description", complaint.Description);
                    cmd.ExecuteNonQuery();
                }

                // ✅ Display success message on the same page
                ViewBag.SuccessMessage = "Your complaint has been successfully submitted!";
                ModelState.Clear(); // clears the form fields
                return View(); // stay on the same page
            }
            catch (SqlException ex)
            {
                ViewBag.Error = "Error saving complaint: " + ex.Message;
                return View(complaint);
            }
        }

        // ===================== EDIT (Form Page) =====================
        public IActionResult Edit(int id)
        {
            if (currentUserRole != "Admin" && currentUserRole != "Staff")
            {
                return Unauthorized("You do not have permission to edit complaints.");
            }

            Complaint complaint = new Complaint();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT * FROM Complaints WHERE ComplaintID=@id", conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        complaint.ComplaintID = (int)reader["ComplaintID"];
                        complaint.Title = reader["Title"].ToString();
                        complaint.Description = reader["Description"].ToString();
                        complaint.Status = reader["Status"].ToString();
                    }
                }
            }
            catch (SqlException ex)
            {
                ViewBag.Error = "Error loading complaint: " + ex.Message;
            }

            return View(complaint);
        }

        // ===================== EDIT (Handle Update) =====================
        [HttpPost]
        public IActionResult Edit(Complaint complaint)
        {
            if (currentUserRole != "Admin" && currentUserRole != "Staff")
            {
                return Unauthorized("You do not have permission to update complaints.");
            }

            if (!ModelState.IsValid)
            {
                return View(complaint);
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(
                        "UPDATE Complaints SET Title=@Title, Description=@Description, Status=@Status WHERE ComplaintID=@ComplaintID",
                        conn);
                    cmd.Parameters.AddWithValue("@Title", complaint.Title);
                    cmd.Parameters.AddWithValue("@Description", complaint.Description);
                    cmd.Parameters.AddWithValue("@Status", complaint.Status);
                    cmd.Parameters.AddWithValue("@ComplaintID", complaint.ComplaintID);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                ViewBag.Error = "Error updating complaint: " + ex.Message;
                return View(complaint);
            }

            return RedirectToAction("Index");
        }

        // ===================== DELETE =====================
        public IActionResult Delete(int id)
        {
            if (currentUserRole != "Admin" && currentUserRole != "Staff")
            {
                return Unauthorized("You do not have permission to delete complaints.");
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("DELETE FROM Complaints WHERE ComplaintID=@id", conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                ViewBag.Error = "Error deleting complaint: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // ===================== DETAILS (View Complaint) =====================
        public IActionResult Details(int id)
        {
            Complaint complaint = new Complaint();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT * FROM Complaints WHERE ComplaintID=@id", conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        complaint.ComplaintID = (int)reader["ComplaintID"];
                        complaint.Title = reader["Title"].ToString();
                        complaint.Description = reader["Description"].ToString();
                        complaint.Status = reader["Status"].ToString();
                        complaint.DateCreated = (System.DateTime)reader["DateCreated"];
                    }
                }
            }
            catch (SqlException ex)
            {
                ViewBag.Error = "Error loading complaint details: " + ex.Message;
            }

            return View(complaint);
        }
    }
}
