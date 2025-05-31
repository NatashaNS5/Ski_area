using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Горнолыжный_комплекс__Благодать_.Models;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Font;
using iText.IO.Font.Constants; 
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Горнолыжный_комплекс__Благодать_
{
    public partial class CreateOrderWindow : Window
    {
        private List<Client> _allClients; 
        private List<Service> _allServices; 

        public CreateOrderWindow()
        {
            InitializeComponent();
            Loaded += async (s, e) => await InitializeAsync(); 
        }

        private async Task InitializeAsync()
        {
            try
            {
                await LoadClientsAsync();
                await LoadServicesAsync();
                await SetDefaultOrderNumberAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при инициализации: {ex.Message}\nInner Exception: {ex.InnerException?.Message}");
            }
        }

        private async Task SetDefaultOrderNumberAsync()
        {
            using (var context = new ApplicationDbContext())
            {
                var lastOrder = await context.Orders.OrderByDescending(o => o.OrderID).FirstOrDefaultAsync();
                if (lastOrder != null)
                {
                    if (!string.IsNullOrEmpty(lastOrder.OrderCode) && lastOrder.OrderCode.Contains('/'))
                    {
                        var parts = lastOrder.OrderCode.Split('/');
                        if (parts.Length == 2 && int.TryParse(parts[0], out int orderNumber))
                        {
                            OrderNumberBox.Text = (orderNumber + 1).ToString() + $"/{DateTime.Now:dd.MM.yyyy}";
                            return;
                        }
                    }
                    MessageBox.Show($"Некорректный формат OrderCode у заказа с ID {lastOrder.OrderID}: '{lastOrder.OrderCode}'. Используется значение по умолчанию.");
                }
                OrderNumberBox.Text = $"1/{DateTime.Now:dd.MM.yyyy}";
            }
        }

        private async Task LoadClientsAsync()
        {
            using (var context = new ApplicationDbContext())
            {
                _allClients = await context.Clients.ToListAsync();
                ClientComboBox.ItemsSource = _allClients ?? new List<Client>(); 
            }
        }

        private async Task LoadServicesAsync()
        {
            using (var context = new ApplicationDbContext())
            {
                _allServices = await context.Services.ToListAsync();
                ServicesListBox.ItemsSource = _allServices ?? new List<Service>(); 
            }
        }

        private void ClientSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_allClients == null) return; 

            string searchText = ClientSearchBox.Text.ToLower();
            if (string.IsNullOrWhiteSpace(searchText) || searchText == "поиск клиента...")
            {
                ClientComboBox.ItemsSource = _allClients;
                return;
            }

            var filteredClients = _allClients
                .Where(c =>
                    (c.FullName?.ToLower().Contains(searchText) ?? false) ||
                    (c.PassportData?.ToLower().Contains(searchText) ?? false))
                .ToList();

            ClientComboBox.ItemsSource = filteredClients;
        }

        private void ServiceSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_allServices == null) return;

            string searchText = ServiceSearchBox.Text.ToLower();
            if (string.IsNullOrWhiteSpace(searchText) || searchText == "поиск услуги...")
            {
                ServicesListBox.ItemsSource = _allServices;
                return;
            }

            var filteredServices = _allServices
                .Where(s =>
                    (s.ServiceName?.ToLower().Contains(searchText) ?? false) ||
                    (s.CostPerHour.ToString().Contains(searchText)))
                .ToList();

            ServicesListBox.ItemsSource = filteredServices;
        }

        private void ClientSearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ClientSearchBox.Text == "Поиск клиента...")
            {
                ClientSearchBox.Text = "";
                ClientSearchBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void ClientSearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ClientSearchBox.Text))
            {
                ClientSearchBox.Text = "Поиск клиента...";
                ClientSearchBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void ServiceSearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ServiceSearchBox.Text == "Поиск услуги...")
            {
                ServiceSearchBox.Text = "";
                ServiceSearchBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void ServiceSearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ServiceSearchBox.Text))
            {
                ServiceSearchBox.Text = "Поиск услуги...";
                ServiceSearchBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void OrderNumberBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GenerateBarcode_Click(sender, e);
            }
        }

        private async void GenerateBarcode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string orderNumber = OrderNumberBox.Text;
                using (var context = new ApplicationDbContext())
                {
                    if (string.IsNullOrEmpty(orderNumber) || await context.Orders.AnyAsync(o => o.OrderCode == orderNumber))
                    {
                        MessageBox.Show("Номер заказа некорректен или уже существует.");
                        return;
                    }
                }

                var order = new Order
                {
                    OrderCode = orderNumber,
                    CreationDate = DateTime.Now,
                    CreationTime = DateTime.Now.TimeOfDay,
                    Status = "Новая",
                    RentalDurationMinutes = 120
                };

                string barcodeText = $"{order.OrderCode.Replace("/", "")}{order.CreationDate:yyyyMMddHHmmss}{order.RentalDurationMinutes}{GenerateRandomCode(6)}";
                GenerateBarcodePdf(barcodeText, orderNumber);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при генерации штрих-кода: {ex.Message}\nInner Exception: {ex.InnerException?.Message}");
            }
        }

        private string GenerateRandomCode(int length)
        {
            const string chars = "0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void GenerateBarcodePdf(string barcodeText, string orderNumber)
        {
            try
            {
                using (var stream = new MemoryStream())
                {
                    using (var writer = new PdfWriter(stream))
                    {
                        using (var pdf = new PdfDocument(writer))
                        {
                            var page = pdf.AddNewPage();
                            var canvas = new PdfCanvas(page);
                            float x = 10, y = page.GetPageSize().GetHeight() - 30;
                            float barHeight = 22.85f * 2.83465f;
                            float digitHeight = 2.75f * 2.83465f;
                            float spacing = 0.2f * 2.83465f;
                            float leftMargin = 3.63f * 2.83465f;
                            float rightMargin = 2.31f * 2.83465f;

                            canvas.SetFillColorRgb(0, 0, 0);
                            canvas.Rectangle(x, y - barHeight - 1.65f * 2.83465f, 0.33f * 2.83465f, barHeight + 1.65f * 2.83465f);
                            canvas.Fill();
                            x += leftMargin;

                            foreach (char c in barcodeText)
                            {
                                float width = c == '0' ? 1.35f * 2.83465f : (0.15f * (c - '0')) * 2.83465f;
                                if (c != '0')
                                {
                                    canvas.SetFillColorRgb(0, 0, 0);
                                    canvas.Rectangle(x, y - barHeight, width, barHeight);
                                    canvas.Fill();
                                }
                                canvas.MoveText(x, y - barHeight - digitHeight - 0.165f * 2.83465f);
                                canvas.SetFontAndSize(PdfFontFactory.CreateFont(StandardFonts.HELVETICA), 8); 
                                canvas.ShowText(c.ToString());
                                x += width + spacing;
                            }

                            canvas.SetFillColorRgb(0, 0, 0);
                            canvas.Rectangle(x, y - barHeight - 1.65f * 2.83465f, 0.33f * 2.83465f, barHeight + 1.65f * 2.83465f);
                            canvas.Fill();
                        }
                        File.WriteAllBytes($"Barcode_{orderNumber.Replace("/", "_")}.pdf", stream.ToArray());
                    }
                }
                MessageBox.Show($"Штрих-код сохранен в Barcode_{orderNumber.Replace("/", "_")}.pdf");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при генерации PDF: {ex.Message}\nInner Exception: {ex.InnerException?.Message}");
            }
        }

        private void AddClient_Click(object sender, RoutedEventArgs e)
        {
            var addClientWindow = new AddClientWindow(new ApplicationDbContext());
            addClientWindow.ShowDialog();
            LoadClientsAsync();
        }

        private void AddService_Click(object sender, RoutedEventArgs e)
        {
            if (ServicesListBox.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите услугу из списка.");
                return;
            }
            ServicesListBox.SelectedItems.Add(ServicesListBox.SelectedItem);
            UpdateTotalCost();
        }

        private async void SaveOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    if (ClientComboBox.SelectedItem == null || ServicesListBox.SelectedItems.Count == 0)
                    {
                        MessageBox.Show("Выберите клиента и хотя бы одну услугу.");
                        return;
                    }

                    string orderCode = OrderNumberBox.Text;
                    if (string.IsNullOrEmpty(orderCode) || !orderCode.Contains('/') || orderCode.Split('/').Length != 2 || !int.TryParse(orderCode.Split('/')[0], out _))
                    {
                        MessageBox.Show("Номер заказа должен быть в формате 'число/дата' (например, '1/31.05.2025').");
                        return;
                    }

                    if (await context.Orders.AnyAsync(o => o.OrderCode == orderCode))
                    {
                        MessageBox.Show("Номер заказа уже существует.");
                        return;
                    }

                    var selectedClient = (Client)ClientComboBox.SelectedItem;
                    var selectedServices = ServicesListBox.SelectedItems.Cast<Service>().ToList();
                    var totalCostText = TotalCost.Text;

                    foreach (var service in selectedServices)
                    {
                        if (!await context.Services.AnyAsync(s => s.ServiceID == service.ServiceID))
                        {
                            MessageBox.Show($"Услуга с ID {service.ServiceID} не найдена в базе данных.");
                            return;
                        }
                    }

                    int nextOrderID = 1;
                    var lastOrder = await context.Orders.OrderByDescending(o => o.OrderID).FirstOrDefaultAsync();
                    if (lastOrder != null)
                    {
                        nextOrderID = lastOrder.OrderID + 1;
                    }

                    var order = new Order
                    {
                        OrderID = nextOrderID,
                        OrderCode = orderCode,
                        CreationDate = DateTime.Now,
                        CreationTime = DateTime.Now.TimeOfDay,
                        ClientID = selectedClient.ClientID,
                        Status = "Новая",
                        RentalDurationMinutes = 120,
                        OrderServices = new List<OrderService>()
                    };

                    foreach (var service in selectedServices)
                    {
                        var orderService = new OrderService
                        {
                            OrderID = nextOrderID, 
                            ServiceID = service.ServiceID
                        };
                        order.OrderServices.Add(orderService);
                    }

                    context.Orders.Add(order);
                    await context.SaveChangesAsync();

                    GenerateBarcode_Click(sender, e);

                    using (var pdfStream = new MemoryStream())
                    {
                        using (var writer = new PdfWriter(pdfStream))
                        {
                            using (var pdf = new PdfDocument(writer))
                            {
                                var page = pdf.AddNewPage();
                                var canvas = new PdfCanvas(page);
                                float y = page.GetPageSize().GetHeight() - 30;
                                canvas.MoveText(10, y);
                                canvas.SetFontAndSize(PdfFontFactory.CreateFont(StandardFonts.HELVETICA), 12);
                                canvas.ShowText($"Заказ: {order.OrderCode}");
                                canvas.MoveText(0, -20);
                                canvas.ShowText($"Клиент: {selectedClient.FullName}");
                                canvas.MoveText(0, -20);
                                canvas.ShowText($"Услуги: {string.Join(", ", selectedServices.Select(s => s.ServiceName))}");
                                canvas.MoveText(0, -20);
                                canvas.ShowText($"Стоимость: {totalCostText}");
                            }
                            File.WriteAllBytes($"Order_{order.OrderCode.Replace("/", "_")}.pdf", pdfStream.ToArray());
                        }
                    }

                    string data = $"data=base64({Convert.ToBase64String(Encoding.UTF8.GetBytes($"дата_заказа={order.CreationDate:yyyy-MM-dd}T{order.CreationTime}&номер_заказа={order.OrderCode}"))}";
                    File.WriteAllText($"Order_{order.OrderCode.Replace("/", "_")}.txt", $"https://wsrussia.ru/?{data}");

                    MessageBox.Show("Заказ успешно создан, штрих-код и документы сгенерированы.");
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении заказа: {ex.Message}\nInner Exception: {ex.InnerException?.Message}");
            }
        }

        private void UpdateTotalCost()
        {
            var selectedServices = ServicesListBox.SelectedItems.Cast<Service>().ToList();
            var total = selectedServices.Sum(s => s.CostPerHour * 120 / 60);
            TotalCost.Text = $"Итоговая стоимость: {total:C}";
        }
    }
}