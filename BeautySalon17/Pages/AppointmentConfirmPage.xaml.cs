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
        private readonly int _serviceId, _masterId;
        private readonly DateTime _dateTime;
        private string _serviceName, _masterName;
        private decimal _price;

        public AppointmentConfirmPage(int serviceId, int masterId, DateTime dateTime)
        {
            InitializeComponent();
            _serviceId = serviceId;
            _masterId = masterId;
            _dateTime = dateTime;
            LoadAndDisplay();
        }

        // Загружаем данные из БД и сразу показываем на экране
        private void LoadAndDisplay()
        {
            try
            {
                using (var db = new BeautySalonEntities())
                {
                    var service = db.Services.Find(_serviceId);
                    if (service != null)
                    {
                        _serviceName = service.Name;
                        _price = service.Price;
                    }

                    var master = db.Users.Find(_masterId);
                    if (master != null)
                        _masterName = $"{master.Surname} {master.Name}";
                }

                TxtServiceInfo.Text = $"{_serviceName} — {_price:F2} руб.";
                TxtMasterInfo.Text = $"Мастер: {_masterName}";
                TxtDateTimeInfo.Text = $"{_dateTime:dd.MM.yyyy HH:mm}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
            else
                NavigationService.Navigate(new StartPage());
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            string payment = RbCash.IsChecked == true ? "Наличные" : "Банковская карта";
            string comment = TxtComment.Text.Trim();

            try
            {
                using (var db = new BeautySalonEntities())
                {
                    // Проверка: не записан ли уже клиент на то же время к тому же мастеру
                    bool exists = db.Appointments.Any(a =>
                        a.ClientId == CurrentUser.Id &&
                        a.MasterId == _masterId &&
                        a.ServiceId == _serviceId &&
                        a.AppointmentDateTime == _dateTime &&
                        a.Status != "Cancelled");

                    if (exists)
                    {
                        MessageBox.Show("Вы уже записаны на это время к данному мастеру.", "Внимание",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    db.Appointments.Add(new Appointments
                    {
                        ClientId = CurrentUser.Id,
                        MasterId = _masterId,
                        ServiceId = _serviceId,
                        AppointmentDateTime = _dateTime,
                        Status = "Pending",
                        PaymentMethod = payment,
                        Comment = string.IsNullOrEmpty(comment) ? null : comment
                    });

                    db.SaveChanges();
                }

                MessageBox.Show("Вы успешно записаны!", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.Navigate(new StartPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при записи: {ex.Message}");
            }
        }
    }
}