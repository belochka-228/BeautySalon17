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
        public static int Id { get; set; }
        public static string Login { get; set; }
        public static string FullName { get; set; }
        public static int RoleId { get; set; }
        public static string RoleName { get; set; }
        public static bool IsAuthenticated
        {
            get { return Id != 0; }
        }
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
