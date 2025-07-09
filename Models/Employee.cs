using AhmadHRManagementSystem.DataAccess;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AhmadHRManagementSystem.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "First Name cannot exceed 20 characters.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "Last Name cannot exceed 20 characters.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [RegularExpression("^(Male|Female|Other)$", ErrorMessage = "Gender must be 'Male', 'Female', or 'Other'.")]
        public string Gender { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        [AgeValidation(ErrorMessage = "Employee must be at least 18 years old.")]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [RegularExpression(@"^\d{5}-\d{7}-\d{1}$", ErrorMessage = "CNIC must be in the format XXXXX-XXXXXXX-X.")]
        [StringLength(15, ErrorMessage = "CNIC cannot exceed 15 characters.")]
        [Display(Name = "CNIC Number")]
        public string CNIC { get; set; }

        [Required]
        [RegularExpression(@"^03[0-9]{2}-[0-9]{7}$", ErrorMessage = "Phone number must be in the format 03XX-XXXXXXX.")]
        [StringLength(12, ErrorMessage = "Phone number cannot exceed 12 characters.")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [StringLength(50, ErrorMessage = "Email cannot exceed 50 characters.")]
        public string? Email { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Hire Date")]
        [NotFutureDate(ErrorMessage = "Hire Date cannot be in the future.")]
        public DateTime HireDate { get; set; }

        [Required]
        [Range(0, 9999999.00, ErrorMessage = "Salary must be a non-negative value and cannot exceed 9,999,999.00.")]
        public decimal Salary { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [DataType(DataType.DateTime)]
        [Display(Name = "Updated Date")]
        public DateTime? UpdatedDate { get; set; }  // Nullable because it's updated later

        [StringLength(100, ErrorMessage = "Address cannot exceed 100 characters.")]
        public string? Address { get; set; }

        [Display(Name = "Designation Name")]
        public int EmployeeDesignationID { get; set; }

        [NotMapped]
        public string? DesignationName { get; set; }

        [NotMapped]
        public string? DepartmentName { get; set; }

        // ----------------------------------------------------------
        // Computed Properties

        // Full Name
        public string DisplayFullName => $"{FirstName} {LastName}";

        // Display age like (30 y 210 d)
        public string DisplayAge
        {
            get
            {
                DateTime today = DateTime.Today;

                // Calculate Age in Years
                int years = today.Year - DateOfBirth.Year;
                if (today < DateOfBirth.AddYears(years)) // Adjust if birthday hasn't happened yet this year
                {
                    years--;
                }

                // Calculate Days after Last Birthday
                DateTime lastBirthday = DateOfBirth.AddYears(years);
                int days = (today - lastBirthday).Days;

                return $"{years} y {days} d";
            }
        }

        // Show Only Dates
        public string DisplayDateOfBirth => DateOfBirth.ToString("dd-MM-yyyy");
        public string DisplayHireDate => HireDate.ToString("dd-MM-yyyy");

        // Show Date with Time (dd-MM-yyyy HH:mm)
        public string DisplayCreatedDate => CreatedDate.ToString("dd-MM-yyyy HH:mm");
        public string DisplayUpdatedDate => UpdatedDate?.ToString("dd-MM-yyyy HH:mm") ?? "-";

        // Convert Email to Lowercase and Handle Null
        public string DisplayEmail => string.IsNullOrEmpty(Email) ? "No Email" : Email.ToLower();

        // Handle Null
        public string DisplayAddress => string.IsNullOrEmpty(Address) ? "No Address" : Address;

    }

    //Validation class for Employee must be at least 18 years old
    public class AgeValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is DateTime dateOfBirth)
            {
                int age = DateTime.Today.Year - dateOfBirth.Year;
                if (dateOfBirth.Date > DateTime.Today.AddYears(-age)) age--;

                return age < 18
                    ? new ValidationResult(ErrorMessage ?? "Employee must be at least 18 years old.")
                    : ValidationResult.Success;
            }
            return ValidationResult.Success;
        }
    }

    public class NotFutureDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is DateTime dateValue)
            {
                return dateValue.Date <= DateTime.Today;
            }
            return true; // Not our concern if value is null (use "Required" for that)
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} cannot be in the future.";
        }
    }
}
