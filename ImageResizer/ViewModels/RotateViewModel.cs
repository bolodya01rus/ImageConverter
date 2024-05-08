using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageResizer.Contracts.Services;
using ImageResizer.Models;
using ImageResizer.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using static System.Windows.Forms.DataFormats;
using Path = System.IO.Path;
using FileInfo = ImageResizer.Models.FileInfo;
using ImageMagick;
using System.Windows;
using System.Windows.Navigation;
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.DragDropEffects;


namespace ImageResizer.ViewModels;

public class RotateViewModel : ObservableObject
{
    IImageResizeServices _imageResizeServices;
    private readonly string destPath = string.Empty;
    private string[] pathFiles;
    private List<string> Extantions = new() { ".jpeg", ".jpg", ".png", ".bmp", ".heic" };

    public RotateViewModel(IImageResizeServices imageResizeServices)
    {
        _imageResizeServices = imageResizeServices;
        fileInfoList = new();
        destPath = Properties.Settings.Default.PathSaveFile;
        _imageResizeServices.CheckFolderForSave(destPath);
        if (App.PathForResize != null)
        {
            if (Path.GetExtension(App.PathForResize.First()).ToLower() == ".pdf")
            {
                App.PathForResize = null;


            }
            else {
                foreach (var item in App.PathForResize)
                {
                    fileInfoList.Add(GetFileInfo(item));
                    if (fileInfoList.Count > 0)
                        buttonDeleteVisibility = Visibility.Visible;


                }
            OnPropertyChanged(nameof(FileInfoList));
        }
        }

        DeleteItemCommand = new RelayCommand<object>(DeleteItem);
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

    private RelayCommand openFileCommand;
    public ICommand OpenFileCommand => openFileCommand ??= new RelayCommand(OpenFile);

    private void OpenFile()
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Multiselect = true;
        openFileDialog.Filter = "Файлы изображений (*.jpeg, *.jpg, *.png, *.bmp, *heic)|*.jpeg;*.jpg;*.png;*.bmp;*.heic";
        if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            pathFiles = openFileDialog.FileNames;

            foreach (var path in pathFiles)
            {
                fileInfoList.Add(GetFileInfo(path));
                if(Path.GetExtension(path).ToLower()==".heic")
                {
                    MagickImage image = new MagickImage(path);
                    
                    using (var memStream = new MemoryStream())
                    {                       
                        image.Write(memStream, MagickFormat.Bmp);
                        ImageOutput = new Bitmap(memStream);
                        ImageOutput.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        ImageOutput.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    }
                }
                else
                { 
                ImageOutput = new Bitmap(System.Drawing.Image.FromFile(path));
                }
                if (fileInfoList.Count > 0)
                    buttonDeleteVisibility = Visibility.Visible;
                OnPropertyChanged(nameof(ImageOutput));
                OnPropertyChanged(nameof(ButtonDeleteEnable));
                OnPropertyChanged(nameof(ButtonDeleteVisibility));

            }
            selectedItem = fileInfoList.Last();
        }
    }

    private RelayCommand rotateCommand;
    public ICommand RotateCommand => rotateCommand ??= new RelayCommand(Rotate);

    private void Rotate()
    {
        if (imageOutput != null && pathFiles != null&& selectedItem!=null)
        {
            _imageResizeServices.SaveImage(((FileInfo)selectedItem).FullName, destPath, ImageOutput);
            Process.Start("explorer", destPath);
        }
        else
        {
            System.Windows.Forms.MessageBox.Show("Необходимо выбрать файл.");
        }
    }

    private RelayCommand cancelCommand;
    public ICommand CancelCommand => cancelCommand ??= new RelayCommand(Cancel);

    private void Cancel()
    {
        App.PathForResize = null;
        pathFiles = null;
        ImageOutput = null;
        fileInfoList.Clear();
        buttonDeleteVisibility = Visibility.Hidden;
        buttonDeleteEnable = true;
        OnPropertyChanged(nameof(ImageOutput));
        OnPropertyChanged(nameof(ButtonDeleteEnable));
        OnPropertyChanged(nameof(ButtonDeleteVisibility));
    }

    private Bitmap imageOutput;

    public Bitmap ImageOutput { get => imageOutput; set { imageOutput = value; OnPropertyChanged(nameof(ImageOutput)); } }

    private RelayCommand rotateLeftCommand;
    public ICommand RotateLeftCommand => rotateLeftCommand ??= new RelayCommand(RotateLeft);

    private void RotateLeft()
    {
        if (imageOutput != null)
            ImageOutput.RotateFlip(RotateFlipType.Rotate270FlipNone);
        OnPropertyChanged(nameof(ImageOutput));

    }

    private RelayCommand rotateRightCommand;
    public ICommand RotateRightCommand => rotateRightCommand ??= new RelayCommand(RotateRight);

    private void RotateRight()
    {
        if (imageOutput != null)
            ImageOutput.RotateFlip(RotateFlipType.Rotate90FlipNone);
        OnPropertyChanged(nameof(ImageOutput));
    }

    private ObservableCollection<FileInfo> fileInfoList;

    public ObservableCollection<FileInfo> FileInfoList { get => fileInfoList; set => SetProperty(ref fileInfoList, value); }

    private object selectedItem;

    public object SelectedItem { get => selectedItem; set => SetProperty(ref selectedItem, value); }

    private RelayCommand selectionChangedCommand;
    public ICommand SelectionChangedCommand => selectionChangedCommand ??= new RelayCommand(SelectionChanged);

    private void SelectionChanged()
    {
        buttonDeleteEnable = true;

        if (selectedItem != null)
        {
            var path = (selectedItem as FileInfo).FullName;

            if (path != null)
            {
                if (Path.GetExtension(path).ToLower() == ".heic")
                {
                    MagickImage image = new MagickImage(path);

                    using (var memStream = new MemoryStream())
                    {
                        image.Write(memStream, MagickFormat.Bmp);
                        ImageOutput = new Bitmap(memStream);
                        ImageOutput.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        ImageOutput.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    }
                }
                else
                ImageOutput = new Bitmap(System.Drawing.Image.FromFile(path));
                buttonDeleteEnable = true;

            }
        }
        else
        {
            ImageOutput=null;
              
        }
        OnPropertyChanged(nameof(ImageOutput));
        OnPropertyChanged(nameof(ButtonDeleteEnable));
        OnPropertyChanged(nameof(ButtonDeleteVisibility));
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
        selectedItem = fileInfoList.LastOrDefault();
        OnPropertyChanged(nameof(FileInfoList));
        OnPropertyChanged(nameof(ButtonDeleteEnable));
        OnPropertyChanged(nameof(ButtonDeleteVisibility));
    }

    private System.Windows.Visibility buttonDeleteVisibility = Visibility.Hidden;

    public System.Windows.Visibility ButtonDeleteVisibility { get => buttonDeleteVisibility; set => SetProperty(ref buttonDeleteVisibility, value); }

    private bool buttonDeleteEnable = false;

    public bool ButtonDeleteEnable { get => buttonDeleteEnable; set => SetProperty(ref buttonDeleteEnable, value); }

    private Visibility dragDropGridVisibility=Visibility.Hidden;

    public Visibility DragDropGridVisibility { get => dragDropGridVisibility; set => SetProperty(ref dragDropGridVisibility, value); }

    private double opacityMainGrid=1;

    public double OpacityMainGrid { get => opacityMainGrid; set => SetProperty(ref opacityMainGrid, value); }

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
