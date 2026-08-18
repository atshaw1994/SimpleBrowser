using SimpleBrowser.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBrowser.Services.Abstractions
{
    public interface IBookmarksRepository
    {
        Task SaveAsync(BookmarkModel item);
        Task DeleteAsync(BookmarkModel item);
        Task<IEnumerable<BookmarkModel>> LoadAllAsync();
        Task ClearAllAsync();
    }

    public interface IBookmarksService
    {
        Task AddBookmark(string url, string title);
        Task RemoveBookmarkAsync(BookmarkModel item);
        Task<IEnumerable<BookmarkModel>> GetBookmarksAsync();
        Task ClearAllAsync();
    }
}
