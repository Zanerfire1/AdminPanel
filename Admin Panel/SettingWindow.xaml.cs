using System;
using System.Windows;
using System.Windows.Input;
using Admin_Panel.Helpers;

namespace Admin_Panel
{
    public partial class SettingWindow : Window
    {
        private readonly Admin _admin;

        public SettingWindow(Admin admin)
        {
            InitializeComponent();

            _admin = admin ?? throw new ArgumentNullException(nameof(admin));

            AdminIdTextBlock.Text = _admin.Id.ToString();
            AdminNameTextBlock.Text = _admin.Username;
            AdminEmailTextBlock.Text = _admin.Email;

            // Заполняем текстовые поля
            UsernameTextBox.Text = _admin.Username;
            EmailTextBox.Text = _admin.Email;

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                string timeOfDayGreeting = GetTimeOfDayGreeting();

                // Получаем данные по Id администратора
                string adminName = DatabaseHelper.GetAdminNameById(_admin.Id);
                string initials = DatabaseHelper.GetAdminInitials(_admin.Id);

                AdminGreetingTextBlock.Text = $"{timeOfDayGreeting}, {adminName}";
                UsernameTextBlock.Text = adminName;
                EmailTextBox.Text = _admin.Email;
                InitialsTextBlock.Text = initials;
                InitialsTextBlock1.Text = initials;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
        }

        private string GetTimeOfDayGreeting()
        {
            int hour = DateTime.Now.Hour;

            if (hour >= 5 && hour < 12)
                return "Good Morning";
            else if (hour >= 12 && hour < 18)
                return "Good Afternoon";
            else if (hour >= 18 && hour < 22)
                return "Good Evening";
            else
                return "Good Night";
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigateToMainWindow(_admin.Id, this);
        }

        private void UsersButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigateToUsersWindow(_admin.Id, this);
        }

        private void CategoriesButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigateToCategoryWindow(_admin.Id, this);
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Logout();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ShowEditGrid_Click(object sender, RoutedEventArgs e)
        {
            EditGrid.Visibility = Visibility.Visible;
        }


        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void SaveAdminButton_Click(object sender, RoutedEventArgs e)
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
                // Проверяем, существует ли другой админ с таким же логином или email
                if (DatabaseHelper.AdminExists(username, email, _admin.Id))
                {
                    ShowError("Логин или email уже заняты другим администратором");
                    return;
                }

                // Обновляем объект администратора
                _admin.Username = username;
                _admin.Email = email;

                // Сохраняем изменения
                if (!string.IsNullOrEmpty(newPassword))
                {
                    DatabaseHelper.UpdateAdmin(_admin, newPassword);
                }
                else
                {
                    DatabaseHelper.UpdateAdmin(_admin);
                }

                ReloadWindow();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка при обновлении: {ex.Message}");
            }
        }

        private void SaveAdminButton_Click1(object sender, RoutedEventArgs e)
        {
            string username = AdminUsernameTextBox.Text.Trim();
            string email = AdminEmailTextBox.Text.Trim();
            string password = AdminPasswordBox.Password.Trim();

            AdminErrorTextBlock.Visibility = Visibility.Collapsed;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowAdminError("Имя пользователя, email и пароль обязательны");
                return;
            }

            try
            {
                // Проверяем, существует ли администратор с таким же логином или email
                if (DatabaseHelper.AdminExists1(username, email))
                {
                    ShowAdminError("Логин или email уже заняты");
                    return;
                }

                // Хэшируем пароль
                string passwordHash = DatabaseHelper.ComputeSha256Hash(password);

                // Добавляем администратора
                DatabaseHelper.AddAdmin(username, email, passwordHash);

                // Обновляем данные и скрываем окно
                ReloadWindow();
                HideAddAdminGrid();
            }
            catch (Exception ex)
            {
                ShowAdminError($"Ошибка при добавлении: {ex.Message}");
            }
        }



        private void ReloadWindow()
        {
            Admin updatedAdmin = DatabaseHelper.GetAdminById(_admin.Id);
            var newWindow = new SettingWindow(updatedAdmin);
            newWindow.Show();

            Close();
        }

        



        private void ShowError(string message)
        {
            ErrorTextBlock.Text = message;
            ErrorTextBlock.Visibility = Visibility.Visible;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ReloadWindow();
        }



        private void ShowAddAdminGrid()
        {
            AddAdminGrid.Visibility = Visibility.Visible;
        }

        private void HideAddAdminGrid()
        {
            AddAdminGrid.Visibility = Visibility.Collapsed;
        }

        private void CancelAdminButton_Click(object sender, RoutedEventArgs e)
        {
            HideAddAdminGrid();
        }

        private void ShowAddAdminButton_Click(object sender, RoutedEventArgs e)
        {
            ShowAddAdminGrid();
        }

        private void ShowAdminError(string message)
        {
            AdminErrorTextBlock.Text = message;
            AdminErrorTextBlock.Visibility = Visibility.Visible;
        }



    }
}
