using SimpleBrowser.Models;
using SimpleBrowser.Services.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleBrowser.Services
{
    public class HistoryService(IHistoryRepository historyRepository) : IHistoryService
    {
        public async Task AddVisitAsync(string url, string title)
        {
            if (string.IsNullOrWhiteSpace(url)) return;

            var entry = new HistoryItemModel
            {
                Url = url,
                Title = string.IsNullOrWhiteSpace(title) ? url : title,
                DateVisited = DateTime.UtcNow
            };

            await historyRepository.SaveAsync(entry);
        }

        public async Task RemoveEntryAsync(HistoryItemModel item)
        {
            if (item == null) return;
            await historyRepository.DeleteAsync(item);
        }

        public async Task<IEnumerable<HistoryItemModel>> GetHistoryAsync() => await historyRepository.LoadAllAsync();

        public async Task ClearHistoryAsync() => await historyRepository.ClearAllAsync();
    }
}
