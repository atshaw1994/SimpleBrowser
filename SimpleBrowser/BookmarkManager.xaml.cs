using System;
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
using System.Windows.Shapes;
using System.Xml;

namespace SimpleBrowser
{
    /// <summary>
    /// Interaction logic for BookmarkManager.xaml
    /// </summary>
    public partial class BookmarkManager : Window
    {
		private readonly XmlDocument BookmarkXML;
		private readonly XmlDocument HistoryXML;

		public BookmarkManager()
        {
            InitializeComponent();
			BookmarkXML = new();
			HistoryXML = new();
			BookmarkXML.Load($"{AppDomain.CurrentDomain.BaseDirectory}\\Bookmarks.xml");
			HistoryXML.Load($"{AppDomain.CurrentDomain.BaseDirectory}\\History.xml");
			LoadBookmarks();
			LoadHistory();
		}
		public BookmarkManager(int TabIndex)
		{
			InitializeComponent();
			baseTabControl.SelectedIndex = TabIndex;
			BookmarkXML = new();
			HistoryXML = new();
			BookmarkXML.Load($"{AppDomain.CurrentDomain.BaseDirectory}\\Bookmarks.xml");
			HistoryXML.Load($"{AppDomain.CurrentDomain.BaseDirectory}\\History.xml");
			LoadBookmarks();
			LoadHistory();
			
		}

		#region BorderlessMethods

		private void CaptionBar_CloseButton_Click(object sender, RoutedEventArgs e) => Close();
        private void CaptionBar_MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Window_SourceInitialized(object sender, EventArgs e) => ((HwndSource)PresentationSource.FromVisual(this)).AddHook(HookProc);
		private void Window_Deactivated(object sender, EventArgs e)
		{
			CaptionBar_Border.Background = (SolidColorBrush)FindResource("Caption.Inactive");
		}
		private void Window_Activated(object sender, EventArgs e)
		{
			CaptionBar_Border.Background = (SolidColorBrush)FindResource("Caption.Active");
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
			foreach (XmlNode node in BookmarkXML.DocumentElement!.ChildNodes)
			{
				ListBoxItem newBookmarkItem = new() { Content = $"{node.Attributes![0].InnerText}, {node.Attributes![1].InnerText}" };
				newBookmarkItem.MouseDoubleClick += BookmarkItem_MouseDoubleClick;
				BookmarkDisplay.Items.Add(newBookmarkItem);
			}
		}
		private void LoadHistory()
		{
			foreach (XmlNode node in HistoryXML.DocumentElement!.ChildNodes)
			{
				ListBoxItem newHistoryItem = new() { Content = $"{node.Attributes![0].InnerText}, {node.Attributes![1].InnerText},  {node.Attributes![2].InnerText}" };
				newHistoryItem.MouseDoubleClick -= HistoryItem_MouseDoubleClick;
				HistoryDisplay.Items.Add(newHistoryItem);
			}
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
			if (Application.Current.MainWindow is MainWindow mainWindow)
			{
				string title = mainWindow.selectedBrowser!.Title;
				string URL = mainWindow.selectedBrowser.Address;
				BookmarkDisplay.Items.Add(new ListBoxItem() { Content = $"{title}, {URL}" });
				BookmarkDisplay.SelectedIndex = BookmarkDisplay.Items.Count - 1;
				XmlElement newElem = BookmarkXML.CreateElement("Bookmark");
				newElem.SetAttribute("Title", title);
				newElem.SetAttribute("URL", URL);
				BookmarkXML.DocumentElement!.AppendChild(newElem);
				BookmarkXML.Save($"{AppDomain.CurrentDomain.BaseDirectory}\\Bookmarks.xml");
			}
		}
		private void DeleteBookmarkButton_Click(object sender, RoutedEventArgs e)
		{
			if (BookmarkDisplay.SelectedItem is ListBoxItem selectedItem && selectedItem.Content is string content)
            {
				XmlNode node = BookmarkXML.SelectSingleNode($"/Bookmarks/[@name='{content[..content.ToString()!.IndexOf(',')]}']")!;
				if (node is not null)
				{
					XmlNode parent = node.ParentNode!;
					parent.RemoveChild(node);
                    BookmarkXML.Save($"{AppDomain.CurrentDomain.BaseDirectory}\\Bookmarks.xml");
				}
			}
		}
        private void HistoryItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

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
				XmlNode node = HistoryXML.SelectSingleNode($"/History/[@name='{content[..content.ToString()!.IndexOf(',')]}']")!;
				if (node is not null)
				{
					XmlNode parent = node.ParentNode!;
					parent.RemoveChild(node);
					HistoryXML.Save($"{AppDomain.CurrentDomain.BaseDirectory}\\History.xml");
				}
			}
		}
        private void ClearHistoryButton_Click(object sender, RoutedEventArgs e) => HistoryXML.DocumentElement!.RemoveAll();

        public void SelectTab(int tabIndex)
        {
			if (tabIndex == 0) baseTabControl.SelectedIndex = tabIndex;
			else if (tabIndex == 1) baseTabControl.SelectedIndex = tabIndex - 1;
			else MessageBox.Show("WTF kinda index is that?!"); // Unreachable code
        }
		        
    }
}
