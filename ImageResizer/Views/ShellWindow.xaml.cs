using System.Windows.Controls;

using ImageResizer.Contracts.Views;
using ImageResizer.ViewModels;

using MahApps.Metro.Controls;

namespace ImageResizer.Views;

public partial class ShellWindow : MetroWindow, IShellWindow
{
    public ShellWindow(ShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public Frame GetNavigationFrame()
        => shellFrame;

    public void ShowWindow()
        => Show();

    public void CloseWindow()
        => Close();
}
