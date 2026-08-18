using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SimpleBrowser.ViewModels
{
    public partial class TabViewModel : ViewModelBase
    {
        #region Properties
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(GoBackCommand))]
        public partial bool CanGoBack { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(GoForwardCommand))]
        public partial bool CanGoForward { get; set; }

        [ObservableProperty]
        public partial double LoadingProgress { get; set; } = 50.0;

        [ObservableProperty]
        public partial bool IsLoading { get; set; } = true;


        [ObservableProperty]
        public partial bool IsSelected { get; set; }

        [ObservableProperty]
        public partial string Title { get; set; } = "New Tab";
        partial void OnTitleChanged(string value)
        {
            if (string.IsNullOrEmpty(value)) return;

            // Trim leading and trailing double quotes
            string sanitized = value.Trim('"');

            // Prevent infinite recursion loops when setting the generated property
            if (sanitized != value)
                Title = sanitized;
        }

        [ObservableProperty]
        public partial string Url { get; set; } = string.Empty;

        #endregion

        #region Fields
        private readonly MainServicesContext? _services;
        private DispatcherTimer? _loadingProgressTimer;

        public ObservableCollection<object?> Content { get; set; } = [];
        #endregion

        public Action<TabViewModel>? CloseTabRequested;

        public TabViewModel()
        {
            _services = null;
            Border designBackground = new()
            {
                Background = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            TextBlock designText = new()
            {
                Text = "WebView Placeholder",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 24,
                Foreground = Brushes.DarkGray
            };
            designBackground.Child = designText;
            Content.Add(designBackground);
        }

        public TabViewModel(string initialUrl, MainServicesContext services)
        {
            _services = services;
            Url = initialUrl;

            var WebView = new NativeWebView
            {
                Source = new Uri(initialUrl),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
            };

            WebView.NavigationStarted += NativeWebView_NavigationStarted;
            WebView.NavigationCompleted += NativeWebView_NavigationCompleted;

            // Re-apply current Url whenever Avalonia re-attaches the native view to the Visual Tree
            WebView.AttachedToVisualTree += (s, e) =>
            {
                if (WebView.Source?.ToString() != Url && Uri.TryCreate(Url, UriKind.Absolute, out var targetUri))
                {
                    WebView.Source = targetUri;
                }
            };
            Content.Add(WebView);
        }

        #region Commands
        [RelayCommand]
        public void Navigate(string url)
        {
            Trace.WriteLine($"TabViewModel: Attempting to navigate to {url}");
            if (string.IsNullOrWhiteSpace(url)) return;

            string targetUrl = url.Trim();

            if (!targetUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !targetUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                targetUrl = "https://" + targetUrl;
            }

            // Explicitly sync Url immediately
            Url = targetUrl;

            if (Uri.TryCreate(targetUrl, UriKind.Absolute, out var uri))
            {
                GetWebView().Source = uri;
            }
        }

        [RelayCommand(CanExecute = nameof(CanGoBack))]
        public void GoBack() => GetWebView().GoBack();

        [RelayCommand(CanExecute = nameof(CanGoForward))]
        public void GoForward() => GetWebView().GoForward();

        [RelayCommand]
        public void Refresh() => GetWebView().Refresh();

        [RelayCommand]
        public void Stop() => GetWebView().Stop();

        [RelayCommand]
        public void CloseTab() => CloseTabRequested?.Invoke(this);
        #endregion

        private void NativeWebView_NavigationStarted(object? sender, WebViewNavigationStartingEventArgs e)
        {
            UpdateLoadingState(true);
            UpdateProgressValue(10);

            _loadingProgressTimer?.Stop();
            _loadingProgressTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _loadingProgressTimer.Tick += (s, args) =>
            {
                if (LoadingProgress < 90)
                {
                    double remaining = 90 - LoadingProgress;
                    UpdateProgressValue(LoadingProgress + (remaining * 0.1));
                }
            };
            _loadingProgressTimer.Start();
        }

        private async void NativeWebView_NavigationCompleted(object? sender, WebViewNavigationCompletedEventArgs e)
        {
            _loadingProgressTimer?.Stop();

            UpdateProgressValue(100);
            await Task.Delay(200);
            UpdateLoadingState(false);
            UpdateProgressValue(0);

            if (e.IsSuccess && sender is NativeWebView webView)
            {
                string rawTitle = await webView.InvokeScript("document.title") ?? string.Empty;

                // Keep ViewModel Url updated on page transitions/redirects
                if (webView.Source != null)
                {
                    Url = webView.Source.ToString();
                }

                Title = string.IsNullOrWhiteSpace(rawTitle)
                    ? "Untitled"
                    : (rawTitle.Length > 15 ? $"{rawTitle[..15]}..." : rawTitle);

                await _services!.HistoryService.AddVisitAsync(Url, rawTitle);
                UpdateNavigationState(webView.CanGoBack, webView.CanGoForward);
            }
        }

        public void UpdateLoadingState(bool isLoading) => IsLoading = isLoading;
        public void UpdateProgressValue(double progress) => LoadingProgress = progress;
        public void UpdateNavigationState(bool canGoBack, bool canGoForward)
        {
            CanGoBack = canGoBack;
            CanGoForward = canGoForward;
        }

        public NativeWebView GetWebView() => Content[0] as NativeWebView ?? throw new InvalidOperationException("WebView is not initialized.");
    }
}