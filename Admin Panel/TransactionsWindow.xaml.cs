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
using System.Windows.Shapes;
using Admin_Panel.Helpers;


namespace Admin_Panel
{
    /// <summary>
    /// Логика взаимодействия для TransactionsWindow.xaml
    /// </summary>
    public partial class TransactionsWindow : Window
    {
        public TransactionsWindow()
        {
            InitializeComponent();
            LoadTransactionData();
        }

        private void LoadTransactionData()
        {
            try
            {
                // Загрузка списка операций
                OperationsGrid.ItemsSource = DatabaseHelper.GetAllTransactions().DefaultView;

                // Загрузка остатка средств
                BalanceGrid.ItemsSource = DatabaseHelper.GetAccountBalances().DefaultView;

                // Загрузка топ счетов по транзакциям
                TopAccountsGrid.ItemsSource = DatabaseHelper.GetTopAccountsByTransactions().DefaultView;

                // Получение общего оборота за месяц
                decimal totalTurnover = DatabaseHelper.GetMonthlyTurnover();
                TotalTurnoverText.Text = $"Оборот: {totalTurnover}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
