using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using Горнолыжный_комплекс__Благодать_.Models;

namespace Горнолыжный_комплекс__Благодать_
{
    public partial class LoginHistoryWindow : Window
    {
        private readonly ApplicationDbContext _context;

        public LoginHistoryWindow()
        {
            InitializeComponent();
            _context = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().Options);
            LoadHistory();
        }

        private async void LoadHistory(string filter = "")
        {
            var history = await _context.LoginHistories
                .Include(h => h.Employee)
                .Where(h => string.IsNullOrEmpty(filter) || h.Employee.Login.Contains(filter))
                .OrderByDescending(h => h.LoginTime)
                .ToListAsync();
            HistoryListView.ItemsSource = history;
        }

        private void FilterLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadHistory(LoginFilterBox.Text);
        }
    }
}