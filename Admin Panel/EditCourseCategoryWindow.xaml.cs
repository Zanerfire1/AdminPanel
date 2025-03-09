using System;
using System.Windows;
using Admin_Panel.Helpers;

namespace Admin_Panel
{
    public partial class EditCourseCategoryWindow : Window
    {
        private readonly Category _category;

        public EditCourseCategoryWindow(Category category)
        {
            InitializeComponent();
            _category = category;
            CategoryNameTextBox.Text = category.CategoryName;
        }

        private void SaveCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            string categoryName = CategoryNameTextBox.Text.Trim();
            ErrorTextBlock.Visibility = Visibility.Collapsed;

            if (string.IsNullOrEmpty(categoryName))
            {
                ShowError("Название категории обязательно");
                return;
            }

            try
            {
                if (DatabaseHelper.CourseCategoryExists(categoryName, _category.Id))
                {
                    ShowError("Категория с таким названием уже существует");
                    return;
                }

                _category.CategoryName = categoryName;
                DatabaseHelper.UpdateCourseCategory(_category);
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
    }
}