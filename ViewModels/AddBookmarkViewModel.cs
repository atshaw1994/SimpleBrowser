using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;
using SimpleBrowser.Models;

namespace SimpleBrowser.ViewModels
{
    public partial class AddBookmarkViewModel : ViewModelBase
    {
        [RelayCommand]
        public void Ok() => AcceptAndCloseRequested?.Invoke(new BookmarkModel { Name = BookmarkName, Url = BookmarkUrl });

        [RelayCommand]
        public void Cancel() => AcceptAndCloseRequested?.Invoke(null!);

        [ObservableProperty]
        public partial string BookmarkName { get; set; }

        [ObservableProperty]
        public partial string BookmarkUrl { get; set; }

        public Action<BookmarkModel>? AcceptAndCloseRequested;

        public AddBookmarkViewModel()
        {
            BookmarkName = "New Bookmark";
            BookmarkUrl = "about:blank";
        }

        public AddBookmarkViewModel(string url, string title) 
        {
            BookmarkName = title;
            BookmarkUrl = url;
        }
    }
}
