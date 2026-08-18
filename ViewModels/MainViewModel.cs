using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SimpleBrowser.Models;
using SimpleBrowser.Services.Abstractions;
using SimpleBrowser.Views;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SimpleBrowser.ViewModels;

public record MainServicesContext(
    IHistoryRepository HistoryRepository,
    IHistoryService HistoryService,
    IBookmarksRepository BookmarksRepository,
    IBookmarksService BookmarkService
);

public partial class MainViewModel : ViewModelBase
{
    public MainServicesContext Services;

    #region Properties
    private TabViewModel? _selectedTab;
    public TabViewModel? SelectedTab
    {
        get => _selectedTab;
        set
        {
            if (SetProperty(ref _selectedTab, value))
            {
                // Sync IsSelected across all tabs
                foreach (var tab in Tabs)
                {
                    tab.IsSelected = (tab == _selectedTab);
                }
            }
        }
    }

    [ObservableProperty]
    public partial bool IsLoading { get; set; } = false;

    [ObservableProperty]
    public partial double LoadingProgress { get; set; } = 0.0;

    [ObservableProperty]
    public partial string AddressBarUrl { get; set; } = "https://www.google.com";

    [ObservableProperty]
    public partial bool IsBookmarksShown { get; set; } = false;

    [ObservableProperty]
    public partial bool IsHistoryShown { get; set; } = false;
    #endregion

    #region Fields
    public ObservableCollection<HistoryItemModel> RecentHistory { get; } = [];
    public ObservableCollection<BookmarkModel> Bookmarks { get; } = [];
    public ObservableCollection<TabViewModel> Tabs { get; } = [];

    public bool HasHistory => RecentHistory.Count > 0;
    #endregion

    #region Commands
    [RelayCommand]
    public void ToggleBookmarksSidebar() => IsBookmarksShown = !IsBookmarksShown;
    [RelayCommand]
    public void ToggleHistorySidebar() => IsHistoryShown = !IsHistoryShown;

    [RelayCommand]
    public void Navigate() => SelectedTab?.Navigate(AddressBarUrl);

    [RelayCommand]
    private void NavigateToHistoryItem(HistoryItemModel? item)
    {
        if (item == null) return;
        SelectedTab?.Navigate(item.Url);
    }

    [RelayCommand(CanExecute = nameof(CanGoBack))]
    public void GoBack() => SelectedTab?.GoBack();
    private bool CanGoBack() => SelectedTab?.CanGoBack ?? false;

    [RelayCommand(CanExecute = nameof(CanGoForward))]
    public void GoForward() => SelectedTab?.GoForward();
    private bool CanGoForward() => SelectedTab?.CanGoForward ?? false;

    [RelayCommand]
    public void Refresh() => SelectedTab?.Refresh();

    [RelayCommand]
    public void Stop() => SelectedTab?.Stop();

    [RelayCommand(CanExecute = nameof(HasHistory))]
    public void ClearHistory()
    {
        _ = Services.HistoryService.ClearHistoryAsync();
        RecentHistory.Clear();
        OnPropertyChanged(nameof(HasHistory));
    }

    [RelayCommand]
    public void CloseTab(TabViewModel? tab)
    {
        if (tab == null) return;

        int index = Tabs.IndexOf(tab);
        Tabs.Remove(tab);

        if (SelectedTab == tab && Tabs.Count > 0)
        {
            SelectedTab = Tabs[System.Math.Max(0, index - 1)];
        }
    }

    [RelayCommand]
    public void AddTab()
    {
        var tab = new TabViewModel("https://www.google.com", Services);
        tab.CloseTabRequested += OnTabCloseRequested;
        Tabs.Add(tab);
        SelectedTab = tab;
    }

    [RelayCommand]
    public static void NewWindow()
    {
        var newWindow = App.Services.GetRequiredService<MainWindow>();
        newWindow.Show();
    }

    [RelayCommand]
    public async Task AddBookmark(Window owner)
    {
        var dialog = new AddBookmarkDialog(AddressBarUrl, SelectedTab?.Title ?? AddressBarUrl);
        BookmarkModel? newBookmark = await dialog.ShowDialog<BookmarkModel?>(owner);

        if (newBookmark != null)
        {
            Bookmarks.Add(newBookmark);
            await Services.BookmarksRepository.SaveAsync(newBookmark);
        }
    }

    [RelayCommand]
    public static void OpenDownloadsFolder()
    {
        var downloadsPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Downloads"
        );
        if (System.IO.Directory.Exists(downloadsPath))
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = downloadsPath,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log the error or show a message to the user)
                Console.WriteLine($"Error opening downloads folder: {ex.Message}");
            }
        }
        else
        {
            // Handle the case where the Downloads folder does not exist
            Console.WriteLine("Downloads folder does not exist.");
        }
    }

    [RelayCommand]
    public static void Exit()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.Shutdown();
    }
    #endregion

    public MainViewModel() => Services = null!;

    public MainViewModel(MainServicesContext services)
    {
        Services = services;
        _ = LoadRecentHistoryAsync();
        _ = LoadBookmarksAsync();
        AddTab();
    }

    private void SelectedTab_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is TabViewModel tab && tab == SelectedTab)
        {
            if (e.PropertyName == nameof(TabViewModel.Url))
            {
                AddressBarUrl = tab.Url;
            }
            else if (e.PropertyName is nameof(TabViewModel.CanGoBack) or nameof(TabViewModel.CanGoForward))
            {
                UpdateActiveTabState();
            }
        }
    }

    private void UpdateActiveTabState()
    {
        GoBackCommand.NotifyCanExecuteChanged();
        GoForwardCommand.NotifyCanExecuteChanged();
    }

    public async Task LoadRecentHistoryAsync()
    {
        RecentHistory.Clear();
        var history = await Services.HistoryService.GetHistoryAsync();
        foreach (var item in history)
            RecentHistory.Add(item);

        OnPropertyChanged(nameof(HasHistory));
    }

    public async Task LoadBookmarksAsync()
    {
        Bookmarks.Clear();
        var bookmarks = await Services.BookmarkService.GetBookmarksAsync();
        foreach (var item in bookmarks)
            Bookmarks.Add(item);
    }

    private void OnTabCloseRequested(TabViewModel sender) => CloseTab(sender);
}