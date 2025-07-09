 using System.ComponentModel.DataAnnotations;

namespace AhmadHRManagementSystem.Models
{
    public class EmployeeDepartment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Department Name cannot exceed 100 characters.")]
        [Display(Name = "Department Name")]
        public string DepartmentName { get; set; }
    }
}
