using System.Windows.Controls;

using ImageResizer.ViewModels;

namespace ImageResizer.Views;

public partial class PdfConverterPage : Page
{
    public PdfConverterPage(PdfConverterViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
