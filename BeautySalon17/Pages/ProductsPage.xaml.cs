using BeautySalon17.Helpers;
using BeautySalon17.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BeautySalon17.Pages
{
    /// <summary>
    /// Логика взаимодействия для ProductsPage.xaml
    /// </summary>
    public partial class ProductsPage : Page
    {
        // Полный список активных товаров
        private List<Products> allProducts;
        // Списки типов товаров и производителей для фильтров
        private List<ProductTypes> productTypes;
        private List<Manufacturers> manufacturers;

        public ProductsPage()
        {
            InitializeComponent();
            LoadFilters();   // Загружаем фильтры (типы и производителей)
            LoadProducts();  // Загружаем товары
        }

        /// <summary>
        /// Загружает активные товары из базы и отображает их.
        /// </summary>
        private void LoadProducts()
        {
            try
            {
                using (var context = new BeautySalonEntities())
                {
                    // Получаем активные товары с связями
                    allProducts = context.Products.Include("Manufacturers").Include("ProductTypes").Where(p => p.IsActive).ToList();
                }
                DisplayProducts(allProducts); // Отобразить список
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Загружает списки типов товаров и производителей для фильтров.
        /// </summary>
        private void LoadFilters()
        {
            try
            {
                using (var context = new BeautySalonEntities())
                {
                    // Получаем все типы товаров
                    productTypes = context.ProductTypes.ToList();
                    // Получаем всех производителей
                    manufacturers = context.Manufacturers.ToList();
                }

                // Добавляем "Все" в начало списков фильтров
                productTypes.Insert(0, new ProductTypes { Id = 0, Name = "Все типы" });
                manufacturers.Insert(0, new Manufacturers { Id = 0, Name = "Все производители" });

                // Настраиваем ComboBox для типов товаров
                CmbProductType.ItemsSource = productTypes;
                CmbProductType.DisplayMemberPath = "Name";   // показываем название типа
                CmbProductType.SelectedValuePath = "Id";     // при выборе будем получать Id

                // Настраиваем ComboBox для производителей
                CmbManufacturer.ItemsSource = manufacturers;
                CmbManufacturer.DisplayMemberPath = "Name";
                CmbManufacturer.SelectedValuePath = "Id";

                // Выбираем первый элемент ("Все") по умолчанию
                CmbProductType.SelectedIndex = 0;
                CmbManufacturer.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки фильтров: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Отображает список товаров в WrapPanel.
        /// </summary>
        private void DisplayProducts(List<Products> products)
        {
            ProductsWrapPanel.Children.Clear();

            if (products == null || products.Count == 0)
            {
                // Если товаров нет, показываем сообщение
                TextBlock noItems = new TextBlock
                {
                    Text = "Товары не найдены.",
                    FontSize = 16,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(20)
                };
                ProductsWrapPanel.Children.Add(noItems);
                return;
            }

            // Создаём карточки для каждого товара
            foreach (var product in products)
            {
                var card = CreateProductCard(product);
                ProductsWrapPanel.Children.Add(card);
            }
        }

        /// <summary>
        /// Создаёт карточку товара для отображения.
        /// </summary>
        private FrameworkElement CreateProductCard(Products product)
        {
            // Внешняя рамка карточки
            Border border = new Border
            {
                Width = 200,
                Height = 330,
                Margin = new Thickness(10),
                Background = Brushes.White,
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8)
            };

            // Основная сетка внутри карточки
            Grid grid = new Grid();

            // Определяем строки для изображения, названия, цены и кнопок
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(150) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // 1. Изображение товара
            Image image = new Image
            {
                Stretch = Stretch.Uniform,
                Margin = new Thickness(5),
                Source = GetImageSource(product.ImagePath)
            };
            Grid.SetRow(image, 0);
            grid.Children.Add(image);

            // 2. Название
            TextBlock nameText = new TextBlock
            {
                Text = product.Name,
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(5, 0, 5, 0)
            };
            Grid.SetRow(nameText, 1);
            grid.Children.Add(nameText);

            // 3. Цена с учётом скидки
            decimal finalPrice = product.Price * (1 - product.Discount / 100m);
            TextBlock priceText = new TextBlock
            {
                Text = $"{finalPrice:F2} руб.",
                FontSize = 14,
                Foreground = Brushes.Green,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(5)
            };

            // Если есть скидка, показываем старую цену
            if (product.Discount > 0)
            {
                StackPanel pricePanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                TextBlock oldPrice = new TextBlock
                {
                    Text = $"{product.Price:F2}",
                    TextDecorations = TextDecorations.Strikethrough,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(0, 0, 5, 0)
                };
                pricePanel.Children.Add(oldPrice);
                pricePanel.Children.Add(priceText);
                Grid.SetRow(pricePanel, 2);
                grid.Children.Add(pricePanel);
            }
            else
            {
                Grid.SetRow(priceText, 2);
                grid.Children.Add(priceText);
            }

            // 4. Панель с кнопками "В корзину" и "Подробнее"
            StackPanel buttonsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 5, 0, 5)
            };

            Button btnAddToCart = new Button
            {
                Content = "В корзину",
                Width = 90,
                Height = 30,
                Tag = product.Id,
                IsEnabled = CurrentUser.IsAuthenticated
            };
            btnAddToCart.Click += BtnAddToCart_Click;
            buttonsPanel.Children.Add(btnAddToCart);

            Button btnDetails = new Button
            {
                Content = "Подробнее",
                Width = 90,
                Height = 30,
                Margin = new Thickness(5, 0, 0, 0),
                Tag = product
            };
            btnDetails.Click += BtnDetails_Click;
            buttonsPanel.Children.Add(btnDetails);

            Grid.SetRow(buttonsPanel, 3);
            grid.Children.Add(buttonsPanel);

            border.Child = grid;

            // Выделение скидки >15%
            if (product.Discount > 15)
            {
                border.Background = new SolidColorBrush(Color.FromRgb(255, 255, 200));
            }

            return border;
        }

        /// <summary>
        /// Загружает изображение по пути.
        /// </summary>
        private BitmapImage GetImageSource(string imagePath)
        {
            try
            {
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, imagePath);
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

        /// <summary>
        /// Выполняет поиск по названию.
        /// </summary>
        private void PerformSearch()
        {
            string searchText = TxtSearch.Text.Trim();
            var result = string.IsNullOrEmpty(searchText) ? allProducts : allProducts.Where(p => p.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);

            // Сортировка по рейтингу
            var selectedSortItem = CmbSorting.SelectedItem as ComboBoxItem;
            if (selectedSortItem != null)
            {
                string sortDirection = selectedSortItem.Tag.ToString();
                result = (sortDirection == "asc") ? result.OrderBy(p => p.Rating) : result.OrderByDescending(p => p.Rating);
            }
            DisplayProducts(result.ToList());
        }

        /// <summary>
        /// Применяет выбранные фильтры и сортировку.
        /// </summary>
        private void ApplyFilters()
        {
            if (allProducts == null) return;

            int typeId = (int)CmbProductType.SelectedValue;
            int manufacturerId = (int)CmbManufacturer.SelectedValue;

            var filtered = allProducts.AsEnumerable();

            if (typeId != 0)
                filtered = filtered.Where(p => p.ProductTypeId == typeId);

            if (manufacturerId != 0)
                filtered = filtered.Where(p => p.ManufacturerId == manufacturerId);

            var selectedSort = CmbSorting.SelectedItem as ComboBoxItem;
            if (selectedSort != null)
            {
                string direction = selectedSort.Tag.ToString();
                filtered = (direction == "asc")
                    ? filtered.OrderBy(p => p.Rating)
                    : filtered.OrderByDescending(p => p.Rating);
            }
            DisplayProducts(filtered.ToList());
        }
        // Обработчик кнопки "Назад"
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
            else
                NavigationService.Navigate(new StartPage());
        }
        // Обработчик поиска по кнопке
        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            PerformSearch();
        }
        // Поиск по нажатию Enter
        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                PerformSearch();
        }
        // Обработка изменения фильтров
        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }
        // Сброс фильтров
        private void BtnResetFilters_Click(object sender, RoutedEventArgs e)
        {
            CmbProductType.SelectedIndex = 0;
            CmbManufacturer.SelectedIndex = 0;
        }
        // Переход в корзину
        private void BtnCart_Click(object sender, RoutedEventArgs e)
        {
            if (!CurrentUser.IsAuthenticated)
            {
                MessageBox.Show("Чтобы перейти в корзину, необходимо войти в систему.", "Требуется авторизация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            NavigationService.Navigate(new CartPage());
        }
        // Добавление товара в корзину
        private void BtnAddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int productId)
            {
                try
                {
                    using (var context = new BeautySalonEntities())
                    {
                        // Проверка, есть ли товар уже в корзине
                        var existingItem = context.CartItems.FirstOrDefault(ci => ci.UserId == CurrentUser.Id && ci.ProductId == productId);
                        if (existingItem != null)
                            existingItem.Quantity++; // Увеличиваем количество
                        else
                            context.CartItems.Add(new CartItems { UserId = CurrentUser.Id, ProductId = productId, Quantity = 1 });
                        context.SaveChanges();
                    }
                    MessageBox.Show("Товар добавлен в корзину!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка добавления в корзину: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        // Обработка выбора сортировки
        private void Sorting_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }
        // Открытие окна деталей товара
        private void BtnDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Products product)
            {
                var detailWindow = new ProductDetailWindow(product);
                detailWindow.Owner = Window.GetWindow(this);
                detailWindow.ShowDialog();
            }
        }
    }
}