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
            private List<Products> _allProducts;                // все активные товары
            private List<ProductTypes> _productTypes;           // список типов для фильтра
            private List<Manufacturers> _manufacturers;         // список производителей

            public ProductsPage()
            {
                InitializeComponent();
                LoadFilters();
                LoadProducts();
            }

            // Загрузка всех активных товаров из БД
            private void LoadProducts()
            {
                try
                {
                    using (var db = new BeautySalonEntities())
                    {
                        _allProducts = db.Products.Include("Manufacturers").Include("ProductTypes").Where(p => p.IsActive).ToList();
                    }
                    DisplayProducts(_allProducts);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}");
                }
            }
            // Загрузка справочников для выпадающих списков
            private void LoadFilters()
            {
                try
                {
                    using (var db = new BeautySalonEntities())
                    {
                        _productTypes = db.ProductTypes.ToList();
                        _manufacturers = db.Manufacturers.ToList();
                    }

                    // Добавляем пункт "Все"
                    _productTypes.Insert(0, new ProductTypes { Id = 0, Name = "Все типы" });
                    _manufacturers.Insert(0, new Manufacturers { Id = 0, Name = "Все производители" });

                    CmbProductType.ItemsSource = _productTypes;
                    CmbProductType.DisplayMemberPath = "Name";
                    CmbProductType.SelectedValuePath = "Id";
                    CmbProductType.SelectedIndex = 0;

                    CmbManufacturer.ItemsSource = _manufacturers;
                    CmbManufacturer.DisplayMemberPath = "Name";
                    CmbManufacturer.SelectedValuePath = "Id";
                    CmbManufacturer.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки фильтров: {ex.Message}");
                }
            }
            // Отображение переданного списка товаров в WrapPanel
            private void DisplayProducts(IEnumerable<Products> products)
            {
                ProductsWrapPanel.Children.Clear();

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

                foreach (var p in products)
                    ProductsWrapPanel.Children.Add(CreateProductCard(p));
            }
            // Создание визуальной карточки товара
            private FrameworkElement CreateProductCard(Products product)
            {
                decimal finalPrice = product.Price * (1 - product.Discount / 100m);

                // Основная рамка
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

                // Если скидка > 15%, подсвечиваем фон
                if (product.Discount > 15)
                    border.Background = new SolidColorBrush(Color.FromRgb(255, 255, 200));

                var grid = new Grid();
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(150) }); // картинка
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });    // название
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });    // цена
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });    // кнопки

                // Изображение
                var img = new Image
                {
                    Stretch = Stretch.Uniform,
                    Margin = new Thickness(5),
                    Source = LoadImage(product.ImagePath)
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

                // Блок с ценой (с учётом скидки)
                if (product.Discount > 0)
                {
                    var pricePanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    pricePanel.Children.Add(new TextBlock
                    {
                        Text = $"{product.Price:F2}",
                        TextDecorations = TextDecorations.Strikethrough,
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

                // Панель кнопок "В корзину" и "Подробнее"
                var btnPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 5, 0, 5)
                };

                var btnCart = new Button
                {
                    Content = "В корзину",
                    Width = 90,
                    Height = 30,
                    Tag = product.Id,
                    IsEnabled = CurrentUser.IsAuthenticated
                };
                btnCart.Click += BtnAddToCart_Click;
                btnPanel.Children.Add(btnCart);

                var btnDetails = new Button
                {
                    Content = "Подробнее",
                    Width = 90,
                    Height = 30,
                    Margin = new Thickness(5, 0, 0, 0),
                    Tag = product
                };
                btnDetails.Click += BtnDetails_Click;
                btnPanel.Children.Add(btnDetails);

                Grid.SetRow(btnPanel, 3);
                grid.Children.Add(btnPanel);

                border.Child = grid;
                return border;
            }
            // Загрузка картинки из файла 
            private BitmapImage LoadImage(string imagePath)
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
                catch { }
                return null;
            }
            // Применение фильтров, поиска и сортировки — единая точка
            private void ApplyAllFilters()
            {
                if (_allProducts == null) return;

                var filtered = _allProducts.AsEnumerable();

                // Поиск по названию
                string search = TxtSearch.Text.Trim();
                if (!string.IsNullOrEmpty(search))
                    filtered = filtered.Where(p => p.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);

                // Фильтр по типу
                if (CmbProductType.SelectedValue is int typeId && typeId != 0)
                    filtered = filtered.Where(p => p.ProductTypeId == typeId);

                // Фильтр по производителю
                if (CmbManufacturer.SelectedValue is int manId && manId != 0)
                    filtered = filtered.Where(p => p.ManufacturerId == manId);

                // Сортировка по рейтингу
                if (CmbSorting.SelectedItem is ComboBoxItem sortItem)
                {
                    bool asc = sortItem.Tag.ToString() == "asc";
                    filtered = asc ? filtered.OrderBy(p => p.Rating) : filtered.OrderByDescending(p => p.Rating);
                }

                DisplayProducts(filtered.ToList());
            }
            // Обработчики событий интерфейса
            private void BtnBack_Click(object sender, RoutedEventArgs e)
            {
                if (NavigationService.CanGoBack)
                    NavigationService.GoBack();
                else
                    NavigationService.Navigate(new StartPage());
            }

            private void BtnSearch_Click(object sender, RoutedEventArgs e) => ApplyAllFilters();

            private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
            {
                if (e.Key == Key.Enter)
                    ApplyAllFilters();
            }

            private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyAllFilters();

            private void Sorting_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyAllFilters();

            private void BtnResetFilters_Click(object sender, RoutedEventArgs e)
            {
                CmbProductType.SelectedIndex = 0;
                CmbManufacturer.SelectedIndex = 0;
                TxtSearch.Text = "";
                ApplyAllFilters();
            }
            private void BtnCart_Click(object sender, RoutedEventArgs e)
            {
                if (!CurrentUser.IsAuthenticated)
                {
                    MessageBox.Show("Сначала войдите в систему.", "Требуется авторизация");
                    return;
                }
                NavigationService.Navigate(new CartPage());
            }
            // Добавление товара в корзину
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
            // Открытие окна с подробной информацией о товаре
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