using SimpleBrowser.Models;
using SimpleBrowser.Services.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SimpleBrowser.Services
{
    public class JsonBookmarksRepository : IBookmarksRepository
    {
        private readonly string _storageFolder;
        private readonly JsonSerializerOptions _jsonOptions;

        public JsonBookmarksRepository()
        {
            _storageFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SimpleBrowser\\Bookmarks\\"
            );
            Directory.CreateDirectory(_storageFolder);

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
        }
        public async Task<HistoryItemModel?> GetByIdAsync(Guid itemId)
        {
            string filePath = Path.Combine(_storageFolder, $"{itemId:N}.json");
            if (!File.Exists(filePath)) return null;

            try
            {
                using FileStream stream = File.OpenRead(filePath);
                var item = await JsonSerializer.DeserializeAsync<HistoryItemModel>(stream, _jsonOptions).ConfigureAwait(false);
                return item!;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Deserialization failed: {ex}");
                return null!;
            }
        }
        public async Task SaveAsync(BookmarkModel item)
        {
            ArgumentNullException.ThrowIfNull(item);

            // Create a safe, fixed-length hash of the URL
            byte[] hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(item.Id.ToString("N") + item.Url));
            string safeUrlHash = Convert.ToHexString(hashBytes);

            string filePath = Path.Combine(_storageFolder, $"{safeUrlHash}.json");
            using FileStream stream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(stream, item, _jsonOptions).ConfigureAwait(false);
        }
        public Task DeleteAsync(BookmarkModel item)
        {
            ArgumentNullException.ThrowIfNull(item);

            string filePath = Path.Combine(_storageFolder, $"{item.Id:N}.json");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.CompletedTask;
        }
        public async Task<IEnumerable<BookmarkModel>> LoadAllAsync()
        {
            var items = new List<BookmarkModel>();
            if (!Directory.Exists(_storageFolder)) return items;

            foreach (string filePath in Directory.GetFiles(_storageFolder, "*.json"))
            {
                try
                {
                    using FileStream stream = File.OpenRead(filePath);
                    var item = await JsonSerializer.DeserializeAsync<BookmarkModel>(stream, _jsonOptions).ConfigureAwait(false);
                    if (item != null)
                        items.Add(item);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Deserialization failed: {ex}");
                }
            }

            return items;
        }
        public Task ClearAllAsync()
        {
            if (Directory.Exists(_storageFolder))
            {
                foreach (string filePath in Directory.GetFiles(_storageFolder, "*.json"))
                {
                    File.Delete(filePath);
                }
            }

            return Task.CompletedTask;
        }

    }
}
