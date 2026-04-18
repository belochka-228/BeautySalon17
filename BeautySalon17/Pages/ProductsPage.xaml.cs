using BeautySalon17;
using BeautySalon17.Helpers;
using BeautySalon17.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

namespace BeautySalon17.Pages
{
    /// <summary>
    /// Логика взаимодействия для ProductsPage.xaml
    /// </summary>
    public partial class ProductsPage : Page
    {
        // Приватное поле для хранения полного списка товаров, загруженного из базы.
        private List<Products> _allProducts;
        private List<ProductTypes> _productTypes;      // список всех типов товаров
        private List<Manufacturers> _manufacturers;    // список производителей
        public ProductsPage()
        {
            InitializeComponent();
            LoadFilters();      // <-- загружаем фильтры
            LoadProducts();
        }

        /// <summary>
        /// Загружает активные товары из базы данных и отображает их.
        /// </summary>
        private void LoadProducts()
        {
            try
            {
                using (var context = new BeautySalonEntities())
                {
                    _allProducts = context.Products
                                          .Include("Manufacturers")   
                                          .Include("ProductTypes")    
                                          .Where(p => p.IsActive == true)
                                          .ToList();
                }
                // Передаём полученный список товаров методу, который отрисует их на экране
                DisplayProducts(_allProducts);

            }
            catch (Exception ex) 
            {
                MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Загружает списки типов товаров и производителей из базы и заполняет ComboBox'ы.
        /// </summary>
        private void LoadFilters()
        {
            try
            {
                using (var context = new BeautySalonEntities())
                {
                    // Получаем все типы товаров
                    _productTypes = context.ProductTypes.ToList();
                    // Получаем всех производителей
                    _manufacturers = context.Manufacturers.ToList();
                }

                // Настраиваем ComboBox для типов товаров
                CmbProductType.ItemsSource = _productTypes;
                CmbProductType.DisplayMemberPath = "Name";   // показываем название типа
                CmbProductType.SelectedValuePath = "Id";     // при выборе будем получать Id

                // Настраиваем ComboBox для производителей
                CmbManufacturer.ItemsSource = _manufacturers;
                CmbManufacturer.DisplayMemberPath = "Name";
                CmbManufacturer.SelectedValuePath = "Id";

                // Добавляем пустой элемент "Все" в начало каждого списка
                // (чтобы можно было сбросить фильтр)
                _productTypes.Insert(0, new ProductTypes { Id = 0, Name = "Все типы" });
                _manufacturers.Insert(0, new Manufacturers { Id = 0, Name = "Все производители" });

                // Обновляем привязку, чтобы новый элемент отобразился
                CmbProductType.ItemsSource = null;
                CmbProductType.ItemsSource = _productTypes;
                CmbManufacturer.ItemsSource = null;
                CmbManufacturer.ItemsSource = _manufacturers;

                // Выбираем первый элемент ("Все") по умолчанию
                CmbProductType.SelectedIndex = 0;
                CmbManufacturer.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки фильтров: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Отображает переданный список товаров в WrapPanel (ProductsWrapPanel).
        /// </summary>
        /// <param name="products">Список товаров для отображения.</param>
        private void DisplayProducts(List<Products> products)
        {
            ProductsWrapPanel.Children.Clear();

            if (products == null || products.Count == 0)
            {   
                // Создаём текстовый блок с сообщением "Товары не найдены"
                TextBlock noItemsText = new TextBlock
                {
                    Text = "Товары не найдены.",
                    FontSize = 16,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(20)
                };
                // Добавляем этот текст в WrapPanel
                ProductsWrapPanel.Children.Add(noItemsText);
                return; // Выходим из метода
            }
            foreach (var product in products)
            {
                // Метод CreateProductCard создаёт готовый визуальный элемент (Border с содержимым)
                FrameworkElement productCard = CreateProductCard(product);
                // Добавляем карточку в панель
                ProductsWrapPanel.Children.Add(productCard);
            }
        }
         /// <summary>
         /// Создаёт визуальную карточку для одного товара.
         /// </summary>
         /// <param name="product">Объект товара из базы данных.</param>
         /// <returns>Готовый FrameworkElement (Border), который можно добавить в панель.</returns>
        private FrameworkElement CreateProductCard(Products product)
        {
            // Внешняя рамка
            Border cardBorder = new Border
            {
                Width = 200,
                Height = 330,
                Margin = new Thickness(10),
                Background = Brushes.White,
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8)
            };

            // Сетка
            Grid cardGrid = new Grid();
            cardGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(150) });
            cardGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            cardGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            cardGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // 1. Изображение
            Image productImage = new Image
            {
                Stretch = Stretch.Uniform,
                Margin = new Thickness(5),
                Source = GetImageSource(product.ImagePath)
            };
            Grid.SetRow(productImage, 0);
            cardGrid.Children.Add(productImage);

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
            cardGrid.Children.Add(nameText);

            // 3. Цена
            decimal finalPrice = product.Price * (1 - product.Discount / 100m);
            TextBlock priceText = new TextBlock
            {
                Text = $"{finalPrice:F2} руб.",
                FontSize = 14,
                Foreground = Brushes.Green,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(5)
            };

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
                cardGrid.Children.Add(pricePanel);
            }
            else
            {
                Grid.SetRow(priceText, 2);
                cardGrid.Children.Add(priceText);
            }

            // 4. Панель с кнопками "В корзину" и "Подробнее"
            StackPanel buttonsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 5, 0, 5)
            };

