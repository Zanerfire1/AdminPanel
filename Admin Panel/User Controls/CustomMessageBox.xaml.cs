using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Admin_Panel.User_Controls
{
    public partial class CustomMessageBox : UserControl
    {
        private Window parentWindow;
        public bool Result { get; private set; }

        public CustomMessageBox(string title, string message, bool isConfirmation = true)
        {
            InitializeComponent();
            TitleTextBlock.Text = title;
            MessageTextBlock.Text = message;

            // Переключение между режимами: "Да/Нет" или "ОК"
            if (!isConfirmation)
            {
                YesButton.Visibility = Visibility.Collapsed;
                NoButton.Visibility = Visibility.Collapsed;
                OkButton.Visibility = Visibility.Visible;
            }
        }

        // Обработка нажатия "Да"
        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            parentWindow?.Close();
        }

        // Обработка нажатия "Нет"
        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            Result = false;
            parentWindow?.Close();
        }

        // Обработка нажатия "ОК"
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Result = true; // Всегда true при нажатии "ОК"
            parentWindow?.Close();
        }

        // Показать окно
        public static bool Show(string title, string message, bool isConfirmation = true)
        {
            var window = new Window
            {
                Content = new CustomMessageBox(title, message, isConfirmation),
                Width = 400,
                Height = 200,
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                Background = System.Windows.Media.Brushes.Transparent,
                AllowsTransparency = true,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Application.Current.MainWindow
            };

            var box = (CustomMessageBox)window.Content;
            box.parentWindow = window;

            window.ShowDialog();
            return box.Result;
        }
    }
}