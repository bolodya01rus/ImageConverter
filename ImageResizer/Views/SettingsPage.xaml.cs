using System.Windows.Controls;

using ImageResizer.ViewModels;

namespace ImageResizer.Views;

public partial class SettingsPage : Page
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
