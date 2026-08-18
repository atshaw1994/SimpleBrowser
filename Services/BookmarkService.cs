using SimpleBrowser.Models;
using SimpleBrowser.Services.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleBrowser.Services
{
    public class BookmarkService(IBookmarksRepository bookmarksRepository) : IBookmarksService
    {
        public async Task AddBookmark(string url, string title)
        {
            if (string.IsNullOrWhiteSpace(url)) return;

            var entry = new BookmarkModel
            {
                Url = url,
                Name = string.IsNullOrWhiteSpace(title) ? url : title
            };

            await bookmarksRepository.SaveAsync(entry);
        }

        public async Task RemoveBookmarkAsync(BookmarkModel item)
        {
            if (item == null) return;
            await bookmarksRepository.DeleteAsync(item);
        }

        public async Task<IEnumerable<BookmarkModel>> GetBookmarksAsync() => await bookmarksRepository.LoadAllAsync();

        public async Task ClearAllAsync() => await bookmarksRepository.ClearAllAsync();
    }
}
