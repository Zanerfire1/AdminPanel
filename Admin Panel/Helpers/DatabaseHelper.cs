﻿using System;
using System.Collections.Generic;
using System.Data;
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

        public static Admin GetAdminById(int adminId)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = "SELECT id, username, email FROM admins WHERE id = @id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", adminId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Admin
                            {
                                Id = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                Email = reader.GetString(2)
                            };
                        }
                    }
                }
            }

            throw new Exception("Администратор с таким ID не найден.");
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
                    return count > 0; 
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
        public static string ComputeSha256Hash(string rawData)
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

        // 1. Список операций для всех счетов
        public static DataTable GetAllTransactions()
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = @"
                    SELECT fo.id, fo.operation_name, fo.operation_type, fo.amount, fo.operation_date, u.username
                    FROM financial_operations fo
                    JOIN users u ON fo.user_id = u.id
                    ORDER BY fo.operation_date DESC;";
                using (var command = new SqlCommand(query, connection))
                using (var adapter = new SqlDataAdapter(command))
                {
                    var dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    return dataTable;
                }
            }
        }

        // 2. Остаток средств на каждом счете
        public static DataTable GetAccountBalances()
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = @"
                    SELECT id, username, current_balance
                    FROM users
                    ORDER BY current_balance DESC;";
                using (var command = new SqlCommand(query, connection))
                using (var adapter = new SqlDataAdapter(command))
                {
                    var dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    return dataTable;
                }
            }
        }

        // 3. Счета с наибольшим количеством транзакций
        public static DataTable GetTopAccountsByTransactions()
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = @"
                    SELECT u.id, u.username, COUNT(fo.id) AS transaction_count
                    FROM financial_operations fo
                    JOIN users u ON fo.user_id = u.id
                    GROUP BY u.id, u.username
                    ORDER BY transaction_count DESC;";
                using (var command = new SqlCommand(query, connection))
                using (var adapter = new SqlDataAdapter(command))
                {
                    var dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    return dataTable;
                }
            }
        }

        // 4. Общий оборот средств за текущий месяц
        public static decimal GetMonthlyTurnover()
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = @"
                    SELECT SUM(amount) 
                    FROM financial_operations
                    WHERE MONTH(operation_date) = MONTH(GETDATE()) 
                    AND YEAR(operation_date) = YEAR(GETDATE());";
                using (var command = new SqlCommand(query, connection))
                {
                    return Convert.ToDecimal(command.ExecuteScalar() ?? 0);
                }
            }
        }

        public static void UpdateAdmin(Admin admin, string newPassword = null)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                string query = newPassword != null
                    ? "UPDATE admins SET username = @username, email = @email, password_hash = @passwordHash WHERE id = @id"
                    : "UPDATE admins SET username = @username, email = @email WHERE id = @id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", admin.Username);
                    command.Parameters.AddWithValue("@email", admin.Email);
                    command.Parameters.AddWithValue("@id", admin.Id);

                    if (newPassword != null)
                    {
                        string passwordHash = ComputeSha256Hash(newPassword);
                        command.Parameters.AddWithValue("@passwordHash", passwordHash);
                    }

                    command.ExecuteNonQuery();
                }
            }
        }

        // Метод проверки существования администратора с такими же username или email (исключая текущего по id)
        public static bool AdminExists(string username, string email, int excludeId)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                string query = "SELECT COUNT(*) FROM admins WHERE (username = @username OR email = @email) AND id != @id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@id", excludeId);

                    return (int)command.ExecuteScalar() > 0;
                }
            }
        }

        public static bool AdminExists1(string username, string email)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                var query = "SELECT COUNT(*) FROM admins WHERE username = @Username OR email = @Email";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Email", email);
                    return (int)command.ExecuteScalar() > 0;
                }
            }
        }

        public static void AddAdmin(string username, string email, string passwordHash)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                var query = "INSERT INTO admins (username, email, password_hash, created_at) VALUES (@Username, @Email, @PasswordHash, GETDATE())";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@PasswordHash", passwordHash);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static List<Category> GetCategories()
        {
            List<Category> categories = new List<Category>();
            string query = "SELECT id, category_name FROM operation_categories";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    categories.Add(new Category
                    {
                        Id = reader.GetInt32(0),
                        CategoryName = reader.GetString(1)
                    });
                }
            }

            return categories;
        }

        public static void AddCategory(string categoryName)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = "INSERT INTO operation_categories (category_name) VALUES (@CategoryName)";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CategoryName", categoryName);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void UpdateCategory(Category category)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = "UPDATE operation_categories SET category_name = @CategoryName WHERE id = @Id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CategoryName", category.CategoryName);
                    command.Parameters.AddWithValue("@Id", category.Id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteCategory(int categoryId)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = "DELETE FROM operation_categories WHERE id = @Id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", categoryId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static bool CategoryExists(string categoryName, int excludeId = -1)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = excludeId == -1
                    ? "SELECT COUNT(*) FROM operation_categories WHERE category_name = @CategoryName"
                    : "SELECT COUNT(*) FROM operation_categories WHERE category_name = @CategoryName AND id != @Id";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CategoryName", categoryName);
                    if (excludeId != -1)
                        command.Parameters.AddWithValue("@Id", excludeId);

                    return (int)command.ExecuteScalar() > 0;
                }
            }
        }



    }
}
