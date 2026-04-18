using BeautySalon17.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
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
using BeautySalon17.Windows;
namespace BeautySalon17.Pages
{
    /// <summary>
    /// Логика взаимодействия для CartPage.xaml
    /// </summary>
    public partial class CartPage : Page
    {
        // Список элементов корзины текущего пользователя (из таблицы CartItems)
        private List<CartItems> _cartItems;
        public CartPage()
        {
            InitializeComponent();
            LoadCartItems(); // Загружаем содержимое корзины при открытии страницы
        }

        /// <summary>
        /// Загружает из базы все позиции корзины для текущего пользователя
        /// и отображает их на странице.
        /// </summary>
        private void LoadCartItems()
        {
            try
            {
                using (var context = new BeautySalonEntities())
                {
                    // Получаем записи из CartItems для текущего пользователя,
                    // включая связанный товар (чтобы получить название, цену, картинку)
                    _cartItems = context.CartItems.Include("Products").Where(ci => ci.UserId == CurrentUser.Id).ToList();
                }
                // Отображаем товары и пересчитываем итоговую сумму
                DisplayCartItems();
                UpdateTotalPrice();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки корзины: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Создаёт карточки для каждого товара в корзине и добавляет их в WrapPanel.
        /// </summary>
        private void DisplayCartItems()
        {
            CartItemsWrapPanel.Children.Clear();

            if (_cartItems == null || _cartItems.Count == 0)
            {
                TextBlock emptyText = new TextBlock
                {
                    Text = "Ваша корзина пуста.",
                    FontSize = 16,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(20)
                };
                CartItemsWrapPanel.Children.Add(emptyText);
                BtnCheckout.IsEnabled = false; // Если корзина пуста, кнопку оформления отключаем
                return;
            }

            BtnCheckout.IsEnabled = true;

            foreach (var item in _cartItems)
            {
                // Создаём карточку для одной позиции корзины
                FrameworkElement card = CreateCartItemCard(item);
                CartItemsWrapPanel.Children.Add(card);
            }
        }
        /// <summary>
        /// Создаёт визуальную карточку для одного элемента корзины.
        /// Похожа на карточку товара, но с элементами управления количеством и кнопкой удаления.
        /// </summary>
        private FrameworkElement CreateCartItemCard(CartItems item)
        {
            // Получаем связанный товар (он уже загружен благодаря Include)
            Products product = item.Products;

            // Внешняя рамка
            Border cardBorder = new Border
            {
                Width = 200,
                Height = 350,
                Margin = new Thickness(10),
                Background = Brushes.White,
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8)
            };

            // Сетка внутри карточки
            Grid cardGrid = new Grid();
            cardGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(150) }); // Изображение
            cardGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });    // Название
            cardGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });    // Цена
            cardGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });    // Управление количеством
            cardGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });    // Кнопка удаления

            // 1. Изображение товара
            Image productImage = new Image
            {
                Stretch = Stretch.Uniform,
                Margin = new Thickness(5),
                Source = GetImageSource(product.ImagePath)
            };
            Grid.SetRow(productImage, 0);
            cardGrid.Children.Add(productImage);

            // 2. Название товара
            TextBlock nameText = new TextBlock
            {
                Text = product.Name,
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(5, 0, 5, 0)
            };
            Grid.SetRow(nameText, 1);
            cardGrid.Children.Add(nameText);

            // 3. Цена за единицу (с учётом скидки, если есть)
            decimal unitPrice = product.Price * (1 - product.Discount / 100m);
            TextBlock priceText = new TextBlock
            {
                Text = $"{unitPrice:F2} руб. / шт.",
                FontSize = 12,
                Foreground = Brushes.Green,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(5)
            };
            Grid.SetRow(priceText, 2);
            cardGrid.Children.Add(priceText);

            // 4. Панель управления количеством
            StackPanel quantityPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 5, 0, 0)
            };

            // Кнопка "-" (уменьшить количество)
            Button btnMinus = new Button
            {
                Content = "−",
                Width = 25,
                Height = 25,
                Tag = item.Id,          // запоминаем Id позиции корзины
                IsEnabled = item.Quantity > 1  // если количество 1, кнопку отключаем
            };
            btnMinus.Click += BtnMinus_Click;
            quantityPanel.Children.Add(btnMinus);

            // Текстовое поле с текущим количеством (только для отображения)
            TextBlock quantityText = new TextBlock
            {
                Text = item.Quantity.ToString(),
                Width = 30,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 0, 5, 0),
                FontWeight = FontWeights.Bold
            };
            quantityPanel.Children.Add(quantityText);

            // Кнопка "+" (увеличить количество)
            Button btnPlus = new Button
            {
                Content = "+",
                Width = 25,
                Height = 25,
                Tag = item.Id
            };
            btnPlus.Click += BtnPlus_Click;
            quantityPanel.Children.Add(btnPlus);

            Grid.SetRow(quantityPanel, 3);
            cardGrid.Children.Add(quantityPanel);

            // 5. Кнопка удаления товара из корзины
            Button btnDelete = new Button
            {
                Content = "🗑️ Удалить",
                Height = 30,
                Margin = new Thickness(10, 5, 10, 10),
                Tag = item.Id
            };
            btnDelete.Click += BtnDelete_Click;
            Grid.SetRow(btnDelete, 4);
            cardGrid.Children.Add(btnDelete);

            cardBorder.Child = cardGrid;
            return cardBorder;
        }

        /// <summary>
        /// Пересчитывает и обновляет итоговую сумму корзины.
        /// </summary>
        private void UpdateTotalPrice()
        {
            if (_cartItems == null || _cartItems.Count == 0)
            {
                TxtTotalPrice.Text = "0.00 руб.";
                return;
            }

            decimal total = 0;
            foreach (var item in _cartItems)
            {
                Products product = item.Products;
                // Цена за единицу с учётом скидки
                decimal unitPrice = product.Price * (1 - product.Discount / 100m);
                total += unitPrice * item.Quantity;
            }
            TxtTotalPrice.Text = $"{total:F2} руб.";
        }
        /// <summary>
        /// Кнопка "Назад" — возврат на предыдущую страницу.
        /// </summary>
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
            else
                NavigationService.Navigate(new StartPage());
        }
        /// <summary>
        /// Кнопка уменьшения количества товара в корзине.
        /// </summary>
        private void BtnMinus_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;
            int cartItemId = (int)btn.Tag;

            try
            {
                using (var context = new BeautySalonEntities())
                {
                    var cartItem = context.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);
                    if (cartItem != null && cartItem.Quantity > 1)
                    {
                        cartItem.Quantity--;
                        context.SaveChanges();
                    }
                }
                // Перезагружаем страницу, чтобы обновить отображение
                LoadCartItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка изменения количества: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Кнопка увеличения количества товара в корзине.
        /// </summary>
        private void BtnPlus_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;
            int cartItemId = (int)btn.Tag;

            try
            {
                using (var context = new BeautySalonEntities())
                {
                    var cartItem = context.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);
                    if (cartItem != null)
                    {
                        cartItem.Quantity++;
                        context.SaveChanges();
                    }
                }
                LoadCartItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка изменения количества: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Кнопка удаления позиции из корзины.
        /// </summary>
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;
            int cartItemId = (int)btn.Tag;

            MessageBoxResult result = MessageBox.Show("Удалить этот товар из корзины?",
                                                      "Подтверждение",
                                                      MessageBoxButton.YesNo,
                                                      MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var context = new BeautySalonEntities())
                    {
                        var cartItem = context.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);
                        if (cartItem != null)
                        {
                            context.CartItems.Remove(cartItem);
                            context.SaveChanges();
                        }
                    }
                    LoadCartItems();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Кнопка "Оформить заказ" — открывает окно для выбора даты получения и способа оплаты.
        /// </summary>
        private void BtnCheckout_Click(object sender, RoutedEventArgs e)
        {
            if (_cartItems == null || _cartItems.Count == 0)
            {
                MessageBox.Show("Корзина пуста. Добавьте товары перед оформлением заказа.",
                                "Корзина пуста", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Открываем окно оформления заказа
            OrderWindow orderWindow = new OrderWindow();   // конструктор без параметров
            orderWindow.Owner = Window.GetWindow(this);    // правильный способ получить главное окно
            bool? result = orderWindow.ShowDialog();       // ShowDialog возвращает bool?

            // Если заказ успешно оформлен, обновляем корзину
            if (result == true)
            {
                LoadCartItems();   // твой метод загрузки корзины
            }
        }
        /// <summary>
        /// Загружает изображение по пути из базы данных.
        /// </summary>
        private BitmapImage GetImageSource(string imagePath)
        {
            try
            {
                string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, imagePath);
                if (File.Exists(fullPath))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(fullPath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    return bitmap;
                }
            }
            catch { }
            return null;
        }
    }
}

