using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AhmadHRManagementSystem.Models
{
    public class EmployeeDesignation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string DesignationName { get; set; }

        // Foreign Key Property
        public int EmployeeDepartmentID { get; set; }

        // Navigation Property
        [NotMapped]
        [ForeignKey("EmployeeDepartmentID")]
        public EmployeeDepartment DepartmentName { get; set; }
    }

}
