using AhmadHRManagementSystem.Data;
using AhmadHRManagementSystem.Exceptions;
using AhmadHRManagementSystem.Models;
using AhmadHRManagementSystem.Repository.Abstractions;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AhmadHRManagementSystem.Repository.Repositories
{
    public class EmployeeDepartmentRepository : IRepository<EmployeeDepartment>
    {
        private readonly DatabaseHelper _dbHelper;

        public EmployeeDepartmentRepository(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<IEnumerable<EmployeeDepartment>> GetAllAsync()
        {
            var employeeDepartment = new List<EmployeeDepartment>();

            try
            {
                using var connection = _dbHelper.GetConnection();
                await connection.OpenAsync();

                using var command = new SqlCommand("SELECT * FROM EmployeeDepartment;", connection);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    employeeDepartment.Add(new EmployeeDepartment
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        DepartmentName = reader["DepartmentName"].ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                throw ex is SqlException sqlEx
                    ? new CustomException("Database error occurred while retrieving Employee Department records.", sqlEx)
                    : new CustomException("An unexpected error occurred while retrieving Employee Department records.", ex);
            }

            return employeeDepartment;
        }

        public async Task<EmployeeDepartment> GetByIdAsync(int id)
        {
            EmployeeDepartment? employeeDepartment = null;

            try
            {
                using var connection = _dbHelper.GetConnection();
                await connection.OpenAsync();

                using var command = new SqlCommand("SELECT * FROM EmployeeDepartment WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);

                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    employeeDepartment = new EmployeeDepartment
                    {
                        Id = (int)reader["Id"],
                        DepartmentName = reader["DepartmentName"].ToString()
                    };
                }

                if (employeeDepartment == null)
                {
                    throw new CustomException($"Employee Department with ID {id} not found.");
                }
            }
            catch (Exception ex)
            {
                throw ex is SqlException sqlEx
                    ? new CustomException("Database error occurred while retrieving Employee Department record.", sqlEx)
                    : new CustomException("An unexpected error occurred while fetching Employee Department record.", ex);
            }

            return employeeDepartment;
        }

        public async Task<int> AddAsync(EmployeeDepartment employeeDepartment)
        {
            try
            {
                using var connection = _dbHelper.GetConnection();
                await connection.OpenAsync();

                string query = "INSERT INTO EmployeeDepartment (DepartmentName) VALUES (@DepartmentName);";

                using var command = new SqlCommand(query, connection);

                // Add parameters
                command.Parameters.AddWithValue("@DepartmentName", employeeDepartment.DepartmentName);

                int rowsInserted = await command.ExecuteNonQueryAsync();

                if (rowsInserted <= 0)
                {
                    throw new CustomException("Employee Department insertion failed. No rows affected.");
                }

                return rowsInserted;
            }
            catch (Exception ex)
            {
                throw ex is SqlException sqlEx
                    ? new CustomException("Database error occurred while adding Employee Department record.", sqlEx)
                    : new CustomException("An unexpected error occurred while adding Employee Department record.", ex);
            }
        }

        public async Task UpdateAsync(EmployeeDepartment employeeDepartment)
        {
            try
            {
                using var connection = _dbHelper.GetConnection();
                await connection.OpenAsync();

                string query = "UPDATE EmployeeDepartment SET DepartmentName = @DepartmentName WHERE Id = @Id";

                using var command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@Id", employeeDepartment.Id);
                command.Parameters.AddWithValue("@DepartmentName", employeeDepartment.DepartmentName);
                
                int rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected <= 0)
                {
                    throw new Exception("Employee Department update failed. No rows were affected.");
                }
            }
            catch (Exception ex)
            {
                throw ex is SqlException sqlEx
                    ? new CustomException("Database error occurred while updating Employee Department record.", sqlEx)
                    : new CustomException("An unexpected error occurred while updating Employee Department record.", ex);
            }
        }
        
        public Task SoftDeleteAsync(int id)
        {
        }
        /*
        public Task SoftDeleteAsync(int id)
        {
            throw new NotImplementedException();
        }
        */
        public Task RestoreDeleteAsync(int id)
        {
        }
    }
}
