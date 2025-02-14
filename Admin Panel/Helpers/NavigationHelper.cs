using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Admin_Panel.Helpers
{
    internal class NavigationHelper
    {
        public static void NavigateToMainWindow(int adminId, Window currentWindow)
        {
            MainWindow mainWindow = new MainWindow(adminId);
            mainWindow.Show();
            currentWindow.Close();
        }

        public static void Logout()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();

                
                foreach (Window window in Application.Current.Windows)
                {
                    if (window != loginWindow)
                    {
                        window.Close();
                    }
                }

                Application.Current.MainWindow = loginWindow;
            });
        }


    }
}
