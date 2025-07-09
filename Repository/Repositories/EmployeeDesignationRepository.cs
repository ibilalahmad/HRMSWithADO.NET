using AhmadHRManagementSystem.Data;
using AhmadHRManagementSystem.Exceptions;
using AhmadHRManagementSystem.Models;
using AhmadHRManagementSystem.Repository.Abstractions;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AhmadHRManagementSystem.Repository.Repositories
{
    public class EmployeeDesignationRepository : IRepository<EmployeeDesignation>
    {
        private readonly DatabaseHelper _dbHelper;

        public EmployeeDesignationRepository(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<IEnumerable<EmployeeDesignation>> GetAllAsync()
        {
            var employeeDesignations = new List<EmployeeDesignation>();

            try
            {
                using var connection = _dbHelper.GetConnection();
                await connection.OpenAsync();

                string query = @"
                            SELECT 
                                ed.Id, 
                                ed.DesignationName, 
                                ed.EmployeeDepartmentID, 
                                d.DepartmentName 
                            FROM EmployeeDesignation ed
                            INNER JOIN EmployeeDepartment d ON ed.EmployeeDepartmentID = d.Id";

                using var command = new SqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var designation = new EmployeeDesignation
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        DesignationName = reader["DesignationName"].ToString(),
                        EmployeeDepartmentID = Convert.ToInt32(reader["EmployeeDepartmentID"]),
                        DepartmentName = new EmployeeDepartment
                        {
                            DepartmentName = reader["DepartmentName"].ToString()
                        }
                    };

                    employeeDesignations.Add(designation);
                }
            }
            catch (Exception ex)
            {
                throw ex is SqlException sqlEx
                    ? new CustomException("Database error occurred while retrieving empolyee designation records.", sqlEx)
                    : new CustomException("An unexpected error occurred while retrieving empolyee designation records.", ex);
            }

            return employeeDesignations;
        }

        public async Task<EmployeeDesignation> GetByIdAsync(int id)
        {
            EmployeeDesignation? employeeDesignation = null;

            try
            {
                using var connection = _dbHelper.GetConnection();
                await connection.OpenAsync();

                string query = @"
                    SELECT ed.Id, ed.DesignationName, ed.EmployeeDepartmentID, d.DepartmentName
                    FROM EmployeeDesignation ed
                    INNER JOIN EmployeeDepartment d ON ed.EmployeeDepartmentID = d.Id
                    WHERE ed.Id = @Id;
                    ";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    employeeDesignation = new EmployeeDesignation
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        DesignationName = reader["DesignationName"].ToString(),
                        EmployeeDepartmentID = Convert.ToInt32(reader["EmployeeDepartmentID"]),
                        DepartmentName = new EmployeeDepartment
                        {
                            DepartmentName = reader["DepartmentName"].ToString()
                        }
                    };
                }

                if (employeeDesignation == null)
                {
                    throw new CustomException($"Employee Designation with ID {id} not found.");
                }
            }
            catch (Exception ex)
            {
                throw ex is SqlException sqlEx
                    ? new CustomException("Database error occurred while retrieving Employee Designation record.", sqlEx)
                    : new CustomException("An unexpected error occurred while fetching Employee Designation record.", ex);
            }      

            return employeeDesignation;
        }

        public async Task<int> AddAsync(EmployeeDesignation designation)
        {
            try
            {
                using var connection = _dbHelper.GetConnection();
                await connection.OpenAsync();

                string query = @"
                        INSERT INTO EmployeeDesignation (DesignationName, EmployeeDepartmentID)
                        VALUES (@DesignationName, @EmployeeDepartmentID);
                        SELECT CAST(SCOPE_IDENTITY() AS INT);";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@DesignationName", designation.DesignationName);
                command.Parameters.AddWithValue("@EmployeeDepartmentID", designation.EmployeeDepartmentID);

                var result = await command.ExecuteScalarAsync();

                if (result == null)
                {
                    throw new CustomException("Failed to insert the new employee designation.");
                }

                return Convert.ToInt32(result); // Return the generated ID
            }
            catch (Exception ex)
            {
                throw ex is SqlException sqlEx
                    ? new CustomException("Database error occurred while adding the designation record.", sqlEx)
                    : new CustomException("An unexpected error occurred while adding the designation record.", ex);
            }
        }

        public async Task UpdateAsync(EmployeeDesignation employeeDesignation)
        {
            try
            {
                using var connection = _dbHelper.GetConnection();
                await connection.OpenAsync();

                string query = @"
                        UPDATE EmployeeDesignation 
                        SET DesignationName = @DesignationName, 
                            EmployeeDepartmentID = @EmployeeDepartmentID 
                        WHERE Id = @Id;";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", employeeDesignation.Id);
                command.Parameters.AddWithValue("@DesignationName", employeeDesignation.DesignationName);
                command.Parameters.AddWithValue("@EmployeeDepartmentID", employeeDesignation.EmployeeDepartmentID);

                int rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected <= 0)
                {
                    throw new CustomException("No designation record was updated. Please check the provided ID.");
                }
            }
            catch (Exception ex)
            {
                throw ex is SqlException sqlEx
                    ? new CustomException("Database error occurred while updating the designation record.", sqlEx)
                    : new CustomException("An unexpected error occurred while updating the designation record.", ex);
            }
        }

        public Task SoftDeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task RestoreDeleteAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
