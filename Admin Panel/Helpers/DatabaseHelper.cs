using System;
using System.Collections.Generic;
using System.Data.SqlClient;

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







    }
}
