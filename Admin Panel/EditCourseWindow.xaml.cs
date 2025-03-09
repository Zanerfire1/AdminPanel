using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Admin_Panel.Helpers;

namespace Admin_Panel
{
    public partial class EditCourseWindow : Window
    {
        private readonly Course _course;

        public EditCourseWindow(Course course)
        {
            InitializeComponent();
            _course = course;
            CourseNameTextBox.Text = course.CourseName;
            DescriptionTextBox.Text = course.Description;
            IsPaidCheckBox.IsChecked = course.IsPaid;
            VideoUrlTextBox.Text = course.VideoUrl;
            VideoUrlBorder.Visibility = course.IsPaid ? Visibility.Visible : Visibility.Collapsed;

            var categories = DatabaseHelper.GetCourseCategories();
            CategoryComboBox.ItemsSource = categories;
            CategoryComboBox.SelectedItem = categories.FirstOrDefault(c => c.Id == course.CategoryId);
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
                _course.CourseName = courseName;
                _course.Description = description;
                _course.CategoryId = selectedCategory.Id;
                _course.IsPaid = isPaid;
                _course.VideoUrl = videoUrl;

                DatabaseHelper.UpdateCourse(_course);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка при обновлении: {ex.Message}");
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

                    if (selectedText.Contains("**"))
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