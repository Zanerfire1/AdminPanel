﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Admin_Panel.Helpers;

namespace Admin_Panel
{
    public partial class UsersWindow : Window
    {
        private int AdminId;
        private int currentPage = 1;
        private const int PageSize = 12;
        private int totalUsers;

        public UsersWindow(int adminId)
        {
            InitializeComponent();
            AdminId = adminId;
            LoadData();
            LoadUsers();
        }

        private void LoadData()
        {
            try
            {
                string timeOfDayGreeting = GetTimeOfDayGreeting();

                string adminName = DatabaseHelper.GetAdminNameById(AdminId);
                string initials = DatabaseHelper.GetAdminInitials(AdminId);

                AdminGreetingTextBlock.Text = $"{timeOfDayGreeting}, {adminName}";
                UsernameTextBlock.Text = adminName;
                InitialsTextBlock.Text = initials;
                this.KeyDown += UsersWindow_KeyDown;

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

        private void LoadUsers()
        {
            try
            {
                var users = DatabaseHelper.GetUsers();
                totalUsers = users.Count;


                var paginatedUsers = users.Skip((currentPage - 1) * PageSize).Take(PageSize).ToList();

                UsersDataGrid.ItemsSource = null;
                UsersDataGrid.Items.Clear();

                UsersDataGrid.ItemsSource = paginatedUsers;
                UpdatePagination();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void UpdatePagination()
        {
            TotalPagesTextBlock.Text = $"{currentPage} из {Math.Max(1, (int)Math.Ceiling((double)totalUsers / PageSize))}";
            PrevPageButton.IsEnabled = currentPage > 1;
            NextPageButton.IsEnabled = currentPage < (int)Math.Ceiling((double)totalUsers / PageSize);
        }

        // Кнопка "Предыдущая страница"
        private void PrevPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                LoadUsers();
            }
        }

        // Кнопка "Следующая страница"
        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage < (int)Math.Ceiling((double)totalUsers / PageSize))
            {
                currentPage++;
                LoadUsers();
            }
        }

        private void UsersWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                PrevPageButton_Click(sender, e); // Переход назад
            }
            else if (e.Key == Key.Right)
            {
                NextPageButton_Click(sender, e); // Переход вперед
            }
        }


        // Добавление пользователя
        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            var addUserWindow = new AddUserWindow();
            if (addUserWindow.ShowDialog() == true)
            {
                LoadUsers();
            }
        }

        // Редактирование пользователя
        private void EditUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is User selectedUser)
            {
                var editUserWindow = new EditUserWindow(selectedUser);
                if (editUserWindow.ShowDialog() == true)
                {
                    LoadUsers();
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя для редактирования.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is User selectedUser)
            {
                var result = MessageBox.Show($"Вы уверены, что хотите удалить пользователя {selectedUser.Username}?",
                                             "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        DatabaseHelper.DeleteUser(selectedUser.Id);
                        LoadUsers();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя для удаления.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Поиск и фильтрация
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            try
            {
                var query = SearchTextBox.Text.Trim().ToLower();
                var filteredUsers = DatabaseHelper.GetUsers()
                    .Where(u => u.Username.ToLower().Contains(query) || u.Email.ToLower().Contains(query))
                    .ToList();

                totalUsers = filteredUsers.Count;
                var paginatedUsers = filteredUsers.Skip((currentPage - 1) * PageSize).Take(PageSize).ToList();

                UsersDataGrid.ItemsSource = paginatedUsers;
                UpdatePagination();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
 

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigateToMainWindow(AdminId, this);
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Logout();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }
    }
}
