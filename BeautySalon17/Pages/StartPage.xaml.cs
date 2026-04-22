using BeautySalon17.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BeautySalon17.Windows;

namespace BeautySalon17.Pages
{
    /// <summary>
    /// Стартовая страница приложения.
    /// Отображает сетку услуг, фильтры, панель выбора мастера/даты/времени
    /// и позволяет авторизованному клиенту записаться на услугу.
    /// </summary>
    public partial class StartPage : Page
    {
        private List<Services> _allServices;      // все услуги (используются при фильтрации)
        private List<Users> _allMasters;          // все мастера (RoleId = 2)
        private Services _selectedService;        // услуга, выбранная пользователем
        private int _selectedMasterId;            // ID выбранного мастера
        private DateTime _selectedDateTime;       // полная дата и время записи
        private DateTime _selectedDate = DateTime.Today; // выбранная дата (по умолчанию сегодня)

        // КОНСТРУКТОР
        public StartPage()
        {
            InitializeComponent();
            UpdateButtonsVisibility();   // скрываем/показываем кнопки входа/аккаунта/выхода
            LoadFilters();               // загружаем данные для фильтров (услуги, мастера)
            LoadAllServices();           // отображаем все услуги в сетке
        }
        /// <summary>
        /// Загружает списки услуг и мастеров из базы и заполняет фильтры.
        /// </summary>
        private void LoadFilters()
        {
            try
            {
                using (var context = new BeautySalonEntities())
                {
                    _allServices = context.Services.ToList();
                    _allMasters = context.Users.Where(u => u.RoleId == 2).ToList();
                }

                // Добавляем пункт "Все" в начало каждого списка
                _allServices.Insert(0, new Services { Id = 0, Name = "Все услуги" });
                _allMasters.Insert(0, new Users { Id = 0, Surname = "Все мастера", Name = "" });

                // Настраиваем ComboBox фильтра по услуге
                CmbFilterService.ItemsSource = _allServices;
                CmbFilterService.DisplayMemberPath = "Name";
                CmbFilterService.SelectedValuePath = "Id";
                CmbFilterService.SelectedIndex = 0;

                // Настраиваем ComboBox фильтра по мастеру
                CmbFilterMaster.ItemsSource = _allMasters;
                CmbFilterMaster.DisplayMemberPath = "Surname";
                CmbFilterMaster.SelectedValuePath = "Id";
                CmbFilterMaster.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки фильтров: {ex.Message}");
            }
        }

