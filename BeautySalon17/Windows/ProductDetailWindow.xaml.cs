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
using System.IO;
using System;
using BeautySalon17.Pages;

namespace BeautySalon17.Windows
{
    /// <summary>
    /// Логика взаимодействия для ProductDetailWindow.xaml
    /// </summary>
    public partial class ProductDetailWindow : Window
    {
        // Конструктор принимает товар, который нужно показать
        public ProductDetailWindow(Products product)
        {
            InitializeComponent();

            // Заполняем все поля данными из товара
            TxtName.Text = product.Name;
            TxtDescription.Text = string.IsNullOrEmpty(product.Description)
                ? "Описание отсутствует."
                : product.Description;

            // Рейтинг (Rating — double, поэтому просто форматируем)
            TxtRating.Text = product.Rating > 0
                ? $"★ {product.Rating:F1}"
                : "Нет оценок";

            // Производитель и тип товара (если они загружены)
            TxtManufacturer.Text = product.Manufacturers != null
                ? $"Производитель: {product.Manufacturers.Name}"
                : "Производитель не указан";
            TxtProductType.Text = product.ProductTypes != null
                ? $"Тип: {product.ProductTypes.Name}"
                : "Тип не указан";

            // Цена с учётом скидки
            decimal finalPrice = product.Price * (1 - product.Discount / 100m);
            TxtPrice.Text = $"{finalPrice:F2} руб.";

            if (product.Discount > 0)
            {
                TxtOldPrice.Text = $"{product.Price:F2} руб.";
                TxtOldPrice.Visibility = Visibility.Visible;
            }

            // Картинка
            ProductImage.Source = GetImageSource(product.ImagePath);
        }

        /// <summary>
        /// Загружает изображение из папки Images.
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
                return null;
            }
            catch
            {
                return null;
            }
        }

        // Кнопка "Закрыть"
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
