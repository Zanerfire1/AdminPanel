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

        public static void NavigateToSettingWindow(int adminId, Window currentWindow)
        {
            Admin admin = DatabaseHelper.GetAdminById(adminId);
            SettingWindow settingWindow = new SettingWindow(admin);
            settingWindow.Show();
            currentWindow.Close();
        }

        public static void NavigateToCategoryWindow(int adminId, Window currentWindow)
        {
            CategoriesWindow categoryWindow = new CategoriesWindow(adminId);
            categoryWindow.Show();
            currentWindow.Close();
        }

        public static void NavigateToCourseWindow(int adminId, Window currentWindow)
        {
            CoursesWindow coursesWindow = new CoursesWindow(adminId);
            coursesWindow.Show();
            currentWindow.Close();
        }

        public static void NavigateToCourseCategoriesWindow(int adminId, Window currentWindow)
        {
            CourseCategoriesWindow courseCategoriesWindow = new CourseCategoriesWindow(adminId);
            courseCategoriesWindow.Show();
            currentWindow.Close();
        }
    }
}
