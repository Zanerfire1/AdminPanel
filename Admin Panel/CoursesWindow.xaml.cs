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
    public partial class CoursesWindow : Window
    {
        private int AdminId;
        private int currentPage = 1;
        private const int PageSize = 12;
        private int totalCourses;

        public CoursesWindow(int adminId)
        {
            InitializeComponent();
            AdminId = adminId;
            LoadData();
            LoadCourses();
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
                this.KeyDown += CoursesWindow_KeyDown;
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

        private void LoadCourses()
        {
            try
            {
                var courses = DatabaseHelper.GetCoursesWithCategories();
                totalCourses = courses.Count;
                var paginatedCourses = courses.Skip((currentPage - 1) * PageSize).Take(PageSize).ToList();

                CoursesDataGrid.ItemsSource = null;
                CoursesDataGrid.Items.Clear();
                CoursesDataGrid.ItemsSource = paginatedCourses;
                UpdatePagination();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки курсов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdatePagination()
        {
            TotalPagesTextBlock.Text = $"{currentPage} из {Math.Max(1, (int)Math.Ceiling((double)totalCourses / PageSize))}";
            PrevPageButton.IsEnabled = currentPage > 1;
            NextPageButton.IsEnabled = currentPage < (int)Math.Ceiling((double)totalCourses / PageSize);
        }

        private void PrevPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                LoadCourses();
            }
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage < (int)Math.Ceiling((double)totalCourses / PageSize))
            {
                currentPage++;
                LoadCourses();
            }
        }

        private void CoursesWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left) PrevPageButton_Click(sender, e);
            else if (e.Key == Key.Right) NextPageButton_Click(sender, e);
        }

        private void AddCourseButton_Click(object sender, RoutedEventArgs e)
        {
            var addCourseWindow = new AddCourseWindow();
            if (addCourseWindow.ShowDialog() == true)
            {
                LoadCourses();
                CustomMessageBox.Show("Успех", "Курс успешно добавлен.", isConfirmation: false);
            }
        }

        private void EditCourseButton_Click(object sender, RoutedEventArgs e)
        {
            if (CoursesDataGrid.SelectedItem is Course selectedCourse)
            {
                var editCourseWindow = new EditCourseWindow(selectedCourse);
                if (editCourseWindow.ShowDialog() == true)
                {
                    LoadCourses();
                    CustomMessageBox.Show("Успех", "Курс успешно обновлён.", isConfirmation: false);
                }
            }
            else
            {
                CustomMessageBox.Show("Внимание", "Выберите курс для редактирования.", isConfirmation: false);
            }
        }

        private void DeleteCourseButton_Click(object sender, RoutedEventArgs e)
        {
            if (CoursesDataGrid.SelectedItem is Course selectedCourse)
            {
                bool result = CustomMessageBox.Show("Подтверждение", $"Вы уверены, что хотите удалить курс {selectedCourse.CourseName}?");
                if (result)
                {
                    try
                    {
                        DatabaseHelper.DeleteCourse(selectedCourse.Id);
                        LoadCourses();
                    }
                    catch (Exception ex)
                    {
                        CustomMessageBox.Show("Ошибка", $"Ошибка удаления: {ex.Message}", isConfirmation: false);
                    }
                }
            }
            else
            {
                CustomMessageBox.Show("Внимание", "Выберите курс для удаления.", isConfirmation: false);
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

                var filteredCourses = DatabaseHelper.GetCoursesWithCategories()
                    .Where(c =>
                        c.CourseName.ToLower().Contains(query) ||
                        c.CategoryName.ToLower().Contains(query) ||
                        (idQuery.HasValue && c.Id == idQuery.Value)
                    ).ToList();

                totalCourses = filteredCourses.Count;
                var paginatedCourses = filteredCourses.Skip((currentPage - 1) * PageSize).Take(PageSize).ToList();

                CoursesDataGrid.ItemsSource = paginatedCourses;
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

        private void UsersButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigateToUsersWindow(AdminId, this);
        }

        private void CategoriesButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigateToCategoryWindow(AdminId, this);
        }

        private void CoursesCategoriesButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigateToCourseCategoriesWindow(AdminId, this);
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