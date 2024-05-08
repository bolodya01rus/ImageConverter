using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageMagick;
using ImageResizer.Contracts.Services;
using ImageResizer.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using UglyToad.PdfPig;
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.DragDropEffects;
using FileInfo = ImageResizer.Models.FileInfo;

namespace ImageResizer.ViewModels;

public class Convert2PdfViewModel : ObservableObject

{
    private List<string> Extantions = new() { ".jpeg", ".jpg", ".png", ".bmp", ".heic" };
    private readonly string destPath = string.Empty;
    private string[] pathFiles;
    IImageResizeServices _resizeServices;

    private System.Windows.Visibility buttonDeleteVisibility = System.Windows.Visibility.Hidden;

    public System.Windows.Visibility ButtonDeleteVisibility { get => buttonDeleteVisibility; set => SetProperty(ref buttonDeleteVisibility, value); }

    private bool buttonDeleteEnable = false;

    public bool ButtonDeleteEnable { get => buttonDeleteEnable; set => SetProperty(ref buttonDeleteEnable, value); }

    public Convert2PdfViewModel(IImageResizeServices resizeServices)
    {
        List<string> PathList = App.PathForResize;

        _resizeServices = resizeServices;
        fileInfoList = new();
        destPath = Properties.Settings.Default.PathSaveFile;
        _resizeServices.CheckFolderForSave(destPath);
        if (App.PathForResize != null)
        {
            foreach (var item in App.PathForResize)
            {
                fileInfoList.Add(GetFileInfo(item));
                if (fileInfoList.Count > 0)
                {
                    buttonDeleteVisibility = Visibility.Visible;
                    buttonMoveUpEnable = true;
                    buttonMoveDownEnable = true;
                }

            }
            OnPropertyChanged(nameof(FileInfoList));
            OnPropertyChanged(nameof(buttonDeleteEnable));
            OnPropertyChanged(nameof(buttonDeleteVisibility));
            OnPropertyChanged(nameof(buttonMoveUpEnable));
            OnPropertyChanged(nameof(buttonMoveDownEnable));
        }


        #region Команды
        DeleteItemCommand = new RelayCommand<object>(DeleteItem);
        MoveDownCommand = new RelayCommand(MoveDown);
        MoveUpCommand = new RelayCommand(MoveUp);
        #endregion     
    }

    private ObservableCollection<FileInfo> fileInfoList;

    public ObservableCollection<FileInfo> FileInfoList { get => fileInfoList; set => SetProperty(ref fileInfoList, value); }

    private RelayCommand openFileCommand;
    public ICommand OpenFileCommand => openFileCommand ??= new RelayCommand(OpenFile);

