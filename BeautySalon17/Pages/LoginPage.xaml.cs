using BeautySalon17.Helpers;
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
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Обработчик кнопки "Назад". Возвращает пользователя на предыдущую страницу.
        /// Если истории нет — переходит на стартовую страницу.
        /// </summary>
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            // NavigationService.CanGoBack — true, если в истории навигации есть предыдущая страница
            if (NavigationService.CanGoBack)
            {
                // Возвращаемся на один шаг назад
                NavigationService.GoBack();
            }
            else
            {
                // Если истории нет (например, открыли LoginPage напрямую), идём на стартовую
                NavigationService.Navigate(new StartPage());
            }
        }
        /// <summary>
        /// Обработчик кнопки "Войти". Проверяет логин и пароль в базе данных.
        /// </summary>
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = TxtLogin.Text.Trim();
            string password = TxtPassword.Text.Trim();

            if(string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                TxtError.Text = "Введите логин и пароль";
                return;
            }

            try
            {
                using (var context = new BeautySalonEntities())
                {
                    var user = context.Users.FirstOrDefault(u => u.Login == login && u.Password == password);
                    if (user != null)
                    {
                        if(user.IsActive == false)
                        {
                            TxtError.Text = "Ваша учётная запись заблокирована. Обратитесь к администратору.";
                            return;
                        }

                        CurrentUser.Id = user.Id;
                        CurrentUser.Login = user.Login;
                        CurrentUser.FullName = $"{user.Surname} {user.Name}";
                        CurrentUser.RoleId = user.RoleId;

                        switch (user.RoleId)
                        {
                            case 1: CurrentUser.RoleName = "Клиент"; break;
                            case 2: CurrentUser.RoleName = "Мастер"; break;
                            case 3: CurrentUser.RoleName = "Менеджер"; break;
                            case 4: CurrentUser.RoleName = "Администратор"; break;
                            default: CurrentUser.RoleName = "Неизвестно"; break;
                        }

                        MessageBox.Show($"Добро пожаловать, {CurrentUser.FullName}!",
                                        "Успешный вход",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Information);

                        NavigationService.Navigate(new StartPage());
                    }
                    else
                    {
                        TxtError.Text = "Неверный логин или пароль.";
                        return;
                    }

                }
            }
            catch(Exception ex)
            {
                TxtError.Text = "Ошибка подключения к базе данных.";
            }
        }
    }
}
