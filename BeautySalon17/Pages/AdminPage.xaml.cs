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
    /// Логика взаимодействия для AdminPage.xaml
    /// </summary>
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
            this.Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadUsers();
        }

        // ==================== ЗАГРУЗКА ПОЛЬЗОВАТЕЛЕЙ ====================
        private void LoadUsers()
        {
            try
            {
                using (var context = new BeautySalonEntities())
                {
                    // Загружаем пользователей вместе с ролями
                    var users = context.Users.Include("Roles").ToList();
                    DgUsers.ItemsSource = users;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}");
            }
        }

        // ==================== ДОБАВЛЕНИЕ ПОЛЬЗОВАТЕЛЯ ====================
        private void BtnAddUser_Click(object sender, RoutedEventArgs e)
        {
            string surname = Microsoft.VisualBasic.Interaction.InputBox("Фамилия:", "Добавить пользователя", "");
            if (string.IsNullOrWhiteSpace(surname)) return;

            string name = Microsoft.VisualBasic.Interaction.InputBox("Имя:", "Добавить пользователя", "");
            if (string.IsNullOrWhiteSpace(name)) return;

            string phone = Microsoft.VisualBasic.Interaction.InputBox("Телефон:", "Добавить пользователя", "");
            if (string.IsNullOrWhiteSpace(phone)) return;

            string login = Microsoft.VisualBasic.Interaction.InputBox("Логин:", "Добавить пользователя", "");
            if (string.IsNullOrWhiteSpace(login)) return;

            string password = Microsoft.VisualBasic.Interaction.InputBox("Пароль:", "Добавить пользователя", "");
            if (string.IsNullOrWhiteSpace(password)) return;

            string roleStr = Microsoft.VisualBasic.Interaction.InputBox("Роль (1-Клиент, 2-Мастер, 3-Менеджер, 4-Админ):", "Добавить пользователя", "1");
            if (!int.TryParse(roleStr, out int roleId) || roleId < 1 || roleId > 4)
            {
                MessageBox.Show("Неверный код роли.", "Ошибка");
                return;
            }

            try
            {
                using (var context = new BeautySalonEntities())
                {
                    var newUser = new Users
                    {
                        Surname = surname,
                        Name = name,
                        Phone = phone,
                        Login = login,
                        Password = password,
                        RoleId = roleId,
                        IsActive = true
                    };
                    context.Users.Add(newUser);
                    context.SaveChanges();
                }
                LoadUsers();
                MessageBox.Show("Пользователь добавлен.", "Успех");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления: {ex.Message}");
            }
        }

        // ==================== ИЗМЕНЕНИЕ ПОЛЬЗОВАТЕЛЯ ====================
        private void BtnEditUser_Click(object sender, RoutedEventArgs e)
        {
            if (!(DgUsers.SelectedItem is Users selectedUser))
            {
                MessageBox.Show("Выберите пользователя.");
                return;
            }

            string surname = Microsoft.VisualBasic.Interaction.InputBox("Фамилия:", "Изменить", selectedUser.Surname);
            if (string.IsNullOrWhiteSpace(surname)) return;

            string name = Microsoft.VisualBasic.Interaction.InputBox("Имя:", "Изменить", selectedUser.Name);
            if (string.IsNullOrWhiteSpace(name)) return;

            string phone = Microsoft.VisualBasic.Interaction.InputBox("Телефон:", "Изменить", selectedUser.Phone);
            if (string.IsNullOrWhiteSpace(phone)) return;

            string login = Microsoft.VisualBasic.Interaction.InputBox("Логин:", "Изменить", selectedUser.Login);
            if (string.IsNullOrWhiteSpace(login)) return;

            string password = Microsoft.VisualBasic.Interaction.InputBox("Пароль (оставьте пустым, чтобы не менять):", "Изменить", "");

            try
            {
                using (var context = new BeautySalonEntities())
                {
                    var user = context.Users.Find(selectedUser.Id);
                    if (user != null)
                    {
                        user.Surname = surname;
                        user.Name = name;
                        user.Phone = phone;
                        user.Login = login;
                        if (!string.IsNullOrWhiteSpace(password))
                            user.Password = password;
                        context.SaveChanges();
                    }
                }
                LoadUsers();
                MessageBox.Show("Данные обновлены.", "Успех");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка изменения: {ex.Message}");
            }
        }

        // ==================== ЗАМОРОЗИТЬ ПОЛЬЗОВАТЕЛЯ ====================
        private void BtnFreezeUser_Click(object sender, RoutedEventArgs e)
        {
            if (!(DgUsers.SelectedItem is Users selectedUser))
            {
                MessageBox.Show("Выберите пользователя.");
                return;
            }

            if (selectedUser.Id == CurrentUser.Id)
            {
                MessageBox.Show("Нельзя заморозить самого себя.", "Ошибка");
                return;
            }

            if (selectedUser.IsActive == false)
            {
                MessageBox.Show("Пользователь уже заморожен.");
                return;
            }

            var result = MessageBox.Show($"Заморозить пользователя {selectedUser.Surname} {selectedUser.Name}?",
                                         "Подтверждение", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var context = new BeautySalonEntities())
                    {
                        var user = context.Users.Find(selectedUser.Id);
                        if (user != null)
                            user.IsActive = false;
                        context.SaveChanges();
                    }
                    LoadUsers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
        }

        // ==================== РАЗМОРОЗИТЬ ПОЛЬЗОВАТЕЛЯ ====================
        private void BtnUnfreezeUser_Click(object sender, RoutedEventArgs e)
        {
            if (!(DgUsers.SelectedItem is Users selectedUser))
            {
                MessageBox.Show("Выберите пользователя.");
                return;
            }

            if (selectedUser.IsActive == true)
            {
                MessageBox.Show("Пользователь уже активен.");
                return;
            }

            var result = MessageBox.Show($"Разморозить пользователя {selectedUser.Surname} {selectedUser.Name}?",
                                         "Подтверждение", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var context = new BeautySalonEntities())
                    {
                        var user = context.Users.Find(selectedUser.Id);
                        if (user != null)
                            user.IsActive = true;
                        context.SaveChanges();
                    }
                    LoadUsers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
        }

        // ==================== СМЕНА РОЛИ ====================
        private void BtnChangeRole_Click(object sender, RoutedEventArgs e)
        {
            if (!(DgUsers.SelectedItem is Users selectedUser))
            {
                MessageBox.Show("Выберите пользователя.");
                return;
            }

            if (selectedUser.Id == CurrentUser.Id)
            {
                MessageBox.Show("Нельзя изменить роль самому себе.", "Ошибка");
                return;
            }

            string roleStr = Microsoft.VisualBasic.Interaction.InputBox(
                "Введите новую роль (1-Клиент, 2-Мастер, 3-Менеджер, 4-Админ):",
                "Смена роли", selectedUser.RoleId.ToString());

            if (int.TryParse(roleStr, out int newRoleId) && newRoleId >= 1 && newRoleId <= 4)
            {
                try
                {
                    using (var context = new BeautySalonEntities())
                    {
                        var user = context.Users.Find(selectedUser.Id);
                        if (user != null)
                            user.RoleId = newRoleId;
                        context.SaveChanges();
                    }
                    LoadUsers();
                    MessageBox.Show("Роль изменена.", "Успех");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Некорректный код роли.", "Ошибка");
            }
        }

        // ==================== КНОПКА "НАЗАД" ====================
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
            else
                NavigationService.Navigate(new StartPage());
        }
    }
}
