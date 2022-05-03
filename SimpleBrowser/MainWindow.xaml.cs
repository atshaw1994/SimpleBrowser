using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Linq;

namespace SimpleBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal ChromiumWebBrowser? selectedBrowser;
        private bool SelectedBrowser_IsLoading = false;
        private readonly XDocument BookmarkXml;
        private readonly XDocument HistoryXml;
        private readonly XDocument SettingsXml;
        private int StartupMode;
        private string HomePage;

        public MainWindow()
        {
            InitializeComponent();
            StartupMode = 0;
            HomePage = "";
            BookmarkXml = XDocument.Load($"{AppDomain.CurrentDomain.BaseDirectory}\\Bookmarks.xml");
            HistoryXml = XDocument.Load($"{AppDomain.CurrentDomain.BaseDirectory}\\History.xml");
            SettingsXml = XDocument.Load($"{AppDomain.CurrentDomain.BaseDirectory}\\Settings.xml");
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSettings();
            LoadBookmarks();
            baseTabControl.Items.Clear();
            NewTabButton_Click(sender, e);
            baseTabControl.SelectedIndex = 0;
            if (baseTabControl.Items[0] is TabItem tabItem && tabItem.Content is ChromiumWebBrowser browser)
                selectedBrowser = browser;
            if (selectedBrowser is not null)
            {
                if (StartupMode == 0) selectedBrowser.Address = HomePage; // Home Page
                else if (StartupMode == 1) selectedBrowser.Address = "about:blank"; // New Tab Page
                else if (StartupMode == 2) selectedBrowser.Address = "about:blank"; // Blank
            }
        }
        private void Window_Deactivated(object sender, EventArgs e)
        {
            CaptionBar_Border.Background = (SolidColorBrush)FindResource("CaptionBar.Background.Inactive");
        }
        private void Window_Activated(object sender, EventArgs e)
        {
            CaptionBar_Border.Background = (SolidColorBrush)FindResource("CaptionBar.Background.Active");
        }

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
            foreach (XElement node in BookmarkXml.Root!.ElementsAfterSelf())
            {
                Button newBkBarButton = new() 
                { 
                    Content = node.Descendants("Title").First().Value, 
                    Style = (Style)FindResource("BookmarkBarButtonStyle"), 
                    Tag = node.Descendants("URL").First().Value
                };
                newBkBarButton.Click += BookmarkBarButton_Click;
                BookmarkBarStackPanel.Children.Add(newBkBarButton);
            }
        }
        private void LoadSettings()
        {
            List<XNode> nodes = SettingsXml.Root!.Nodes().ToList();
            StartupMode = Convert.ToInt32(((nodes[0] as XElement)!.Nodes().First() as XText)!.Value);
            bool ShowBBar = Convert.ToBoolean(((nodes[1] as XElement)!.Nodes().First() as XText)!.Value);
            BookmarkBar_Row.Height = ShowBBar ? new GridLength(24) : new GridLength(0);
            HomePage = ((nodes[2] as XElement)!.Nodes().First() as XText)!.Value;
            string theme = ((nodes[3] as XElement)!.Nodes().First() as XText)!.Value;
            Application.Current.Resources.MergedDictionaries[0].Source = new Uri($"\\Themes\\{theme}.xaml", UriKind.Relative);
        }

        private void BookmarkBarButton_Click(object sender, RoutedEventArgs e)
        {
            selectedBrowser!.Load((sender as Button)!.Tag.ToString());
        }

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
            TabItem newTab = new() { Header = "Loading..." };
            newWeb.LoadingStateChanged += NewWeb_LoadingStateChanged;
            newTab.Content = newWeb;
            baseTabControl.SelectedIndex = baseTabControl.Items.Add(newTab);
        }

        private void NewWeb_LoadingStateChanged(object? sender, CefSharp.LoadingStateChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (selectedBrowser is not null && selectedBrowser.Address is not null &&
                selectedBrowser.Title is not null && selectedBrowser.Parent is TabItem parentTab)
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
                        Icon = LoadFavicon(selectedBrowser.Address);

                        SelectedBrowser_IsLoading = false;
                        if (RefreshButton.Content is Grid grid && grid.Children[0] is TextBlock RefreshText && grid.Children[1] is TextBlock StopText)
                        {
                            RefreshText.Visibility = Visibility.Visible;
                            StopText.Visibility = Visibility.Collapsed;
                        }
                        Title = $"{selectedBrowser.Title}  - SimpleBrowser";
                        (selectedBrowser.Parent as TabItem)!.Header = selectedBrowser.Title;
                        Omnibar.Text = selectedBrowser.Address;
                        if (BookmarkExists(selectedBrowser.Address, selectedBrowser.Title) is null)
                        {
                            NewBookmarkButton.Visibility = Visibility.Visible;
                            EditBookmarkButton.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            NewBookmarkButton.Visibility = Visibility.Collapsed;
                            EditBookmarkButton.Visibility = Visibility.Visible;
                        }
                    }
                    AddHistoryItem(selectedBrowser.Address, selectedBrowser.Title);
                }
            });
        }

        private void AddHistoryItem(string PageURL, string PageTitle)
        {
            if (HistoryItemExists(PageURL))
            {
                List<XNode> nodes = HistoryXml.Root!.Nodes().ToList();
                foreach (var node in nodes.Where(node => (node as XElement)!.Descendants("URL")!.First().Value.Equals(PageURL)))
                {
                    (node as XElement)!.Descendants("LastVisited")!.First().Value = DateTime.Now.ToString("yyyyMMddHHmmss");
                }
            }
            else
            {
                Omnibar.Items.Add(PageURL);
                XElement newHistoryItem =
                new("HistoryItem",
                    new XElement("Title", PageTitle),
                    new XElement("URL", PageURL),
                    new XElement("LastVisited", DateTime.Now.ToString("yyyyMMddHHmmss"))
                );
                HistoryXml.Root!.Add(newHistoryItem);
            }
            HistoryXml.Save($"{AppDomain.CurrentDomain.BaseDirectory}\\History.xml");
        }
        private bool HistoryItemExists(string URL = "")
        {
            List<XNode> nodes = HistoryXml.Root!.NodesAfterSelf().ToList();
            foreach (XNode node in nodes)
            {
                if ((node as XElement)!.Descendants("URL").First().Value.Equals(URL))
                {
                    return true;
                }
            }
            return false;
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
                        Icon = LoadFavicon(selectedBrowser.Address);
                        SelectedBrowser_IsLoading = false;
                        if (RefreshButton.Content is Grid grid && grid.Children[0] is TextBlock RefreshText && grid.Children[1] is TextBlock StopText)
                        {
                            RefreshText.Visibility = Visibility.Visible;
                            StopText.Visibility = Visibility.Collapsed;
                        }
                        Title = $"{selectedBrowser.Title}  - SimpleBrowser";
                        (selectedBrowser.Parent as TabItem)!.Header = selectedBrowser.Title;
                        Omnibar.Text = selectedBrowser.Address;
                        if (BookmarkExists(selectedBrowser.Address, selectedBrowser.Title) is null)
                        {
                            NewBookmarkButton.Visibility = Visibility.Visible;
                            EditBookmarkButton.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            NewBookmarkButton.Visibility = Visibility.Collapsed;
                            EditBookmarkButton.Visibility = Visibility.Visible;
                        }

                        AddHistoryItem(selectedBrowser.Address, selectedBrowser.Title);
                    }
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) => selectedBrowser!.BrowserCore.GoBack();
        private void ForwardButton_Click(object sender, RoutedEventArgs e) => selectedBrowser!.BrowserCore.GoForward();
        private void HomeButton_Click(object sender, RoutedEventArgs e) => selectedBrowser!.Load(Properties.Settings.Default.HomeURL);
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedBrowser_IsLoading)
            {
                selectedBrowser!.BrowserCore.StopLoad();
            }
            else
            {
                selectedBrowser!.BrowserCore.Reload();
            }
        }

        private void Omnibar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                selectedBrowser!.Load(EnsureHttps(Omnibar.Text));
            }
        }
        private static string EnsureHttps(string url)
        {
            if (url.Equals(""))
            {
                return "about:blank";
            }
            else if (!url.ToLower().StartsWith("http://") && !url.ToLower().StartsWith("https://"))
            {
                return url.Insert(0, "http://");
            }

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
            BookmarkManager manager = new(1);
            manager.Show();
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
            TabItem newTab = new() { Content = new SettingsTabItem(this), Header = "Settings" };
            baseTabControl.SelectedIndex = baseTabControl.Items.Add(newTab);
            MenuButton.IsChecked = false;
        }

        private XElement? BookmarkExists(string URL, string Title)
        {
            if (BookmarkXml.Root is not null)
                foreach (XElement node in BookmarkXml.Root!.ElementsAfterSelf())
                    if (node.Descendants("Title").First().Value.Equals(Title) && node.Descendants("URL").First().Value.Equals(URL)) 
                        return node;
            return null;
        }
        private void NewBookmarkButton_Popup_Opened(object sender, EventArgs e)
        {
            if (selectedBrowser is not null)
            {
                NewBookmarkPopup_TitleTextBox.Text = selectedBrowser.Title;
                NewBookmarkPopup_URLTextBox.Text = selectedBrowser.Address;
            }
        }
        private void NewBookmarkPopup_CloseButton_Click(object sender, RoutedEventArgs e)
        {
            NewBookmarkPopup.IsOpen = false;
            NewBookmarkPopup_TitleTextBox.Text = "";
            NewBookmarkPopup_URLTextBox.Text = "";
        }
        private void NewBookmarkPopup_LibraryButton_Click(object sender, RoutedEventArgs e) => new BookmarkManager().Show();

        private void NewBookmarkPopup_AddButton_Click(object sender, RoutedEventArgs e)
        {
            Button newBkBarButton = new()
            {
                Content = NewBookmarkPopup_TitleTextBox.Text,
                Style = (Style)FindResource("BookmarkBarButtonStyle"),
                Tag = NewBookmarkPopup_URLTextBox.Text
            };
            newBkBarButton.Click += BookmarkBarButton_Click;
            BookmarkBarStackPanel.Children.Add(newBkBarButton);
            XElement newElem = 
                new("Bookmark",
                    new XElement("Title", NewBookmarkPopup_TitleTextBox.Text),
                    new XElement("URL", NewBookmarkPopup_URLTextBox.Text)
            );
            BookmarkXml.Root!.Add(newElem);
            BookmarkXml.Save($"{AppDomain.CurrentDomain.BaseDirectory}\\Bookmarks.xml");
        }
        private void EditBookmarkButton_Popup_Opened(object sender, EventArgs e)
        {
            if (selectedBrowser is not null)
            {
                foreach (XElement bookmark in BookmarkXml.Root!.ElementsAfterSelf())
                {
                    if(bookmark.Descendants("Title").First().Value.Equals(selectedBrowser.Title) &&
                        bookmark.Descendants("URL").First().Value.Equals(selectedBrowser.Address))
                    {
                        EditBookmarkPopup_TitleTextBox.Text = bookmark.Descendants("Title").First().Value;
                        EditBookmarkPopup_URLTextBox.Text = bookmark.Descendants("URL").First().Value;
                    }
                }
                BookmarkXml.Save($"{AppDomain.CurrentDomain.BaseDirectory}\\Bookmarks.xml");
            }
        }
        private void EditBookmarkPopup_CloseButton_Click(object sender, RoutedEventArgs e)
        {
            EditBookmarkPopup.IsOpen = false;
            EditBookmarkPopup_TitleTextBox.Text = "";
            EditBookmarkPopup_URLTextBox.Text = "";
        }
        private void EditBookmarkPopup_LibraryButton_Click(object sender, RoutedEventArgs e) => new BookmarkManager().Show();
        private void EditBookmarkPopup_SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (BookmarkExists(EditBookmarkPopup_URLTextBox.Text, EditBookmarkPopup_TitleTextBox.Text) is XElement element)
            {
                element.Descendants("Title").First().Value = EditBookmarkPopup_TitleTextBox.Text;
                element.Descendants("URL").First().Value = EditBookmarkPopup_URLTextBox.Text;
            }
            BookmarkXml.Save($"{AppDomain.CurrentDomain.BaseDirectory}\\Bookmarks.xml");
        }

        private void BookmarkBar_CheckBox_Checked(object sender, RoutedEventArgs e) => BookmarkBar_Row.Height = new GridLength(32);
        private void BookmarkBar_CheckBox_Unchecked(object sender, RoutedEventArgs e) => BookmarkBar_Row.Height = new GridLength(0);

        public BitmapImage LoadFavicon(string URL)
        {
            if (URL != "")
            {
                return new BitmapImage(new Uri($"http://www.google.com/s2/favicons?domain={URL}"));
            }
            return (BitmapImage)FindResource("\\LoadingWebPage.png");
        }
    }
}
