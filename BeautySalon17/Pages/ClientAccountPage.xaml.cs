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
        public ClientAccountPage()
        {
            InitializeComponent();
            // Загружаем записи и заказы при открытии страницы
            LoadAppointments();
            LoadOrders();
        }
        private void LoadAppointments()
        {
            try
            {
                using (var context = new BeautySalonEntities())
                {
                    // Берём все записи, где ClientId == ID текущего пользователя
                    List<Appointments> appointments = context.Appointments.Where(a => a.ClientId == CurrentUser.Id).OrderByDescending(a => a.AppointmentDateTime).ToList();
                    // Отдаём список таблице
                    DgAppointments.ItemsSource = appointments;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки записей: {ex.Message}");
            }
        }

        // ==================== ЗАГРУЗКА ЗАКАЗОВ ====================
        private void LoadOrders()
        {
            try
            {
                using (var context = new BeautySalonEntities())
                {
                    // Берём все заказы текущего клиента
                    List<Orders> orders = context.Orders.Where(o => o.ClientId == CurrentUser.Id).OrderByDescending(o => o.OrderDate).ToList();
                    // Отдаём список таблице
                    DgOrders.ItemsSource = orders;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}");
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
    
