using System;
using System.Windows;
using Admin_Panel.Helpers;

namespace Admin_Panel
{
    public partial class AddCourseCategoryWindow : Window
    {
        public AddCourseCategoryWindow()
        {
            InitializeComponent();
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
                if (DatabaseHelper.CourseCategoryExists(categoryName))
                {
                    ShowError("Категория с таким названием уже существует");
                    return;
                }

                DatabaseHelper.AddCourseCategory(categoryName);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка при добавлении: {ex.Message}");
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