        /// <summary>
        /// Загружает все услуги из базы и отображает их в сетке (WrapPanel).
        /// </summary>
        private void LoadAllServices()
        {
            try
            {
                using (var context = new BeautySalonEntities())
                {
                    var services = context.Services.ToList();
                    DisplayServices(services);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки услуг: {ex.Message}");
            }
        }

        // ФИЛЬТРАЦИЯ
        /// <summary>
        /// Срабатывает при изменении выбора в любом из фильтров. Применяет фильтры к сетке услуг.
        /// </summary>
        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilters();

        /// <summary>
        /// Сбрасывает оба фильтра на значение "Все".
        /// </summary>
        private void BtnResetFilters_Click(object sender, RoutedEventArgs e)
        {
            CmbFilterService.SelectedIndex = 0;
            CmbFilterMaster.SelectedIndex = 0;
        }

        /// <summary>
        /// Применяет текущие значения фильтров к списку услуг и обновляет сетку.
        /// </summary>
        private void ApplyFilters()
        {
            int serviceId = GetSelectedId(CmbFilterService);
            int masterId = GetSelectedId(CmbFilterMaster);

            using (var context = new BeautySalonEntities())
            {
                IEnumerable<Services> filtered = context.Services.AsEnumerable();

                if (serviceId != 0)
                    filtered = filtered.Where(s => s.Id == serviceId);

                if (masterId != 0)
                {
                    var masterServiceIds = context.MasterServices.Where(ms => ms.MasterId == masterId).Select(ms => ms.ServiceId).ToList();
                    filtered = filtered.Where(s => masterServiceIds.Contains(s.Id));
                }

                DisplayServices(filtered.ToList());
            }
            ClearSelection(); // сбрасываем правую панель выбора
        }
        /// <summary>
        /// Безопасно получает целочисленный ID из выбранного значения ComboBox.
        /// </summary>
        private int GetSelectedId(ComboBox cmb)
        {
            if (cmb.SelectedValue != null && cmb.SelectedValue is int id)
                return id;
            return 0;
        }

        // ОТОБРАЖЕНИЕ СЕТКИ УСЛУГ
        /// <summary>
        /// Отображает переданный список услуг в WrapPanel в виде карточек.
        /// </summary>
        private void DisplayServices(List<Services> services)
        {
            ServicesWrapPanel.Children.Clear();

            if (services == null || services.Count == 0)
            {
                ServicesWrapPanel.Children.Add(new TextBlock
                {
                    Text = "Услуги не найдены.",
                    FontSize = 16,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(20)
                });
                return;
            }

            foreach (var service in services)
                ServicesWrapPanel.Children.Add(CreateServiceCard(service));
        }
        /// <summary>
        /// Создаёт визуальную карточку для одной услуги.
        /// </summary>
        private FrameworkElement CreateServiceCard(Services service)
        {
            Border border = new Border
            {
                Width = 200,
                Height = 260,
                Margin = new Thickness(10),
                Background = Brushes.White,
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Tag = service,
                Cursor = System.Windows.Input.Cursors.Hand
            };
            border.MouseLeftButtonDown += ServiceCard_Click;

            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(120) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Картинка-заглушка
            Border imgPlaceholder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(230, 230, 250)),
                Margin = new Thickness(10),
                CornerRadius = new CornerRadius(4)
            };
            imgPlaceholder.Child = new TextBlock
            {
                Text = service.Name.Substring(0, Math.Min(2, service.Name.Length)),
                FontSize = 30,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.DarkSlateBlue,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(imgPlaceholder, 0);
            grid.Children.Add(imgPlaceholder);

            // Название услуги
            TextBlock nameText = new TextBlock
            {
                Text = service.Name,
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(5, 5, 5, 0)
            };
            Grid.SetRow(nameText, 1);
            grid.Children.Add(nameText);

            // Длительность
            TextBlock durationText = new TextBlock
            {
                Text = $"{service.Duration} мин.",
                FontSize = 12,
                Foreground = Brushes.Gray,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(5, 2, 5, 0)
            };
            Grid.SetRow(durationText, 2);
            grid.Children.Add(durationText);

            // Цена
            TextBlock priceText = new TextBlock
            {
                Text = $"{service.Price:F2} руб.",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Green,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(5, 2, 5, 0)
            };
            Grid.SetRow(priceText, 3);
            grid.Children.Add(priceText);

            border.Child = grid;
            return border;
        }

