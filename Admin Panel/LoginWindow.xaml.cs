using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace Admin_Panel
{
    public partial class LoginWindow : Window
    {
        private const string ConnectionString = "Server=WIN-6OQEGBE24R9\\SQLEXPRESS;Database=Finances;Trusted_Connection=True;";

        public LoginWindow()
        {
            InitializeComponent();
        }

        public int AuthenticatedUserId { get; private set; }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            UsernameTextBox.Clear();
            PasswordBox.Clear();

            int adminId = AuthenticateAdmin(username, password); 

            if (adminId > 0) 
            {
                
                MainWindow mainWindow = new MainWindow(adminId);
                this.Hide();

                mainWindow.Show();
                this.Close();
            }
            else
            {
                ErrorTextBlock.Text = "Неверные учетные данные";
                ErrorTextBlock.Visibility = Visibility.Visible;
            }
        }


        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }



        private int AuthenticateAdmin(string username, string password)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = "SELECT id FROM admins WHERE username = @username AND LOWER(password_hash) = LOWER(@password_hash)";



                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password_hash", ComputeSha256Hash(password));

                    object result = command.ExecuteScalar();
                    int adminId = Convert.ToInt32(result);
                    return adminId;
                }
            }
        }

        private static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
