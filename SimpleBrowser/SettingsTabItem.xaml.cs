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
using System.Xml.Linq;

namespace SimpleBrowser
{
    /// <summary>
    /// Interaction logic for SettingsTabItem.xaml
    /// </summary>
    public partial class SettingsTabItem : UserControl
    {
        private readonly MainWindow _mainWindow;
        private readonly XDocument SettingsXml;
        private readonly List<XNode> nodes;

        public SettingsTabItem()
        {
            _mainWindow = (MainWindow)Application.Current.MainWindow;
            InitializeComponent();
            SettingsXml = XDocument.Load($"{AppDomain.CurrentDomain.BaseDirectory}\\Settings.xml");
            nodes = SettingsXml.Root!.Nodes().ToList();
        }
        public SettingsTabItem(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            InitializeComponent();
            SettingsXml = XDocument.Load($"{AppDomain.CurrentDomain.BaseDirectory}\\Settings.xml");
            nodes = SettingsXml.Root!.Nodes().ToList();
        }
        private void SettingsTabItem_Loaded(object sender, RoutedEventArgs e)
        {
            StartupSelection.SelectedIndex = Convert.ToInt32(((nodes[0] as XElement)!.Nodes().First() as XText)!.Value);
            ShowBookmarkBar.IsChecked = Convert.ToBoolean(((nodes[1] as XElement)!.Nodes().First() as XText)!.Value);
            HomeURLTextBox.Text = ((nodes[2] as XElement)!.Nodes().First() as XText)!.Value;
            string theme = ((nodes[3] as XElement)!.Nodes().First() as XText)!.Value;
            ThemeSelection.SelectedIndex = theme.Equals("Light", StringComparison.Ordinal) ? 0 : 1;
        }

        private void ThemeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeSelection.SelectedItem is ComboBoxItem item && nodes is not null)
            {
                Application.Current.Resources.MergedDictionaries[0].Source = new Uri($"\\Themes\\{item.Content}.xaml", UriKind.Relative);
                ((nodes[3] as XElement)!.Nodes().First() as XText)!.Value = item.Content.ToString()!;
                SettingsXml.Save($"{AppDomain.CurrentDomain.BaseDirectory}\\Settings.xml");
            }
        }

        private void ShowBookmarkBar_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if(ShowBookmarkBar.IsChecked == true) _mainWindow.BookmarkBar_Row.Height = new GridLength(24);
            else _mainWindow.BookmarkBar_Row.Height = new GridLength(0);
            ((nodes[1] as XElement)!.Nodes().First() as XText)!.Value = ShowBookmarkBar.IsChecked.ToString()!;
            SettingsXml.Save($"{AppDomain.CurrentDomain.BaseDirectory}\\Settings.xml");
        }

        private void StartupSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((nodes[0] as XElement)!.Nodes().First() as XText)!.Value = StartupSelection.SelectedIndex.ToString();
            SettingsXml.Save($"{AppDomain.CurrentDomain.BaseDirectory}\\Settings.xml");
        }

        private void HomeURLTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((nodes[2] as XElement)!.Nodes().First() as XText)!.Value = HomeURLTextBox.Text;
            SettingsXml.Save($"{AppDomain.CurrentDomain.BaseDirectory}\\Settings.xml");
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
        }

    }
}
