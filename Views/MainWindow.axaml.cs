using Avalonia;
using Avalonia.Controls;
using SimpleBrowser.ViewModels;
using System;

namespace SimpleBrowser.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public Action? SelectedTabChanged;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = null!;
    }

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        if (!Design.IsDesignMode && !OperatingSystem.IsWindows())
            Background = Avalonia.Media.Brushes.DarkGray;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (Design.IsDesignMode)
            return;
    }
}
