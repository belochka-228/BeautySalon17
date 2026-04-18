using BeautySalon17.Helpers;
using BeautySalon17.Windows;
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
    /// Логика взаимодействия для StartPage.xaml
    /// </summary>
    public partial class StartPage : Page
    {
        private List<Services> _allServices;  // все услуги
        private List<Users> _allMasters;          // все мастера (пользователи с ролью 2)
        public StartPage()
        {
            InitializeComponent();
            UpdateButtonsVisibility();
            LoadServices();   // загружаем услуги
            LoadFilters();
        }

        /// <summary>
        /// Загружает все услуги из базы и отображает их в WrapPanel.
        /// </summary>
        private void LoadServices()
        {
            try
            {
                using (var context = new BeautySalonEntities())
                {
                    _allServices = context.Services.ToList();
                }
                DisplayServices(_allServices);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки услуг: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LoadFilters()
        {
            try
            {
                using (var context = new BeautySalonEntities())
                {
                    // Загружаем все услуги
                    _allServices = context.Services.ToList();
                    // Загружаем всех мастеров (RoleId = 2)
                    _allMasters = context.Users.Where(u => u.RoleId == 2).ToList();
                }

                // Добавляем "Все" в начало списков
                _allServices.Insert(0, new Services { Id = 0, Name = "Все услуги" });
                _allMasters.Insert(0, new Users { Id = 0, Surname = "Все мастера", Name = "" });

                // Настраиваем ComboBox услуг
                CmbFilterService.ItemsSource = _allServices;
                CmbFilterService.DisplayMemberPath = "Name";
                CmbFilterService.SelectedValuePath = "Id";
                CmbFilterService.SelectedIndex = 0;

                // Настраиваем ComboBox мастеров (показываем фамилию)
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
        private void ApplyServiceFilters()
        {
            if (_allServices == null) return;

            int selectedServiceId = 0;
            if (CmbFilterService.SelectedValue != null && CmbFilterService.SelectedValue is int)
                selectedServiceId = (int)CmbFilterService.SelectedValue;

            int selectedMasterId = 0;
            if (CmbFilterMaster.SelectedValue != null && CmbFilterMaster.SelectedValue is int)
                selectedMasterId = (int)CmbFilterMaster.SelectedValue;

            IEnumerable<Services> filtered = _allServices.Where(s => s.Id != 0); // исключаем "Все услуги"

            // Фильтр по конкретной услуге
            if (selectedServiceId != 0)
                filtered = filtered.Where(s => s.Id == selectedServiceId);

            // Фильтр по мастеру: оставляем услуги, которые оказывает выбранный мастер
            if (selectedMasterId != 0)
            {
                using (var context = new BeautySalonEntities())
                {
                    // Получаем Id услуг, которые делает этот мастер
                    var masterServiceIds = context.MasterServices
                                                  .Where(ms => ms.MasterId == selectedMasterId)
                                                  .Select(ms => ms.ServiceId)
                                                  .ToList();
                    filtered = filtered.Where(s => masterServiceIds.Contains(s.Id));
                }
            }

            DisplayServices(filtered.ToList());
        }

        /// <summary>
        /// Отображает список услуг в виде карточек.
        /// </summary>
        private void DisplayServices(List<Services> services)
        {
            ServicesWrapPanel.Children.Clear();

            if (services == null || services.Count == 0)
            {
                TextBlock noItems = new TextBlock
                {
                    Text = "Услуги временно недоступны.",
                    FontSize = 16,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(20)
                };
                ServicesWrapPanel.Children.Add(noItems);
                return;
            }

            foreach (var service in services)
            {
                FrameworkElement card = CreateServiceCard(service);
                ServicesWrapPanel.Children.Add(card);
            }
        }

        /// <summary>
        /// Создаёт карточку для одной услуги.
        /// </summary>
        private FrameworkElement CreateServiceCard(Services service)
        {
            Border border = new Border
            {
                Width = 200,
                Height = 280,
                Margin = new Thickness(10),
                Background = Brushes.White,
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8)
            };

            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(120) }); // картинка
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });     // название
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });     // длительность
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });     // цена
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });     // кнопка

            // 1. Картинка (пока просто цветной прямоугольник с текстом)
            //    Можно потом заменить на настоящие картинки, если добавишь в базу поле ImagePath для услуг.
            Border imagePlaceholder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(230, 230, 250)),
                Margin = new Thickness(10),
                CornerRadius = new CornerRadius(4)
            };
            TextBlock imageText = new TextBlock
            {
                Text = service.Name.Substring(0, Math.Min(2, service.Name.Length)),
                FontSize = 30,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.DarkSlateBlue,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            imagePlaceholder.Child = imageText;
            Grid.SetRow(imagePlaceholder, 0);
            grid.Children.Add(imagePlaceholder);

            // 2. Название
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

            // 3. Длительность
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

            // 4. Цена
            TextBlock priceText = new TextBlock
            {
                Text = $"{service.Price:F2} руб.",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Green,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(5, 2, 5, 0)
            };
            Grid.SetRow(priceText, 3);
            grid.Children.Add(priceText);

            // 5. Кнопка "Записаться"
            Button bookBtn = new Button
            {
                Content = "Записаться",
                Height = 28,
                Margin = new Thickness(10, 5, 10, 10),
                Tag = service.Id,
                IsEnabled = CurrentUser.IsAuthenticated   // только для авторизованных
            };
            bookBtn.Click += BtnBook_Click;
            Grid.SetRow(bookBtn, 4);
            grid.Children.Add(bookBtn);

            border.Child = grid;
            return border;
        }

        /// <summary>
        /// Обработчик кнопки "Записаться" — откроем окно выбора мастера и времени (позже).
        /// </summary>
        private void BtnBook_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn?.Tag is int serviceId)
            {
                // Проверяем, авторизован ли пользователь
                if (!CurrentUser.IsAuthenticated)
                {
                    MessageBox.Show("Чтобы записаться, необходимо войти в систему.", "Требуется авторизация",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Открываем окно выбора мастера и времени
                SelectMasterTimeWindow selectWindow = new SelectMasterTimeWindow(serviceId);
                selectWindow.Owner = Window.GetWindow(this);
                bool? result = selectWindow.ShowDialog();

                // Если окно закрыто с результатом true, то навигация уже произошла внутри окн
            }
        }
        /// <summary>
        /// Обновляет видимость кнопок "Войти" и "Аккаунт" на основе статуса авторизации.
        /// </summary>
        private void UpdateButtonsVisibility()
                {
                    // CurrentUser.IsAuthenticated — свойство, которое возвращает true, если Id пользователя не равен 0
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
        /// <summary>
        /// Обработчик нажатия на кнопку "Войти"
        /// </summary>
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new LoginPage());
        }

        /// <summary>
        /// Обработчик нажатия на кнопку "Аккаунт"
        /// </summary>
        private void BtnAccount_Click(object sender, RoutedEventArgs e)
        {
            // В зависимости от роли пользователя открываем нужную страницу
            switch (CurrentUser.RoleId)
            {
                case 1:
                    NavigationService.Navigate(new ClientAccountPage());
                    break;
                case 2:
                    NavigationService.Navigate(new MasterPage());
                    break;
                case 3:
                    NavigationService.Navigate(new ManagerPage());
                    break;
                case 4:
                    NavigationService.Navigate(new AdminPage());
                    break;
                default:
                    MessageBox.Show("Неизвестная роль пользователя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }
        }
        /// <summary>
        /// Обработчик нажатия на кнопку "Товары"
        /// </summary>
        private void BtnProducts_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ProductsPage());
        }
        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyServiceFilters();
        }

        private void BtnResetFilters_Click(object sender, RoutedEventArgs e)
        {
            CmbFilterService.SelectedIndex = 0;
            CmbFilterMaster.SelectedIndex = 0;
            // ApplyServiceFilters вызовется автоматически через SelectionChanged
        }
    }
}
