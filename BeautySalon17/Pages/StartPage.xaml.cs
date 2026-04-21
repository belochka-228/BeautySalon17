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
        private List<Services> _allServices;      // все услуги (после фильтрации)
        private List<Users> _allMasters;          // все мастера (для фильтра)
        private Services _selectedService;        // выбранная услуга
        private int _selectedMasterId;            // ID выбранного мастера
        private DateTime _selectedDate = DateTime.Today; // выбранная дата
        private DateTime _selectedDateTime;       // полная дата и время записи

        public StartPage()
        {
            InitializeComponent();
            UpdateButtonsVisibility();
            LoadFilters();
            LoadAllServices();
        }

        // ==================== ЗАГРУЗКА ДАННЫХ ====================
        private void LoadFilters()
        {
            try
            {
                using (var db = new BeautySalonEntities())
                {
                    _allServices = db.Services.ToList();
                    _allMasters = db.Users.Where(u => u.RoleId == 2).ToList();
                }

                // Добавляем пункт "Все"
                _allServices.Insert(0, new Services { Id = 0, Name = "Все услуги" });
                _allMasters.Insert(0, new Users { Id = 0, Surname = "Все мастера", Name = "" });

                // ComboBox услуг
                CmbFilterService.ItemsSource = _allServices;
                CmbFilterService.DisplayMemberPath = "Name";
                CmbFilterService.SelectedValuePath = "Id";
                CmbFilterService.SelectedIndex = 0;

                // ComboBox мастеров
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
                using (var db = new BeautySalonEntities())
                    DisplayServices(db.Services.ToList());
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

            using (var db = new BeautySalonEntities())
            {
                var filtered = db.Services.AsEnumerable();

                if (serviceId != 0)
                    filtered = filtered.Where(s => s.Id == serviceId);

                if (masterId != 0)
                {
                    var masterServiceIds = db.MasterServices
                        .Where(ms => ms.MasterId == masterId)
                        .Select(ms => ms.ServiceId)
                        .ToList();
                    filtered = filtered.Where(s => masterServiceIds.Contains(s.Id));
                }

                DisplayServices(filtered.ToList());
            }

            ClearSelection(); // сбрасываем правую панель
        }

        private int GetSelectedId(ComboBox cmb) =>cmb.SelectedValue is int id ? id : 0;

        // ==================== ОТОБРАЖЕНИЕ СЕТКИ УСЛУГ ====================
        private void DisplayServices(List<Services> services)
        {
            ServicesWrapPanel.Children.Clear();

            if (!services.Any())
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

            foreach (var s in services)
                ServicesWrapPanel.Children.Add(CreateServiceCard(s));
        }

        private FrameworkElement CreateServiceCard(Services service)
        {
            var border = new Border
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

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(120) }); // картинка
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });    // название
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });    // длительность
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });    // цена

            // Заглушка картинки
            var imgPlaceholder = new Border
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
            var txtName = new TextBlock
            {
                Text = service.Name,
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(5, 5, 5, 0)
            };
            Grid.SetRow(txtName, 1);
            grid.Children.Add(txtName);

            // Длительность
            var txtDuration = new TextBlock
            {
                Text = $"⏱ {service.Duration} мин.",
                FontSize = 12,
                Foreground = Brushes.Gray,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(5, 2, 5, 0)
            };
            Grid.SetRow(txtDuration, 2);
            grid.Children.Add(txtDuration);

            // Цена
            var txtPrice = new TextBlock
            {
                Text = $"{service.Price:F2} руб.",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Green,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(5, 2, 5, 0)
            };
            Grid.SetRow(txtPrice, 3);
            grid.Children.Add(txtPrice);

            border.Child = grid;
            return border;
        }

        // ==================== ВЫБОР УСЛУГИ ====================
        private void ServiceCard_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!(sender is Border border) || !(border.Tag is Services service)) return;

            _selectedService = service;
            TxtSelectedService.Text = $"Услуга: {service.Name}";
            TxtHint.Visibility = Visibility.Collapsed;
            SelectionPanel.Visibility = Visibility.Visible;

            LoadMastersForService(service.Id);
            LbTimeSlots.ItemsSource = null;
            BtnBook.IsEnabled = false;
            DpAppointmentDate.SelectedDate = DateTime.Today;
        }

        private void LoadMastersForService(int serviceId)
        {
            try
            {
                using (var db = new BeautySalonEntities())
                {
                    var masterIds = db.MasterServices.Where(ms => ms.ServiceId == serviceId).Select(ms => ms.MasterId).ToList();
                    var masters = db.Users.Where(u => masterIds.Contains(u.Id) && u.RoleId == 2).Select(u => new { u.Id, FullName = u.Surname + " " + u.Name }).ToList();
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
                LbTimeSlots.ItemsSource = null;
                BtnBook.IsEnabled = false;
            }
        }

        // ==================== ВЫБОР ДАТЫ И ВРЕМЕНИ ====================
        private void DpAppointmentDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DpAppointmentDate.SelectedDate.HasValue)
            {
                _selectedDate = DpAppointmentDate.SelectedDate.Value;
                if (_selectedMasterId != 0)
                    LoadTimeSlots();
                else
                    LbTimeSlots.ItemsSource = null;
            }
            else
            {
                LbTimeSlots.ItemsSource = null;
            }
            BtnBook.IsEnabled = false;
        }

        private void LoadTimeSlots()
        {
            var slots = new List<object>();
            DateTime start = _selectedDate.AddHours(9);  // салон работает с 9:00
            DateTime end = _selectedDate.AddHours(18);   // до 18:00

            while (start <= end)
            {
                slots.Add(new { TimeSlot = start.ToString("HH:mm") });
                start = start.AddHours(1);
            }
            LbTimeSlots.ItemsSource = slots;
            BtnBook.IsEnabled = false;
        }

        private void LbTimeSlots_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LbTimeSlots.SelectedItem != null)
            {
                dynamic slot = LbTimeSlots.SelectedItem;
                _selectedDateTime = _selectedDate.Date.Add(TimeSpan.Parse(slot.TimeSlot));
                BtnBook.IsEnabled = true;
            }
            else
            {
                BtnBook.IsEnabled = false;
            }
        }

        // ==================== ЗАПИСЬ ====================
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
            LbTimeSlots.ItemsSource = null;
            BtnBook.IsEnabled = false;
        }

        // ==================== НАВИГАЦИЯ И АВТОРИЗАЦИЯ ====================
        private void UpdateButtonsVisibility()
        {
            bool isAuth = CurrentUser.IsAuthenticated;
            BtnLogin.Visibility = isAuth ? Visibility.Collapsed : Visibility.Visible;
            BtnAccount.Visibility = isAuth ? Visibility.Visible : Visibility.Collapsed;
            BtnLogout.Visibility = isAuth ? Visibility.Visible : Visibility.Collapsed;

            if (isAuth)
                BtnAccount.Content = CurrentUser.FullName ?? "Аккаунт";
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            CurrentUser.Clear();
            UpdateButtonsVisibility();
            ClearSelection();
            LoadAllServices();
            CmbFilterService.SelectedIndex = 0;
            CmbFilterMaster.SelectedIndex = 0;
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e) =>
            NavigationService.Navigate(new LoginPage());

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

        private void BtnProducts_Click(object sender, RoutedEventArgs e) =>
            NavigationService.Navigate(new ProductsPage());
    }
}