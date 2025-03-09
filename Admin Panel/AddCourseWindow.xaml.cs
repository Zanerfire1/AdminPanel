using System;
using System.Windows;
using Admin_Panel.Helpers;

namespace Admin_Panel
{
    public partial class AddCourseWindow : Window
    {
        public AddCourseWindow()
        {
            InitializeComponent();
            CategoryComboBox.ItemsSource = DatabaseHelper.GetCourseCategories();
        }

        private void IsPaidCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            VideoUrlBorder.Visibility = Visibility.Visible;
        }

        private void IsPaidCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            VideoUrlBorder.Visibility = Visibility.Collapsed;
            VideoUrlTextBox.Text = string.Empty;
        }

        private void SaveCourseButton_Click(object sender, RoutedEventArgs e)
        {
            string courseName = CourseNameTextBox.Text.Trim();
            string description = DescriptionTextBox.Text.Trim();
            var selectedCategory = CategoryComboBox.SelectedItem as Category;
            bool isPaid = IsPaidCheckBox.IsChecked ?? false;
            string videoUrl = isPaid ? VideoUrlTextBox.Text.Trim() : null;

            ErrorTextBlock.Visibility = Visibility.Collapsed;

            if (string.IsNullOrEmpty(courseName) || selectedCategory == null)
            {
                ShowError("Название курса и категория обязательны");
                return;
            }

            if (isPaid && string.IsNullOrEmpty(videoUrl))
            {
                ShowError("Для платного курса требуется ссылка на видео");
                return;
            }

            try
            {
                DatabaseHelper.AddCourse(courseName, description, videoUrl, selectedCategory.Id, isPaid);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка при добавлении: {ex.Message}");
            }
        }

        private void BoldButton_Click(object sender, RoutedEventArgs e)
        {
            if (DescriptionTextBox.SelectedText.Length > 0)
            {
                int selectionStart = DescriptionTextBox.SelectionStart;
                string selectedText = DescriptionTextBox.SelectedText;


                if (selectedText.StartsWith("*") && selectedText.EndsWith("*") && selectedText.Length > 4)
                {

                    string newText = selectedText.Substring(2, selectedText.Length - 4);
                    DescriptionTextBox.Text = DescriptionTextBox.Text.Remove(selectionStart, DescriptionTextBox.SelectionLength).Insert(selectionStart, newText);
                    DescriptionTextBox.SelectionStart = selectionStart;
                    DescriptionTextBox.SelectionLength = newText.Length;
                }
                else
                {

                    if (selectedText.Contains("*"))
                    {

                        string newText = selectedText.Replace("*", "");
                        DescriptionTextBox.Text = DescriptionTextBox.Text.Remove(selectionStart, DescriptionTextBox.SelectionLength).Insert(selectionStart, newText);
                        DescriptionTextBox.SelectionStart = selectionStart;
                        DescriptionTextBox.SelectionLength = newText.Length;
                    }
                    else
                    {

                        string newText = $"*{selectedText}*";
                        DescriptionTextBox.Text = DescriptionTextBox.Text.Remove(selectionStart, DescriptionTextBox.SelectionLength).Insert(selectionStart, newText);
                        DescriptionTextBox.SelectionStart = selectionStart;
                        DescriptionTextBox.SelectionLength = newText.Length;
                    }
                }
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