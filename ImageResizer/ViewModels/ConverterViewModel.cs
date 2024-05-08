using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageMagick;
using ImageResizer.Contracts.Services;
using ImageResizer.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Input;
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.DragDropEffects;
using FileInfo = ImageResizer.Models.FileInfo;

namespace ImageResizer.ViewModels;

public class ConverterViewModel : ObservableObject
{
    IImageResizeServices _resizeServices;
    private readonly string destPath = string.Empty;
    private List<string> Extantions = new() { ".jpeg", ".jpg", ".png", ".bmp", ".heic" };
    private string[] pathFiles;
    public ConverterViewModel(IImageResizeServices resizeServices)
    {
        _resizeServices = resizeServices;
        fileInfoList = new();
        fileFormatItems = new ObservableCollection<FileFormat>()
        {
            
            new FileFormat(){Name="JPG",Format=ImageFormat.Jpeg},
            new FileFormat(){Name="PNG", Format=ImageFormat.Png},
            new FileFormat(){Name="BMP", Format=ImageFormat.Bmp}
        };

        destPath = Properties.Settings.Default.PathSaveFile;
        _resizeServices.CheckFolderForSave(destPath);
        if (App.PathForResize != null&& Path.GetExtension(App.PathForResize.First()).ToLower() != ".pdf")
        {
            if (Path.GetExtension(App.PathForResize.First()).ToLower() == ".pdf")
            {
                App.PathForResize = null;


            }
            else
            {
                foreach (var item in App.PathForResize)
                {
                    fileInfoList.Add(GetFileInfo(item));
                    if (fileInfoList.Count > 0)
                        buttonDeleteVisibility = Visibility.Visible;                  
                }
                OnPropertyChanged(nameof(FileInfoList));
            }
        }
        #region Команды
        DeleteItemCommand = new RelayCommand<object>(DeleteItem);

        #endregion
    }
    private FileInfo GetFileInfo(string path)
    {
        FileInfo fileInfo = new();

        var format = Path.GetExtension(path);
        if (format.ToLower() == ".heic")
        {
            using MagickImage magickImage = new(path);
            fileInfo.FullName = path;
            fileInfo.ShortName = Path.GetFileName(path);
            fileInfo.Width = magickImage.Width;
            fileInfo.Height = magickImage.Height;
        }
        else
        {
            var image = System.Drawing.Image.FromFile(path);
            fileInfo.FullName = path;
            fileInfo.ShortName = Path.GetFileName(path);
            fileInfo.Width = image.Width;
            fileInfo.Height = image.Height;
        }

        return fileInfo;

    }
    private ObservableCollection<FileFormat> fileFormatItems;

    public ObservableCollection<FileFormat> FileFormatItems { get => fileFormatItems; set => SetProperty(ref fileFormatItems, value); }

    private int selectedFileFormatIndex;

    public int SelectedFileFormatIndex { get => selectedFileFormatIndex; set => SetProperty(ref selectedFileFormatIndex, value); }

    private RelayCommand openFileCommand;
    public ICommand OpenFileCommand => openFileCommand ??= new RelayCommand(OpenFile);

    private void OpenFile()
    {
        fileInfoList.Clear();
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Multiselect = true;
        openFileDialog.Filter = "Файлы изображений (*.jpeg, *.jpg, *.png, *.bmp, *.heic)|*.jpeg;*.jpg;*.png;*.bmp;*.heic";
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

    private RelayCommand cancelCommand;
    public ICommand CancelCommand => cancelCommand ??= new RelayCommand(Cancel);

    private void Cancel()
    {
        App.PathForResize=null;

        pathFiles = null;
        fileInfoList.Clear();
        buttonDeleteEnable = false;
        buttonDeleteVisibility = Visibility.Hidden;
        PerformValueChangedResolutionCombobox();
        OnPropertyChanged(nameof(ButtonDeleteEnable));
        OnPropertyChanged(nameof(ButtonDeleteVisibility));
    }

    private RelayCommand startConvertCommand;
    public ICommand StartConvertCommand => startConvertCommand ??= new RelayCommand(StartConvert);

    private void StartConvert()
    {
        if (fileInfoList != null && fileInfoList.Count > 0 && selectedOpenFileItem != null)
        {
            try
            {
                var formatSelected = fileFormatItems[selectedFileFormatIndex];
                _resizeServices.ConvertImage(((FileInfo)selectedOpenFileItem).FullName, destPath, formatSelected.Format);
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

    private ObservableCollection<FileInfo> fileInfoList;

    public ObservableCollection<FileInfo> FileInfoList { get => fileInfoList; set => SetProperty(ref fileInfoList, value); }

    private object selectedOpenFileItem;

    public object SelectedOpenFileItem { get => selectedOpenFileItem; set => SetProperty(ref selectedOpenFileItem, value); }

    private RelayCommand selectListboxOpenFiles;
    public ICommand SelectListboxOpenFiles => selectListboxOpenFiles ??= new RelayCommand(PerformSelectListboxOpenFiles);

    private void PerformSelectListboxOpenFiles()
    {
        buttonDeleteEnable = true;
        OnPropertyChanged(nameof(ButtonDeleteEnable));
    }
    public ICommand DeleteItemCommand { get; set; }

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

    private System.Windows.Visibility buttonDeleteVisibility = Visibility.Hidden;

    public System.Windows.Visibility ButtonDeleteVisibility { get => buttonDeleteVisibility; set => SetProperty(ref buttonDeleteVisibility, value); }

    private bool buttonDeleteEnable = false;

    public bool ButtonDeleteEnable { get => buttonDeleteEnable; set => SetProperty(ref buttonDeleteEnable, value); }

    private RelayCommand valueChangedResolutionCombobox;
    public ICommand ValueChangedResolutionCombobox => valueChangedResolutionCombobox ??= new RelayCommand(PerformValueChangedResolutionCombobox);

    private void PerformValueChangedResolutionCombobox()
    {

    }

    private double opacityMainGrid =1;

    public double OpacityMainGrid { get => opacityMainGrid; set => SetProperty(ref opacityMainGrid, value); }

    private Visibility dragDropGridVisibility = Visibility.Hidden;

    public Visibility DragDropGridVisibility { get => dragDropGridVisibility; set => SetProperty(ref dragDropGridVisibility, value); }

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

                foreach (var extantion in Extantions)
                {

                    if (Path.GetExtension(item).ToLower() == extantion)
                    {
                        fileInfoList.Add(GetFileInfo(item));

                        if (fileInfoList.Count > 0)
                            buttonDeleteVisibility = Visibility.Visible;
                    }
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

}
