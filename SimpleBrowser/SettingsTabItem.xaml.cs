using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace SimpleBrowser
{
    /// <summary>
    /// Interaction logic for SettingsTabItem.xaml
    /// </summary>
    public partial class SettingsTabItem : UserControl
    {
        private readonly MainWindow _mainWindow;

        public SettingsTabItem()
        {
            _mainWindow = (MainWindow)Application.Current.MainWindow;
            InitializeComponent();
        }
        public SettingsTabItem(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            InitializeComponent();
        }
        private void SettingsTabItem_Loaded(object sender, RoutedEventArgs e) => LoadSettings();

        private void LoadSettings()
        {
            StartupSelection.SelectedIndex = Properties.Settings.Default.StartupMode;
            HomeURLTextBox.Text = Properties.Settings.Default.HomeURL;
            ThemeSelection.SelectedIndex = Properties.Settings.Default.Theme;
            ShowBookmarkBar.IsChecked = Properties.Settings.Default.ShowBookmarkBar;
        }

        private void ThemeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeSelection is not null && ThemeSelection.SelectedItem is ComboBoxItem item && item.Content is not null)
            {
                Application.Current.Resources.MergedDictionaries[0].Source = new Uri($"\\Themes\\{item.Content}.xaml", UriKind.Relative);
                Properties.Settings.Default.Theme = ThemeSelection.SelectedIndex;
            }
            Properties.Settings.Default.Save();
        }

        private void ShowBookmarkBar_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if(ShowBookmarkBar.IsChecked == true) _mainWindow.BookmarkBar_Row.Height = new GridLength(24);
            else _mainWindow.BookmarkBar_Row.Height = new GridLength(0);
            Properties.Settings.Default.ShowBookmarkBar = ShowBookmarkBar.IsChecked == true;
            Properties.Settings.Default.Save();
        }

        private void StartupSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.StartupMode = StartupSelection.SelectedIndex;
            Properties.Settings.Default.Save();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
        }
    }
}
