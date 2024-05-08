using System.Windows.Controls;

using ImageResizer.ViewModels;

namespace ImageResizer.Views;

public partial class Convert2PdfPage : Page
{
    public Convert2PdfPage(Convert2PdfViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
