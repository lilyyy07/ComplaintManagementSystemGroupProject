using System;
using System.ComponentModel.DataAnnotations;

namespace ComplaintManagementSystem.Models
{
    public class Complaint
    {
        public int ComplaintID { get; set; }

        public int UserID { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        public string? Status { get; set; }

        public DateTime DateCreated { get; set; }
    }
}
