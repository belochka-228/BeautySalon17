using BeautySalon17.Helpers;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BeautySalon17.Pages
{
    /// <summary>
    /// Личный кабинет менеджера.
    /// </summary>
    public partial class ManagerPage : Page
    {
        // Здесь временно храню найденного клиента, чтобы потом создать для него запись
        private Users _selectedClient;

        /// <summary>
        /// Конструктор страницы. Подписываюсь на событие Loaded, чтобы данные обновлялись каждый раз при показе страницы
        /// </summary>
        public ManagerPage()
        {
            InitializeComponent();
            this.Loaded += Page_Loaded;
        }

        /// <summary>
        /// Срабатывает каждый раз при загрузке страницы
        /// </summary>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAppointments();   // обновляю список записей
            LoadOrders();         // обновляю список заказов
            LoadProducts();       // обновляю список товаров
            LoadManufacturers();  // обновляю список производителей
            LoadProductTypes();   // обновляю список типов товаров
            LoadServices();       // обновляю список услуг
        }
        /// <summary>
        /// Загружаю все записи из базы данных
        /// Подгружаю связанные данные: услугу (Services), клиента (Users) и мастера (Users1)
        /// </summary>
        private void LoadAppointments()
        {
            using (var context = new BeautySalonEntities())
            {
                var list = context.Appointments.Include("Services").Include("Users").Include("Users1").ToList();
                DgAppointments.ItemsSource = list;
            }
        }

        /// <summary>
        /// Ищу клиента по ФИО или номеру телефона
        /// Результат сохраняю в поле _selectedClient, чтобы потом использовать при создании записи
        /// </summary>
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
                // Ищу только среди клиентов (RoleId == 1)
                var clients = context.Users.Where(u => u.RoleId == 1).AsEnumerable().Where(u => (u.Surname + " " + u.Name + " " + u.Patronymic).IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 || u.Phone.Contains(query)).ToList();

                if (clients.Count == 1)
                {
                    // Нашёлся ровно один клиент, отлично
                    _selectedClient = clients[0];
                    MessageBox.Show($"Выбран клиент: {_selectedClient.Surname} {_selectedClient.Name} {_selectedClient.Patronymic}\nТелефон: {_selectedClient.Phone}",
                                    "Клиент найден", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (clients.Count > 1)
                {
                    // Нашлось несколько беру первого
                    _selectedClient = clients[0];
                    MessageBox.Show($"Найдено несколько клиентов. Выбран первый:\n{_selectedClient.Surname} {_selectedClient.Name}\nТелефон: {_selectedClient.Phone}",
                                    "Клиент найден", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Никого не нашли
                    _selectedClient = null;
                    MessageBox.Show("Клиент не найден.", "Поиск", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        /// <summary>
        /// Очищаю поле поиска клиента и сбрасываю выбранного клиента
        /// </summary>
        private void BtnClearClientSearch_Click(object sender, RoutedEventArgs e)
        {
            TxtClientSearch.Text = "";
            _selectedClient = null;
        }

        /// <summary>
        /// Создаю новую запись для найденного клиента
        /// Пошагово через простые окна ввода (InputBox) спрашиваю: услугу, мастера, дату и время. После этого сохраняю запись в базу
        /// </summary>
        private void BtnAddAppointment_Click(object sender, RoutedEventArgs e)
        {
            // Сначала проверяю, что клиент найден
            if (_selectedClient == null)
            {
                MessageBox.Show("Сначала найдите и выберите клиента.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var context = new BeautySalonEntities())
            {
                // Выбор услуги
                var services = context.Services.ToList();
                if (services.Count == 0)
                {
                    MessageBox.Show("Нет доступных услуг.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Показываю список услуг с ID для выбора
                string serviceList = string.Join("\n", services.Select(s => $"{s.Id} - {s.Name}"));
                string input = Interaction.InputBox($"Введите ID услуги:\n{serviceList}", "Выбор услуги", "");
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
                input = Interaction.InputBox($"Введите ID мастера:\n{masterList}", "Выбор мастера", "");
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

                // Ввод даты и времени
                string dateStr = Interaction.InputBox("Введите дату (ДД.ММ.ГГГГ):", "Дата", DateTime.Today.ToShortDateString());
                if (!DateTime.TryParse(dateStr, out DateTime date))
                {
                    MessageBox.Show("Неверный формат даты.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string timeStr = Interaction.InputBox("Введите время (ЧЧ:ММ):", "Время", "10:00");
                if (!TimeSpan.TryParse(timeStr, out TimeSpan time))
                {
                    MessageBox.Show("Неверный формат времени.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                DateTime appointmentDateTime = date.Date.Add(time);

                // Создаю запись в базе
                var newAppointment = new Appointments
                {
                    ClientId = _selectedClient.Id,
                    MasterId = masterId,
                    ServiceId = serviceId,
                    AppointmentDateTime = appointmentDateTime,
                    Status = "Pending",
                    PaymentMethod = "Наличные"   // способ оплаты по умолчанию
                };

                context.Appointments.Add(newAppointment);
                context.SaveChanges();

                MessageBox.Show("Запись успешно создана!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadAppointments();                  // обновляю таблицу записей
                _selectedClient = null;              // сбрасываю выбранного клиента
                TxtClientSearch.Text = "";           // очищаю поле поиска
            }
        }

        /// <summary>
        /// Отменяю выбранную запись: меняю её статус на "Cancelled"
        /// Выполненные записи отменять нельзя
        /// </summary>
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
                    LoadAppointments();   // обновляю таблицу
                }
            }
            else
            {
                MessageBox.Show("Выберите запись.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Переношу выбранную запись на другую дату и врем.
        /// Работает только для записей со статусом "Pending"
        /// </summary>
        private void BtnMoveAppointment_Click(object sender, RoutedEventArgs e)
        {
            if (DgAppointments.SelectedItem is Appointments app)
            {
                if (app.Status == "Completed" || app.Status == "Cancelled")
                {
                    MessageBox.Show("Эту запись нельзя перенести.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                string dateStr = Interaction.InputBox("Введите новую дату (ДД.ММ.ГГГГ):", "Перенос", app.AppointmentDateTime.ToShortDateString());
                if (!DateTime.TryParse(dateStr, out DateTime newDate)) return;

                string timeStr = Interaction.InputBox("Введите новое время (ЧЧ:ММ):", "Перенос", app.AppointmentDateTime.ToString("HH:mm"));
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
            else
            {
                MessageBox.Show("Выберите запись.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        /// <summary>
        /// Загружаю все заказы из базы вместе с данными клиентов
        /// </summary>
        private void LoadOrders()
        {
            using (var context = new BeautySalonEntities())
            {
                var list = context.Orders.Include("Users").ToList();
                DgOrders.ItemsSource = list;
            }
        }

        /// <summary>
        /// Отмечаю выбранный заказ как выданный (меняю статус на "Completed")
        /// </summary>
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
            else
            {
                MessageBox.Show("Выберите заказ.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        /// <summary>
        /// Загружаю список всех товаров из базы
        /// </summary>
        private void LoadProducts()
        {
            using (var context = new BeautySalonEntities())
            {
                DgProducts.ItemsSource = context.Products.ToList();
            }
        }

        /// <summary>
        /// Добавляю новый товар. Для простоты ManufacturerId и ProductTypeId ставлю в 1
        /// </summary>
        private void BtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            string name = Interaction.InputBox("Название товара:", "Добавить", "");
            if (string.IsNullOrWhiteSpace(name)) return;

            string priceStr = Interaction.InputBox("Цена:", "Добавить", "100");
            if (!decimal.TryParse(priceStr, out decimal price)) return;

            string discountStr = Interaction.InputBox("Скидка %:", "Добавить", "0");
            if (!int.TryParse(discountStr, out int discount)) return;

            using (var context = new BeautySalonEntities())
            {
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

        /// <summary>
        /// Изменяю название и цену выбранного товара
        /// </summary>
        private void BtnEditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (DgProducts.SelectedItem is Products prod)
            {
                string name = Interaction.InputBox("Новое название:", "Изменить", prod.Name);
                if (string.IsNullOrWhiteSpace(name)) return;

                string priceStr = Interaction.InputBox("Новая цена:", "Изменить", prod.Price.ToString());
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
            else
            {
                MessageBox.Show("Выберите товар.");
            }
        }

        /// <summary>
        /// Замораживаю товар – он перестаёт отображаться в каталоге (IsActive = false)
        /// </summary>
        private void BtnFreezeProduct_Click(object sender, RoutedEventArgs e)
        {
            SetProductActive(false);
        }

        /// <summary>
        /// Размораживаю товар – он снова появляется в каталоге (IsActive = true)
        /// </summary>
        private void BtnUnfreezeProduct_Click(object sender, RoutedEventArgs e)
        {
            SetProductActive(true);
        }

        /// <summary>
        /// Общий метод для заморозки / разморозки товара
        /// </summary>
        /// <param name="active">true – разморозить, false – заморозить</param>
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
            else
            {
                MessageBox.Show("Выберите товар.");
            }
        }

        /// <summary>
        /// Меняю процент скидки на выбранный товар
        /// </summary>
        private void BtnDiscountProduct_Click(object sender, RoutedEventArgs e)
        {
            if (DgProducts.SelectedItem is Products prod)
            {
                string input = Interaction.InputBox("Введите новую скидку (%):", "Скидка", prod.Discount.ToString());
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
            else
            {
                MessageBox.Show("Выберите товар.");
            }
        }
        /// <summary>
        /// Загружаю список всех производителей
        /// </summary>
        private void LoadManufacturers()
        {
            using (var context = new BeautySalonEntities())
            {
                DgManufacturers.ItemsSource = context.Manufacturers.ToList();
            }
        }

        /// <summary>
        /// Добавляю нового производителя
        /// </summary>
        private void BtnAddManufacturer_Click(object sender, RoutedEventArgs e)
        {
            string name = Interaction.InputBox("Название производителя:", "Добавить", "");
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

        /// <summary>
        /// Изменяю название выбранного производителя
        /// </summary>
        private void BtnEditManufacturer_Click(object sender, RoutedEventArgs e)
        {
            if (DgManufacturers.SelectedItem is Manufacturers m)
            {
                string name = Interaction.InputBox("Новое название:", "Изменить", m.Name);
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
            else
            {
                MessageBox.Show("Выберите производителя.");
            }
        }
        /// <summary>
        /// Загружаю список всех типов товаров.
        /// </summary>
        private void LoadProductTypes()
        {
            using (var context = new BeautySalonEntities())
            {
                DgProductTypes.ItemsSource = context.ProductTypes.ToList();
            }
        }

        /// <summary>
        /// Добавляю новый тип товара
        /// </summary>
        private void BtnAddProductType_Click(object sender, RoutedEventArgs e)
        {
            string name = Interaction.InputBox("Название типа товара:", "Добавить", "");
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
        /// <summary>
        /// Измение название выбранного типа товара
        /// </summary>
        private void BtnEditProductType_Click(object sender, RoutedEventArgs e)
        {
            if (DgProductTypes.SelectedItem is ProductTypes pt)
            {
                string name = Interaction.InputBox("Новое название:", "Изменить", pt.Name);
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
            else
            {
                MessageBox.Show("Выберите тип товара.");
            }
        }
        /// <summary>
        /// Загружаю список всех услуг
        /// </summary>
        private void LoadServices()
        {
            using (var context = new BeautySalonEntities())
            {
                DgServices.ItemsSource = context.Services.ToList();
            }
        }
        /// <summary>
        /// Добавляю новую услугу
        /// </summary>
        private void BtnAddService_Click(object sender, RoutedEventArgs e)
        {
            string name = Interaction.InputBox("Название услуги:", "Добавить", "");
            if (string.IsNullOrWhiteSpace(name)) return;

            string durStr = Interaction.InputBox("Длительность (мин):", "Добавить", "60");
            if (!int.TryParse(durStr, out int dur)) return;

            string priceStr = Interaction.InputBox("Цена:", "Добавить", "1000");
            if (!decimal.TryParse(priceStr, out decimal price)) return;

            using (var context = new BeautySalonEntities())
            {
                context.Services.Add(new Services { Name = name, Duration = dur, Price = price });
                context.SaveChanges();
            }
            LoadServices();
        }
        /// <summary>
        /// Изменяю название, длительность и цену выбранной услуги
        /// </summary>
        private void BtnEditService_Click(object sender, RoutedEventArgs e)
        {
            if (DgServices.SelectedItem is Services srv)
            {
                string name = Interaction.InputBox("Новое название:", "Изменить", srv.Name);
                if (string.IsNullOrWhiteSpace(name)) return;

                string durStr = Interaction.InputBox("Новая длительность (мин):", "Изменить", srv.Duration.ToString());
                if (!int.TryParse(durStr, out int dur)) return;

                string priceStr = Interaction.InputBox("Новая цена:", "Изменить", srv.Price.ToString());
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
            else
            {
                MessageBox.Show("Выберите услугу.");
            }
        }
        /// <summary>
        /// Кнопка "Назад"
        /// </summary>
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
            else
                NavigationService.Navigate(new StartPage());
        }
    }
}