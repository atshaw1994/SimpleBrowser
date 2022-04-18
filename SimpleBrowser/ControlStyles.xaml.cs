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
    }
}
