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
                        ? sqlEx.Message
                        : $"An unexpected error occurred while retrieving employees.",
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
                        SELECT e.*, e.EmployeeDesignationID, d.DesignationName, dept.DepartmentName
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
                        EmployeeDesignationID = reader.GetInt32(reader.GetOrdinal("EmployeeDesignationID")),
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
                        : $"An unexpected error occurred while adding the employee.",
                    ex);
            }
        }

        public async Task UpdateAsync(Employee employee)
        {
            try
            {
                using var connection = _dbHelper.GetConnection();
                await connection.OpenAsync();

                // Get current employee data for comparison
                var currentEmployee = await GetByIdAsync(employee.Id);
                if (currentEmployee == null)
                {
                    throw new KeyNotFoundException($"Employee with ID {employee.Id} not found");
                }

                // Validate CNIC if it's being changed
                if (employee.CNIC?.Trim() != currentEmployee.CNIC?.Trim())
                {
                    // Check for duplicate CNIC
                    using var checkCommand = new SqlCommand(
                        "SELECT COUNT(1) FROM Employee WHERE CNIC = @CNIC AND Id <> @Id",
                        connection);

                    checkCommand.Parameters.AddWithValue("@CNIC", employee.CNIC?.Trim());
                    checkCommand.Parameters.AddWithValue("@Id", employee.Id);

                    var exists = (int)await checkCommand.ExecuteScalarAsync();
                    if (exists > 0)
                    {
                        throw new InvalidOperationException("Another employee with this CNIC already exists");
                    }
                }

                // Update query
                var query = @"
                        UPDATE Employee
                        SET 
                            FirstName = @FirstName,
                            LastName = @LastName,
                            Gender = @Gender,
                            DateOfBirth = @DateOfBirth,
                            CNIC = @CNIC,
                            PhoneNumber = @PhoneNumber,
                            Email = @Email,
                            HireDate = @HireDate,
                            Salary = @Salary,
                            IsActive = @IsActive,
                            Address = @Address,
                            EmployeeDesignationID = @EmployeeDesignationID,
                            UpdatedDate = GETDATE()
                        WHERE Id = @Id";

                using var command = new SqlCommand(query, connection);

                // Add parameters
                command.Parameters.AddWithValue("@Id", employee.Id);
                command.Parameters.AddWithValue("@FirstName", employee.FirstName);
                command.Parameters.AddWithValue("@LastName", employee.LastName);
                command.Parameters.AddWithValue("@Gender", employee.Gender);
                command.Parameters.AddWithValue("@DateOfBirth", employee.DateOfBirth);
                command.Parameters.AddWithValue("@CNIC", employee.CNIC?.Trim() ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@PhoneNumber", employee.PhoneNumber);
                command.Parameters.AddWithValue("@Email", employee.Email?.ToLower() ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@HireDate", employee.HireDate);
                command.Parameters.AddWithValue("@Salary", employee.Salary);
                command.Parameters.AddWithValue("@IsActive", employee.IsActive);
                command.Parameters.AddWithValue("@Address", string.IsNullOrEmpty(employee.Address) ? DBNull.Value : employee.Address);
                command.Parameters.AddWithValue("@EmployeeDesignationID", employee.EmployeeDesignationID);

                int rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected <= 0)
                {
                    throw new Exception("Employee update failed. No rows were affected.");
                }
            }
            catch (Exception ex) when (ex is not CustomException)
            {
                throw new CustomException(
                    ex is SqlException
                        ? "Database error occurred while updating employee"
                        : "An unexpected error occurred while updating employee",
                    ex);
            }
        }

        public async Task SoftDeleteAsync(int id)
        {
            try
            {
                using var connection = _dbHelper.GetConnection();
                await connection.OpenAsync();

                using var command = new SqlCommand("UPDATE Employee SET IsActive = 0 WHERE Id = @Id AND IsActive = 1;", connection);

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
                    ? new CustomException("Database error occurred while deleting the employee.", sqlEx)
                    : new CustomException("An unexpected error occurred while deleting the employee.", ex);
            }
        }

        public async Task RestoreDeleteAsync(int id)
        {
            try
            {
                using var connection = _dbHelper.GetConnection();
                await connection.OpenAsync();

                using var command = new SqlCommand("UPDATE Employee SET IsActive = 1 WHERE Id = @Id AND IsActive = 0;", connection);

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
    }
}
