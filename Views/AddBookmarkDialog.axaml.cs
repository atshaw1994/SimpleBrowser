using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SimpleBrowser.Models;
using SimpleBrowser.ViewModels;

namespace SimpleBrowser.Views
{
    public partial class AddBookmarkDialog : Window
    {
        private AddBookmarkViewModel _viewModel;

        public AddBookmarkDialog()
        {
            InitializeComponent();
            _viewModel = new AddBookmarkViewModel();
            _viewModel.AcceptAndCloseRequested += OnAcceptAndCloseRequested;
            DataContext = _viewModel;
        }

        public AddBookmarkDialog(string url, string title)
        {
            InitializeComponent();
            _viewModel = new AddBookmarkViewModel(url, title);
            _viewModel.AcceptAndCloseRequested += OnAcceptAndCloseRequested;
            DataContext = _viewModel;
        }

        private void OnAcceptAndCloseRequested(BookmarkModel? bookmark)
        {
            _viewModel.AcceptAndCloseRequested -= OnAcceptAndCloseRequested;
            Close(bookmark);
        }
    }
}