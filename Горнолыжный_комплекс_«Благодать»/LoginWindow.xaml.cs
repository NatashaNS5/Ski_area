using Microsoft.EntityFrameworkCore;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Горнолыжный_комплекс__Благодать_.Models;

namespace Горнолыжный_комплекс__Благодать_
{
    public partial class LoginWindow : Window
    {
        private readonly ApplicationDbContext _context;
        private int _loginAttempts = 0;
        private string _captchaText;
        private DateTime? _blockTime;
        private Control _currentPasswordControl;

        public LoginWindow()
        {
            InitializeComponent();
            _context = new ApplicationDbContext();
            _currentPasswordControl = PasswordBox;
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_blockTime.HasValue && DateTime.Now < _blockTime.Value.AddSeconds(10))
                {
                    ErrorMessage.Text = $"Вход заблокирован до {_blockTime.Value.AddSeconds(10):HH:mm:ss}";
                    ErrorMessage.Visibility = Visibility.Visible;
                    return;
                }

                string login = LoginBox.Text;
                string password = _currentPasswordControl is PasswordBox passwordBox ? passwordBox.Password : ((TextBox)_currentPasswordControl).Text;

                try
                {
                    await _context.Employees.CountAsync(); 
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при тестовом запросе: {ex.Message}\nInner Exception: {ex.InnerException?.Message}");
                    return;
                }

                var employee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.Login == login && e.Password == password);

                await LogLoginAttempt(employee?.EmployeeID ?? 0, employee != null);

                if (_loginAttempts >= 2)
                {
                    if (string.IsNullOrEmpty(CaptchaBox.Text))
                    {
                        ErrorMessage.Text = "Введите код с картинки";
                        ErrorMessage.Visibility = Visibility.Visible;
                        return;
                    }

                    if (CaptchaBox.Text != _captchaText)
                    {
                        ErrorMessage.Text = "Неверная капча";
                        ErrorMessage.Visibility = Visibility.Visible;
                        _blockTime = DateTime.Now;
                        await Task.Delay(10000);
                        RegenerateCaptcha();
                        return;
                    }
                }

                if (employee == null)
                {
                    _loginAttempts++;
                    if (_loginAttempts >= 2)
                    {
                        ErrorMessage.Text = "Логин или пароль неверный, введите код с картинки";
                        CaptchaLabel.Visibility = Visibility.Visible;
                        CaptchaImage.Visibility = Visibility.Visible;
                        CaptchaBox.Visibility = Visibility.Visible;
                        RegenerateCaptchaButton.Visibility = Visibility.Visible;
                        RegenerateCaptcha();
                    }
                    else
                    {
                        ErrorMessage.Text = "Неверный логин или пароль";
                    }
                    ErrorMessage.Visibility = Visibility.Visible;
                    return;
                }

                _loginAttempts = 0;
                CaptchaLabel.Visibility = Visibility.Collapsed;
                CaptchaImage.Visibility = Visibility.Collapsed;
                CaptchaBox.Visibility = Visibility.Collapsed;
                RegenerateCaptchaButton.Visibility = Visibility.Collapsed;
                ErrorMessage.Visibility = Visibility.Collapsed;

                var dashboard = new DashboardWindow(employee);
                dashboard.Show();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при авторизации: {ex.Message}\nInner Exception: {ex.InnerException?.Message}");
            }
        }

        private void ShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            if (_currentPasswordControl == null) return;

            var parent = _currentPasswordControl.Parent as Panel;
            if (parent == null) return;

            var textBox = new TextBox
            {
                Text = _currentPasswordControl is PasswordBox pb ? pb.Password : ((TextBox)_currentPasswordControl).Text,
                Margin = _currentPasswordControl.Margin
            };
            int index = parent.Children.IndexOf(_currentPasswordControl);
            parent.Children.Remove(_currentPasswordControl);
            parent.Children.Insert(index, textBox);
            _currentPasswordControl = textBox;
            textBox.Focus();
        }

        private void ShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_currentPasswordControl == null || !(_currentPasswordControl is TextBox)) return;

            var parent = _currentPasswordControl.Parent as Panel;
            if (parent == null) return;

            var textBox = (TextBox)_currentPasswordControl;
            var passwordBox = new PasswordBox
            {
                Password = textBox.Text,
                Margin = textBox.Margin
            };
            int index = parent.Children.IndexOf(textBox);
            parent.Children.Remove(textBox);
            parent.Children.Insert(index, passwordBox);
            _currentPasswordControl = passwordBox;
            passwordBox.Focus();
        }

        private void RegenerateCaptcha_Click(object sender, RoutedEventArgs e)
        {
            RegenerateCaptcha();
        }

        private void RegenerateCaptcha()
        {
            _captchaText = GenerateCaptchaText(3);
            using (var bitmap = GenerateCaptchaImage(_captchaText))
            {
                using (var stream = new MemoryStream())
                {
                    bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    stream.Position = 0;
                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = stream;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    CaptchaImage.Source = bitmapImage;
                }
            }
        }

        private string GenerateCaptchaText(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private Bitmap GenerateCaptchaImage(string text)
        {
            var bitmap = new Bitmap(150, 50);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.White);
                using (var font = new Font("Arial", 20))
                {
                    for (int i = 0; i < text.Length; i++)
                    {
                        graphics.DrawString(text[i].ToString(), font, Brushes.Black,
                            new PointF(30 + i * 30 + new Random().Next(-10, 10), 20 + new Random().Next(-10, 10)));
                    }
                    for (int i = 0; i < 100; i++)
                    {
                        int x = new Random().Next(bitmap.Width);
                        int y = new Random().Next(bitmap.Height);
                        bitmap.SetPixel(x, y, Color.Gray);
                    }
                }
            }
            return bitmap;
        }

        private async Task LogLoginAttempt(int employeeId, bool success)
        {
            try
            {
                if (employeeId == 0)
                    return;

                var history = new LoginHistory
                {
                    EmployeeID = employeeId,
                    LoginTime = DateTime.Now,
                    LoginStatus = success ? "Успешно" : "Неуспешно"
                };
                _context.LoginHistories.Add(history);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при записи истории входа: {ex.Message}\nInner Exception: {ex.InnerException?.Message}");
            }
        }
    }
}