using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Admin_Panel.Helpers;
using LiveCharts;
using LiveCharts.Wpf;


namespace Admin_Panel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private int AdminId;
        public ChartValues<int> RegistrationCounts { get; set; }
        public List<string> RegistrationDates { get; set; }

        public MainWindow(int adminId)
        {
            InitializeComponent();
            AdminId = adminId;
            LoadData();
        }


        private void LoadData()
        {
            try
            {
                TotalUsersCard.Number = DatabaseHelper.GetTotalUsers().ToString();

                TotalOrdersCard.Number = DatabaseHelper.GetTotalBalance().ToString("N2");

                NewUsersCard.Number = DatabaseHelper.GetNewUsersLastDay().ToString();

                string timeOfDayGreeting = GetTimeOfDayGreeting();

                LoadNewUsersChart();

                string adminName = DatabaseHelper.GetAdminNameById(AdminId);
                string initials = DatabaseHelper.GetAdminInitials(AdminId);

                AdminGreetingTextBlock.Text = $"{timeOfDayGreeting}, {adminName}";
                UsernameTextBlock.Text = adminName;
                InitialsTextBlock.Text = initials;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
        }

        private string GetTimeOfDayGreeting()
        {
            int hour = DateTime.Now.Hour;

            if (hour >= 5 && hour < 12)
                return "Good Morning";
            else if (hour >= 12 && hour < 18)
                return "Good Afternoon";
            else if (hour >= 18 && hour < 22)
                return "Good Evening";
            else
                return "Good Night";
        }

        private void LoadNewUsersChart()
        {
            try
            {
                Dictionary<string, int> newUsersData = DatabaseHelper.GetNewUsersPerDay();

                // Заполняем массив дат (если какой-то день отсутствует, добавляем 0)
                List<string> labels = new List<string>();
                List<int> values = new List<int>();

                for (int i = 6; i >= 0; i--)
                {
                    string date = DateTime.Now.AddDays(-i).ToString("yyyy-MM-dd");
                    labels.Add(date);
                    values.Add(newUsersData.ContainsKey(date) ? newUsersData[date] : 0);
                }

                // Привязываем данные к графику
                NewUsersChart.AxisX[0].Labels = labels;
                NewUsersChart.Series = new SeriesCollection
        {
            new LineSeries
            {
                Title = "New Users",
                Values = new ChartValues<int>(values),
                Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ada4fd")),
                Fill = new LinearGradientBrush(
                    (Color)ColorConverter.ConvertFromString("#b397e2"),
                    (Color)ColorConverter.ConvertFromString("#6a6ae4"),
                    new Point(0.5, 0), new Point(0.5, 1)),
                PointGeometrySize = 8
            }
        };
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Ошибка загрузки графика: " + ex.Message);
            }
        }

        private void TotalUsersCard_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigateToUsersWindow(AdminId, this);
        }


        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigateToMainWindow(AdminId, this);
        }

        private void UsersButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigateToUsersWindow(AdminId, this);
        }

        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigateToSettingWindow(AdminId, this);
        }



        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Logout();
        }


    }
}
