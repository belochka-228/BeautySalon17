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
    /// Логика взаимодействия для ClientAccountPage.xaml
    /// </summary>
    public partial class ClientAccountPage : Page
    {
        private List<Appointments> _appointments; // все записи клиента

        public ClientAccountPage()
        {
            InitializeComponent();
            LoadAllData();
        }

        // Загружаем записи на услуги и заказы товаров
        private void LoadAllData()
        {
            try
            {
                using (var db = new BeautySalonEntities())
                {
                    // Записи (с мастером и услугой)
                    _appointments = db.Appointments
                                      .Include("Services")
                                      .Include("Users1")   // мастер (навигационное свойство)
                                      .Where(a => a.ClientId == CurrentUser.Id)
                                      .OrderByDescending(a => a.AppointmentDateTime)
                                      .ToList();
                    DgAppointments.ItemsSource = _appointments;

                    // Заказы товаров
                    var orders = db.Orders
                                   .Where(o => o.ClientId == CurrentUser.Id)
                                   .OrderByDescending(o => o.OrderDate)
                                   .ToList();
                    DgOrders.ItemsSource = orders;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        // Фильтр записей по выбранной дате
        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            if (_appointments == null) return;

            if (DpFilterDate.SelectedDate.HasValue)
            {
                DateTime date = DpFilterDate.SelectedDate.Value.Date;
                DgAppointments.ItemsSource = _appointments.Where(a => a.AppointmentDateTime.Date == date).ToList();
            }
            else
            {
                DgAppointments.ItemsSource = _appointments;
            }
        }

        // Сброс фильтра
        private void BtnResetFilter_Click(object sender, RoutedEventArgs e)
        {
            DpFilterDate.SelectedDate = null;
            DgAppointments.ItemsSource = _appointments;
        }

        // Кнопка "Назад"
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
            else
                NavigationService.Navigate(new StartPage());
        }
    }
}

