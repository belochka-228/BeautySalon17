using BeautySalon17.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BeautySalon17.Pages
{
    /// <summary>
    /// Личный кабинет мастера
    /// Здесь видно записи, можно отметить их выполненными и управлять списком услуг
    /// </summary>
    public partial class MasterPage : Page
    {
        public MasterPage()
        {
            InitializeComponent();
            // Событие Loaded – оно нужно, чтобы данные обновлялись каждый раз
            this.Loaded += Page_Loaded;
        }
        /// <summary>
        /// Срабатывает каждый раз, когда страница загружается
        /// Обновляю все списки, чтобы видеть актуальную информацию
        /// </summary>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAppointments();      // обновляю таблицу с записями
            LoadMyServices();        // обновляю список услуг
            LoadAvailableServices(); // обновляю список всех доступных услуг
        }
        /// <summary>
        /// Загружаю из базы все записи, которые назначены на меня (текущего мастера)
        /// Подгружаю связанные услугу и клиента, чтобы показать их в таблице
        /// </summary>
        private void LoadAppointments()
        {
            try
            {
                using (var context = new BeautySalonEntities())
                {
                    var appointments = context.Appointments.Include("Services").Include("Users").Where(a => a.MasterId == CurrentUser.Id).OrderByDescending(a => a.AppointmentDateTime) .ToList();
                    DgAppointments.ItemsSource = appointments;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки записей: {ex.Message}");
            }
        }
        /// <summary>
        /// Обработчик двойного клика по строке таблицы записей
        /// Открываю страницу с подробной информацией о выбранной записи
        /// </summary>
        private void DgAppointments_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DgAppointments.SelectedItem is Appointments appointment)
                NavigationService.Navigate(new AppointmentDetailPage(appointment.Id));
        }
        /// <summary>
        /// Кнопка "Выполнено" в таблице записей
        /// Меняю статус записи на "Completed", если клиент пришёл и услуга оказана
        /// </summary>
        private void BtnComplete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Appointments appointment)
            {
                if (appointment.Status == "Completed")
                {
                    MessageBox.Show("Эта запись уже выполнена.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                // Спрашиваю подтверждение, чтобы случайно не нажать
                var result = MessageBox.Show($"Отметить запись на {appointment.AppointmentDateTime:dd.MM.yyyy HH:mm} как выполненную?",
                                             "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var context = new BeautySalonEntities())
                        {
                            var dbAppointment = context.Appointments.Find(appointment.Id);
                            if (dbAppointment != null)
                            {
                                dbAppointment.Status = "Completed";
                                context.SaveChanges();
                            }
                        }
                        LoadAppointments(); // перезагружаю таблицу, чтобы статус обновился
                        MessageBox.Show("Статус записи обновлён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка обновления статуса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        /// <summary>
        /// Загружаю список услуг, которые уже оказываются
        /// </summary>
        private void LoadMyServices()
        {
            try
            {
                using (var context = new BeautySalonEntities())
                {
                    // Получаю ID услуг из таблицы MasterServices
                    var myServiceIds = context.MasterServices.Where(ms => ms.MasterId == CurrentUser.Id).Select(ms => ms.ServiceId).ToList();

                    // Загружаю сами услуги по этим ID
                    var myServices = context.Services.Where(s => myServiceIds.Contains(s.Id)).ToList();
                    LbMyServices.ItemsSource = myServices;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки моих услуг: {ex.Message}");
            }
        }
        /// <summary>
        /// Загружаю список вообще всех услуг из базы (для левого списка "Доступные услуги")
        /// </summary>
        private void LoadAvailableServices()
        {
            try
            {
                using (var context = new BeautySalonEntities())
                {
                    var allServices = context.Services.ToList();
                    LbAvailableServices.ItemsSource = allServices;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки доступных услуг: {ex.Message}");
            }
        }
        /// <summary>
        /// Кнопка "Добавить" – добавляю выбранную в левом списке услугу в список
        /// </summary>
        private void BtnAddService_Click(object sender, RoutedEventArgs e)
        {
            if (LbAvailableServices.SelectedItem is Services selectedService)
            {
                try
                {
                    using (var context = new BeautySalonEntities())
                    {
                        // Проверяю, нет ли уже такой услуги меня
                        bool exists = context.MasterServices.Any(ms => ms.MasterId == CurrentUser.Id && ms.ServiceId == selectedService.Id);
                        if (!exists)
                        {
                            context.MasterServices.Add(new MasterServices
                            {
                                MasterId = CurrentUser.Id,
                                ServiceId = selectedService.Id
                            });
                            context.SaveChanges();
                            LoadMyServices(); // обновляю правый список
                        }
                        else
                        {
                            MessageBox.Show("Эта услуга уже добавлена.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка добавления услуги: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Выберите услугу из списка доступных.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        /// <summary>
        /// Кнопка "Удалить" – убираю выбранную в правом списке услугу из своего списка
        /// </summary>
        private void BtnRemoveService_Click(object sender, RoutedEventArgs e)
        {
            if (LbMyServices.SelectedItem is Services selectedService)
            {
                var result = MessageBox.Show($"Удалить услугу \"{selectedService.Name}\" из вашего списка?",
                                             "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var context = new BeautySalonEntities())
                        {
                            var link = context.MasterServices.FirstOrDefault(ms => ms.MasterId == CurrentUser.Id && ms.ServiceId == selectedService.Id);
                            if (link != null)
                            {
                                context.MasterServices.Remove(link);
                                context.SaveChanges();
                                LoadMyServices();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления услуги: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите услугу из списка 'Мои услуги'.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
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