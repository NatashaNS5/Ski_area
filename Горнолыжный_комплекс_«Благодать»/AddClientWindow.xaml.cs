using System;
using System.Windows;
using Горнолыжный_комплекс__Благодать_.Models;
using Microsoft.EntityFrameworkCore;

namespace Горнолыжный_комплекс__Благодать_
{
    public partial class AddClientWindow : Window
    {
        private readonly ApplicationDbContext _context;

        public AddClientWindow(ApplicationDbContext context)
        {
            InitializeComponent();
            _context = context;
        }

        private async void SaveClient_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ClientCodeBox.Text) || string.IsNullOrEmpty(FullNameBox.Text) ||
                string.IsNullOrEmpty(PassportBox.Text) || BirthDatePicker.SelectedDate == null ||
                string.IsNullOrEmpty(AddressBox.Text))
            {
                MessageBox.Show("Заполните все поля.");
                return;
            }

            if (await _context.Clients.AnyAsync(c => c.ClientID == long.Parse(ClientCodeBox.Text) ||
                                                    c.PassportData == PassportBox.Text))
            {
                MessageBox.Show("Клиент с таким кодом, паспортом или email уже существует.");
                return;
            }

            var client = new Client
            {
                ClientID = long.Parse(ClientCodeBox.Text),
                FullName = FullNameBox.Text,
                PassportData = PassportBox.Text,
                BirthDate = BirthDatePicker.SelectedDate.Value,
                Address = AddressBox.Text,
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            MessageBox.Show("Клиент добавлен.");
            Close();
        }
    }
}