using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageResizer.Contracts.Services;
using ImageResizer.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using UglyToad.PdfPig;
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.DragDropEffects;
using FileInfo = ImageResizer.Models.FileInfo;
using Path = System.IO.Path;

namespace ImageResizer.ViewModels;

public class PdfConverterViewModel : ObservableObject
{
    #region Поля
    IImageResizeServices _resizeServices;
    private readonly string destPath = string.Empty;
    private Visibility loadingResultVisibility = Visibility.Hidden;
    private string[] pathFiles;
    private ObservableCollection<FileInfo> fileInfoList;
    private object selectedOpenFileItem;
    private ObservableCollection<FileFormat> fileFormatItems;
    private RelayCommand openFileCommand;
    private int selectedFileFormatIndex;
    private RelayCommand cancelCommand;
    private RelayCommand selectListboxOpenFiles;
    private System.Windows.Visibility buttonDeleteVisibility = Visibility.Hidden;
    private RelayCommand startConvertCommand;
    private const string extantion = ".pdf";
    private bool buttonDeleteEnable = false;



    #endregion

    #region Свойства
    public Visibility LoadingResultVisibility { get => loadingResultVisibility; set => SetProperty(ref loadingResultVisibility, value); }

    public ObservableCollection<FileInfo> FileInfoList { get => fileInfoList; set => SetProperty(ref fileInfoList, value); }
    public object SelectedOpenFileItem { get => selectedOpenFileItem; set => SetProperty(ref selectedOpenFileItem, value); }
    public ICommand OpenFileCommand => openFileCommand ??= new RelayCommand(OpenFile);
    public ObservableCollection<FileFormat> FileFormatItems { get => fileFormatItems; set => SetProperty(ref fileFormatItems, value); }
    public int SelectedFileFormatIndex { get => selectedFileFormatIndex; set => SetProperty(ref selectedFileFormatIndex, value); }
    public ICommand CancelCommand => cancelCommand ??= new RelayCommand(Cancel);
    public ICommand SelectListboxOpenFiles => selectListboxOpenFiles ??= new RelayCommand(PerformSelectListboxOpenFiles);
    public ICommand DeleteItemCommand { get; set; }
    public System.Windows.Visibility ButtonDeleteVisibility { get => buttonDeleteVisibility; set => SetProperty(ref buttonDeleteVisibility, value); }
    public bool ButtonDeleteEnable { get => buttonDeleteEnable; set => SetProperty(ref buttonDeleteEnable, value); }
    public ICommand StartConvertCommand => startConvertCommand ??= new RelayCommand(StartConvert);

    #endregion

