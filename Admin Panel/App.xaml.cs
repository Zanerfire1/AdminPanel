using System;
using System.Windows;

namespace Admin_Panel
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            LoginWindow loginWindow = new LoginWindow();
            if (loginWindow.ShowDialog() == true) 
            {
                int userId = loginWindow.AuthenticatedUserId; 
                MainWindow mainWindow = new MainWindow(userId);
                MainWindow = mainWindow; 
                mainWindow.Show();
            }
        }
    }
}
