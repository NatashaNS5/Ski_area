using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Горнолыжный_комплекс__Благодать_.Models;

namespace Горнолыжный_комплекс__Благодать_
{
    public partial class DashboardWindow : Window
    {
        private readonly Employee _employee;
        private readonly DispatcherTimer _sessionTimer;
        private DateTime _sessionStart;
        private readonly TimeSpan _sessionDuration = TimeSpan.FromMinutes(10);
        private readonly TimeSpan _warningTime = TimeSpan.FromMinutes(5);

        public DashboardWindow(Employee employee)
        {
            InitializeComponent();
            _employee = employee;
            UserInfo.Text = $"{_employee.FullName}, {_employee.Position}";
            CreateOrderButton.Visibility = _employee.Position == "Продавец" || _employee.Position == "Старший смены" ? Visibility.Visible : Visibility.Collapsed;
            ReturnEquipmentButton.Visibility = _employee.Position == "Старший смены" ? Visibility.Visible : Visibility.Collapsed;
            ViewHistoryButton.Visibility = _employee.Position == "Администратор" ? Visibility.Visible : Visibility.Collapsed;
            ManageConsumablesButton.Visibility = _employee.Position == "Администратор" ? Visibility.Visible : Visibility.Collapsed;

            _sessionStart = DateTime.Now;
            _sessionTimer = new DispatcherTimer();
            _sessionTimer.Interval = TimeSpan.FromSeconds(1);
            _sessionTimer.Tick += UpdateSessionTimer;
            _sessionTimer.Start();
        }

        private void UpdateSessionTimer(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                var timeLeft = _sessionStart.Add(_sessionDuration) - DateTime.Now;
                if (timeLeft <= TimeSpan.Zero)
                {
                    _sessionTimer.Stop();
                    MessageBox.Show("Сеанс завершен. Вход заблокирован на 3 минуты.");
                    var loginWindow = new LoginWindow();
                    loginWindow.IsEnabled = false;
                    loginWindow.Show();
                    Close();
                    Task.Delay(180000).ContinueWith(_ => Application.Current.Dispatcher.Invoke(() => loginWindow.IsEnabled = true));
                }
                else if (timeLeft <= _warningTime && timeLeft > TimeSpan.FromMinutes(4.9))
                {
                    MessageBox.Show($"Сеанс завершится через {timeLeft:mm\\:ss}.");
                }
                SessionTimer.Text = $"Время сессии: {timeLeft:mm\\:ss}";
            });
        }

        private void CreateOrder_Click(object sender, RoutedEventArgs e)
        {
            var orderWindow = new CreateOrderWindow();
            orderWindow.ShowDialog();
        }

        private void ReturnEquipment_Click(object sender, RoutedEventArgs e)
        {
            var returnWindow = new ReturnEquipmentWindow();
            returnWindow.ShowDialog();
        }

        private void ViewHistory_Click(object sender, RoutedEventArgs e)
        {
            var historyWindow = new LoginHistoryWindow();
            historyWindow.ShowDialog();
        }

        private void ManageConsumables_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Управление расходными материалами пока не реализовано.");
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            _sessionTimer.Stop();
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }
    }
}