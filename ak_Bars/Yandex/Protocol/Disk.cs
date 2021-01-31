using System;
using System.Collections.Generic;
using System.Text;

namespace ak_Bars.Yandex.Protocol
{
    /// <summary>
    /// Данные о свободном и занятом пространстве на Диск
    /// </summary>
    public class Disk
    {
        public long max_file_size { get; set; }

        public long unlimited_autoupload_enabled { get; set; }

        public long total_space { get; set; }

        public long is_paid { get; set; }

        public long used_space { get; set; }

        public SystemFolders SystemFolders { get; set; }

        public User user { get; set; }
    }

    /// <summary>
    /// Системные папки Диска
    /// </summary>
    public class SystemFolders
    {
        /// <summary>
        /// папка для файлов приложений
        /// </summary>
        public string Applications { get; set; }

        /// <summary>
        /// папка для файлов, загруженных из интернета(не с устройства пользователя).
        /// </summary>
        public string Downloads { get; set; }
    }

    public class User
    {
        public string country { get; set; }
        public string login { get; set; }
        public string display_name { get; set; }
        public string uid { get; set; }
    }
}
