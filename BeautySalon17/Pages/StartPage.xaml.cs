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
using BeautySalon17.Helpers;

namespace BeautySalon17.Pages
{
    /// <summary>
    /// Логика взаимодействия для StartPage.xaml
    /// </summary>
    public partial class StartPage : Page
    {
        public StartPage()
        {
            InitializeComponent();
            UpdateButtonsVisibility();
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
    }
}
