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

namespace BeautySalon17.Windows
{
    /// <summary>
    /// Логика взаимодействия для OrderWindow.xaml
    /// </summary>
    public partial class OrderWindow : Window
    {
        private List<CartItems> _cartItems;

        public OrderWindow()
        {
            InitializeComponent();

            // Загружаем корзину текущего пользователя
            LoadCartItems();

            // Устанавливаем ограничения на выбор даты: от сегодня до +7 дней
            DpPickupDate.DisplayDateStart = DateTime.Today;
            DpPickupDate.DisplayDateEnd = DateTime.Today.AddDays(7);

            // Если корзина пуста, сразу предупреждаем и закрываем окно
            if (_cartItems == null || _cartItems.Count == 0)
            {
                MessageBox.Show("Ваша корзина пуста.",
                                "Корзина пуста", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
        }

        /// <summary>
        /// Загружает все позиции корзины для текущего авторизованного пользователя.
        /// </summary>
        private void LoadCartItems()
        {
            try
            {
                using (var context = new BeautySalonEntities())
                {
                    _cartItems = context.CartItems.Include("Products") .Where(ci => ci.UserId == Helpers.CurrentUser.Id).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки корзины: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        /// <summary>
        /// Обработчик кнопки "Отмена". Просто закрывает окно без сохранения.
        /// </summary>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;  // Указываем, что окно закрыто с результатом false
            this.Close();
        }

        /// <summary>
        /// Обработчик кнопки "Подтвердить". Создаёт заказ и очищает корзину.
        /// </summary>
        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, что дата выбрана
            if (!DpPickupDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Пожалуйста, выберите дату получения.", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime selectedDate = DpPickupDate.SelectedDate.Value.Date;
            DateTime today = DateTime.Today;
            DateTime maxDate = today.AddDays(7);

            // Проверяем корректность даты
            if (selectedDate < today)
            {
                MessageBox.Show("Дата получения не может быть раньше сегодняшнего дня.",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (selectedDate > maxDate)
            {
                MessageBox.Show("Дата получения не может быть позже, чем через 7 дней от сегодня.",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверяем, что корзина не пуста
            if (_cartItems == null || _cartItems.Count == 0)
            {
                MessageBox.Show("Ваша корзина пуста. Нечего оформлять.",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.Close();
                return;
            }

            // Определяем способ оплаты
            string paymentMethod = RbCash.IsChecked == true ? "Наличные" : "Банковская карта";

            // Создаём заказ в базе данных
            try
            {
                using (var context = new BeautySalonEntities())
                {
                    // Создаём "шапку" заказа
                    Orders newOrder = new Orders
                    {
                        ClientId = Helpers.CurrentUser.Id,
                        OrderDate = DateTime.Now,
                        PickupDate = selectedDate,
                        PaymentMethod = paymentMethod,
                        Status = "New"
                    };
                    context.Orders.Add(newOrder);
                    context.SaveChanges();  // чтобы получить Id нового заказа

                    // Переносим позиции из корзины в OrderItems
                    foreach (var cartItem in _cartItems)
                    {
                        // Цена товара на момент заказа (берём из связанного Product)
                        decimal price = cartItem.Products?.Price ?? 0;

                        OrderItems orderItem = new OrderItems
                        {
                            OrderId = newOrder.Id,
                            ProductId = cartItem.ProductId,
                            Quantity = cartItem.Quantity,
                            Price = price * (1 - cartItem.Products.Discount / 100m)  // цена со скидкой
                        };
                        context.OrderItems.Add(orderItem);
                    }

                    // Удаляем все позиции корзины текущего пользователя
                    var userCartItems = context.CartItems.Where(ci => ci.UserId == Helpers.CurrentUser.Id);
                    context.CartItems.RemoveRange(userCartItems);

                    // Сохраняем изменения
                    context.SaveChanges();
                }

                MessageBox.Show($"Заказ успешно оформлен! Дата получения: {selectedDate:dd.MM.yyyy}",
                                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true;  // Окно закрыто с успехом
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при оформлении заказа: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
