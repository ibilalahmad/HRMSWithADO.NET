using Microsoft.Data.SqlClient;
using System.Data;

namespace AhmadHRManagementSystem.Repository
{
    public class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("AHRMS_CS");
        }

        // Validate User with Username and Password
        public DataRow ValidateUser(string identifier, string password)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT U.*, R.RoleName 
                    FROM Users U 
                    JOIN Roles R ON U.RoleId = R.Id 
                    WHERE (U.Username = @Identifier OR U.Email = @Identifier) 
                    AND U.Password = @Password", con))
                {
                    cmd.Parameters.AddWithValue("@Identifier", identifier);
                    cmd.Parameters.AddWithValue("@Password", password);

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        return dt.Rows.Count > 0 ? dt.Rows[0] : null;
                    }
                }
            }
        }

    }
}
