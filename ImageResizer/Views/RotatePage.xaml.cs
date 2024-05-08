using System.Windows.Controls;

using ImageResizer.ViewModels;

namespace ImageResizer.Views;

public partial class RotatePage : Page
{
    public RotatePage(RotateViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

   
}
