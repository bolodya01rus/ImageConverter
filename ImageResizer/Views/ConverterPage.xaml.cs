using System.Windows.Controls;

using ImageResizer.ViewModels;

namespace ImageResizer.Views;

public partial class ConverterPage : Page
{
    public ConverterPage(ConverterViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
