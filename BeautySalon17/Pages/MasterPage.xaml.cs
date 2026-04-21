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
    /// Логика взаимодействия для MasterPage.xaml
    /// </summary>
    public partial class MasterPage : Page
    {
        public MasterPage()
        {
            InitializeComponent();
            // Подписываемся на событие Loaded – оно срабатывает каждый раз,
            // когда страница загружается (в том числе после возврата с другой страницы)
            this.Loaded += Page_Loaded;
        }

        // ==================== СОБЫТИЕ ЗАГРУЗКИ СТРАНИЦЫ ====================
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAppointments();      // обновляем список записей
            LoadMyServices();        // обновляем список "Мои услуги"
            LoadAvailableServices(); // обновляем список доступных услуг
        }

        // ==================== ЗАПИСИ МАСТЕРА ====================
        private void LoadAppointments()
        {
            try
            {
                using (var context = new BeautySalonEntities())
                {
                    var appointments = context.Appointments
                                              .Include("Services")   // услуга
                                              .Include("Users")      // клиент
                                              .Where(a => a.MasterId == CurrentUser.Id)
                                              .OrderByDescending(a => a.AppointmentDateTime)
                                              .ToList();
                    DgAppointments.ItemsSource = appointments;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки записей: {ex.Message}");
            }
        }

        // Обработчик двойного клика по строке таблицы – открывает страницу деталей записи
        private void DgAppointments_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DgAppointments.SelectedItem is Appointments appointment)
            {
                NavigationService.Navigate(new AppointmentDetailPage(appointment.Id));
            }
        }

        // Кнопка "Выполнено" в таблице записей
        private void BtnComplete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Appointments appointment)
            {
                if (appointment.Status == "Completed")
                {
                    MessageBox.Show("Эта запись уже выполнена.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

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
                        LoadAppointments(); // обновляем таблицу
                        MessageBox.Show("Статус записи обновлён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка обновления статуса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        // ==================== УПРАВЛЕНИЕ УСЛУГАМИ МАСТЕРА ====================
        private void LoadMyServices()
        {
            try
            {
                using (var context = new BeautySalonEntities())
                {
                    var myServiceIds = context.MasterServices
                                              .Where(ms => ms.MasterId == CurrentUser.Id)
                                              .Select(ms => ms.ServiceId)
                                              .ToList();

                    var myServices = context.Services
                                            .Where(s => myServiceIds.Contains(s.Id))
                                            .ToList();
                    LbMyServices.ItemsSource = myServices;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки моих услуг: {ex.Message}");
            }
        }

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

        private void BtnAddService_Click(object sender, RoutedEventArgs e)
        {
            if (LbAvailableServices.SelectedItem is Services selectedService)
            {
                try
                {
                    using (var context = new BeautySalonEntities())
                    {
                        bool exists = context.MasterServices.Any(ms => ms.MasterId == CurrentUser.Id && ms.ServiceId == selectedService.Id);
                        if (!exists)
                        {
                            context.MasterServices.Add(new MasterServices
                            {
                                MasterId = CurrentUser.Id,
                                ServiceId = selectedService.Id
                            });
                            context.SaveChanges();
                            LoadMyServices();
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