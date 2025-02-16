using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using Admin_Panel.Helpers;

namespace Admin_Panel
{
    public partial class EditUserWindow : Window
    {
        private readonly User _user;

        public EditUserWindow(User user)
        {
            InitializeComponent();
            _user = user;
            UsernameTextBox.Text = user.Username;
            EmailTextBox.Text = user.Email;
        }

        private void SaveUserButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string newPassword = PasswordBox.Password;

            ErrorTextBlock.Visibility = Visibility.Collapsed;


            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email))
            {
                ShowError("Имя пользователя и email обязательны");
                return;
            }

            try
            {

                if (DatabaseHelper.EditUserExists(username, email, _user.Id))
                {
                    ShowError("Логин или email уже заняты другим пользователем");
                    return;
                }


                _user.Username = username;
                _user.Email = email;

                if (!string.IsNullOrEmpty(newPassword))
                {
                    DatabaseHelper.UpdateUser(_user, newPassword);
                }
                else
                {
                    DatabaseHelper.UpdateUser(_user);
                }


                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка при обновлении: {ex.Message}");
            }
        }


        private void ShowError(string message)
        {
            ErrorTextBlock.Text = message;
            ErrorTextBlock.Visibility = Visibility.Visible;
        }



        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes) builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }
    }
}
