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
    public partial class StartPage : Page
    {
        // Данные для работы страницы
        private List<Services> _allServices;      // все услуги (после фильтрации)
        private List<Users> _allMasters;          // все мастера (для фильтра)
        private Services _selectedService;        // выбранная услуга
        private int _selectedMasterId;            // ID выбранного мастера
        private DateTime _selectedDateTime;       // полная дата и время записи
        private DateTime _selectedDate = DateTime.Today; // выбранная дата (по умолчанию сегодня)

        public StartPage()
        {
            InitializeComponent();
            UpdateButtonsVisibility();
            LoadFilters();      // загружаем данные для фильтров
            LoadAllServices();  // отображаем все услуги в сетке
        }

        // ==================== ЗАГРУЗКА ДАННЫХ ====================
        private void LoadFilters()
        {
            try
            {
                using (var context = new BeautySalonEntities())
                {
                    _allServices = context.Services.ToList();
                    _allMasters = context.Users.Where(u => u.RoleId == 2).ToList();
                }

                // Добавляем пункт "Все"
                _allServices.Insert(0, new Services { Id = 0, Name = "Все услуги" });
                _allMasters.Insert(0, new Users { Id = 0, Surname = "Все мастера", Name = "" });

                // Настраиваем ComboBox'ы
                CmbFilterService.ItemsSource = _allServices;
                CmbFilterService.DisplayMemberPath = "Name";
                CmbFilterService.SelectedValuePath = "Id";
                CmbFilterService.SelectedIndex = 0;

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

        // ==================== ФИЛЬТРАЦИЯ ====================
        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilters();

        private void BtnResetFilters_Click(object sender, RoutedEventArgs e)
        {
            CmbFilterService.SelectedIndex = 0;
            CmbFilterMaster.SelectedIndex = 0;
        }

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
                    var masterServiceIds = context.MasterServices
                        .Where(ms => ms.MasterId == masterId)
                        .Select(ms => ms.ServiceId)
                        .ToList();
                    filtered = filtered.Where(s => masterServiceIds.Contains(s.Id));
                }

                DisplayServices(filtered.ToList());
            }

            ClearSelection(); // сбрасываем правую панель
        }

        private int GetSelectedId(ComboBox cmb)
        {
            if (cmb.SelectedValue != null && cmb.SelectedValue is int id)
                return id;
            return 0;
        }

        // ==================== ОТОБРАЖЕНИЕ СЕТКИ УСЛУГ ====================
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

            // Заглушка картинки
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

            // Название
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
                Text = $"⏱ {service.Duration} мин.",
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

        // ==================== ВЫБОР УСЛУГИ ====================
        private void ServiceCard_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is Services service)
            {
                _selectedService = service;
                TxtSelectedService.Text = $"Услуга: {service.Name}";
                TxtHint.Visibility = Visibility.Collapsed;
                SelectionPanel.Visibility = Visibility.Visible;

                LoadMastersForService(service.Id);
                LbTimeSlots.ItemsSource = null;
                BtnBook.IsEnabled = false;
                DpAppointmentDate.SelectedDate = DateTime.Today;
            }
        }

        private void LoadMastersForService(int serviceId)
        {
            try
            {
                using (var context = new BeautySalonEntities())
                {
                    var masterIds = context.MasterServices
                        .Where(ms => ms.ServiceId == serviceId)
                        .Select(ms => ms.MasterId)
                        .ToList();
                    var masters = context.Users
                        .Where(u => masterIds.Contains(u.Id) && u.RoleId == 2)
                        .Select(u => new { u.Id, FullName = u.Surname + " " + u.Name })
                        .ToList();
                    LbMasters.ItemsSource = masters;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки мастеров: {ex.Message}");
            }
        }

        // ==================== ВЫБОР МАСТЕРА ====================
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

        private void LoadTimeSlots()
        {
            if (LbTimeSlots == null) return;

            var slots = new List<object>();
            DateTime start = _selectedDate.AddHours(9);
            DateTime end = _selectedDate.AddHours(18);

            while (start <= end)
            {
                slots.Add(new { TimeSlot = start.ToString("HH:mm") });
                start = start.AddHours(1);
            }
            LbTimeSlots.ItemsSource = slots;
            BtnBook.IsEnabled = false;
        }

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

        // ==================== ВЫБОР ВРЕМЕНИ ====================
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

        // ==================== КНОПКА "ЗАПИСАТЬСЯ" ====================
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

        private void ClearSelection()
        {
            _selectedService = null;
            SelectionPanel.Visibility = Visibility.Collapsed;
            TxtHint.Visibility = Visibility.Visible;
            LbMasters.ItemsSource = null;
            if (LbTimeSlots != null) LbTimeSlots.ItemsSource = null;
            if (BtnBook != null) BtnBook.IsEnabled = false;
        }

        // ==================== КНОПКИ ВЕРХНЕЙ ПАНЕЛИ ====================
        private void UpdateButtonsVisibility()
        {
            if (CurrentUser.IsAuthenticated)
            {
                BtnLogin.Visibility = Visibility.Collapsed;
                BtnAccount.Visibility = Visibility.Visible;
                BtnAccount.Content = CurrentUser.FullName ?? "Аккаунт";
            }
            else
            {
                BtnLogin.Visibility = Visibility.Visible;
                BtnAccount.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new LoginPage());

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

        private void BtnProducts_Click(object sender, RoutedEventArgs e) => NavigationService.Navigate(new ProductsPage());
    }
}