using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeautySalon17.Helpers
{
    /// <summary>
    /// Статический класс для хранения информации о текущем вошедшем пользователе.
    /// Доступен из любого места приложения.
    /// </summary>
    public static class CurrentUser
    {
        /// <summary>
        /// ID пользователя в базе данных.
        /// </summary>
        public static int Id { get; set; }

        /// <summary>
        /// Логин пользователя.
        /// </summary>
        public static string Login { get; set; }

        /// <summary>
        /// Фамилия и Имя (можно использовать для отображения).
        /// </summary>
        public static string FullName { get; set; }

        /// <summary>
        /// ID роли пользователя:
        /// 1 - Клиент
        /// 2 - Мастер
        /// 3 - Менеджер
        /// 4 - Администратор
        /// </summary>
        public static int RoleId { get; set; }

        /// <summary>
        /// Название роли (для удобства).
        /// </summary>
        public static string RoleName { get; set; }

        /// <summary>
        /// Флаг, авторизован ли пользователь в данный момент.
        /// </summary>
        public static bool IsAuthenticated
        {
            get { return Id != 0; }
        }

        /// <summary>
        /// Очищает данные текущего пользователя (выход из системы).
        /// </summary>
        public static void Clear()
        {
            Id = 0;
            Login = null;
            FullName = null;
            RoleId = 0;
            RoleName = null;
        }
    }
}
