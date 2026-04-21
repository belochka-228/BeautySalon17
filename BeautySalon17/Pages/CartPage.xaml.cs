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
    public partial class CartPage : Page
    {
        private List<CartItems> _cartItems; // список товаров в корзине текущего юзера
        public CartPage()
        {
            InitializeComponent();
            LoadCartItems(); // при открытии страницы сразу грузим корзину
        }
        // Загрузка позиций корзины из БД
        private void LoadCartItems()
        {
            try
            {
                using (var db = new BeautySalonEntities())
                {
                    _cartItems = db.CartItems.Include("Products")
                                             .Where(c => c.UserId == CurrentUser.Id)
                                             .ToList();
                }
                DisplayCartItems();
                UpdateTotalPrice();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки корзины: {ex.Message}");
            }
        }
        // Отрисовка карточек товаров в WrapPanel
        private void DisplayCartItems()
        {
            CartItemsWrapPanel.Children.Clear();

            if (_cartItems == null || !_cartItems.Any())
            {
                // если пусто – показываем заглушку
                CartItemsWrapPanel.Children.Add(new TextBlock
                {
                    Text = "Корзина пуста",
                    FontSize = 16,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(20)
                });
                BtnCheckout.IsEnabled = false;
                return;
            }

            BtnCheckout.IsEnabled = true;
            foreach (var item in _cartItems)
                CartItemsWrapPanel.Children.Add(CreateCartItemCard(item));
        }
        // Создание UI-карточки для одного элемента корзины
        private FrameworkElement CreateCartItemCard(CartItems item)
        {
            var product = item.Products;
            decimal unitPrice = product.Price * (1 - product.Discount / 100m);

            // Основная рамка карточки
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
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(150) }); // картинка
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });    // название
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });    // цена
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });    // кол-во
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });    // кнопка удалить
            // Изображение товара
            var img = new Image
            {
                Stretch = Stretch.Uniform,
                Margin = new Thickness(5),
                Source = LoadProductImage(product.ImagePath)
            };
            Grid.SetRow(img, 0);
            grid.Children.Add(img);
            // Название
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
            // Цена за штуку
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
            // Панель управления количеством
            var qtyPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 5, 0, 0)
            };
            Button btnMinus = new Button
            {
                Content = "−",
                Width = 25,
                Height = 25,
                Tag = item.Id,
                IsEnabled = item.Quantity > 1
            };
            btnMinus.Click += (s, e) => ChangeQuantity(item.Id, -1);

            TextBlock txtQty = new TextBlock
            {
                Text = item.Quantity.ToString(),
                Width = 30,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 0, 5, 0),
                FontWeight = FontWeights.Bold
            };
            Button btnPlus = new Button
            {
                Content = "+",
                Width = 25,
                Height = 25,
                Tag = item.Id
            };
            btnPlus.Click += (s, e) => ChangeQuantity(item.Id, 1);

            qtyPanel.Children.Add(btnMinus);
            qtyPanel.Children.Add(txtQty);
            qtyPanel.Children.Add(btnPlus);
            Grid.SetRow(qtyPanel, 3);
            grid.Children.Add(qtyPanel);
            // Кнопка удаления
            Button btnDelete = new Button
            {
                Content = "🗑️ Удалить",
                Height = 30,
                Margin = new Thickness(10, 5, 10, 10),
                Tag = item.Id
            };
            btnDelete.Click += BtnDelete_Click;
            Grid.SetRow(btnDelete, 4);
            grid.Children.Add(btnDelete);

            border.Child = grid;
            return border;
        }
        // Изменение количества (+1 / -1)
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
                    }
                }
                LoadCartItems(); // перерисовываем
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка изменения количества: {ex.Message}");
            }
        }
        // Удаление позиции из корзины
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn)) return;
            int id = (int)btn.Tag;

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
                    LoadCartItems();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}");
                }
            }
        }
        // Пересчёт итоговой суммы
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
                return p.Price * (1 - p.Discount / 100m) * item.Quantity;
            });

            TxtTotalPrice.Text = $"{total:F2} руб.";
        }
        // Оформление заказа – открывает окно OrderWindow
        private void BtnCheckout_Click(object sender, RoutedEventArgs e)
        {
            if (_cartItems == null || !_cartItems.Any())
            {
                MessageBox.Show("Корзина пуста. Добавьте товары.");
                return;
            }

            var orderWindow = new OrderWindow();
            orderWindow.Owner = Window.GetWindow(this);
            if (orderWindow.ShowDialog() == true)
                LoadCartItems(); // после успешного заказа обновляем корзину
        }
        // Загрузка картинки товара из файла
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
            catch { /* если не загрузилось – вернём null */ }
            return null;
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