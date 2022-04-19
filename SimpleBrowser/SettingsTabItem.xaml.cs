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

namespace SimpleBrowser
{
    /// <summary>
    /// Interaction logic for SettingsTabItem.xaml
    /// </summary>
    public partial class SettingsTabItem : UserControl
    {
        public SettingsTabItem() => InitializeComponent();

        private void ThemeChanged(object sender, SelectionChangedEventArgs e)
        {
            Application.Current.Resources.MergedDictionaries[0].Source = new Uri($"\\Themes\\{(ThemeSelection.SelectedItem as ComboBoxItem)!.Content}.xaml", UriKind.Relative);
        }
    }
}