    private void OpenFile()
    {
        fileInfoList.Clear();
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Multiselect = true;
        openFileDialog.Filter = "Файлы изображений (*.jpeg, *.jpg, *.png, *.bmp, *.heic,*.pdf)|*.jpeg;*.jpg;*.png;*.bmp;*.heic;*.pdf;";
        if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            pathFiles = openFileDialog.FileNames;

            foreach (string item in pathFiles)
            {
                fileInfoList.Add(GetFileInfo(item));

                if (fileInfoList.Count > 0)
                {
                    buttonDeleteVisibility = Visibility.Visible;
                    buttonMoveUpEnable = true;
                    buttonMoveDownEnable = true;
                }
            }

            OnPropertyChanged(nameof(buttonDeleteEnable));
            OnPropertyChanged(nameof(buttonDeleteVisibility));
            OnPropertyChanged(nameof(buttonMoveUpEnable));
            OnPropertyChanged(nameof(buttonMoveDownEnable));
            selectedOpenFileItem = fileInfoList.LastOrDefault();
        }
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
            fileInfo.CountPage = 1;
        }
        else if (format.ToLower() == ".pdf")
        {
            fileInfo = GetFilePDFInfo(path);
        }
        else
        {
            var image = System.Drawing.Image.FromFile(path);
            fileInfo.FullName = path;
            fileInfo.ShortName = Path.GetFileName(path);
            fileInfo.Width = image.Width;
            fileInfo.Height = image.Height;
            fileInfo.CountPage = 1;
        }

        return fileInfo;

    }
    private FileInfo GetFilePDFInfo(string path)
    {
        using PdfDocument document = PdfDocument.Open(path);
        FileInfo fileInfo = new();
        fileInfo.FullName = path;
        fileInfo.ShortName = Path.GetFileName(path);
        fileInfo.CountPage = document.GetPages().Count();
        return fileInfo;
    }
    private RelayCommand startConvertCommand;
    public ICommand StartConvertCommand => startConvertCommand ??= new RelayCommand(StartConvert);

    private void StartConvert()
    {

        if (fileInfoList != null && fileInfoList.Count > 0)
        {
            try
            {
                var filesPath = fileInfoList.Select(e => e.FullName).ToArray();


                _resizeServices.ImageToPdf(filesPath, destPath);
                Process.Start("explorer", destPath);
            }
            catch (Exception ex)
            {

                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }
        else
        {
            System.Windows.Forms.MessageBox.Show("Необходимо добавить файлы.");
        }
    }

    private RelayCommand cancelCommand;
    public ICommand CancelCommand => cancelCommand ??= new RelayCommand(Cancel);

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

    private object selectedOpenFileItem;

    public object SelectedOpenFileItem { get => selectedOpenFileItem; set => SetProperty(ref selectedOpenFileItem, value); }

    private RelayCommand selectListboxOpenFiles;
    public ICommand SelectListboxOpenFiles => selectListboxOpenFiles ??= new RelayCommand(PerformSelectListboxOpenFiles);

    private void PerformSelectListboxOpenFiles()
    {
        buttonDeleteEnable = true;
        buttonDeleteVisibility = Visibility.Visible;
        OnPropertyChanged(nameof(ButtonDeleteEnable));
        OnPropertyChanged(nameof(ButtonDeleteVisibility));
    }

    public ICommand DeleteItemCommand { get; set; }


    private void DeleteItem(object obj)
    {
        var listBox = (System.Windows.Controls.ListBox)obj;
        if (listBox.Items.Count > 0 && listBox.SelectedIndex >= 0)
            fileInfoList.RemoveAt(listBox.SelectedIndex);

        if (listBox.Items.Count <= 0)
        {
            buttonDeleteVisibility = Visibility.Hidden;

        }
        if (listBox.SelectedIndex < 0)
        {

            buttonDeleteEnable = false;
        }
        OnPropertyChanged(nameof(fileInfoList));
        OnPropertyChanged(nameof(ButtonDeleteEnable));
        OnPropertyChanged(nameof(ButtonDeleteVisibility));
    }
    public ICommand MoveDownCommand { get; set; }

    private void MoveUp()
    {
        if (selectedIndex > 0)
        {
            fileInfoList.Move(selectedIndex, selectedIndex - 1);

        }
        OnPropertyChanged(nameof(fileInfoList));
    }

    private void MoveDown()
    {
        if (selectedIndex < fileInfoList.Count - 1 && selectedIndex >= 0)
        {
            fileInfoList.Move(selectedIndex, selectedIndex + 1);

        }
        OnPropertyChanged(nameof(fileInfoList));

    }

    public ICommand MoveUpCommand { get; set; }


    private int selectedIndex;

    public int SelectedIndex { get => selectedIndex; set => SetProperty(ref selectedIndex, value); }

    private bool buttonMoveUpEnable = false;

    public bool ButtonMoveUpEnable { get => buttonMoveUpEnable; set => SetProperty(ref buttonMoveUpEnable, value); }

    private bool buttonMoveDownEnable = false;

    public bool ButtonMoveDownEnable { get => buttonMoveDownEnable; set => SetProperty(ref buttonMoveDownEnable, value); }

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
                        {
                            buttonDeleteVisibility = Visibility.Visible;
                            buttonMoveDownEnable = true;
                            buttonMoveUpEnable = true;
                        }
                    }
                }

            }
            OnPropertyChanged(nameof(ButtonDeleteVisibility));
            OnPropertyChanged(nameof(buttonMoveUpEnable));
            OnPropertyChanged(nameof(buttonMoveDownEnable));
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