        // ВЫБОР УСЛУГИ
        /// <summary>
        /// Обрабатывает клик по карточке услуги. Показывает панель выбора мастера/даты/времени.
        /// </summary>
        private void ServiceCard_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is Services service)
            {
                _selectedService = service;
                TxtSelectedService.Text = $"Выбранная услуга: {service.Name}";
                TxtHint.Visibility = Visibility.Collapsed;
                SelectionPanel.Visibility = Visibility.Visible;

                LoadMastersForService(service.Id);
                LbTimeSlots.ItemsSource = null;
                BtnBook.IsEnabled = false;
                DpAppointmentDate.SelectedDate = DateTime.Today;
            }
        }
        /// <summary>
        /// Загружает мастеров, которые оказывают выбранную услугу, и отображает их в списке.
        /// </summary>
        private void LoadMastersForService(int serviceId)
        {
            try
            {
                using (var context = new BeautySalonEntities())
                {
                    var masterIds = context.MasterServices.Where(ms => ms.ServiceId == serviceId).Select(ms => ms.MasterId).ToList();
                    var masters = context.Users.Where(u => masterIds.Contains(u.Id) && u.RoleId == 2).Select(u => new { u.Id, FullName = u.Surname + " " + u.Name }).ToList();

                    LbMasters.ItemsSource = masters;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки мастеров: {ex.Message}");
            }
        }
        /// <summary>
        /// Срабатывает при выборе мастера. Генерирует временные слоты для текущей даты.
        /// </summary>
        private void LbMasters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LbMasters.SelectedValue is int masterId)
            {
                _selectedMasterId = masterId;
                DpAppointmentDate.SelectedDate = DateTime.Today;
                LoadTimeSlots();
            }
            else
            {
                _selectedMasterId = 0;
                if (LbTimeSlots != null) LbTimeSlots.ItemsSource = null;
                if (BtnBook != null) BtnBook.IsEnabled = false;
            }
        }

        /// <summary>
        /// Генерирует список временных слотов (каждый час с 9:00 до 18:00) для выбранной даты.
        /// </summary>
        private void LoadTimeSlots()
        {
            if (LbTimeSlots == null) return;

            var slots = new List<object>();
            DateTime start = _selectedDate.AddHours(8);
            DateTime end = _selectedDate.AddHours(20);

            while (start <= end)
            {
                slots.Add(new { TimeSlot = start.ToString("HH:mm") });
                start = start.AddHours(1);
            }
            LbTimeSlots.ItemsSource = slots;
            BtnBook.IsEnabled = false;
        }
        /// <summary>
        /// Срабатывает при изменении даты в DatePicker. Перегенерирует слоты, если мастер уже выбран.
        /// </summary>
        private void DpAppointmentDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DpAppointmentDate.SelectedDate.HasValue)
            {
                _selectedDate = DpAppointmentDate.SelectedDate.Value;
                if (_selectedMasterId != 0)
                    LoadTimeSlots();
                else if (LbTimeSlots != null)
                    LbTimeSlots.ItemsSource = null;
            }
            else if (LbTimeSlots != null)
            {
                LbTimeSlots.ItemsSource = null;
            }

            if (BtnBook != null) BtnBook.IsEnabled = false;
        }
        /// <summary>
        /// Срабатывает при выборе временного слота. Сохраняет полную дату/время и активирует кнопку "Записаться".
        /// </summary>
        private void LbTimeSlots_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LbTimeSlots.SelectedItem != null)
            {
                dynamic slot = LbTimeSlots.SelectedItem;
                string timeStr = slot.TimeSlot;
                _selectedDateTime = _selectedDate.Date.Add(TimeSpan.Parse(timeStr));
                BtnBook.IsEnabled = true;
            }
            else
            {
                BtnBook.IsEnabled = false;
            }
        }
        /// <summary>
        /// Открывает страницу подтверждения записи, если пользователь авторизован.
        /// </summary>
        private void BtnBook_Click(object sender, RoutedEventArgs e)
        {
            if (!CurrentUser.IsAuthenticated)
            {
                MessageBox.Show("Чтобы записаться, необходимо войти в систему.",
                    "Требуется авторизация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            NavigationService.Navigate(new AppointmentConfirmPage(
                _selectedService.Id, _selectedMasterId, _selectedDateTime));
        }
        /// <summary>
        /// Сбрасывает выбранную услугу и скрывает правую панель выбора.
        /// </summary>
        private void ClearSelection()
        {
            _selectedService = null;
            SelectionPanel.Visibility = Visibility.Collapsed;
            TxtHint.Visibility = Visibility.Visible;
            LbMasters.ItemsSource = null;
            if (LbTimeSlots != null) LbTimeSlots.ItemsSource = null;
            if (BtnBook != null) BtnBook.IsEnabled = false;
        }
        /// <summary>
        /// Обновляет видимость и текст кнопок входа, аккаунта и выхода в зависимости от авторизации.
        /// </summary>
        private void UpdateButtonsVisibility()
        {
            if (CurrentUser.IsAuthenticated)
            {
                BtnLogin.Visibility = Visibility.Collapsed;
                BtnAccount.Visibility = Visibility.Visible;
                BtnLogout.Visibility = Visibility.Visible;
                BtnAccount.Content = CurrentUser.FullName ?? "Аккаунт";
            }
            else
            {
                BtnLogin.Visibility = Visibility.Visible;
                BtnAccount.Visibility = Visibility.Collapsed;
                BtnLogout.Visibility = Visibility.Collapsed;
            }
        }
        /// <summary>
        /// Выход из учётной записи: очищает CurrentUser, сбрасывает фильтры и обновляет интерфейс.
        /// </summary>
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            CurrentUser.Clear();
            UpdateButtonsVisibility();
            ClearSelection();
            LoadAllServices();
            CmbFilterService.SelectedIndex = 0;
            CmbFilterMaster.SelectedIndex = 0;
        }

        /// <summary>
        /// Переход на страницу входа.
        /// </summary>
        private void BtnLogin_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new LoginPage());

        /// <summary>
        /// Переход в личный кабинет, соответствующий роли текущего пользователя.
        /// </summary>
        private void BtnAccount_Click(object sender, RoutedEventArgs e)
        {
            switch (CurrentUser.RoleId)
            {
                case 1: NavigationService.Navigate(new ClientAccountPage()); break;
                case 2: NavigationService.Navigate(new MasterPage()); break;
                case 3: NavigationService.Navigate(new ManagerPage()); break;
                case 4: NavigationService.Navigate(new AdminPage()); break;
                default: MessageBox.Show("Неизвестная роль"); break;
            }
        }

        /// <summary>
        /// Переход на страницу товаров.
        /// </summary>
        private void BtnProducts_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new ProductsPage());
    }
}