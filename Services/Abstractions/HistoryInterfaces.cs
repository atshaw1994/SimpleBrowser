using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleBrowser.Models;

namespace SimpleBrowser.Services.Abstractions
{
    public interface IHistoryRepository
    {
        Task SaveAsync(HistoryItemModel item);
        Task DeleteAsync(HistoryItemModel item);
        Task<IEnumerable<HistoryItemModel>> LoadAllAsync();
        Task ClearAllAsync();
    }

    public interface IHistoryService
    {
        Task AddVisitAsync(string url, string title);
        Task RemoveEntryAsync(HistoryItemModel item);
        Task<IEnumerable<HistoryItemModel>> GetHistoryAsync();
        Task ClearHistoryAsync();
    }
}
