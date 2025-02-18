using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using Admin_Panel.Helpers;

namespace Admin_Panel
{
    public partial class AddUserWindow : Window
    {
        public AddUserWindow()
        {
            InitializeComponent();
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            // Проверка на пустые поля
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ErrorTextBlock.Text = "Заполните все поля!";
                ErrorTextBlock.Visibility = Visibility.Visible;
                return;
            }

            try
            {
                if (DatabaseHelper.UserExists(username, email))
                {
                    ErrorTextBlock.Text = "Пользователь с таким логином или email уже существует.";
                    ErrorTextBlock.Visibility = Visibility.Visible;
                    return;
                }


                string passwordHash = ComputeSha256Hash(password);


                DatabaseHelper.AddUser(username, email, passwordHash);

                DialogResult = true; 
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false; // Закрыть окно без изменений
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

        // Метод для отображения сообщений
        private void ShowMessage(string message, bool isError)
        {
            ErrorTextBlock.Text = message;
            ErrorTextBlock.Foreground = isError ? System.Windows.Media.Brushes.Red : System.Windows.Media.Brushes.Green;
            ErrorTextBlock.Visibility = Visibility.Visible;
        }
    }
}
