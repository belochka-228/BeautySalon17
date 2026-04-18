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
using System.Windows.Shapes;
using BeautySalon17.Pages;
namespace BeautySalon17.Windows
{
    /// <summary>
    /// Логика взаимодействия для SelectMasterTimeWindow.xaml
    /// </summary>
    public partial class SelectMasterTimeWindow : Window
    {
        private int _serviceId;   // ID выбранной услуги

        public SelectMasterTimeWindow(int serviceId)
        {
            InitializeComponent();
            _serviceId = serviceId;

            LoadMasters();        // загружаем мастеров, которые делают эту услугу
            LoadTimeSlots();      // загружаем фиксированные слоты времени
        }

        /// <summary>
        /// Загружает в ComboBox мастеров, оказывающих выбранную услугу.
        /// </summary>
        private void LoadMasters()
        {
            try
            {
                using (var context = new BeautySalonEntities())
                {
                    // Получаем Id мастеров из таблицы MasterServices
                    var masterIds = context.MasterServices
                                           .Where(ms => ms.ServiceId == _serviceId)
                                           .Select(ms => ms.MasterId)
                                           .ToList();

                    // Получаем самих мастеров (пользователи с ролью 2)
                    var masters = context.Users
                                         .Where(u => masterIds.Contains(u.Id) && u.RoleId == 2)
                                         .Select(u => new { u.Id, FullName = u.Surname + " " + u.Name })
                                         .ToList();

                    CmbMaster.ItemsSource = masters;
                    CmbMaster.SelectedIndex = 0;   // выбираем первого по умолчанию
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки мастеров: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        /// <summary>
        /// Создаёт фиксированные временные слоты с 9:00 до 20:00 с шагом 1 час.
        /// </summary>
        private void LoadTimeSlots()
        {
            var timeSlots = new List<object>();
            DateTime start = DateTime.Today.AddHours(9);  // 9:00
            DateTime end = DateTime.Today.AddHours(20);   // 20:00

            while (start <= end)
            {
                timeSlots.Add(new { TimeSlot = start.ToString("HH:mm") });
                start = start.AddHours(1);
            }

            CmbTime.ItemsSource = timeSlots;
            CmbTime.SelectedIndex = 0;
        }

        // Кнопка "Отмена"
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        // Кнопка "Далее"
        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, что мастер и время выбраны
            if (CmbMaster.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите мастера.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CmbTime.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите время.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Получаем выбранные значения
            dynamic selectedMaster = CmbMaster.SelectedItem;
            int masterId = selectedMaster.Id;

            dynamic selectedTime = CmbTime.SelectedItem;
            string timeStr = selectedTime.TimeSlot;  // "HH:mm"

            // Формируем дату и время (сегодня + выбранное время)
            DateTime appointmentDateTime = DateTime.Today.Add(TimeSpan.Parse(timeStr));

            // Открываем страницу подтверждения записи
            AppointmentConfirmPage confirmPage = new AppointmentConfirmPage(_serviceId, masterId, appointmentDateTime);

            // Закрываем окно и возвращаем успешный результат
            this.DialogResult = true;
            this.Close();

            // Переход на страницу подтверждения (через главное окно)
            // Находим главное окно и его Frame для навигации
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.MainFrame.Navigate(confirmPage);
            }
        }
    }
}
