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
        private List<Appointments> _allAppointments;

        public ClientAccountPage()
        {
            InitializeComponent();
            LoadAppointments();
            LoadOrders();
        }

        private void LoadAppointments()
        {
            try
            {
                using (var context = new BeautySalonEntities())
                {
                    _allAppointments = context.Appointments
                                              .Include("Services")
                                              .Include("Users1")   // мастер
                                              .Where(a => a.ClientId == CurrentUser.Id)
                                              .OrderByDescending(a => a.AppointmentDateTime)
                                              .ToList();
                }
                DgAppointments.ItemsSource = _allAppointments;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки записей: {ex.Message}");
            }
        }

        private void LoadOrders()
        {
            try
            {
                using (var context = new BeautySalonEntities())
                {
                    var orders = context.Orders
                                        .Where(o => o.ClientId == CurrentUser.Id)
                                        .OrderByDescending(o => o.OrderDate)
                                        .ToList();
                    DgOrders.ItemsSource = orders;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}");
            }
        }

        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            if (_allAppointments == null) return;

            if (DpFilterDate.SelectedDate.HasValue)
            {
                DateTime selected = DpFilterDate.SelectedDate.Value.Date;
                var filtered = _allAppointments.Where(a => a.AppointmentDateTime.Date == selected).ToList();
                DgAppointments.ItemsSource = filtered;
            }
            else
            {
                DgAppointments.ItemsSource = _allAppointments;
            }
        }

        private void BtnResetFilter_Click(object sender, RoutedEventArgs e)
        {
            DpFilterDate.SelectedDate = null;
            DgAppointments.ItemsSource = _allAppointments;
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

