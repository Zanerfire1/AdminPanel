using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Admin_Panel.Helpers;
using Admin_Panel.User_Controls;

namespace Admin_Panel
{
    public partial class CourseCategoriesWindow : Window
    {
        private int AdminId;
        private int currentPage = 1;
        private const int PageSize = 12;
        private int totalCategories;

        public CourseCategoriesWindow(int adminId)
        {
            InitializeComponent();
            AdminId = adminId;
            LoadData();
            LoadCategories();
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
                this.KeyDown += CourseCategoriesWindow_KeyDown;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
        }

        private string GetTimeOfDayGreeting()
        {
            int hour = DateTime.Now.Hour;
            if (hour >= 5 && hour < 12) return "Good Morning";
            else if (hour >= 12 && hour < 18) return "Good Afternoon";
            else if (hour >= 18 && hour < 22) return "Good Evening";
            else return "Good Night";
        }

        private void LoadCategories()
        {
            try
            {
                var categories = DatabaseHelper.GetCourseCategories();
                totalCategories = categories.Count;
                var paginatedCategories = categories.Skip((currentPage - 1) * PageSize).Take(PageSize).ToList();

                CourseCategoriesDataGrid.ItemsSource = null;
                CourseCategoriesDataGrid.Items.Clear();
                CourseCategoriesDataGrid.ItemsSource = paginatedCategories;
                UpdatePagination();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdatePagination()
        {
            TotalPagesTextBlock.Text = $"{currentPage} из {Math.Max(1, (int)Math.Ceiling((double)totalCategories / PageSize))}";
            PrevPageButton.IsEnabled = currentPage > 1;
            NextPageButton.IsEnabled = currentPage < (int)Math.Ceiling((double)totalCategories / PageSize);
        }

        private void PrevPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                LoadCategories();
            }
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage < (int)Math.Ceiling((double)totalCategories / PageSize))
            {
                currentPage++;
                LoadCategories();
            }
        }

        private void CourseCategoriesWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left) PrevPageButton_Click(sender, e);
            else if (e.Key == Key.Right) NextPageButton_Click(sender, e);
        }

        private void AddCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            var addCategoryWindow = new AddCourseCategoryWindow();
            if (addCategoryWindow.ShowDialog() == true)
            {
                LoadCategories();
                CustomMessageBox.Show("Успех", "Категория успешно добавлена.", isConfirmation: false);
            }
        }

        private void EditCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (CourseCategoriesDataGrid.SelectedItem is Category selectedCategory)
            {
                var editCategoryWindow = new EditCourseCategoryWindow(selectedCategory);
                if (editCategoryWindow.ShowDialog() == true)
                {
                    LoadCategories();
                    CustomMessageBox.Show("Успех", "Категория успешно обновлена.", isConfirmation: false);
                }
            }
            else
            {
                CustomMessageBox.Show("Внимание", "Выберите категорию для редактирования.", isConfirmation: false);
            }
        }

        private void DeleteCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (CourseCategoriesDataGrid.SelectedItem is Category selectedCategory)
            {
                bool result = CustomMessageBox.Show("Подтверждение", $"Вы уверены, что хотите удалить категорию {selectedCategory.CategoryName}? Это удалит все связанные курсы.");
                if (result)
                {
                    try
                    {
                        DatabaseHelper.DeleteCourseCategory(selectedCategory.Id);
                        LoadCategories();
                    }
                    catch (Exception ex)
                    {
                        CustomMessageBox.Show("Ошибка", $"Ошибка удаления: {ex.Message}", isConfirmation: false);
                    }
                }
            }
            else
            {
                CustomMessageBox.Show("Внимание", "Выберите категорию для удаления.", isConfirmation: false);
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            try
            {
                var query = SearchTextBox.Text.Trim().ToLower();
                int? idQuery = null;
                if (int.TryParse(query, out int id)) idQuery = id;

                var filteredCategories = DatabaseHelper.GetCourseCategories()
                    .Where(c =>
                        c.CategoryName.ToLower().Contains(query) ||
                        (idQuery.HasValue && c.Id == idQuery.Value)
                    ).ToList();

                totalCategories = filteredCategories.Count;
                var paginatedCategories = filteredCategories.Skip((currentPage - 1) * PageSize).Take(PageSize).ToList();

                CourseCategoriesDataGrid.ItemsSource = paginatedCategories;
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

        private void UsersButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigateToUsersWindow(AdminId, this);
        }

        private void CategoriesButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigateToCategoryWindow(AdminId, this);
        }

        private void CoursesButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigateToCourseWindow(AdminId, this);
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Logout();
        }

        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigateToSettingWindow(AdminId, this);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }
    }
}