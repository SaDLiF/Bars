using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ak_Bars.Settings;
using ak_Bars.Yandex;
using ak_Bars.Yandex.Protocol;

namespace ak_Bars
{
    class Program
    {

        static string oauthToken { get; set; }
        Yandex.Yandex yandex { get; set; }

        static void Main(string[] args)
        {
            Program p = new Program();
            p.Start();
        }

        private void Start()
        {
            Login().Wait();
            //Выбрать файлы для загрузки на яндекс диск
            var selected_files = Select_Directory();
            //Выбрать путь для загрузки
            var path_yandex = GetInfo_Disk().Result;
            //Загрузить
            UploadFile(selected_files, path_yandex).Wait();
        }


        #region Login
        private async Task Login()
        {
            oauthToken = Config.Read().Token;
            if (oauthToken == null)
            {
                Console.WriteLine("Введите токен яндекс диска");
                oauthToken = Console.ReadLine();
                yandex = new Yandex.Yandex(oauthToken);
            }
            else
            {
                yandex = new Yandex.Yandex(oauthToken);
            }

            var y_client = await yandex.Get_Info_Disk();
            if (y_client.user != null)
            {
                Console.WriteLine($"Подключено к яндекс диск {y_client.user.display_name}");
                Config.Write(oauthToken);
                Thread.Sleep(2000);
            }
            else
            {
                Console.WriteLine($"Ошибка подключения к яндекс диску");
                await Login();
            }


        }

        #endregion

        #region Directory
        private List<Folders> Select_Directory(string PathToFolder = "/")
        {
            List<Folders> folders = new List<Folders>();
            for (; ; )
            {
                folders = new List<Folders>();

                if (PathToFolder == "/")
                {
                    DriveInfo[] allDrives = DriveInfo.GetDrives();
                    foreach (DriveInfo d in allDrives)
                    {
                        folders.Add(new Folders { type = Type.Folder, name = d.Name, path = d.Name });
                    }

                }
                else
                {
                    folders.Add(new Folders { type = Type.Back, name = $"Назад" });

                    string[] allfiles = Directory.GetFiles(PathToFolder);
                    string[] allfolders = Directory.GetDirectories(PathToFolder);
                    foreach (string folder in allfolders)
                    {
                        folders.Add(new Folders { type = Type.Folder, name = new DirectoryInfo(folder).Name, path = folder });
                    }
                    foreach (string filename in allfiles)
                    {
                        folders.Add(new Folders { type = Type.File, name = new DirectoryInfo(filename).Name, path = filename });
                    }
                }

                var Message = $"Выберите папку для загрузки на яндекс диск";

                var path = PathToFolder;
                PathToFolder = GetListDirectory(folders, Message, path);

                if (PathToFolder == ":select:")
                    break;
                else if (PathToFolder == ":back:")
                {
                    var dir = Path.GetDirectoryName(path);
                    if (dir != null)
                        PathToFolder = dir.ToString();
                    else
                        PathToFolder = "/";
                }
                else if (PathToFolder == ":error:")
                    PathToFolder = path;


            }
            return folders.Where(t => t.type == Type.File).ToList();
        }

        #endregion

        #region Работа с папками на яндекс диск
        /// <summary>
        /// Получить список файлов и папок на яндекс диске
        /// </summary>
        private async Task<string> GetInfo_Disk(string Path = "/")
        {


            string path = "/";
            for (; ; )
            {
                string Message = "Выберите папку для загрузки файлов на яндекс диск (ENTER)";
                List<Folders> folders = new List<Folders>();
                Console.WriteLine("Загрузка списка папок яндекс диска");
                Resource result = await yandex.Get_Files(Path);

                if (Path != "/")
                    folders.Add(new Folders { name = "Назад", type = Type.Back });
                foreach (var r in result.Embedded.Items)
                {
                    var folder = new Folders
                    {
                        name = r.Name,
                        path = r.Path
                    };
                    if (r.Type == ResourceType.Dir)
                        folder.type = Type.Folder;
                    else
                        folder.type = Type.File;
                    folders.Add(folder);
                };

                path = Path;
                Path = GetListDirectory(folders, Message, path);

                if (Path == ":select:")
                    break;
                else if (Path == ":back:")
                    Path = path.GetLastLink();


            }
            return path;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folders"></param>
        /// <returns>path</returns>
        public string GetListDirectory(List<Folders> folders, string Message, string path)
        {
            Console.Clear();
            Console.WriteLine(Message);
            Console.WriteLine($"Выбрать текущий каталог ({path}) ENTER");
            Console.WriteLine();
            int i = 0;
            foreach (var f in folders)
            {
                if (f.type == Type.Folder || f.type == Type.Back)
                    Console.WriteLine($"{i++}.{f.name}");
                else
                    Console.WriteLine(f.name);
            }
            var command = Console.ReadLine();


            if (command == "0")
            {
                if (folders[0].type == Type.Back)
                    return ":back:";
                else
                    return folders[0].path;
            }
            else if (command != "")
            {
                int ind = 0;
                Int32.TryParse(command, out ind);
                if (ind != 0)
                {
                    Console.Clear();
                    if (folders.Where(t => t.type != Type.File).ToList().Count > ind)
                    {
                        return folders[ind].path;
                    }
                    else
                        command = ":error:";
                }
            }
            else
            {
                return ":select:";
            }

            return command;
        }



        #endregion

        #region Загрузка файлов на яндекс диск

        List<Status> List_Upload;

        private async Task UploadFile(List<Folders> files, string path_disk)
        {
            List_Upload = new List<Status>();
            System.Timers.Timer timer = new System.Timers.Timer(500);

            timer.Elapsed += Timer_Elapsed;
            Console.CursorVisible = false;
            Console.Clear();
            Thread.Sleep(100);

            Console.WriteLine("Загрузка файлов на яндекс диск");
            Console.WriteLine("");
            Console.WriteLine("");

            IEnumerable<Task<string>> downloadTasksQuery =
                from url in files
                select ProcessUrlAsync(url, path_disk);
            timer.Start();
            List<Task<string>> downloadTasks = downloadTasksQuery.ToList();

            await Task.WhenAll(downloadTasks);

            timer.Stop();
            Console.ReadKey();
            Thread.Sleep(500);
        }



        async Task<string> ProcessUrlAsync(Folders files, string path_disk)
        {
            List_Upload.Add(new Status { name = files.name, status = "Загрузка 0 %" });
            Console.WriteLine($"{files.name} Загрузка 0 %");
            await yandex.UploadAsync(files.path, $"{path_disk}/{files.name}", new AsyncProgress(this.UpdateProgress));
            return files.name;
        }

        #region Timer
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Console.SetCursorPosition(0, 3);
            foreach (var t in List_Upload)
            {
                Console.WriteLine($"{t.name} - {t.status}");
            }

            Thread.Sleep(100);
            Console.SetCursorPosition(0, 2);

            var count = List_Upload.Where(t => t.status == "Загрузка 100 %").ToList().Count;
            Console.WriteLine($"Загружено файлов {count} из {List_Upload.Count}");
        }

        #endregion

        #region Event

        private void UpdateProgress(long arg1, long arg2, string arg3)
        {
            double percent = Math.Round((double)arg1 / (double)arg2 * 100, 0);
            List_Upload.Where(t => t.name == arg3).FirstOrDefault().status = $"Загрузка {percent} %";
        }

        #endregion



        #endregion
    }
}
