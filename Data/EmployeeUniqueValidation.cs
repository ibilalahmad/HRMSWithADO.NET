using AhmadHRManagementSystem.Models;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;

namespace AhmadHRManagementSystem.DataAccess
{
    public class EmployeeUniqueValidation
    {
        private readonly string _connectionString;

        public EmployeeUniqueValidation(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("AHRMS_CS");
        }

        public async Task<List<ValidationResult>> ValidateEmployeeAsync(Employee employee)
        {
            List<ValidationResult> errors = new List<ValidationResult>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = @"
                    SELECT 
                        (SELECT COUNT(*) FROM Employee WHERE CNIC = @CNIC AND Id <> @Id) AS CNICCount,
                        (SELECT COUNT(*) FROM Employee WHERE PhoneNumber = @PhoneNumber AND Id <> @Id) AS PhoneCount,
                        (SELECT COUNT(*) FROM Employee WHERE Email = @Email AND Id <> @Id) AS EmailCount";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@CNIC", employee.CNIC);
                    cmd.Parameters.AddWithValue("@PhoneNumber", employee.PhoneNumber);
                    cmd.Parameters.AddWithValue("@Email", (object?)employee.Email ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Id", employee.Id);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            if (reader.GetInt32(0) > 0)
                                errors.Add(new ValidationResult("CNIC already exists. Please enter another CNIC.", new[] { "CNIC" }));

                            if (reader.GetInt32(1) > 0)
                                errors.Add(new ValidationResult("Phone number already exists. Please enter another phone number.", new[] { "PhoneNumber" }));

                            if (reader.GetInt32(2) > 0)
                                errors.Add(new ValidationResult("Email already exists. Please enter another email.", new[] { "Email" }));
                        }
                    }
                }
            }
            return errors;
        }
    }
}
