using ak_Bars.Yandex.Protocol;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ak_Bars.Yandex
{
    public class Yandex
    {
        private string Token { get; }
        public Yandex(string token)
        {
            Token = token;
        }

        private string url_disk = "https://cloud-api.yandex.net/v1/disk/";

        /// <summary>
        /// Получить каталок файлов и папок по указаному пути
        /// </summary>
        /// <param name="path">путь к папке на яндекс диск</param>
        /// <returns></returns>
        public async Task<Resource> Get_Files(string path)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["path"] = path;
            query["limit"] = "100";
            string queryString = query.ToString();

            return JsonConvert.DeserializeObject<Resource>(await GetAsync("resources", query));
        }

        public async Task<Disk> Get_Info_Disk()
        {
            return JsonConvert.DeserializeObject<Disk>(await GetAsync("", ""));
        }

        /// <summary>
        /// Получить ссылку для загрузки на яндекс диск
        /// </summary>
        /// <param name="path"></param>
        /// <param name="overwrite"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<Link> GetUploadLinkAsync(string path, bool overwrite, CancellationToken cancellationToken = default(CancellationToken))
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["path"] = path;
            query["overwrite"] = overwrite.ToString();
            string queryString = query.ToString();

            return JsonConvert.DeserializeObject<Link>(await GetAsync("resources/upload", query));
        }

        private async Task<string> GetAsync(string command, object param)
        {
            var builder = new UriBuilder(url_disk + command);
            builder.Port = -1;
            builder.Query = param.ToString();
            string url = builder.ToString();

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"OAuth {Token}");
            var response = await client.GetAsync(url);
            string responseBody = await response.Content.ReadAsStringAsync();

            return responseBody;
        }

        public async Task UploadAsync(string path_file, string path_save, IProgress progress, CancellationToken cancelToken = default(CancellationToken))
        {

            Stream stream = new FileStream(path_file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (progress == null) throw new ArgumentNullException(nameof(progress));


            var upload_link = await GetUploadLinkAsync(path_save, true);
            // Название файла
            var fileName = Path.GetFileName(((FileStream)stream).Name);

            // Подготавливаем контент потока
            var streamContent = new ProgressStreamContent(stream);
            streamContent.ProgressChanged += (bytes, currBytes, totalBytes) => progress.UpdateProgress(currBytes, totalBytes, fileName);

            // Данные на отправление
            var content = new MultipartFormDataContent();
            content.Add(streamContent, "file", fileName);

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"OAuth {Token}");

                    // Посылаем Post запрос
                    var response = await client.PostAsync(upload_link.Href, content, cancelToken);

                    // Проверяем статус ответа
                    switch (response.StatusCode)
                    {

                    }
                }
            }
            catch (HttpRequestException)
            {
                throw;
            }
        }
    }
}