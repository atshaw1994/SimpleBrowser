using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;

namespace SimpleBrowser
{
    /// <summary>
    /// Interaction logic for BookmarkManager.xaml
    /// </summary>
    public partial class LibraryWindow : Window
    {
		private readonly XDocument BookmarkXml;
		private readonly XDocument HistoryXml;

		public LibraryWindow()
        {
            InitializeComponent();
			BookmarkXml = XDocument.Load($"{AppDomain.CurrentDomain.BaseDirectory}\\Bookmarks.xml");
			HistoryXml = XDocument.Load($"{AppDomain.CurrentDomain.BaseDirectory}\\History.xml");
		}
		public LibraryWindow(int TabIndex)
		{
			InitializeComponent();
			baseTabControl.SelectedIndex = TabIndex;
			BookmarkXml = XDocument.Load($"{AppDomain.CurrentDomain.BaseDirectory}\\Bookmarks.xml");
			HistoryXml = XDocument.Load($"{AppDomain.CurrentDomain.BaseDirectory}\\History.xml");
		}
		private void LibraryWindow_Loaded(object sender, RoutedEventArgs e)
		{
			LoadBookmarks();
			LoadHistory();
		}

		#region BorderlessMethods

		private void CaptionBar_CloseButton_Click(object sender, RoutedEventArgs e) => Close();
        private void CaptionBar_MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Window_SourceInitialized(object sender, EventArgs e) => ((HwndSource)PresentationSource.FromVisual(this)).AddHook(HookProc);
		private void Window_Deactivated(object sender, EventArgs e) => CaptionBar_Border.Background = (SolidColorBrush)FindResource("CaptionBar.Background.Inactive");
		private void Window_Activated(object sender, EventArgs e) => CaptionBar_Border.Background = (SolidColorBrush)FindResource("CaptionBar.Background.Active");

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
			foreach (XElement element in BookmarkXml.Root!.Elements())
			{
				ListBoxItem newBookmarkItem = new() 
				{ 
					Content = $"{element.Descendants("Title").First().Value}, {element.Descendants("URL").First().Value}" 
				};
				newBookmarkItem.MouseDoubleClick += BookmarkItem_MouseDoubleClick;
				BookmarkDisplay.Items.Add(newBookmarkItem);
			}
		}
		private void LoadHistory()
		{
			foreach (XElement HistoryItem in HistoryXml.Root!.Nodes().ToList())
			{
                ListBoxItem newHistoryItem = new()
				{
					
					Content = $"Title: {HistoryItem.Descendants("Title").First().Value}\n" +
                    $"URL: {HistoryItem.Descendants("URL").First().Value}\n" +
                    $"Last Visited: {DateTime.ParseExact(HistoryItem.Descendants("LastVisited").First().Value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture)}" 
				};
				newHistoryItem.MouseDoubleClick += HistoryItem_MouseDoubleClick;
				HistoryDisplay.Items.Add(newHistoryItem);
			}
		}

		private XElement? BookmarkExists(string URL, string Title)
		{
			if (BookmarkXml.Root is not null)
				foreach (XElement node in BookmarkXml.Root!.Elements())
					if (node.Descendants("Title").First().Value.Equals(Title) && node.Descendants("URL").First().Value.Equals(URL))
						return node;
			return null;
		}

		private void BookmarkDisplay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
			if (BookmarkDisplay.SelectedItem is ListBoxItem selectedItem && selectedItem.Content is string content)
			{
				TitleTextBox.Text = content[..content.ToString()!.IndexOf(',')];
				URLTextBox.Text = content[(content.ToString()!.IndexOf(',') + 2)..];
				DeleteBookmarkButton.IsEnabled = true;
			}
		}

		private void AddButton_Click(object sender, RoutedEventArgs e)
        {
			if (TitleTextBox.Text.Length > 1 && URLTextBox.Text.ToLower().Remove(0, URLTextBox.Text.IndexOf("www")).StartsWith("www.") &&
				URLTextBox.Text.ToLower().EndsWith(".com"))
            {
				BookmarkDisplay.Items.Add(new ListBoxItem() { Content = $"{TitleTextBox.Text}, {URLTextBox.Text}" });
				BookmarkDisplay.SelectedIndex = BookmarkDisplay.Items.Count - 1;
				XElement newBookmark =
					new("Bookmark",
					new XElement("Title", TitleTextBox.Text),
					new XElement("URL", URLTextBox.Text)
				);
				BookmarkXml.Root!.Add(newBookmark);
				BookmarkXml.Save($"{AppDomain.CurrentDomain.BaseDirectory}\\Bookmarks.xml");
			}
			else
            {
				MessageBox.Show("Please enter a valid URL in the URL textbox.");
            }
		}
		private void DeleteBookmarkButton_Click(object sender, RoutedEventArgs e)
		{
			if (BookmarkDisplay.SelectedItem is ListBoxItem selectedItem && selectedItem.Content is string content)
            {
				string Title = content[..(content.IndexOf(",") - 1)];
				string URL = content[(content.IndexOf(",") + 2)..];
				if (BookmarkExists(URL, Title) is XElement item) 
				{ 
					item.Remove();
					BookmarkDisplay.Items.Remove(selectedItem);
					BookmarkXml.Save($"{AppDomain.CurrentDomain.BaseDirectory}\\Bookmarks.xml");
				}
			}
		}
        private void HistoryItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
			if (Application.Current.MainWindow is MainWindow mainWindow &&
				mainWindow.selectedBrowser is not null &&
				sender is ListBoxItem selectedItem &&
				selectedItem.Content is string content)
			{
				mainWindow.selectedBrowser.Load(content[(content.ToString()!.IndexOf("URL: ") + 5)..]);
			}
		}
		private void BookmarkItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (Application.Current.MainWindow is MainWindow mainWindow &&
				mainWindow.selectedBrowser is not null &&
				sender is ListBoxItem selectedItem &&
				selectedItem.Content is string content)
			{
				mainWindow.selectedBrowser.Load(content[(content.ToString()!.IndexOf(',') + 2)..]);
			}
		}
		private void DeleteHistoryItemButton_Click(object sender, RoutedEventArgs e)
		{
			if (HistoryDisplay.SelectedItem is ListBoxItem selectedItem && selectedItem.Content is string content)
			{
				string url = content[(content.ToString()!.IndexOf(',') + 2)..];
				foreach (XElement item in HistoryXml.Elements())
					if (item!.Descendants("URL").First().Value.Equals(url))
						item.Remove();
				HistoryXml.Save($"{AppDomain.CurrentDomain.BaseDirectory}\\History.xml");
			}
		}
        private void ClearHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            HistoryXml.Root!.Remove();
			HistoryXml.Add(new XElement("History"));
			HistoryXml.Save($"{AppDomain.CurrentDomain.BaseDirectory}\\History.xml");
		}

        public void SelectTab(int tabIndex)
        {
			if (tabIndex == 0) baseTabControl.SelectedIndex = tabIndex;
			else if (tabIndex == 1) baseTabControl.SelectedIndex = tabIndex - 1;
			else MessageBox.Show("WTF kinda index is that?!"); // Unreachable code
        }
    }
}
