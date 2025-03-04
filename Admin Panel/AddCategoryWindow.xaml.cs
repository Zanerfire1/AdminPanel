using System;
using System.Windows;
using Admin_Panel.Helpers;

namespace Admin_Panel
{
    public partial class AddCategoryWindow : Window
    {
        public AddCategoryWindow()
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
                // Проверка на существование категории с таким же именем
                if (DatabaseHelper.CategoryExists(categoryName))
                {
                    ShowError("Категория с таким названием уже существует");
                    return;
                }

                DatabaseHelper.AddCategory(categoryName);
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