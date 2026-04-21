using BeautySalon17.Helpers;
using Microsoft.VisualBasic;
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
    /// Логика взаимодействия для AdminPage.xaml
    /// </summary>
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
            Loaded += (s, e) => LoadUsers();
        }

        // Загрузка списка пользователей в DataGrid
        private void LoadUsers()
        {
            try
            {
                using (var db = new BeautySalonEntities())
                    DgUsers.ItemsSource = db.Users.Include("Roles").ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        // Получение выбранного пользователя с проверкой
        private Users GetSelectedUser()
        {
            var user = DgUsers.SelectedItem as Users;
            if (user == null)
                MessageBox.Show("Выберите пользователя из списка.");
            return user;
        }

        // Проверка, не пытается ли админ изменить сам себя
        private bool IsSelf(Users user, string action)
        {
            if (user.Id == CurrentUser.Id)
            {
                MessageBox.Show($"Нельзя {action} самому себе.", "Ограничение");
                return true;
            }
            return false;
        }

        // Добавление нового пользователя (через последовательные InputBox)
        private void BtnAddUser_Click(object sender, RoutedEventArgs e)
        {
            // Сбор данных от пользователя
            string surname = Interaction.InputBox("Фамилия:", "Новый пользователь", "");
            if (string.IsNullOrWhiteSpace(surname)) return;
            string name = Interaction.InputBox("Имя:", "Новый пользователь", "");
            if (string.IsNullOrWhiteSpace(name)) return;
            string phone = Interaction.InputBox("Телефон:", "Новый пользователь", "");
            if (string.IsNullOrWhiteSpace(phone)) return;
            string login = Interaction.InputBox("Логин:", "Новый пользователь", "");
            if (string.IsNullOrWhiteSpace(login)) return;
            string password = Interaction.InputBox("Пароль:", "Новый пользователь", "");
            if (string.IsNullOrWhiteSpace(password)) return;
            string roleStr = Interaction.InputBox("Роль (1-Клиент,2-Мастер,3-Менеджер,4-Админ):", "Новый пользователь", "1");
            if (!int.TryParse(roleStr, out int roleId) || roleId < 1 || roleId > 4)
            {
                MessageBox.Show("Некорректный код роли.");
                return;
            }

            try
            {
                using (var db = new BeautySalonEntities())
                {
                    db.Users.Add(new Users
                    {
                        Surname = surname,
                        Name = name,
                        Phone = phone,
                        Login = login,
                        Password = password,
                        RoleId = roleId,
                        IsActive = true
                    });
                    db.SaveChanges();
                }
                LoadUsers();
                MessageBox.Show("Пользователь успешно добавлен.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления: {ex.Message}");
            }
        }

        // Редактирование выбранного пользователя
        private void BtnEditUser_Click(object sender, RoutedEventArgs e)
        {
            var user = GetSelectedUser();
            if (user == null) return;

            string surname = Interaction.InputBox("Фамилия:", "Редактирование", user.Surname);
            if (string.IsNullOrWhiteSpace(surname)) return;
            string name = Interaction.InputBox("Имя:", "Редактирование", user.Name);
            if (string.IsNullOrWhiteSpace(name)) return;
            string phone = Interaction.InputBox("Телефон:", "Редактирование", user.Phone);
            if (string.IsNullOrWhiteSpace(phone)) return;
            string login = Interaction.InputBox("Логин:", "Редактирование", user.Login);
            if (string.IsNullOrWhiteSpace(login)) return;
            string password = Interaction.InputBox("Новый пароль (оставьте пустым, чтобы не менять):", "Редактирование", "");

            try
            {
                using (var db = new BeautySalonEntities())
                {
                    var dbUser = db.Users.Find(user.Id);
                    if (dbUser != null)
                    {
                        dbUser.Surname = surname;
                        dbUser.Name = name;
                        dbUser.Phone = phone;
                        dbUser.Login = login;
                        if (!string.IsNullOrWhiteSpace(password))
                            dbUser.Password = password;
                        db.SaveChanges();
                    }
                }
                LoadUsers();
                MessageBox.Show("Данные обновлены.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при редактировании: {ex.Message}");
            }
        }

        // Универсальный метод блокировки/разблокировки
        private void SetUserActiveState(bool activate)
        {
            var user = GetSelectedUser();
            if (user == null) return;
            if (IsSelf(user, activate ? "разблокировать" : "заблокировать")) return;

            if (user.IsActive == activate)
            {
                MessageBox.Show($"Пользователь уже {(activate ? "активен" : "заблокирован")}.");
                return;
            }

            string action = activate ? "Разблокировать" : "Заблокировать";
            if (MessageBox.Show($"{action} пользователя {user.Surname} {user.Name}?",
                "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new BeautySalonEntities())
                    {
                        var dbUser = db.Users.Find(user.Id);
                        if (dbUser != null)
                            dbUser.IsActive = activate;
                        db.SaveChanges();
                    }
                    LoadUsers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
        }

        private void BtnFreezeUser_Click(object sender, RoutedEventArgs e) => SetUserActiveState(false);
        private void BtnUnfreezeUser_Click(object sender, RoutedEventArgs e) => SetUserActiveState(true);

        // Смена роли выбранного пользователя
        private void BtnChangeRole_Click(object sender, RoutedEventArgs e)
        {
            var user = GetSelectedUser();
            if (user == null) return;
            if (IsSelf(user, "менять роль")) return;

            string roleStr = Interaction.InputBox(
                "Новая роль (1-Клиент,2-Мастер,3-Менеджер,4-Админ):",
                "Смена роли", user.RoleId.ToString());

            if (int.TryParse(roleStr, out int newRoleId) && newRoleId >= 1 && newRoleId <= 4)
            {
                try
                {
                    using (var db = new BeautySalonEntities())
                    {
                        var dbUser = db.Users.Find(user.Id);
                        if (dbUser != null)
                            dbUser.RoleId = newRoleId;
                        db.SaveChanges();
                    }
                    LoadUsers();
                    MessageBox.Show("Роль успешно изменена.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Некорректный код роли.");
            }
        }

        // Возврат на предыдущую страницу
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
            else
                NavigationService.Navigate(new StartPage());
        }
    }
}