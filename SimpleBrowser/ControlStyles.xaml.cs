using System.Windows;
using System.Windows.Controls;

namespace SimpleBrowser
{
    public partial class ControlStyles
    {
        private void CloseTabButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is Button senderButton && senderButton.Tag is MainWindow window)
                window.CloseTabButton();
        }

        private void newTabButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is Button senderButton && senderButton.Tag is MainWindow window)
                window.NewTabButton_Click(this, e);
        }

        private void BookmarkBar_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow!.BookmarkBar_Row.Height = new GridLength(32);
        }

        private void BookmarkBar_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow!.BookmarkBar_Row.Height = new GridLength(0);
        }
    }
}
