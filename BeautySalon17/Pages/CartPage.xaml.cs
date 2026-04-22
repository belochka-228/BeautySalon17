using BeautySalon17.Helpers;
using BeautySalon17.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BeautySalon17.Pages
{
    /// <summary>
    /// Страница корзины
    /// </summary>
    public partial class CartPage : Page
    {
        // Список позиций в корзине для текущего пользователя
        private List<CartItems> _cartItems;

        /// <summary>
        /// Конструктор. При создании страницы сразу загружаю содержимое корзины из базы
        /// </summary>
        public CartPage()
        {
            InitializeComponent();
            LoadCartItems();   // гружу товары и отображаю их
        }
        /// <summary>
        /// Загружаю из базы все позиции корзины для текущего авторизованного пользователя
        /// Подгружаю связанный товар (Products), чтобы получить его название, цену и картинку
        /// После загрузки отрисовываю карточки и обновляю итоговую сумму
        /// </summary>
        private void LoadCartItems()
        {
            try
            {
                using (var db = new BeautySalonEntities())
                {
                    // Include("Products") нужен, чтобы вместе с позицией корзины загрузился сам товар
                    _cartItems = db.CartItems.Include("Products").Where(c => c.UserId == CurrentUser.Id).ToList();
                }
                DisplayCartItems();   // создаю визуальные карточки для каждого товара
                UpdateTotalPrice();   // пересчитываю и показываю итоговую сумму
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки корзины: {ex.Message}");
            }
        }
        /// <summary>
        /// Очищаю панель с товарами и заново заполняю её карточками из списка _cartItems
        /// Если корзина пуста – показываю сообщение и блокирую кнопку оформления заказа
        /// </summary>
        private void DisplayCartItems()
        {
            CartItemsWrapPanel.Children.Clear();

            // Если список пуст или ещё не загружен
            if (_cartItems == null || !_cartItems.Any())
            {
                CartItemsWrapPanel.Children.Add(new TextBlock
                {
                    Text = "Корзина пуста",
                    FontSize = 16,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(20)
                });
                BtnCheckout.IsEnabled = false;   // нельзя оформить пустой заказ
                return;
            }

            // Если есть товары – активирую кнопку оформления и рисую карточки
            BtnCheckout.IsEnabled = true;
            foreach (var item in _cartItems)
                CartItemsWrapPanel.Children.Add(CreateCartItemCard(item));
        }

        /// <summary>
        /// Создаю визуальную карточку для одной позиции корзины
        /// Внутри: картинка, название, цена за штуку, управление количеством и кнопка удаления
        /// </summary>
        /// <param name="item">Позиция корзины (содержит товар и количество)</param>
        /// <returns>Готовый элемент управления (Border) с карточкой</returns>
        private FrameworkElement CreateCartItemCard(CartItems item)
        {
            var product = item.Products;   // сам товар
            // Цена за одну штуку с учётом скидки
            decimal unitPrice = product.Price * (1 - product.Discount / 100m);

            // Основная рамка с закруглёнными углами
            var border = new Border
            {
                Width = 200,
                Height = 350,
                Margin = new Thickness(10),
                Background = Brushes.White,
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8)
            };

            // Сетка на 5 строк: картинка, название, цена, количество, кнопка удаления
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(150) }); // картинка
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });    // название
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });    // цена за шт.
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });    // управление кол-вом
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });    // кнопка "Удалить"

            // Картинка товара
            var img = new Image
            {
                Stretch = Stretch.Uniform,
                Margin = new Thickness(5),
                Source = LoadProductImage(product.ImagePath)   // загружаю картинку с диска
            };
            Grid.SetRow(img, 0);
            grid.Children.Add(img);

            // Название товара
            var txtName = new TextBlock
            {
                Text = product.Name,
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(5, 0, 5, 0)
            };
            Grid.SetRow(txtName, 1);
            grid.Children.Add(txtName);

            // Цена за одну штуку
            var txtPrice = new TextBlock
            {
                Text = $"{unitPrice:F2} руб.",
                FontSize = 12,
                Foreground = Brushes.Green,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(5)
            };
            Grid.SetRow(txtPrice, 2);
            grid.Children.Add(txtPrice);

            // Панель управления количеством (кнопки "-" и "+", число) 
            var qtyPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 5, 0, 0)
            };

            // Кнопка "минус" – активна только если количество > 1
            Button btnMinus = new Button
            {
                Content = "−",
                Width = 25,
                Height = 25,
                Tag = item.Id,
                IsEnabled = item.Quantity > 1
            };
            btnMinus.Click += (s, e) => ChangeQuantity(item.Id, -1);

            // Текстовое поле с текущим количеством
            TextBlock txtQty = new TextBlock
            {
                Text = item.Quantity.ToString(),
                Width = 30,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 0, 5, 0),
                FontWeight = FontWeights.Bold
            };

            // Кнопка "плюс"
            Button btnPlus = new Button
            {
                Content = "+",
                Width = 25,
                Height = 25,
                Tag = item.Id
            };
            btnPlus.Click += (s, e) => ChangeQuantity(item.Id, 1);

            // Собираю панель
            qtyPanel.Children.Add(btnMinus);
            qtyPanel.Children.Add(txtQty);
            qtyPanel.Children.Add(btnPlus);

            Grid.SetRow(qtyPanel, 3);
            grid.Children.Add(qtyPanel);

            // Кнопка удаления товара из корзины
            Button btnDelete = new Button
            {
                Content = "Удалить",
                Height = 30,
                Margin = new Thickness(10, 5, 10, 10),
                Tag = item.Id
            };
            btnDelete.Click += BtnDelete_Click;

            Grid.SetRow(btnDelete, 4);
            grid.Children.Add(btnDelete);

            // Вкладываю сетку в рамку
            border.Child = grid;
            return border;
        }
        /// <summary>
        /// Изменяю количество конкретного товара в корзине
        /// После изменения перезагружаю всю корзину, чтобы обновился интерфейс
        /// </summary>
        /// <param name="cartItemId">ID позиции в корзине (таблица CartItems)</param>
        /// <param name="delta">На сколько изменить (+1 или -1)</param>
        private void ChangeQuantity(int cartItemId, int delta)
        {
            try
            {
                using (var db = new BeautySalonEntities())
                {
                    var cartItem = db.CartItems.Find(cartItemId);
                    if (cartItem != null)
                    {
                        int newQty = cartItem.Quantity + delta;
                        if (newQty > 0)
                        {
                            cartItem.Quantity = newQty;
                            db.SaveChanges();
                        }
                        // Если количество стало 0 – удаляем через отдельный метод
                    }
                }
                LoadCartItems();   // перерисовываю корзину с обновлёнными данными
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка изменения количества: {ex.Message}");
            }
        }

        /// <summary>
        /// Удаляю позицию из корзины после подтверждения пользователем
        /// </summary>
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn)) return;
            int id = (int)btn.Tag;   // ID позиции в корзине

            // Спрашиваю подтверждение, чтобы случайно не удалить
            if (MessageBox.Show("Удалить товар из корзины?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new BeautySalonEntities())
                    {
                        var item = db.CartItems.Find(id);
                        if (item != null)
                        {
                            db.CartItems.Remove(item);
                            db.SaveChanges();
                        }
                    }
                    LoadCartItems();   // перерисовываю корзину
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}");
                }
            }
        }
        /// <summary>
        /// Пересчитываю общую стоимость всех товаров в корзине с учётом количества и скидок
        /// Результат вывожу в текстовое поле TxtTotalPrice
        /// </summary>
        private void UpdateTotalPrice()
        {
            if (_cartItems == null || !_cartItems.Any())
            {
                TxtTotalPrice.Text = "0.00 руб.";
                return;
            }

            decimal total = _cartItems.Sum(item =>
            {
                var p = item.Products;
                // Цена со скидкой * количество
                return p.Price * (1 - p.Discount / 100m) * item.Quantity;
            });

            TxtTotalPrice.Text = $"{total:F2} руб.";
        }
        /// <summary>
        /// Открываю окно оформления заказа (OrderWindow)
        /// Если заказ успешно создан (окно вернуло true), перезагружаю корзину
        /// </summary>
        private void BtnCheckout_Click(object sender, RoutedEventArgs e)
        {
            if (_cartItems == null || !_cartItems.Any())
            {
                MessageBox.Show("Корзина пуста. Добавьте товары.");
                return;
            }

            var orderWindow = new OrderWindow();
            orderWindow.Owner = Window.GetWindow(this);
            // ShowDialog() возвращает true, если заказ оформлен
            if (orderWindow.ShowDialog() == true)
                LoadCartItems();   // корзина очистилась – обновляю страницу
        }
        /// <summary>
        /// Загружаю картинку товара с диска по относительному пути из базы
        /// Если файл не найден – возвращаю null (картинка не отобразится)
        /// </summary>
        private BitmapImage LoadProductImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath)) return null;
            try
            {
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, imagePath);
                if (File.Exists(fullPath))
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(fullPath);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    return bmp;
                }
            }
            catch { /* если что-то пошло не так – просто верну null */ }
            return null;
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