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
    /// Логика взаимодействия для AppointmentDetailPage.xaml
    /// </summary>
    public partial class AppointmentDetailPage : Page
    {
        private int _appointmentId;          // ID записи
        private Appointments _appointment;   // сама запись

        public AppointmentDetailPage(int appointmentId)
        {
            InitializeComponent();
            _appointmentId = appointmentId;
            LoadAppointmentDetails();
        }

        private void LoadAppointmentDetails()
        {
            try
            {
                using (var context = new BeautySalonEntities())
                {
                    // Загружаем запись вместе с услугой и клиентом
                    _appointment = context.Appointments
                                          .Include("Services")
                                          .Include("Users")   // клиент
                                          .FirstOrDefault(a => a.Id == _appointmentId);

                    if (_appointment == null)
                    {
                        MessageBox.Show("Запись не найдена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        NavigationService.GoBack();
                        return;
                    }

                    // Заполняем текстовые поля
                    TxtDateTime.Text = $"Дата и время: {_appointment.AppointmentDateTime:dd.MM.yyyy HH:mm}";
                    TxtService.Text = $"Услуга: {_appointment.Services?.Name ?? "Не указана"}";
                    TxtClientName.Text = $"Клиент: {_appointment.Users?.Surname} {_appointment.Users?.Name}";
                    TxtClientPhone.Text = $"Телефон: {_appointment.Users?.Phone ?? "Не указан"}";
                    TxtStatus.Text = $"Статус: {_appointment.Status}";


                    // Если запись уже выполнена, скрываем кнопку "Завершить"
                    if (_appointment.Status == "Completed")
                        BtnComplete.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnComplete_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Отметить запись как выполненную?", "Подтверждение",
                                         MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var context = new BeautySalonEntities())
                    {
                        var dbAppointment = context.Appointments.Find(_appointmentId);
                        if (dbAppointment != null)
                        {
                            dbAppointment.Status = "Completed";
                            context.SaveChanges();
                        }
                    }
                    MessageBox.Show("Статус записи обновлён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    NavigationService.GoBack(); // возвращаемся к списку
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления статуса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
            else
                NavigationService.Navigate(new StartPage());
        }
    }
}
