using System;
using CefSharp.Wpf;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Diagnostics;

namespace SimpleBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal ChromiumWebBrowser? selectedBrowser;
        private bool SelectedBrowser_IsLoading = false;
        private readonly List<Bookmark> BookmarkList;
        private readonly List<HistoryItem> HistoryList;

        public MainWindow()
        {
            InitializeComponent();
            BookmarkList = new List<Bookmark>();
            HistoryList = new List<HistoryItem>();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            baseTabControl.Items.Clear();
            NewTabButton_Click(sender, e);
            baseTabControl.SelectedIndex = 0;
            if (baseTabControl.Items[0] is TabItem tabItem && tabItem.Content is ChromiumWebBrowser browser)
                selectedBrowser = browser;
            LoadBookmarks();

        }
        private void Window_Deactivated(object sender, EventArgs e) => CaptionBar_Border.Background = (SolidColorBrush)FindResource("Caption.Inactive");
        private void Window_Activated(object sender, EventArgs e) => CaptionBar_Border.Background = (SolidColorBrush)FindResource("Caption.Active");

        #region BorderlessMethods

        private void CaptionBar_CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void CaptionBar_RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }
        private void CaptionBar_MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void RefreshMaximizeRestoreButton()
        {
            if (WindowState == WindowState.Maximized)
            {
                CaptionBar_MaximizeButton.Visibility = Visibility.Collapsed;
                CaptionBar_RestoreButton.Visibility = Visibility.Visible;
            }
            else
            {
                CaptionBar_MaximizeButton.Visibility = Visibility.Visible;
                CaptionBar_RestoreButton.Visibility = Visibility.Collapsed;
            }
        }
        private void Window_StateChanged(object sender, EventArgs e)
        {
            RefreshMaximizeRestoreButton();
        }
        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            ((HwndSource)PresentationSource.FromVisual(this)).AddHook(HookProc);
        }

        public static IntPtr HookProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_GETMINMAXINFO)
            {
                // We need to tell the system what our size should be when maximized. Otherwise it will cover the whole screen,
                // including the task bar.
                MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO))!;

                // Adjust the maximized size and position to fit the work area of the correct monitor
                IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

                if (monitor != IntPtr.Zero)
                {
                    MONITORINFO monitorInfo = new()
                    {
                        cbSize = Marshal.SizeOf(typeof(MONITORINFO))
                    };
                    GetMonitorInfo(monitor, ref monitorInfo);
                    RECT rcWorkArea = monitorInfo.rcWork;
                    RECT rcMonitorArea = monitorInfo.rcMonitor;
                    mmi.ptMaxPosition.X = Math.Abs(rcWorkArea.Left - rcMonitorArea.Left);
                    mmi.ptMaxPosition.Y = Math.Abs(rcWorkArea.Top - rcMonitorArea.Top);
                    mmi.ptMaxSize.X = Math.Abs(rcWorkArea.Right - rcWorkArea.Left);
                    mmi.ptMaxSize.Y = Math.Abs(rcWorkArea.Bottom - rcWorkArea.Top);
                }

                Marshal.StructureToPtr(mmi, lParam, true);
            }

            return IntPtr.Zero;
        }
        private const int WM_GETMINMAXINFO = 0x0024;
        private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;
        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr handle, uint flags);
        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);
        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                this.Left = left;
                this.Top = top;
                this.Right = right;
                this.Bottom = bottom;
            }
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }
        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }

        #endregion

        private void LoadBookmarks()
        {
            BookmarkBarStackPanel.Children.Clear();
            XmlDocument doc = new();
            doc.Load($"{AppDomain.CurrentDomain.BaseDirectory}\\Bookmarks.xml");
            foreach (XmlNode node in doc.DocumentElement!.ChildNodes)
            {
                Trace.WriteLine($"{node.Attributes![0].InnerText}, {node.Attributes[1]!.InnerText}");
                BookmarkList.Add(new Bookmark(node.Attributes![0].InnerText, node.Attributes[1]!.InnerText));
            }

            foreach (Bookmark bookmark in BookmarkList)
            {
                Button newBookmarkBarButton = new() { Content = bookmark.Title, Style = (Style)FindResource("BookmarkBarButtonStyle"), Tag = bookmark.URL };
                newBookmarkBarButton.Click += BookmarkBarButton_Click;
                BookmarkBarStackPanel.Children.Add(newBookmarkBarButton);
            }
        }
        private bool IsCurrentPageBookmarked(string URL)
        {
            foreach (Bookmark bookmark in BookmarkList)
                if (bookmark.URL == URL) return true;
            return false;
        }
        private void BookmarkBarButton_Click(object sender, RoutedEventArgs e) => selectedBrowser!.Load((sender as Button)!.Tag.ToString());

        public void CloseTabButton()
        {
            // TODO: Make it close the tab where the button was clicked, instead of selected tab.
            baseTabControl.Items.RemoveAt(baseTabControl.SelectedIndex);
        }
        public void NewTabButton_Click(object sender, RoutedEventArgs e)
        {
            ChromiumWebBrowser newWeb = new()
            {
                Address = "https://www.google.com"
            };
            TabItem newTab = new()
            {
                Header = "Loading...",
                Style = (Style)FindResource("TabItemStyle")
            };
            newWeb.LoadingStateChanged += NewWeb_LoadingStateChanged;
            newTab.Content = newWeb;
            baseTabControl.SelectedIndex = baseTabControl.Items.Add(newTab);
        }

        private void NewWeb_LoadingStateChanged(object? sender, CefSharp.LoadingStateChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (selectedBrowser is not null && selectedBrowser.Address is not null && selectedBrowser.Title is not null)
                {
                    if (e.IsLoading)
                    {
                        SelectedBrowser_IsLoading = true;
                        if (RefreshButton.Content is Grid grid && grid.Children[0] is TextBlock RefreshText && grid.Children[1] is TextBlock StopText)
                        {
                            RefreshText.Visibility = Visibility.Collapsed;
                            StopText.Visibility = Visibility.Visible;
                        }
                        Title = $"{e.Browser.MainFrame.Url} - SimpleBrowser";
                        (selectedBrowser.Parent as TabItem)!.Header = e.Browser.MainFrame.Url;
                        Omnibar.Text = e.Browser.MainFrame.Url;
                    }
                    else
                    {
                        SelectedBrowser_IsLoading = false;
                        if (RefreshButton.Content is Grid grid && grid.Children[0] is TextBlock RefreshText && grid.Children[1] is TextBlock StopText)
                        {
                            RefreshText.Visibility = Visibility.Visible;
                            StopText.Visibility = Visibility.Collapsed;
                        }
                        Title = $"{selectedBrowser.Title}  - SimpleBrowser";
                        (selectedBrowser.Parent as TabItem)!.Header = selectedBrowser.Title;
                        Omnibar.Text = selectedBrowser.Address;
                        if (IsCurrentPageBookmarked(selectedBrowser.Address)) BookmarkButton.Content = new TextBlock() 
                        { 
                            Text = "\xE735", 
                            FontSize = 11, 
                            FontFamily = new FontFamily("Segoe MDL2 Assets"),
                            Foreground = (SolidColorBrush)FindResource("MDL2Button.Overlay")
                        };
                    }
                }
            });
        }

        private void TabChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is TabItem selectedTab)
            {
                if (selectedTab.Content is ChromiumWebBrowser webBrowser)
                {
                    selectedBrowser = webBrowser;
                    if (selectedBrowser.Address is not null)
                    {
                        Omnibar.Text = selectedBrowser.Address;
                        Title = $"{selectedTab.Header} - SimpleBrowser";
                    }
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) => selectedBrowser!.BrowserCore.GoBack();
        private void ForwardButton_Click(object sender, RoutedEventArgs e) => selectedBrowser!.BrowserCore.GoForward();
        private void HomeButton_Click(object sender, RoutedEventArgs e) => selectedBrowser!.Load(Properties.Settings.Default.HomeURL);
        private void BookmarkButton_Popup_Opened(object sender, EventArgs e)
        {
            if (selectedBrowser is not null)
            {
                BookmarkButtonPopup_TitleTextBox.Text = selectedBrowser.Title;
                BookmarkButtonPopup_URLTextBox.Text = selectedBrowser.Address;
            }
        }
        private void BookmarkButtonPopup_CloseBookmarkButtonPopup_Click(object sender, RoutedEventArgs e)
        {
            BookmarkButton_Popup.IsOpen = false;
            BookmarkButtonPopup_TitleTextBox.Text = "";
            BookmarkButtonPopup_URLTextBox.Text = "";
        }
        private void BookmarkButtonPopup_AddBookmarkButton_Click(object sender, RoutedEventArgs e)
        {
            Bookmark newBookmark = new() { Title = BookmarkButtonPopup_TitleTextBox.Text,  URL = BookmarkButtonPopup_URLTextBox.Text };
            BookmarkList.Add(newBookmark);
            XmlDocument doc = new();
            XmlElement newElem = doc.CreateElement("Bookmark");
            newElem.SetAttribute("Title", newBookmark.Title);
            newElem.SetAttribute("URL", newBookmark.URL);
            doc.DocumentElement!.AppendChild(newElem);
            doc.Save($"{AppDomain.CurrentDomain.BaseDirectory}\\Bookmarks.xml");
        }
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedBrowser_IsLoading) selectedBrowser!.BrowserCore.StopLoad();
            else selectedBrowser!.BrowserCore.Reload();
        }

        private void Omnibar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) selectedBrowser!.Load(EnsureHttps(Omnibar.Text));
        }
        private static string EnsureHttps(string url)
        {
            if (url.Equals(""))
                return "about:blank";
            else if (!url.ToLower().StartsWith("http://") && !url.ToLower().StartsWith("https://"))
                return url.Insert(0, "http://");
            return url;
        }

        private void Menu_NewTab_Click(object sender, RoutedEventArgs e)
        {
            NewTabButton_Click(this, e);
            MenuButton.IsChecked = false;
        }
        private void Menu_NewWindow_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            MenuButton.IsChecked = false;
        }
        private void Menu_History_Click(object sender, RoutedEventArgs e)
        {
            MenuButton.IsChecked = false;
        }
        private void Menu_Bookmarks_Click(object sender, RoutedEventArgs e)
        {
            new BookmarkManager().Show();
            MenuButton.IsChecked = false;
        }
        private void Menu_Downloads_Click(object sender, RoutedEventArgs e)
        {
            MenuButton.IsChecked = false;
        }
        private void Menu_ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            // TODO: After switch to Chromium, ZoomIn
            MenuButton.IsChecked = false;
        }
        private void Menu_ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            // TODO: After switch to Chromium, ZoomOut
            MenuButton.IsChecked = false;
        }
        private void Menu_ZoomReset_Click(object sender, RoutedEventArgs e)
        {
            // TODO: After switch to Chromium, ZoomReset
            MenuButton.IsChecked = false;
        }
        private void Menu_Settings_Click(object sender, RoutedEventArgs e)
        {
            TabItem newTab = new() { Content = new SettingsTabItem(), Header = "Settings", Style = (Style)FindResource("TabItemStyle") };
            baseTabControl.SelectedIndex = baseTabControl.Items.Add(newTab);
            MenuButton.IsChecked = false;
        }

        private void BookmarkBar_CheckBox_Checked(object sender, RoutedEventArgs e) => BookmarkBar_Row.Height = new GridLength(32);
        private void BookmarkBar_CheckBox_Unchecked(object sender, RoutedEventArgs e) => BookmarkBar_Row.Height = new GridLength(0);

        
    }
}
