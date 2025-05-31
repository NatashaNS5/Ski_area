using System;
using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Горнолыжный_комплекс__Благодать_.Models;

namespace Горнолыжный_комплекс__Благодать_
{
    public partial class ReturnEquipmentWindow : Window
    {
        private readonly ApplicationDbContext _context;

        public ReturnEquipmentWindow()
        {
            InitializeComponent();
            _context = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().Options);
        }

        private async void ReturnEquipment_Click(object sender, RoutedEventArgs e)
        {
            string barcode = BarcodeBox.Text;
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderCode == barcode);

            if (order == null)
            {
                StatusMessage.Text = "Заказ не найден.";
                return;
            }

            order.Status = "Закрыта";
            order.ClosingDate = DateTime.Now;
            await _context.SaveChangesAsync();
            StatusMessage.Text = $"Заказ {barcode} закрыт.";
        }
    }
}