using AhmadHRManagementSystem.Data;
using AhmadHRManagementSystem.Exceptions;
using AhmadHRManagementSystem.Models;
using AhmadHRManagementSystem.Repository.Abstractions;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AhmadHRManagementSystem.Repository.Repositories
{
    public class EmployeeRepository : IRepository<Employee>
    {
        private readonly DatabaseHelper _dbHelper;

        public EmployeeRepository(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            var employees = new List<Employee>();

            try
            {
                using var connection = _dbHelper.GetConnection();
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_GetEmployeesDetail", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    employees.Add(new Employee
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("EmployeeID")),
                        FirstName = reader["FirstName"].ToString(),
                        LastName = reader["LastName"].ToString(),
                        Gender = reader["Gender"].ToString(),
                        DateOfBirth = reader.GetDateTime(reader.GetOrdinal("DateOfBirth")),
                        CNIC = reader["CNIC"].ToString(),
                        PhoneNumber = reader["PhoneNumber"].ToString(),
                        Email = reader["Email"] == DBNull.Value ? null : reader["Email"].ToString(),
                        HireDate = reader.GetDateTime(reader.GetOrdinal("HireDate")),
                        Salary = reader.GetDecimal(reader.GetOrdinal("Salary")),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                        CreatedDate = DateTime.Parse(reader["CreatedDate"].ToString()),
                        UpdatedDate = reader["UpdatedDate"] == DBNull.Value ? null : DateTime.Parse(reader["UpdatedDate"].ToString()),
                        Address = reader["Address"] == DBNull.Value ? null : reader["Address"].ToString(),
                        DesignationName = reader["DesignationName"].ToString(),
                        DepartmentName = reader["DepartmentName"].ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(
                    ex is SqlException sqlEx
                        ? $"Database error: {sqlEx.Message}"
                        : $"An unexpected error occurred while retrieving employees: {ex.Message}",
                    ex);
            }

            return employees;
        }

        public async Task<Employee> GetByIdAsync(int id)
        {
            Employee? employee = null;

            try
            {
                using var connection = _dbHelper.GetConnection();
                await connection.OpenAsync();

                var query = @"
                        SELECT e.*, d.DesignationName, dept.DepartmentName
                        FROM Employee e
                        INNER JOIN EmployeeDesignation d ON e.EmployeeDesignationID = d.Id
                        INNER JOIN EmployeeDepartment dept ON d.EmployeeDepartmentID = dept.Id
                        WHERE e.Id = @Id";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    employee = new Employee
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        FirstName = reader["FirstName"].ToString(),
                        LastName = reader["LastName"].ToString(),
                        Gender = reader["Gender"].ToString(),
                        DateOfBirth = reader.GetDateTime(reader.GetOrdinal("DateOfBirth")),
                        CNIC = reader["CNIC"].ToString(),
                        PhoneNumber = reader["PhoneNumber"].ToString(),
                        Email = reader["Email"] == DBNull.Value ? null : reader["Email"].ToString(),
                        HireDate = reader.GetDateTime(reader.GetOrdinal("HireDate")),
                        Salary = reader.GetDecimal(reader.GetOrdinal("Salary")),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                        CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                        UpdatedDate = reader["UpdatedDate"] == DBNull.Value ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedDate")),
                        Address = reader["Address"] == DBNull.Value ? null : reader["Address"].ToString(),
                        DesignationName = reader["DesignationName"].ToString(),
                        DepartmentName = reader["DepartmentName"].ToString()
                    };
                }

                if (employee == null)
                {
                    throw new CustomException($"Employee with ID {id} not found.");
                }
            }
            catch (Exception ex)
            {
                throw ex is SqlException sqlEx
                    ? new CustomException("Database error occurred while retrieving employee.", sqlEx)
                    : new CustomException("An unexpected error occurred while fetching employee records.", ex);
            }

            return employee;
        }

        public async Task<int> AddAsync(Employee employee)
        {
            try
            {
                using var connection = _dbHelper.GetConnection();
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_InsertEmployee", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                // Add parameters
                command.Parameters.AddWithValue("@FirstName", employee.FirstName);
                command.Parameters.AddWithValue("@LastName", employee.LastName);
                command.Parameters.AddWithValue("@Gender", employee.Gender);
                command.Parameters.AddWithValue("@DateOfBirth", employee.DateOfBirth);
                command.Parameters.AddWithValue("@CNIC", employee.CNIC);
                command.Parameters.AddWithValue("@PhoneNumber", employee.PhoneNumber);
                command.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(employee.Email) ? DBNull.Value : employee.Email.ToLower());
                command.Parameters.AddWithValue("@HireDate", employee.HireDate);
                command.Parameters.AddWithValue("@Salary", employee.Salary);
                command.Parameters.AddWithValue("@IsActive", employee.IsActive);
                command.Parameters.AddWithValue("@Address", (object?)employee.Address ?? DBNull.Value);
                command.Parameters.AddWithValue("@EmployeeDesignationID", employee.EmployeeDesignationID);

                // Execute the command and capture the returned value
                int rowsInserted = Convert.ToInt32(await command.ExecuteScalarAsync());

                // Validate insertion
                if (rowsInserted <= 0)
                {
                    throw new CustomException("Employee insertion failed. No rows affected.");
                }

                return rowsInserted;
            }
            catch (Exception ex)
            {
                throw new CustomException(
                    ex is SqlException sqlEx
                    ? sqlEx.Message
                    : "An unexpected error occurred while adding the employee.", ex
                );
            }
        }

        public async Task UpdateAsync(Employee employee)
        {
            try
            {
                using var connection = _dbHelper.GetConnection();
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_UpdateEmployee", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                // Add parameters
                command.Parameters.AddWithValue("@Id", employee.Id);
                command.Parameters.AddWithValue("@FirstName", employee.FirstName);
                command.Parameters.AddWithValue("@LastName", employee.LastName);
                command.Parameters.AddWithValue("@Gender", employee.Gender);
                command.Parameters.AddWithValue("@DateOfBirth", employee.DateOfBirth);
                command.Parameters.AddWithValue("@CNIC", employee.CNIC);
                command.Parameters.AddWithValue("@PhoneNumber", employee.PhoneNumber);
                command.Parameters.AddWithValue("@Email", employee.Email?.ToLower() ?? (object)DBNull.Value); // Convert email to lowercase
                command.Parameters.AddWithValue("@HireDate", employee.HireDate);
                command.Parameters.AddWithValue("@Salary", employee.Salary);
                command.Parameters.AddWithValue("@IsActive", employee.IsActive);
                command.Parameters.AddWithValue("@Address", string.IsNullOrEmpty(employee.Address) ? DBNull.Value : employee.Address);

                // Execute the command
                int rowsAffected = await command.ExecuteNonQueryAsync();

                // Throw an exception if no rows were affected
                if (rowsAffected <= 0)
                {
                    throw new Exception("Employee update failed. No rows were affected.");
                }
            }
            catch (Exception ex)
            {
                throw ex is SqlException sqlEx
                    ? sqlEx.Number == 2627
                        ? new CustomException("An employee with the same CNIC, Phone Number or Email already exists.", sqlEx)
                        : new CustomException("Database error occurred while updating the employee.", sqlEx)
                    : new CustomException("An unexpected error occurred while updating the employee.", ex);
            }
        }

        public async Task SoftDeleteAsync(int id)
        {
            try
            {
                using var connection = _dbHelper.GetConnection();
                await connection.OpenAsync();

                using var command = new SqlCommand("UPDATE Employees SET IsActive = 0 WHERE Id = @Id AND IsActive = 1;", connection);

                command.Parameters.AddWithValue("@Id", id);

                int rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected <= 0)
                {
                    throw new KeyNotFoundException($"Employee with ID {id} not found or already inactive.");
                }
            }
            catch (Exception ex)
            {
                throw ex is SqlException sqlEx
                    ? new CustomException("Database error occurred while deactivating the employee.", sqlEx)
                    : new CustomException("An unexpected error occurred while deactivating the employee.", ex);
            }
        }

        public async Task RestoreDeleteAsync(int id)
        {
            try
            {
                using var connection = _dbHelper.GetConnection();
                await connection.OpenAsync();

                using var command = new SqlCommand("UPDATE Employees SET IsActive = 1 WHERE Id = @Id AND IsActive = 0;", connection);

                command.Parameters.AddWithValue("@Id", id);

                int rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected <= 0)
                {
                    throw new KeyNotFoundException($"Employee with ID {id} not found or is already active.");
                }
            }
            catch (Exception ex)
            {
                throw ex is SqlException sqlEx
                    ? new CustomException("Database error occurred while restoring the employee.", sqlEx)
                    : new CustomException("An unexpected error occurred while restoring the employee.", ex);
            }
        }

        //public async Task<List<Employee>> GetAllEmployeesAsync()
        //{
        //    var employees = new List<Employee>();
        //    try
        //    {
        //        using var connection = new SqlConnection(_connectionString);
        //        await connection.OpenAsync();

        //        using var command = new SqlCommand("sp_GetEmployeesDetail", connection)
        //        {
        //            CommandType = CommandType.StoredProcedure
        //        };

        //        using var reader = await command.ExecuteReaderAsync();
        //        while (await reader.ReadAsync())
        //        {
        //            // Try parsing the gender string into the Gender enum
        //            Enum.TryParse(reader["Gender"].ToString(), out Gender gender);

        //            employees.Add(new Employee
        //            {
        //                Id = Convert.ToInt32(reader["Id"]),
        //                FirstName = reader["FirstName"].ToString(),
        //                LastName = reader["LastName"].ToString(),
        //                Gender = gender,
        //                DateOfBirth = Convert.ToDateTime(reader["DateOfBirth"]),
        //                CNIC = reader["CNIC"].ToString(),
        //                PhoneNumber = reader["PhoneNumber"].ToString(),
        //                Email = reader["Email"] == DBNull.Value ? null : reader["Email"].ToString(),
        //                HireDate = Convert.ToDateTime(reader["HireDate"]),
        //                JobTitle = reader["JobTitle"].ToString(),
        //                Salary = Convert.ToDecimal(reader["Salary"]),
        //                IsActive = Convert.ToBoolean(reader["IsActive"]),
        //                CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
        //                UpdatedDate = reader["UpdatedDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["UpdatedDate"]),
        //                Address = reader["Address"] == DBNull.Value ? null : reader["Address"].ToString()
        //            });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log error without throwing an exception
        //        _logger.LogError(ex, "An error occurred while retrieving employees in {Method}", nameof(GetAllEmployeesAsync));
        //    }

        //    return employees;
        //}


        //public async Task<Employee> GetEmployeeByIdAsync(int id)
        //{
        //    Employee employee = null;

        //    try
        //    {
        //        using (SqlConnection connection = new SqlConnection(_connectionString))
        //        {
        //            await connection.OpenAsync();

        //            using (SqlCommand command = new SqlCommand("SELECT * FROM Employee WHERE Id = @Id", connection))
        //            {
        //                command.Parameters.AddWithValue("@Id", id);

        //                using (SqlDataReader reader = await command.ExecuteReaderAsync())
        //                {
        //                    if (await reader.ReadAsync())
        //                    {
        //                        if (!Enum.TryParse(reader["Gender"].ToString(), out Gender gender))
        //                        {
        //                            throw new Exception($"Invalid gender value '{reader["Gender"]}' found in database.");
        //                        }

        //                        employee = new Employee
        //                        {
        //                            Id = Convert.ToInt32(reader["Id"]),
        //                            FirstName = reader["FirstName"].ToString(),
        //                            LastName = reader["LastName"].ToString(),
        //                            Gender = gender,
        //                            DateOfBirth = Convert.ToDateTime(reader["DateOfBirth"]),
        //                            CNIC = reader["CNIC"].ToString(),
        //                            PhoneNumber = reader["PhoneNumber"].ToString(),
        //                            Email = reader["Email"].ToString(),
        //                            HireDate = Convert.ToDateTime(reader["HireDate"]),
        //                            JobTitle = reader["JobTitle"].ToString(),
        //                            Salary = Convert.ToDecimal(reader["Salary"]),
        //                            IsActive = Convert.ToBoolean(reader["IsActive"]),
        //                            CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
        //                            UpdatedDate = reader["UpdatedDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["UpdatedDate"]),
        //                            Address = reader["Address"].ToString()
        //                        };
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error fetching employee with ID {Id}", id);
        //    }

        //    return employee;
        //}

        //public async Task InsertEmployeeAsync(Employee employee)
        //{
        //    try
        //    {
        //        using var connection = new SqlConnection(_connectionString);
        //        await connection.OpenAsync();

        //        using var command = new SqlCommand("sp_InsertEmployee", connection)
        //        {
        //            CommandType = CommandType.StoredProcedure
        //        };

        //        command.Parameters.AddWithValue("@FirstName", employee.FirstName);
        //        command.Parameters.AddWithValue("@LastName", employee.LastName);
        //        command.Parameters.AddWithValue("@Gender", employee.Gender.ToString());
        //        command.Parameters.AddWithValue("@DateOfBirth", employee.DateOfBirth);
        //        command.Parameters.AddWithValue("@CNIC", employee.CNIC);
        //        command.Parameters.AddWithValue("@PhoneNumber", employee.PhoneNumber);
        //        command.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(employee.Email) ? DBNull.Value : employee.Email.ToLower());
        //        command.Parameters.AddWithValue("@HireDate", employee.HireDate);
        //        command.Parameters.AddWithValue("@JobTitle", employee.JobTitle);
        //        command.Parameters.AddWithValue("@Salary", employee.Salary);
        //        command.Parameters.AddWithValue("@IsActive", employee.IsActive);
        //        command.Parameters.AddWithValue("@Address", (object?)employee.Address ?? DBNull.Value);

        //        if (await command.ExecuteNonQueryAsync() <= 0)
        //        {
        //            throw new Exception("Employee insertion failed. No rows affected.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error inserting employee in {Method}", nameof(InsertEmployeeAsync));
        //        throw;
        //    }
        //}


        //public async Task UpdateEmployeeAsync(Employee employee)
        //{
        //    try
        //    {
        //        using var connection = new SqlConnection(_connectionString);
        //        await connection.OpenAsync();

        //        using var command = new SqlCommand("sp_UpdateEmployee", connection)
        //        {
        //            CommandType = CommandType.StoredProcedure
        //        };

        //        command.Parameters.AddWithValue("@Id", employee.Id);
        //        command.Parameters.AddWithValue("@FirstName", employee.FirstName);
        //        command.Parameters.AddWithValue("@LastName", employee.LastName);
        //        command.Parameters.AddWithValue("@Gender", employee.Gender.ToString());
        //        command.Parameters.AddWithValue("@DateOfBirth", employee.DateOfBirth);
        //        command.Parameters.AddWithValue("@CNIC", employee.CNIC);
        //        command.Parameters.AddWithValue("@PhoneNumber", employee.PhoneNumber);
        //        command.Parameters.AddWithValue("@Email", employee.Email?.ToLower() ?? (object)DBNull.Value); // Convert email to lowercase
        //        command.Parameters.AddWithValue("@HireDate", employee.HireDate);
        //        command.Parameters.AddWithValue("@JobTitle", employee.JobTitle);
        //        command.Parameters.AddWithValue("@Salary", employee.Salary);
        //        command.Parameters.AddWithValue("@IsActive", employee.IsActive);
        //        command.Parameters.AddWithValue("@Address", string.IsNullOrEmpty(employee.Address) ? DBNull.Value : employee.Address);

        //        var rowsAffected = await command.ExecuteNonQueryAsync();

        //        // Throw an exception if no rows were affected
        //        if (rowsAffected <= 0)
        //        {
        //            throw new Exception("Employee update failed. No rows were affected.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "An error occurred while updating employee with ID {EmployeeId}", employee.Id);
        //        throw;  // Re-throw the exception to be handled by the caller
        //    }
        //}
    }
}