            Button addToCartBtn = new Button
            {
                Content = "🛒 В корзину",
                Width = 90,
                Height = 30,
                Tag = product.Id,
                IsEnabled = CurrentUser.IsAuthenticated
            };
            addToCartBtn.Click += BtnAddToCart_Click;
            buttonsPanel.Children.Add(addToCartBtn);

            Button detailsBtn = new Button
            {
                Content = "Подробнее",
                Width = 90,
                Height = 30,
                Margin = new Thickness(5, 0, 0, 0),
                Tag = product
            };
            detailsBtn.Click += BtnDetails_Click;
            buttonsPanel.Children.Add(detailsBtn);

            Grid.SetRow(buttonsPanel, 3);
            cardGrid.Children.Add(buttonsPanel);

            // Собираем карточку
            cardBorder.Child = cardGrid;

            // Выделение скидки >15%
            if (product.Discount > 15)
            {
                cardBorder.Background = new SolidColorBrush(Color.FromRgb(255, 255, 200));
            }

            return cardBorder;
        }

        /// <summary>
        /// Загружает изображение по относительному пути (например, "Images\\file.jpg").
        /// Если файл не найден, возвращает null.
        /// </summary>
        /// <param name="imagePath">Путь к файлу изображения, хранящийся в базе данных.</param>
        /// <returns>Объект BitmapImage для отображения или null.</returns>
        private BitmapImage GetImageSource(string imagePath)
        {
            try
            {
                // Получаем полный путь к файлу, комбинируя базовую папку приложения (bin\Debug\) и путь из базы
                string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, imagePath);
                // Проверяем, существует ли файл по этому пути
                if (File.Exists(fullPath))
                {
                    // Создаём объект BitmapImage и загружаем в него картинку
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();                     // Начинаем инициализацию
                    bitmap.UriSource = new Uri(fullPath);   // Указываем путь к файлу
                    bitmap.CacheOption = BitmapCacheOption.OnLoad; // Кешируем изображение при загрузке
                    bitmap.EndInit();                       // Завершаем инициализацию
                    return bitmap;
                }
                // Если файл не найден, возвращаем null (изображение не отобразится)
                return null;
            }
            catch // Если возникла любая ошибка при загрузке
            {
                return null; // Тоже возвращаем null
            }
        }
        /// <summary>
        /// Выполняет фильтрацию товаров по тексту, введённому в TxtSearch.
        /// </summary>
        private void PerformSearch()
        {
            string searchText = TxtSearch.Text.Trim();

            IEnumerable<Products> result;
            if (string.IsNullOrEmpty(searchText))
            {
                result = _allProducts;
            }
            else
            {
                result = _allProducts.Where(p =>
                    p.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            // Применяем сортировку (такую же, как в ApplyFilters)
            ComboBoxItem selectedSortItem = CmbSorting.SelectedItem as ComboBoxItem;
            if (selectedSortItem != null)
            {
                string sortDirection = selectedSortItem.Tag.ToString();
                if (sortDirection == "asc")
                    result = result.OrderBy(p => p.Rating);
                else
                    result = result.OrderByDescending(p => p.Rating);
            }

            DisplayProducts(result.ToList());
        }


        /// <summary>
        /// Применяет фильтры по типу и производителю и отображает отфильтрованные товары.
        /// </summary>
        private void ApplyFilters()
        {
            // Если _allProducts ещё не загружен — выходим
            if (_allProducts == null) return;

            // Получаем выбранные Id из ComboBox'ов
            int selectedTypeId = (int)CmbProductType.SelectedValue;
            int selectedManufacturerId = (int)CmbManufacturer.SelectedValue;

            // Начинаем с полного списка товаров
            IEnumerable<Products> filtered = _allProducts;

            // Фильтр по типу товара (если выбрано что-то кроме "Все")
            if (selectedTypeId != 0)
            {
                filtered = filtered.Where(p => p.ProductTypeId == selectedTypeId);
            }

            // Фильтр по производителю (если выбрано что-то кроме "Все")
            if (selectedManufacturerId != 0)
            {
                filtered = filtered.Where(p => p.ManufacturerId == selectedManufacturerId);
            }

            // Получаем выбранный ComboBoxItem
            ComboBoxItem selectedSortItem = CmbSorting.SelectedItem as ComboBoxItem;
            if (selectedSortItem != null)
            {
                string sortDirection = selectedSortItem.Tag.ToString(); // "asc" или "desc"
                if (sortDirection == "asc")
                    filtered = filtered.OrderBy(p => p.Rating);
                else
                    filtered = filtered.OrderByDescending(p => p.Rating);
            }

            // Отображаем отфильтрованный список
            DisplayProducts(filtered.ToList());
        }

        /// <summary>
        /// Обработчик нажатия на кнопку "Назад".
        /// </summary>
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, есть ли предыдущая страница в истории навигации
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();          // Если есть, возвращаемся на неё
            else
                NavigationService.Navigate(new StartPage()); // Иначе переходим на стартовую страницу
        }

        /// <summary>
        /// Обработчик нажатия на кнопку поиска (лупа).
        /// </summary>
        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            // Вызываем общий метод выполнения поиска
            PerformSearch();
        }

        /// <summary>
        /// Обработчик нажатия клавиш в поле поиска. Нужен для поиска по Enter.
        /// </summary>
        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            // Если нажата клавиша Enter
            if (e.Key == Key.Enter)
                PerformSearch(); // Выполняем поиск
        }

        /// <summary>
        /// Срабатывает при изменении выбора в любом из ComboBox фильтров.
        /// </summary>
        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        /// <summary>
        /// Сбрасывает оба фильтра на значение "Все" и показывает все товары.
        /// </summary>
        private void BtnResetFilters_Click(object sender, RoutedEventArgs e)
        {
            CmbProductType.SelectedIndex = 0;
            CmbManufacturer.SelectedIndex = 0;
            // ApplyFilters вызовется автоматически из-за изменения SelectedIndex
        }

        /// <summary>
        /// Обработчик нажатия на кнопку "Корзина" в верхней панели.
        /// </summary>
        private void BtnCart_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, авторизован ли пользователь (корзина доступна только после входа)
            if (!CurrentUser.IsAuthenticated)
            {
                MessageBox.Show("Чтобы перейти в корзину, необходимо войти в систему.",
                                "Требуется авторизация",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                return; // Прерываем выполнение
            }

            // Переходим на страницу корзины
            NavigationService.Navigate(new CartPage());
        }

        /// <summary>
        /// Обработчик нажатия на кнопку "В корзину" внутри карточки товара.
        /// </summary>
        private void BtnAddToCart_Click(object sender, RoutedEventArgs e)
        {
            // Получаем кнопку, на которую нажали
            Button clickedButton = sender as Button;
            if (clickedButton == null) return; // Если это не кнопка (на всякий случай) — выходим

            // Из свойства Tag извлекаем ID товара (мы сохранили его при создании кнопки)
            int productId = (int)clickedButton.Tag;

            try
            {
                // Создаём контекст для работы с базой данных
                using (var context = new BeautySalonEntities())
                {
                    // Проверяем, есть ли уже такой товар в корзине у текущего пользователя
                    var existingItem = context.CartItems.FirstOrDefault(ci =>
                                        ci.UserId == CurrentUser.Id && ci.ProductId == productId);

                    if (existingItem != null)
                    {
                        // Если товар уже есть в корзине, увеличиваем количество на 1
                        existingItem.Quantity++;
                    }
                    else
                    {
                        // Если товара ещё нет, создаём новую запись в таблице CartItems
                        context.CartItems.Add(new CartItems
                        {
                            UserId = CurrentUser.Id,
                            ProductId = productId,
                            Quantity = 1
                        });
                    }

                    // Сохраняем изменения в базе данных
                    context.SaveChanges();
                }

                // Сообщаем пользователю об успешном добавлении
                MessageBox.Show("Товар добавлен в корзину!", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) // Если произошла ошибка при работе с базой
            {
                // Показываем сообщение с ошибкой
                MessageBox.Show($"Ошибка добавления в корзину: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Срабатывает при изменении выбора в ComboBox сортировки.
        /// </summary>
        private void Sorting_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters(); // Переприменяем фильтры (а внутри будет и сортировка)
        }
        private void BtnDetails_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            if (clickedButton?.Tag is Products product)
            {
                ProductDetailWindow detailWindow = new ProductDetailWindow(product);
                detailWindow.Owner = Window.GetWindow(this);
                detailWindow.ShowDialog();
            }
        }
    }
}