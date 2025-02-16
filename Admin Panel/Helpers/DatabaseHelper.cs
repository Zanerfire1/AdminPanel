using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace Admin_Panel.Helpers
{
    public static class DatabaseHelper
    {
        private const string ConnectionString = "Server=WIN-6OQEGBE24R9\\SQLEXPRESS;Database=Finances;Trusted_Connection=True;";


        /// Получает количество пользователей

        public static int GetTotalUsers()
        {
            int totalUsers = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM users;";
                SqlCommand command = new SqlCommand(query, connection);
                totalUsers = Convert.ToInt32(command.ExecuteScalar());
            }
            return totalUsers;
        }

  
        /// Получает средний баланс пользователей
       
        public static double GetAverageBalance()
        {
            double avgBalance = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = "SELECT AVG(current_balance) FROM users;";
                SqlCommand command = new SqlCommand(query, connection);
                object result = command.ExecuteScalar();
                avgBalance = result != DBNull.Value ? Convert.ToDouble(result) : 0;
            }
            return avgBalance;
        }


        public static decimal GetTotalBalance()
        {
            decimal totalBalance = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = "SELECT SUM(current_balance) FROM users;"; 
                SqlCommand command = new SqlCommand(query, connection);
                object result = command.ExecuteScalar();
                if (result != DBNull.Value && result != null)
                {
                    totalBalance = Convert.ToDecimal(result);
                }
            }
            return totalBalance;
        }


        /// Получает количество новых пользователей за день

        public static int GetNewUsersLastDay()
        {
            int newUsers = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM users WHERE created_at >= CAST(GETDATE() AS DATE);";
                SqlCommand command = new SqlCommand(query, connection);
                newUsers = Convert.ToInt32(command.ExecuteScalar());
            }
            return newUsers;
        }




        public static string GetAdminNameById(int adminId)
        {
            string adminName = "Неизвестный администратор";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = "SELECT username FROM admins WHERE id = @adminId";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@adminId", adminId);
                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        adminName = result.ToString();
                    }
                }
            }
            return adminName;
        }

        public static string GetAdminInitials(int adminId)
        {
            string initials = "--";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = "SELECT LEFT(username, 2) FROM admins WHERE id = @adminId";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@adminId", adminId);
                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        initials = result.ToString().ToUpper();
                    }
                }
            }
            return initials;
        }

        public static Dictionary<string, int> GetNewUsersPerDay()
        {
            Dictionary<string, int> userCounts = new Dictionary<string, int>();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = @"
                        SELECT FORMAT(created_at, 'yyyy-MM-dd') AS Date, COUNT(*) AS UserCount
                        FROM users
                        WHERE created_at >= DATEADD(DAY, -6, CAST(GETDATE() AS DATE))
                        GROUP BY FORMAT(created_at, 'yyyy-MM-dd')
                        ORDER BY Date;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string date = reader.GetString(0);  
                            int count = reader.GetInt32(1);    
                            userCounts[date] = count;
                        }
                    }
                }
            }

            return userCounts;
        }


        public static List<User> GetUsers()
        {
            List<User> users = new List<User>();
            string query = "SELECT id, username, email, current_balance, created_at, password_hash FROM users";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    users.Add(new User
                    {
                        Id = reader.GetInt32(0),
                        Username = reader.GetString(1),
                        Email = reader.GetString(2),
                        CurrentBalance = reader.GetDecimal(3),
                        CreatedAt = reader.GetDateTime(4),
                        PasswordHash = reader.GetString(5)
                    });
                }
            }

            return users;
        }

        public static bool UserExists(string username, string email)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM users WHERE username = @username OR email = @email";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@email", email);

                    int count = (int)command.ExecuteScalar();
                    return count > 0; // true, если есть совпадение
                }
            }
        }

        public static bool EditUserExists(string username, string email, int userId)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM users WHERE (username = @username OR email = @email) AND id != @id";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@id", userId);

                    int count = (int)command.ExecuteScalar();
                    return count > 0; // true, если дубликат найден
                }
            }
        }




        //public static List<User> GetUsers(string searchQuery, int page, int pageSize)
        //{
        //    var users = new List<User>();

        //    using (SqlConnection connection = new SqlConnection(ConnectionString))
        //    {
        //        connection.Open();
        //        string query = @"SELECT id, username, email FROM users 
        //                 WHERE username LIKE @searchQuery 
        //                 ORDER BY id 
        //                 OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

        //        using (var command = new SqlCommand(query, connection))
        //        {
        //            command.Parameters.AddWithValue("@searchQuery", $"%{searchQuery}%");
        //            command.Parameters.AddWithValue("@offset", (page - 1) * pageSize);
        //            command.Parameters.AddWithValue("@pageSize", pageSize);

        //            using (var reader = command.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    users.Add(new User
        //                    {
        //                        Id = reader.GetInt32(0),
        //                        Username = reader.GetString(1),
        //                        Email = reader.GetString(2)
        //                    });
        //                }
        //            }
        //        }
        //    }

        //    return users;
        //}

        public static int GetUsersCount(string searchQuery)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM users WHERE username LIKE @searchQuery";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@searchQuery", $"%{searchQuery}%");
                    return (int)command.ExecuteScalar();
                }
            }
        }

        public static void AddUser(string username, string email, string passwordHash)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = @"INSERT INTO users (username, email, password_hash, created_at, last_login)
                         VALUES (@Username, @Email, @PasswordHash, GETDATE(), NULL)";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@PasswordHash", passwordHash);
                    command.ExecuteNonQuery();
                }
            }
        }



        public static void UpdateUser(User user, string newPassword = null)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                string query = newPassword != null
                    ? "UPDATE users SET username = @username, email = @email, password_hash = @passwordHash WHERE id = @id"
                    : "UPDATE users SET username = @username, email = @email WHERE id = @id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", user.Username);
                    command.Parameters.AddWithValue("@email", user.Email);
                    command.Parameters.AddWithValue("@id", user.Id);

                    if (newPassword != null)
                    {
                        string passwordHash = ComputeSha256Hash(newPassword);
                        command.Parameters.AddWithValue("@passwordHash", passwordHash);
                    }

                    command.ExecuteNonQuery();
                }
            }
        }

        // Метод для вычисления SHA-256 хэша
        private static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }


        public static void DeleteUser(int userId)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string query = @"
                    DELETE FROM expenses WHERE user_id = @id;
                    DELETE FROM incomes WHERE user_id = @id;
                    DELETE FROM instant_expenses WHERE user_id = @id;
                    DELETE FROM financial_operations WHERE user_id = @id;
                    DELETE FROM user_sessions WHERE user_id = @id;
                    DELETE FROM user_values WHERE user_id = @id;
                    DELETE FROM users WHERE id = @id;";

                        using (var command = new SqlCommand(query, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@id", userId);
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit(); 
                    }
                    catch
                    {
                        transaction.Rollback(); 
                        throw; 
                    }
                }
            }
        }


        public static void InsertUser(string username, string email, string password, decimal balance)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                string query = @"
            INSERT INTO users (username, email, password_hash, current_balance, created_at, last_login) 
            VALUES (@Username, @Email, HASHBYTES('SHA2_256', @Password), @Balance, GETDATE(), GETDATE());";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@Password", password);
                    command.Parameters.AddWithValue("@Balance", balance);

                    command.ExecuteNonQuery();
                }
            }
        }

    }
}
