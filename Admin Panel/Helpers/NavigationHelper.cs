using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

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
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();

                
                foreach (Window window in System.Windows.Application.Current.Windows)
                {
                    if (window != loginWindow)
                    {
                        window.Close();
                    }
                }

                System.Windows.Application.Current.MainWindow = loginWindow;
            });
        }

        public static void NavigateToUsersWindow(int adminId, Window currentWindow)
        {
            UsersWindow usersWindow = new UsersWindow(adminId);
            usersWindow.Show();
            currentWindow.Close();
        }







    }
}
