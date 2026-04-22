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
    /// Страница с каталогом товаров.
    /// </summary>
    public partial class ProductsPage : Page
    {
        // Полный список всех активных товаров, загруженный из базы
        private List<Products> _allProducts;
        // Список всех типов товаров
        private List<ProductTypes> _productTypes;
        // Список всех производителей
        private List<Manufacturers> _manufacturers;
        /// <summary>
        /// Конструктор страницы. При создании сразу загружаю фильтры и товары
        /// </summary>
        public ProductsPage()
        {
            InitializeComponent();
            LoadFilters();   // сначала загружаю типы и производителей
            LoadProducts();  // потом сами товары
        }
        /// <summary>
        /// Загружаю из базы данных все активные товары (которые продаются)
        /// Подгружаю связанных производителей и типы, чтобы потом не бегать в базу лишний раз
        /// </summary>
        private void LoadProducts()
        {
            try
            {
                using (var db = new BeautySalonEntities())
                {
                    // Include нужны, чтобы сразу подтянуть Manufacturer и ProductType
                    _allProducts = db.Products.Include("Manufacturers").Include("ProductTypes").Where(p => p.IsActive).ToList();
                }
                DisplayProducts(_allProducts); // все товары на экране
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}");
            }
        }
        /// <summary>
        /// Загружаю справочники типов товаров и производителей
        /// В каждый ComboBox добавляю пункт "Все", чтобы можно было сбросить фильтр
        /// </summary>
        private void LoadFilters()
        {
            try
            {
                using (var db = new BeautySalonEntities())
                {
                    _productTypes = db.ProductTypes.ToList();
                    _manufacturers = db.Manufacturers.ToList();
                }
                // пункт "Все" в начало каждого списка
                _productTypes.Insert(0, new ProductTypes { Id = 0, Name = "Все типы" });
                _manufacturers.Insert(0, new Manufacturers { Id = 0, Name = "Все производители" });
                // Настраиваю ComboBox типов
                CmbProductType.ItemsSource = _productTypes;
                CmbProductType.DisplayMemberPath = "Name";   // название
                CmbProductType.SelectedValuePath = "Id";     // значение – ID
                CmbProductType.SelectedIndex = 0;            // по умолчанию
                // Настраиваю ComboBox производителей
                CmbManufacturer.ItemsSource = _manufacturers;
                CmbManufacturer.DisplayMemberPath = "Name";
                CmbManufacturer.SelectedValuePath = "Id";
                CmbManufacturer.SelectedIndex = 0;           // "Все производители"
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки фильтров: {ex.Message}");
            }
        }
        /// <summary>
        /// Очищение панелм с товарами и заполнение её карточками из переданного списка
        /// </summary>
        private void DisplayProducts(IEnumerable<Products> products)
        {
            ProductsWrapPanel.Children.Clear();
            // Если список пустой показываю сообщение
            if (!products.Any())
            {
                ProductsWrapPanel.Children.Add(new TextBlock
                {
                    Text = "Товары не найдены",
                    FontSize = 16,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(20)
                });
                return;
            }
            // Для каждого товара создаю карточку и добавляю в панель
            foreach (var p in products)
                ProductsWrapPanel.Children.Add(CreateProductCard(p));
        }
        /// <summary>
        /// Карточка товара. Внутри: картинка, название, цена, кнопки
        /// </summary>
        private FrameworkElement CreateProductCard(Products product)
        {
            // Считаю итоговую цену с учётом скидки
            decimal finalPrice = product.Price * (1 - product.Discount / 100m);
            var border = new Border
            {
                Width = 200,
                Height = 330,
                Margin = new Thickness(10),
                Background = Brushes.White,
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8)
            };
            // Если скидка больше 15% – крашу фон в светло-жёлтый
            if (product.Discount > 15)
                border.Background = new SolidColorBrush(Color.FromRgb(255, 255, 200));
            // Сетка из четырёх строк: картинка, название, цена, кнопки
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(150) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Картинка
            var img = new Image
            {
                Stretch = Stretch.Uniform,
                Margin = new Thickness(5),
                Source = LoadImage(product.ImagePath)   // загружаю картинку
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

            // Блок с ценой
            if (product.Discount > 0)
            {
                // Если есть скидка, показываю старую цену зачёркнутой и новую зелёную
                var pricePanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                pricePanel.Children.Add(new TextBlock
                {
                    Text = $"{product.Price:F2}",
                    TextDecorations = TextDecorations.Strikethrough, //зачеркивание
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(0, 0, 5, 0)
                });
                pricePanel.Children.Add(new TextBlock
                {
                    Text = $"{finalPrice:F2} руб.",
                    FontSize = 14,
                    Foreground = Brushes.Green
                });
                Grid.SetRow(pricePanel, 2);
                grid.Children.Add(pricePanel);
            }
            else
            {
                // Без скидки – просто зелёная цена
                var txtPrice = new TextBlock
                {
                    Text = $"{finalPrice:F2} руб.",
                    FontSize = 14,
                    Foreground = Brushes.Green,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(5)
                };
                Grid.SetRow(txtPrice, 2);
                grid.Children.Add(txtPrice);
            }

            // Панель с кнопками "В корзину" и "Подробнее" 
            var btnPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 5, 0, 5)
            };

            // Кнопка "В корзину" – активна только если пользователь вошёл
            var btnCart = new Button
            {
                Content = "В корзину",
                Width = 90,
                Height = 30,
                Tag = product.Id,                       // ID товара
                IsEnabled = CurrentUser.IsAuthenticated
            };
            btnCart.Click += BtnAddToCart_Click;
            btnPanel.Children.Add(btnCart);
            // Кнопка "Подробнее" – открывает отдельное окно с полной информацией
            var btnDetails = new Button
            {
                Content = "Подробнее",
                Width = 90,
                Height = 30,
                Margin = new Thickness(5, 0, 0, 0),
                Tag = product                            // передаю объект товара
            };
            btnDetails.Click += BtnDetails_Click;
            btnPanel.Children.Add(btnDetails);

            Grid.SetRow(btnPanel, 3);
            grid.Children.Add(btnPanel);

            border.Child = grid;
            return border;
        }
        /// <summary>
        /// Загружаю картинку товара с диска. Если файла нет – возвращаю null
        /// </summary>
        private BitmapImage LoadImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath)) return null;
            try
            {
                // Собираю полный путь к файлу
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
            catch { }
            return null;
        }
        /// <summary>
        /// Главный метод, который собирает все условия:
        /// поиск по названию, фильтр по типу, фильтр по производителю, сортировка по рейтингу
        /// После применения всего – обновляю отображение
        /// </summary>
        private void ApplyAllFilters()
        {
            if (_allProducts == null) return;
            // Начинаю с полного списка
            var filtered = _allProducts.AsEnumerable();

            // Поиск по названию (без учёта регистра)
            string search = TxtSearch.Text.Trim();
            if (!string.IsNullOrEmpty(search))
                filtered = filtered.Where(p => p.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);

            // Фильтр по типу
            if (CmbProductType.SelectedValue is int typeId && typeId != 0)
                filtered = filtered.Where(p => p.ProductTypeId == typeId);

            // Фильтр по производителю
            if (CmbManufacturer.SelectedValue is int manId && manId != 0)
                filtered = filtered.Where(p => p.ManufacturerId == manId);

            // Сортировка по рейтингу (беру направление из Tag выбранного ComboBoxItem)
            if (CmbSorting.SelectedItem is ComboBoxItem sortItem)
            {
                bool asc = sortItem.Tag.ToString() == "asc";
                filtered = asc ? filtered.OrderBy(p => p.Rating) : filtered.OrderByDescending(p => p.Rating);
            }
            // Показываю итоговый список
            DisplayProducts(filtered.ToList());
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

        /// <summary>
        /// Кнопка поиска – применяю все фильтры.
        /// </summary>
        private void BtnSearch_Click(object sender, RoutedEventArgs e) => ApplyAllFilters();

        /// <summary>
        /// Поиск по нажатию Enter в текстовом поле
        /// </summary>
        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                ApplyAllFilters();
        }

        /// <summary>
        /// Срабатывает при изменении фильтров (тип или производитель)
        /// </summary>
        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyAllFilters();

        /// <summary>
        /// Срабатывает при смене сортировки
        /// </summary>
        private void Sorting_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyAllFilters();

        /// <summary>
        /// Кнопка "Сбросить"
        /// </summary>
        private void BtnResetFilters_Click(object sender, RoutedEventArgs e)
        {
            CmbProductType.SelectedIndex = 0;
            CmbManufacturer.SelectedIndex = 0;
            TxtSearch.Text = "";
            ApplyAllFilters();
        }

        /// <summary>
        /// Кнопка перехода в корзину
        /// </summary>
        private void BtnCart_Click(object sender, RoutedEventArgs e)
        {
            if (!CurrentUser.IsAuthenticated)
            {
                MessageBox.Show("Сначала войдите в систему.", "Требуется авторизация");
                return;
            }
            NavigationService.Navigate(new CartPage());
        }

        /// <summary>
        /// Добавляю товар в корзину текущего пользователя
        /// Если такой товар уже лежит увеличиваю количество
        /// </summary>
        private void BtnAddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn) || !(btn.Tag is int productId)) return;

            try
            {
                using (var db = new BeautySalonEntities())
                {
                    var existing = db.CartItems.FirstOrDefault(c => c.UserId == CurrentUser.Id && c.ProductId == productId);
                    if (existing != null)
                        existing.Quantity++;
                    else
                        db.CartItems.Add(new CartItems { UserId = CurrentUser.Id, ProductId = productId, Quantity = 1 });

                    db.SaveChanges();
                }
                MessageBox.Show("Товар добавлен в корзину!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Открываю окно с подробной информацией о товаре
        /// </summary>
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