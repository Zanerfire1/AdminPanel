using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Admin_Panel.Helpers
{
    public static class DatabaseHelper
    {
        private const string ConnectionString = "Server=WIN-6OQEGBE24R9\\SQLEXPRESS;Database=Finances2;Trusted_Connection=True;";


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


        public static int GetActiveSubscribersCount()
        {
            int activeSubscribers = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = @"
            SELECT COUNT(DISTINCT user_id) 
            FROM paid_subscribers 
            WHERE GETDATE() BETWEEN subscription_start_date AND subscription_end_date";

                SqlCommand command = new SqlCommand(query, connection);
                object result = command.ExecuteScalar();
                if (result != DBNull.Value && result != null)
                {
                    activeSubscribers = Convert.ToInt32(result);
                }
            }
            return activeSubscribers;
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

            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand("SELECT id, username, email, password_hash, current_balance, created_at, last_login FROM Users", connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new User
                            {
                                Id = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                Email = reader.GetString(2),
                                PasswordHash = reader.GetString(3),
                                CurrentBalance = reader.IsDBNull(4) ? 0m : reader.GetDecimal(4),
                                CreatedAt = reader.IsDBNull(5) ? DateTime.MinValue : reader.GetDateTime(5),
                                LastLogin = reader.IsDBNull(6) ? DateTime.MinValue : reader.GetDateTime(6)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке пользователей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    DELETE FROM user_currencies WHERE user_id = @id;
                    DELETE FROM paid_subscribers WHERE user_id = @id;
                    DELETE FROM users WHERE id = @id;";

                        using (var command = new SqlCommand(query, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@id", userId);
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Ошибка при удалении пользователя: " + ex.Message, ex);
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

        public static List<Course> GetCoursesWithCategories()
        {
            List<Course> courses = new List<Course>();
            string query = @"
        SELECT c.id, c.course_name, c.description, c.video_url, c.category_id, c.is_paid, cc.category_name
        FROM courses c
        JOIN course_categories cc ON c.category_id = cc.id";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    courses.Add(new Course
                    {
                        Id = reader.GetInt32(0),
                        CourseName = reader.GetString(1),
                        Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                        VideoUrl = reader.IsDBNull(3) ? null : reader.GetString(3),
                        CategoryId = reader.GetInt32(4),
                        IsPaid = reader.GetBoolean(5),
                        CategoryName = reader.GetString(6)
                    });
                }
            }

            return courses;
        }

        public static List<Category> GetCourseCategories()
        {
            List<Category> categories = new List<Category>();
            string query = "SELECT id, category_name FROM course_categories";

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

        public static void AddCourse(string courseName, string description, string videoUrl, int categoryId, bool isPaid)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = "INSERT INTO courses (course_name, description, video_url, category_id, is_paid) " +
                               "VALUES (@CourseName, @Description, @VideoUrl, @CategoryId, @IsPaid)";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CourseName", courseName);
                    command.Parameters.AddWithValue("@Description", (object)description ?? DBNull.Value);
                    command.Parameters.AddWithValue("@VideoUrl", (object)videoUrl ?? DBNull.Value);
                    command.Parameters.AddWithValue("@CategoryId", categoryId);
                    command.Parameters.AddWithValue("@IsPaid", isPaid);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void UpdateCourse(Course course)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = "UPDATE courses SET course_name = @CourseName, description = @Description, " +
                               "video_url = @VideoUrl, category_id = @CategoryId, is_paid = @IsPaid WHERE id = @Id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CourseName", course.CourseName);
                    command.Parameters.AddWithValue("@Description", (object)course.Description ?? DBNull.Value);
                    command.Parameters.AddWithValue("@VideoUrl", (object)course.VideoUrl ?? DBNull.Value);
                    command.Parameters.AddWithValue("@CategoryId", course.CategoryId);
                    command.Parameters.AddWithValue("@IsPaid", course.IsPaid);
                    command.Parameters.AddWithValue("@Id", course.Id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteCourse(int courseId)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = "DELETE FROM courses WHERE id = @Id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", courseId);
                    command.ExecuteNonQuery();
                }
            }
        }


        public static bool CourseCategoryExists(string categoryName, int excludeId = -1)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = excludeId == -1
                    ? "SELECT COUNT(*) FROM course_categories WHERE category_name = @CategoryName"
                    : "SELECT COUNT(*) FROM course_categories WHERE category_name = @CategoryName AND id != @Id";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CategoryName", categoryName);
                    if (excludeId != -1)
                        command.Parameters.AddWithValue("@Id", excludeId);

                    return (int)command.ExecuteScalar() > 0;
                }
            }
        }

        public static void AddCourseCategory(string categoryName)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = "INSERT INTO course_categories (category_name) VALUES (@CategoryName)";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CategoryName", categoryName);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void UpdateCourseCategory(Category category)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = "UPDATE course_categories SET category_name = @CategoryName WHERE id = @Id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CategoryName", category.CategoryName);
                    command.Parameters.AddWithValue("@Id", category.Id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteCourseCategory(int categoryId)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = "DELETE FROM course_categories WHERE id = @Id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", categoryId);
                    command.ExecuteNonQuery();
                }
            }
        }



    }
}