    #region Методы
    private FileInfo GetFileInfo(string path)
    {
        using PdfDocument document = PdfDocument.Open(path);
        FileInfo fileInfo = new();
        fileInfo.FullName = path;
        fileInfo.ShortName = Path.GetFileName(path);
        fileInfo.CountPage = document.GetPages().Count();

        return fileInfo;
    }
    private void OpenFile()
    {
        fileInfoList.Clear();
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Multiselect = true;
        openFileDialog.Filter = "Файлы Adobe PDF (*.pdf)|*.pdf";
        if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            pathFiles = openFileDialog.FileNames;

            foreach (string item in pathFiles)
            {
                fileInfoList.Add(GetFileInfo(item));

                if (fileInfoList.Count > 0)
                    buttonDeleteVisibility = Visibility.Visible;
            }
            OnPropertyChanged(nameof(ButtonDeleteVisibility));
            selectedOpenFileItem = fileInfoList.LastOrDefault();
        }
    }
   private void Cancel()
    {
        App.PathForResize = null;

        pathFiles = null;
        fileInfoList.Clear();
        buttonDeleteEnable = false;
        buttonDeleteVisibility = Visibility.Hidden;

        OnPropertyChanged(nameof(ButtonDeleteEnable));
        OnPropertyChanged(nameof(ButtonDeleteVisibility));
    }
    private void PerformSelectListboxOpenFiles()
    {
        buttonDeleteEnable = true;
        OnPropertyChanged(nameof(ButtonDeleteEnable));
    }
    private void DeleteItem(object obj)
    {
        var listBox = (System.Windows.Controls.ListBox)obj;
        if (listBox.Items.Count > 0 && listBox.SelectedIndex >= 0)
            fileInfoList.RemoveAt(listBox.SelectedIndex);

        if (listBox.Items.Count <= 0 && listBox.SelectedIndex < 0)
        {
            buttonDeleteEnable = false;
            buttonDeleteVisibility = Visibility.Hidden;
        }
        OnPropertyChanged(nameof(fileInfoList));
        OnPropertyChanged(nameof(ButtonDeleteEnable));
        OnPropertyChanged(nameof(ButtonDeleteVisibility));
    }

    private async void StartConvert()
    {
        if (fileInfoList != null && fileInfoList.Count > 0 && selectedOpenFileItem != null)
        {
            try
            {
                var formatSelected = fileFormatItems[selectedFileFormatIndex];
                await Task.Run(async () => await PdfToJpeg(((FileInfo)selectedOpenFileItem).FullName, destPath, formatSelected.Format));
                Process.Start("explorer", destPath);
            }
            catch (Exception ex)
            {

                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }
        else
        {
            System.Windows.Forms.MessageBox.Show("Необходимо выбрать файл.");
        }
    }

    private async Task PdfToJpeg(string filePath, string destPath, ImageFormat imageFormat)
    {
        string fileName = Path.GetFileNameWithoutExtension(filePath);
        var dd = System.IO.File.ReadAllBytes(filePath);

        using PdfDocument document = PdfDocument.Open(filePath);
        int countPage = document.GetPages().Count();
        for (int i = 1; i <= countPage; i++)
        {
            loadingResultVisibility = Visibility.Visible;
            byte[] pngByte = Freeware.Pdf2Png.Convert(dd, i);
            using MemoryStream memory = new MemoryStream();
            await memory.WriteAsync(pngByte);
            Image image = Image.FromStream(memory);
            image.Save(Path.Combine(destPath, fileName + DateTime.Now.ToString("dd-MM-yyyy-H-mm-ss") + "." + imageFormat.ToString().ToLower()), imageFormat);
            OnPropertyChanged(nameof(loadingResultVisibility));
        }
        loadingResultVisibility = Visibility.Hidden;
        OnPropertyChanged(nameof(loadingResultVisibility));


    }

    public void UIElement_OnDrop(object sender, System.Windows.DragEventArgs e)
    {

        opacityMainGrid = 1;
        dragDropGridVisibility = Visibility.Hidden;
        OnPropertyChanged(nameof(dragDropGridVisibility));
        OnPropertyChanged(nameof(opacityMainGrid));
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {


            pathFiles = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (string item in pathFiles)
            {

                    if (Path.GetExtension(item).ToLower() == extantion)
                    {
                        fileInfoList.Add(GetFileInfo(item));

                        if (fileInfoList.Count > 0)
                            buttonDeleteVisibility = Visibility.Visible;
                    }
   
            }
            OnPropertyChanged(nameof(ButtonDeleteVisibility));
        }
    }
    public void UIElement_OnDragLeave(object sender, System.Windows.DragEventArgs e)
    {
        opacityMainGrid = 1;
        dragDropGridVisibility = Visibility.Hidden;
        OnPropertyChanged(nameof(dragDropGridVisibility));
        OnPropertyChanged(nameof(opacityMainGrid));
    }

    public void UIElement_OnDragEnter(object sender, System.Windows.DragEventArgs e)
    {

        opacityMainGrid = 0.3;
        dragDropGridVisibility = Visibility.Visible;
        OnPropertyChanged(nameof(dragDropGridVisibility));
        OnPropertyChanged(nameof(opacityMainGrid));

        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.All;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
    }

    #endregion
    public PdfConverterViewModel(IImageResizeServices resizeServices)
    {
        List<string> PathList = App.PathForResize;

        _resizeServices = resizeServices;
        fileInfoList = new();
        destPath = Properties.Settings.Default.PathSaveFile;
        _resizeServices.CheckFolderForSave(destPath);
        fileFormatItems = new ObservableCollection<FileFormat>()
        {

            new FileFormat(){Name="JPG",Format=ImageFormat.Jpeg},
            new FileFormat(){Name="PNG", Format=ImageFormat.Png},
            new FileFormat(){Name="BMP", Format=ImageFormat.Bmp}
        };
        if (PathList!=null&& Path.GetExtension(App.PathForResize.First()).ToLower() == ".pdf")
        {
            foreach (var item in App.PathForResize)
            {
                fileInfoList.Add(GetFileInfo(item));
            }
            
        }
        #region Команды
        DeleteItemCommand = new RelayCommand<object>(DeleteItem);
        #endregion
    }

    private Visibility dragDropGridVisibility=Visibility.Hidden;

    public Visibility DragDropGridVisibility { get => dragDropGridVisibility; set => SetProperty(ref dragDropGridVisibility, value); }

    private double opacityMainGrid=1;

    public double OpacityMainGrid { get => opacityMainGrid; set => SetProperty(ref opacityMainGrid, value); }
}
