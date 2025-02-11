using System;
using System.Data.SqlClient;

namespace FinanseService.Helpers
{
    public static class DatabaseHelper
    {
        private const string ConnectionString = "Server=WIN-6OQEGBE24R9\\SQLEXPRESS;Database=Finances;Trusted_Connection=True;";

        /// <summary>
        /// Получает количество пользователей
        /// </summary>
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

        /// <summary>
        /// Получает средний баланс пользователей
        /// </summary>
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
    }
}
