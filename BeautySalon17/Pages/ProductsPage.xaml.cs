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
using BeautySalon17.Helpers;
using System.IO;
using BeautySalon17;
using System.ComponentModel.DataAnnotations;

namespace BeautySalon17.Pages
{
    /// <summary>
    /// Логика взаимодействия для ProductsPage.xaml
    /// </summary>
    public partial class ProductsPage : Page
    {
        // Приватное поле для хранения полного списка товаров, загруженного из базы.
        private List<Products> _allProducts;

        public ProductsPage()
        {
            InitializeComponent();
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
                    _allProducts = context.Products.Include("Manufacturers").Include("ProductTypes").Where(p => p.IsActive == true).ToList();
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
        /// Отображает переданный список товаров в WrapPanel (ProductsWrapPanel).
        /// </summary>
        /// <param name="products">Список товаров для отображения.</param>
        private void DisplayProducts(List<Products> products)
        {
            // Очищаем панель от всех дочерних элементов, чтобы заново заполнить её
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
            // Внешняя рамка с закруглёнными углами, фоном и границей
            Border cardBorder = new Border
            {
                Width = 200,                           // Ширина карточки
                Height = 330,                          // Высота карточки
                Margin = new Thickness(10),            // Отступы со всех сторон по 10 пикселей
                Background = Brushes.White,            // Белый фон
                BorderBrush = Brushes.LightGray,       // Светло-серая рамка
                BorderThickness = new Thickness(1),    // Толщина рамки 1 пиксель
                CornerRadius = new CornerRadius(8)     // Скругление углов 8 пикселей
            };

            // Создаём сетку (Grid) для размещения элементов внутри карточки
            Grid cardGrid = new Grid();
            // Добавляем 4 строки с разной высотой
            cardGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(150) }); // Строка 0: фиксированная высота 150 (для картинки)
            cardGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });     // Строка 1: автоматическая высота (название)
            cardGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });     // Строка 2: автоматическая высота (цена)
            cardGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });     // Строка 3: автоматическая высота (кнопка)

            // ----- 1. Изображение товара -----
            Image productImage = new Image
            {
                Stretch = Stretch.Uniform,                // Изображение масштабируется с сохранением пропорций
                Margin = new Thickness(5),                // Отступы 5 пикселей со всех сторон
                Source = GetImageSource(product.ImagePath) // Получаем картинку из файла
            };
            Grid.SetRow(productImage, 0); // Помещаем изображение в строку 0
            cardGrid.Children.Add(productImage); // Добавляем в сетку

            // ----- 2. Название товара -----
            TextBlock nameText = new TextBlock
            {
                Text = product.Name,                          // Текст названия товара
                FontWeight = FontWeights.Bold,                // Жирный шрифт
                TextWrapping = TextWrapping.Wrap,             // Перенос текста, если не помещается
                TextAlignment = TextAlignment.Center,         // Выравнивание по центру
                Margin = new Thickness(5, 0, 5, 0)            // Отступы: слева 5, сверху 0, справа 5, снизу 0
            };
            Grid.SetRow(nameText, 1); // Строка 1
            cardGrid.Children.Add(nameText);

            // ----- 3. Цена (с учётом скидки) -----
            // Вычисляем конечную цену с учётом скидки (скидка в процентах)
            decimal finalPrice = product.Price * (1 - product.Discount / 100m);
            // Текстовый блок для отображения конечной цены
            TextBlock priceText = new TextBlock
            {
                Text = $"{finalPrice:F2} руб.",    // Форматируем цену с двумя знаками после запятой
                FontSize = 14,
                Foreground = Brushes.Green,        // Зелёный цвет текста
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(5)
            };

            // Если у товара есть скидка, показываем старую цену зачёркнутой
            if (product.Discount > 0)
            {
                // Создаём горизонтальную панель, чтобы разместить старую и новую цену рядом
                StackPanel pricePanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                // Текст со старой ценой (зачёркнут)
                TextBlock oldPrice = new TextBlock
                {
                    Text = $"{product.Price:F2}",
                    TextDecorations = TextDecorations.Strikethrough, // Зачёркивание
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(0, 0, 5, 0)               // Отступ справа 5
                };
                pricePanel.Children.Add(oldPrice);  // Добавляем старую цену
                pricePanel.Children.Add(priceText); // Добавляем новую цену

                Grid.SetRow(pricePanel, 2); // Помещаем панель в строку 2
                cardGrid.Children.Add(pricePanel);
            }
            else
            {
                // Если скидки нет, просто добавляем один текстовый блок с ценой
                Grid.SetRow(priceText, 2);
                cardGrid.Children.Add(priceText);
            }

            // ----- 4. Кнопка "В корзину" -----
            Button addToCartBtn = new Button
            {
                Content = "🛒 В корзину",                 // Текст на кнопке (с иконкой тележки)
                Height = 30,
                Margin = new Thickness(10, 0, 10, 10),   // Отступы: слева 10, сверху 0, справа 10, снизу 10
                Tag = product.Id,                        // Сохраняем ID товара в свойстве Tag (потом извлечём)
                IsEnabled = CurrentUser.IsAuthenticated   // Кнопка активна только если пользователь авторизован
            };
            // Подписываемся на событие клика по кнопке
            addToCartBtn.Click += BtnAddToCart_Click;

            Grid.SetRow(addToCartBtn, 3); // Строка 3
            cardGrid.Children.Add(addToCartBtn);

            // Помещаем сетку внутрь рамки
            cardBorder.Child = cardGrid;

            // Возвращаем готовую рамку (карточку)
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

        // ==================== ОБРАБОТЧИКИ СОБЫТИЙ ====================

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
        /// Выполняет фильтрацию товаров по тексту, введённому в TxtSearch.
        /// </summary>
        private void PerformSearch()
        {
            // Получаем текст из поля поиска и убираем лишние пробелы в начале и конце
            string searchText = TxtSearch.Text.Trim();

            // Если текст пустой — показываем все товары
            if (string.IsNullOrEmpty(searchText))
            {
                DisplayProducts(_allProducts);
            }
            else
            {
                // Иначе фильтруем список _allProducts: оставляем только те товары,
                // в названии которых содержится искомая строка (без учёта регистра)
                var filtered = _allProducts.Where(p =>
                    p.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0
                ).ToList();

                // Отображаем отфильтрованный список
                DisplayProducts(filtered);
            }
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
    }
}