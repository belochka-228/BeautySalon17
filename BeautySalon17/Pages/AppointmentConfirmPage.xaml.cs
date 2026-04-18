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
    /// Логика взаимодействия для AppointmentConfirmPage.xaml
    /// </summary>
    public partial class AppointmentConfirmPage : Page
    {
        private int _serviceId;
        private int _masterId;
        private DateTime _appointmentDateTime;

        private string _serviceName;
        private string _masterFullName;
        private decimal _servicePrice;

        public AppointmentConfirmPage(int serviceId, int masterId, DateTime appointmentDateTime)
        {
            InitializeComponent();
            _serviceId = serviceId;
            _masterId = masterId;
            _appointmentDateTime = appointmentDateTime;

            LoadDetails();   // загружаем названия услуги и мастера
            DisplayInfo();   // показываем на экране
        }

        /// <summary>
        /// Загружает из базы название услуги и имя мастера.
        /// </summary>
        private void LoadDetails()
        {
            try
            {
                using (var context = new BeautySalonEntities())
                {
                    var service = context.Services.Find(_serviceId);
                    if (service != null)
                    {
                        _serviceName = service.Name;
                        _servicePrice = service.Price;
                    }

                    var master = context.Users.Find(_masterId);
                    if (master != null)
                    {
                        _masterFullName = $"{master.Surname} {master.Name}";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Отображает информацию на странице.
        /// </summary>
        private void DisplayInfo()
        {
            TxtServiceInfo.Text = $"Услуга: {_serviceName} ( {_servicePrice:F2} руб. )";
            TxtMasterInfo.Text = $"Мастер: {_masterFullName}";
            TxtDateTimeInfo.Text = $"Дата и время: {_appointmentDateTime:dd.MM.yyyy HH:mm}";
        }

        // Кнопка "Назад"
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
            else
                NavigationService.Navigate(new StartPage());
        }

        // Кнопка "Записаться"
        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            // Получаем способ оплаты
            string paymentMethod = RbCash.IsChecked == true ? "Наличные" : "Банковская карта";
            string comment = TxtComment.Text.Trim();

            try
            {
                using (var context = new BeautySalonEntities())
                {
                    Appointments newAppointment = new Appointments
                    {
                        ClientId = CurrentUser.Id,
                        MasterId = _masterId,
                        ServiceId = _serviceId,
                        AppointmentDateTime = _appointmentDateTime,
                        Status = "Pending",
                        Comment = string.IsNullOrEmpty(comment) ? null : comment,
                        PaymentMethod = paymentMethod
                    };
                    context.Appointments.Add(newAppointment);
                    context.SaveChanges();
                }

                MessageBox.Show("Вы успешно записаны!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Возвращаемся на стартовую страницу
                NavigationService.Navigate(new StartPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при записи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
