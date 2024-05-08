using System.Windows.Controls;

using ImageResizer.ViewModels;

namespace ImageResizer.Views;

public partial class ResizePage : Page
{
    public ResizePage(ResizeViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
