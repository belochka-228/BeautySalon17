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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BeautySalon17.Pages
{
    /// <summary>
    /// Логика взаимодействия для ManagerPage.xaml
    /// </summary>
    public partial class ManagerPage : Page
    {
        // Для временного хранения найденного клиента при создании записи
        private Users _selectedClient;

        public ManagerPage()
        {
            InitializeComponent();
            this.Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAppointments();
            LoadOrders();
            LoadProducts();
            LoadManufacturers();
            LoadProductTypes();
            LoadServices();
        }

        // ==================== ЗАПИСИ ====================
        private void LoadAppointments()
        {
            using (var context = new BeautySalonEntities())
            {
                var list = context.Appointments.Include("Services").Include("Users").Include("Users1").ToList();
                DgAppointments.ItemsSource = list;
            }
        }

        // Поиск клиента
        private void BtnSearchClient_Click(object sender, RoutedEventArgs e)
        {
            string query = TxtClientSearch.Text.Trim();
            if (string.IsNullOrEmpty(query))
            {
                MessageBox.Show("Введите ФИО или номер телефона для поиска.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var context = new BeautySalonEntities())
            {
                var clients = context.Users
                    .Where(u => u.RoleId == 1) // только клиенты
                    .AsEnumerable()
                    .Where(u => (u.Surname + " " + u.Name + " " + u.Patronymic).IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0
                             || u.Phone.Contains(query))
                    .ToList();

                if (clients.Count == 1)
                {
                    _selectedClient = clients[0];
                    MessageBox.Show($"Выбран клиент: {_selectedClient.Surname} {_selectedClient.Name} {_selectedClient.Patronymic}\nТелефон: {_selectedClient.Phone}",
                                    "Клиент найден", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (clients.Count > 1)
                {
                    // Для простоты выберем первого
                    _selectedClient = clients[0];
                    MessageBox.Show($"Найдено несколько клиентов. Выбран первый:\n{_selectedClient.Surname} {_selectedClient.Name}\nТелефон: {_selectedClient.Phone}",
                                    "Клиент найден", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _selectedClient = null;
                    MessageBox.Show("Клиент не найден.", "Поиск", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void BtnClearClientSearch_Click(object sender, RoutedEventArgs e)
        {
            TxtClientSearch.Text = "";
            _selectedClient = null;
        }

        // Создание записи менеджером (упрощённо – через InputBox)
        private void BtnAddAppointment_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedClient == null)
            {
                MessageBox.Show("Сначала найдите и выберите клиента.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Выбор услуги
            using (var context = new BeautySalonEntities())
            {
                var services = context.Services.ToList();
                if (services.Count == 0)
                {
                    MessageBox.Show("Нет доступных услуг.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Простой выбор услуги – через строку с ID
                string serviceList = string.Join("\n", services.Select(s => $"{s.Id} - {s.Name}"));
                string input = Microsoft.VisualBasic.Interaction.InputBox($"Введите ID услуги:\n{serviceList}", "Выбор услуги", "");
                if (!int.TryParse(input, out int serviceId))
                {
                    MessageBox.Show("Неверный ID услуги.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var service = context.Services.Find(serviceId);
                if (service == null)
                {
                    MessageBox.Show("Услуга не найдена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Выбор мастера, который оказывает эту услугу
                var masterIds = context.MasterServices.Where(ms => ms.ServiceId == serviceId).Select(ms => ms.MasterId).ToList();
                var masters = context.Users.Where(u => masterIds.Contains(u.Id) && u.RoleId == 2).ToList();
                if (masters.Count == 0)
                {
                    MessageBox.Show("Нет мастеров, оказывающих эту услугу.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string masterList = string.Join("\n", masters.Select(m => $"{m.Id} - {m.Surname} {m.Name}"));
                input = Microsoft.VisualBasic.Interaction.InputBox($"Введите ID мастера:\n{masterList}", "Выбор мастера", "");
                if (!int.TryParse(input, out int masterId))
                {
                    MessageBox.Show("Неверный ID мастера.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var master = context.Users.Find(masterId);
                if (master == null || master.RoleId != 2)
                {
                    MessageBox.Show("Мастер не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Выбор даты и времени (упрощённо)
                string dateStr = Microsoft.VisualBasic.Interaction.InputBox("Введите дату (ДД.ММ.ГГГГ):", "Дата", DateTime.Today.ToShortDateString());
                if (!DateTime.TryParse(dateStr, out DateTime date))
                {
                    MessageBox.Show("Неверный формат даты.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string timeStr = Microsoft.VisualBasic.Interaction.InputBox("Введите время (ЧЧ:ММ):", "Время", "10:00");
                if (!TimeSpan.TryParse(timeStr, out TimeSpan time))
                {
                    MessageBox.Show("Неверный формат времени.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                DateTime appointmentDateTime = date.Date.Add(time);

                // Создаём запись
                var newAppointment = new Appointments
                {
                    ClientId = _selectedClient.Id,
                    MasterId = masterId,
                    ServiceId = serviceId,
                    AppointmentDateTime = appointmentDateTime,
                    Status = "Pending",
                    PaymentMethod = "Наличные" // по умолчанию
                };

                context.Appointments.Add(newAppointment);
                context.SaveChanges();

                MessageBox.Show("Запись успешно создана!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadAppointments();
                _selectedClient = null;
                TxtClientSearch.Text = "";
            }
        }

        private void BtnCancelAppointment_Click(object sender, RoutedEventArgs e)
        {
            if (DgAppointments.SelectedItem is Appointments app)
            {
                if (app.Status == "Completed")
                {
                    MessageBox.Show("Выполненную запись нельзя отменить.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                if (MessageBox.Show("Отменить эту запись?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    using (var context = new BeautySalonEntities())
                    {
                        var dbApp = context.Appointments.Find(app.Id);
                        if (dbApp != null) dbApp.Status = "Cancelled";
                        context.SaveChanges();
                    }
                    LoadAppointments();
                }
            }
            else MessageBox.Show("Выберите запись.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void BtnMoveAppointment_Click(object sender, RoutedEventArgs e)
        {
            if (DgAppointments.SelectedItem is Appointments app)
            {
                if (app.Status == "Completed" || app.Status == "Cancelled")
                {
                    MessageBox.Show("Эту запись нельзя перенести.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                string dateStr = Microsoft.VisualBasic.Interaction.InputBox("Введите новую дату (ДД.ММ.ГГГГ):", "Перенос", app.AppointmentDateTime.ToShortDateString());
                if (!DateTime.TryParse(dateStr, out DateTime newDate)) return;

                string timeStr = Microsoft.VisualBasic.Interaction.InputBox("Введите новое время (ЧЧ:ММ):", "Перенос", app.AppointmentDateTime.ToString("HH:mm"));
                if (!TimeSpan.TryParse(timeStr, out TimeSpan newTime)) return;

                DateTime newDateTime = newDate.Date.Add(newTime);

                using (var context = new BeautySalonEntities())
                {
                    var dbApp = context.Appointments.Find(app.Id);
                    if (dbApp != null) dbApp.AppointmentDateTime = newDateTime;
                    context.SaveChanges();
                }
                LoadAppointments();
                MessageBox.Show("Запись перенесена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else MessageBox.Show("Выберите запись.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        // ==================== ЗАКАЗЫ ====================
        private void LoadOrders()
        {
            using (var context = new BeautySalonEntities())
            {
                var list = context.Orders.Include("Users").ToList();
                DgOrders.ItemsSource = list;
            }
        }

        private void BtnCompleteOrder_Click(object sender, RoutedEventArgs e)
        {
            if (DgOrders.SelectedItem is Orders order)
            {
                if (order.Status == "Completed")
                {
                    MessageBox.Show("Заказ уже выдан.");
                    return;
                }
                using (var context = new BeautySalonEntities())
                {
                    var dbOrder = context.Orders.Find(order.Id);
                    if (dbOrder != null) dbOrder.Status = "Completed";
                    context.SaveChanges();
                }
                LoadOrders();
                MessageBox.Show("Заказ отмечен как выданный.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else MessageBox.Show("Выберите заказ.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        // ==================== ТОВАРЫ ====================
        private void LoadProducts()
        {
            using (var context = new BeautySalonEntities())
            {
                DgProducts.ItemsSource = context.Products.ToList();
            }
        }

        private void BtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            // Простое добавление через InputBox
            string name = Microsoft.VisualBasic.Interaction.InputBox("Название товара:", "Добавить", "");
            if (string.IsNullOrWhiteSpace(name)) return;
            string priceStr = Microsoft.VisualBasic.Interaction.InputBox("Цена:", "Добавить", "100");
            if (!decimal.TryParse(priceStr, out decimal price)) return;
            string discountStr = Microsoft.VisualBasic.Interaction.InputBox("Скидка %:", "Добавить", "0");
            if (!int.TryParse(discountStr, out int discount)) return;

            using (var context = new BeautySalonEntities())
            {
                // Для простоты – ManufacturerId = 1, ProductTypeId = 1, IsActive = true
                var product = new Products
                {
                    Name = name,
                    Price = price,
                    Discount = discount,
                    ManufacturerId = 1,
                    ProductTypeId = 1,
                    IsActive = true,
                    Rating = 0,
                    ImagePath = "Images\\placeholder.jpg"
                };
                context.Products.Add(product);
                context.SaveChanges();
            }
            LoadProducts();
        }

        private void BtnEditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (DgProducts.SelectedItem is Products prod)
            {
                string name = Microsoft.VisualBasic.Interaction.InputBox("Новое название:", "Изменить", prod.Name);
                if (string.IsNullOrWhiteSpace(name)) return;
                string priceStr = Microsoft.VisualBasic.Interaction.InputBox("Новая цена:", "Изменить", prod.Price.ToString());
                if (!decimal.TryParse(priceStr, out decimal price)) return;

                using (var context = new BeautySalonEntities())
                {
                    var p = context.Products.Find(prod.Id);
                    if (p != null)
                    {
                        p.Name = name;
                        p.Price = price;
                        context.SaveChanges();
                    }
                }
                LoadProducts();
            }
            else MessageBox.Show("Выберите товар.");
        }

        private void BtnFreezeProduct_Click(object sender, RoutedEventArgs e)
        {
            SetProductActive(false);
        }

        private void BtnUnfreezeProduct_Click(object sender, RoutedEventArgs e)
        {
            SetProductActive(true);
        }

        private void SetProductActive(bool active)
        {
            if (DgProducts.SelectedItem is Products prod)
            {
                using (var context = new BeautySalonEntities())
                {
                    var p = context.Products.Find(prod.Id);
                    if (p != null) p.IsActive = active;
                    context.SaveChanges();
                }
                LoadProducts();
            }
            else MessageBox.Show("Выберите товар.");
        }

        private void BtnDiscountProduct_Click(object sender, RoutedEventArgs e)
        {
            if (DgProducts.SelectedItem is Products prod)
            {
                string input = Microsoft.VisualBasic.Interaction.InputBox("Введите новую скидку (%):", "Скидка", prod.Discount.ToString());
                if (int.TryParse(input, out int newDiscount))
                {
                    using (var context = new BeautySalonEntities())
                    {
                        var p = context.Products.Find(prod.Id);
                        if (p != null) p.Discount = newDiscount;
                        context.SaveChanges();
                    }
                    LoadProducts();
                }
            }
            else MessageBox.Show("Выберите товар.");
        }

        // ==================== ПРОИЗВОДИТЕЛИ ====================
        private void LoadManufacturers()
        {
            using (var context = new BeautySalonEntities())
            {
                DgManufacturers.ItemsSource = context.Manufacturers.ToList();
            }
        }

        private void BtnAddManufacturer_Click(object sender, RoutedEventArgs e)
        {
            string name = Microsoft.VisualBasic.Interaction.InputBox("Название производителя:", "Добавить", "");
            if (!string.IsNullOrWhiteSpace(name))
            {
                using (var context = new BeautySalonEntities())
                {
                    context.Manufacturers.Add(new Manufacturers { Name = name });
                    context.SaveChanges();
                }
                LoadManufacturers();
            }
        }

        private void BtnEditManufacturer_Click(object sender, RoutedEventArgs e)
        {
            if (DgManufacturers.SelectedItem is Manufacturers m)
            {
                string name = Microsoft.VisualBasic.Interaction.InputBox("Новое название:", "Изменить", m.Name);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    using (var context = new BeautySalonEntities())
                    {
                        var man = context.Manufacturers.Find(m.Id);
                        if (man != null) man.Name = name;
                        context.SaveChanges();
                    }
                    LoadManufacturers();
                }
            }
            else MessageBox.Show("Выберите производителя.");
        }

        // ==================== ТИПЫ ТОВАРОВ ====================
        private void LoadProductTypes()
        {
            using (var context = new BeautySalonEntities())
            {
                DgProductTypes.ItemsSource = context.ProductTypes.ToList();
            }
        }

        private void BtnAddProductType_Click(object sender, RoutedEventArgs e)
        {
            string name = Microsoft.VisualBasic.Interaction.InputBox("Название типа товара:", "Добавить", "");
            if (!string.IsNullOrWhiteSpace(name))
            {
                using (var context = new BeautySalonEntities())
                {
                    context.ProductTypes.Add(new ProductTypes { Name = name });
                    context.SaveChanges();
                }
                LoadProductTypes();
            }
        }

        private void BtnEditProductType_Click(object sender, RoutedEventArgs e)
        {
            if (DgProductTypes.SelectedItem is ProductTypes pt)
            {
                string name = Microsoft.VisualBasic.Interaction.InputBox("Новое название:", "Изменить", pt.Name);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    using (var context = new BeautySalonEntities())
                    {
                        var item = context.ProductTypes.Find(pt.Id);
                        if (item != null) item.Name = name;
                        context.SaveChanges();
                    }
                    LoadProductTypes();
                }
            }
            else MessageBox.Show("Выберите тип товара.");
        }

        // ==================== УСЛУГИ ====================
        private void LoadServices()
        {
            using (var context = new BeautySalonEntities())
            {
                DgServices.ItemsSource = context.Services.ToList();
            }
        }

        private void BtnAddService_Click(object sender, RoutedEventArgs e)
        {
            string name = Microsoft.VisualBasic.Interaction.InputBox("Название услуги:", "Добавить", "");
            if (string.IsNullOrWhiteSpace(name)) return;
            string durStr = Microsoft.VisualBasic.Interaction.InputBox("Длительность (мин):", "Добавить", "60");
            if (!int.TryParse(durStr, out int dur)) return;
            string priceStr = Microsoft.VisualBasic.Interaction.InputBox("Цена:", "Добавить", "1000");
            if (!decimal.TryParse(priceStr, out decimal price)) return;

            using (var context = new BeautySalonEntities())
            {
                context.Services.Add(new Services { Name = name, Duration = dur, Price = price });
                context.SaveChanges();
            }
            LoadServices();
        }

        private void BtnEditService_Click(object sender, RoutedEventArgs e)
        {
            if (DgServices.SelectedItem is Services srv)
            {
                string name = Microsoft.VisualBasic.Interaction.InputBox("Новое название:", "Изменить", srv.Name);
                if (string.IsNullOrWhiteSpace(name)) return;
                string durStr = Microsoft.VisualBasic.Interaction.InputBox("Новая длительность (мин):", "Изменить", srv.Duration.ToString());
                if (!int.TryParse(durStr, out int dur)) return;
                string priceStr = Microsoft.VisualBasic.Interaction.InputBox("Новая цена:", "Изменить", srv.Price.ToString());
                if (!decimal.TryParse(priceStr, out decimal price)) return;

                using (var context = new BeautySalonEntities())
                {
                    var service = context.Services.Find(srv.Id);
                    if (service != null)
                    {
                        service.Name = name;
                        service.Duration = dur;
                        service.Price = price;
                        context.SaveChanges();
                    }
                }
                LoadServices();
            }
            else MessageBox.Show("Выберите услугу.");
        }

        // ==================== НАЗАД ====================
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack) NavigationService.GoBack();
            else NavigationService.Navigate(new StartPage());
        }
    }
}