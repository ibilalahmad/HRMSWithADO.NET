using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AhmadHRManagementSystem.Models
{
    public class Users
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "First name cannot exceed 20 characters.")]
        public string FirstName { get; set; }

        [StringLength(20, ErrorMessage = "Last name cannot exceed 20 characters.")]
        public string? LastName { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "Username cannot exceed 20 characters.")]
        public string Username { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Password cannot exceed 50 characters.")]
        public string Password { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Email cannot exceed 50 characters.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; }

        [Required]
        public int RoleId { get; set; }

        [NotMapped]
        public string RoleName { get; set; } = string.Empty; // Add this for role name
    }

}
