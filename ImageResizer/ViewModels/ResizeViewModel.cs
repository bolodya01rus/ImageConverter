using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageMagick;
using ImageResizer.Contracts.Services;
using ImageResizer.Models;
using ImageResizer.Views;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using static System.Net.WebRequestMethods;
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.DragDropEffects;
using FileInfo = ImageResizer.Models.FileInfo;
using ListBox = System.Windows.Controls.ListBox;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

namespace ImageResizer.ViewModels;

public class ResizeViewModel : ObservableObject
{
    private readonly string destPath = string.Empty;
    private List<string> Extantions = new() { ".jpeg", ".jpg", ".png", ".bmp", ".heic" };
    IImageResizeServices _resizeServices;
    INavigationService _navigationService;
    private string[] pathFiles;
    private int indexRadioButton;
    public ResizeViewModel(IImageResizeServices resizeServices, INavigationService navigationService)
    {
        _navigationService = navigationService;
        _resizeServices = resizeServices;
        fileInfoList = new();

        resolutionItems = new ObservableCollection<StandartResolution>()
        {
            new StandartResolution(){ Name="Мелкое",Width=1280,Height=1024},
            new StandartResolution() {Name="Среднее", Width=1600, Height=1200 },
            new StandartResolution() {Name="Крупное", Width=1920,Height=1080}
        };
        destPath = Properties.Settings.Default.PathSaveFile;
        _resizeServices.CheckFolderForSave(destPath);
        if (App.PathForResize != null)
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

        // CheckRadioButtonCommand = new RelayCommand<object>(CheckRadioButton);
        ValueChangedNumericUpDownCommand = new RelayCommand(ValueChangedNumericUpDown);
        DeleteItemCommand = new RelayCommand<object>(DeleteItem);
        DropFileCommand = new RelayCommand<object>(DropFile);
        #endregion
    }


    private void ValueChangedNumericUpDown()
    {
        checkStandartRadioButtonCommand = false;
        checkOptionalRadioButtonCommand = true;
        checkPercentRadioButtonCommand = false;

        OnPropertyChanged(nameof(CheckOptionalRadioButtonCommand));
        OnPropertyChanged(nameof(CheckPercentRadioButtonCommand));
        OnPropertyChanged(nameof(CheckStandartRadioButtonCommand));
    }
    public ICommand DeleteItemCommand { get; set; }

    private void DeleteItem(object obj)
    {
        var listBox = (ListBox)obj;
        if (listBox.Items.Count > 0 && listBox.SelectedIndex >= 0)
            fileInfoList.RemoveAt(listBox.SelectedIndex);

        if (listBox.Items.Count <= 0 && listBox.SelectedIndex < 0)
        {
            buttonDeleteEnableCommand = false;
            buttonDeleteVisibility = Visibility.Hidden;
        }
        OnPropertyChanged(nameof(fileInfoList));
        OnPropertyChanged(nameof(ButtonDeleteEnable));
        OnPropertyChanged(nameof(ButtonDeleteVisibility));

    }
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
        }
        else if (format.ToLower() == ".pdf")
        {

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
    public string ShortName { get; set; }
    public string FullName { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    private RelayCommand startResizeCommand;
    public ICommand StartResizeCommand => startResizeCommand ??= new RelayCommand(StartResize);

    private async void StartResize()
    {
        if (fileInfoList != null && fileInfoList.Count > 0)
        {


            if (checkStandartRadioButtonCommand)
            {
                var resolution = resolutionItems[selectedResolutionIndex];

                foreach (var path in fileInfoList)
                {
                    try
                    {
                        if (selectedOpenFileItem == null)
                            _resizeServices.ResizeImages(path.FullName, destPath, resolution.Width, resolution.Height);
                        else
                            _resizeServices.ResizeImages(((FileInfo)selectedOpenFileItem).FullName, destPath, resolution.Width, resolution.Height);

                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message); }
                }
            }
            if (checkOptionalRadioButtonCommand)
            {

                foreach (var path in fileInfoList)
                {
                    try
                    {
                        if (selectedOpenFileItem == null)
                            _resizeServices.ResizeImages(path.FullName, destPath, (int)widthNumericUpDown, (int)heightNumericUpDown);
                        else
                            _resizeServices.ResizeImages(((FileInfo)selectedOpenFileItem).FullName, destPath, (int)widthNumericUpDown, (int)heightNumericUpDown);
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message); }
                }

            }
            if (checkPercentRadioButtonCommand)
            {
                foreach (var path in fileInfoList)
                {
                    try
                    {
                        if (selectedOpenFileItem == null)
                            _resizeServices.ResizeImages(path.FullName, destPath, percentNumericUpDown);
                        else
                            _resizeServices.ResizeImages(((FileInfo)selectedOpenFileItem).FullName, destPath, percentNumericUpDown);
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message); }
                }

            }
            Process.Start("explorer", destPath);
        }
        else { MessageBox.Show("Необходимо выбрать хотя бы один файл"); }
    }

    private ObservableCollection<StandartResolution> resolutionItems;

    public ObservableCollection<StandartResolution> ResolutionItems { get => resolutionItems; set => SetProperty(ref resolutionItems, value); }

    private int selectedResolutionIndex;

    public int SelectedResolutionIndex { get => selectedResolutionIndex; set => SetProperty(ref selectedResolutionIndex, value); }

    private double widthNumericUpDown;

    public double WidthNumericUpDown { get => widthNumericUpDown; set => SetProperty(ref widthNumericUpDown, value); }

    private double heightNumericUpDown;

    public double HeightNumericUpDown { get => heightNumericUpDown; set => SetProperty(ref heightNumericUpDown, value); }

    private double percentNumericUpDown;

    public double PercentNumericUpDown { get => percentNumericUpDown; set => SetProperty(ref percentNumericUpDown, value); }

    private ObservableCollection<FileInfo> fileInfoList;
    public ObservableCollection<FileInfo> FileInfoList { get => fileInfoList; set => SetProperty(ref fileInfoList, value); }

    private RelayCommand cancelCommand;
    public ICommand CancelCommand => cancelCommand ??= new RelayCommand(Cancel);

    private void Cancel()
    {
        App.PathForResize = null;
        indexRadioButton = 0;
        pathFiles = null;
        fileInfoList.Clear();
        buttonDeleteEnableCommand = false;
        buttonDeleteVisibility = Visibility.Hidden;
        PerformValueChangedResolutionCombobox();
        OnPropertyChanged(nameof(ButtonDeleteEnable));
        OnPropertyChanged(nameof(ButtonDeleteVisibility));
    }


    public ICommand ValueChangedNumericUpDownCommand { get; set; }

    private bool checkStandartRadioButtonCommand = true;

    public bool CheckStandartRadioButtonCommand { get => checkStandartRadioButtonCommand; set => SetProperty(ref checkStandartRadioButtonCommand, value); }

    private bool checkOptionalRadioButtonCommand;

    public bool CheckOptionalRadioButtonCommand { get => checkOptionalRadioButtonCommand; set => SetProperty(ref checkOptionalRadioButtonCommand, value); }

    private bool checkPercentRadioButtonCommand;

    public bool CheckPercentRadioButtonCommand { get => checkPercentRadioButtonCommand; set => SetProperty(ref checkPercentRadioButtonCommand, value); }

    private RelayCommand valueChangedPercentNumericUpDownCommand;
    public ICommand ValueChangedPercentNumericUpDownCommand => valueChangedPercentNumericUpDownCommand ??= new RelayCommand(ValueChangedPercentNumericUpDown);

    private void ValueChangedPercentNumericUpDown()
    {
        checkStandartRadioButtonCommand = false;
        checkOptionalRadioButtonCommand = false;
        checkPercentRadioButtonCommand = true;

        OnPropertyChanged(nameof(CheckOptionalRadioButtonCommand));
        OnPropertyChanged(nameof(CheckPercentRadioButtonCommand));
        OnPropertyChanged(nameof(CheckStandartRadioButtonCommand));
    }

    private RelayCommand valueChangedResolutionCombobox;
    public ICommand ValueChangedResolutionCombobox => valueChangedResolutionCombobox ??= new RelayCommand(PerformValueChangedResolutionCombobox);

    private void PerformValueChangedResolutionCombobox()
    {
        checkStandartRadioButtonCommand = true;
        checkOptionalRadioButtonCommand = false;
        checkPercentRadioButtonCommand = false;

        OnPropertyChanged(nameof(CheckOptionalRadioButtonCommand));
        OnPropertyChanged(nameof(CheckPercentRadioButtonCommand));
        OnPropertyChanged(nameof(CheckStandartRadioButtonCommand));
    }

    private object selectedOpenFileItem;

    public object SelectedOpenFileItem { get => selectedOpenFileItem; set => SetProperty(ref selectedOpenFileItem, value); }

    private bool buttonDeleteEnableCommand = false;

    public bool ButtonDeleteEnable { get => buttonDeleteEnableCommand; set => SetProperty(ref buttonDeleteEnableCommand, value); }

    private Visibility buttonDeleteVisibility = Visibility.Hidden;

    public Visibility ButtonDeleteVisibility { get => buttonDeleteVisibility; set => SetProperty(ref buttonDeleteVisibility, value); }

    private RelayCommand selectListboxOpenFiles;
    public ICommand SelectListboxOpenFiles => selectListboxOpenFiles ??= new RelayCommand(PerformSelectListboxOpenFiles);

    private void PerformSelectListboxOpenFiles()
    {
        buttonDeleteEnableCommand = true;
        OnPropertyChanged(nameof(ButtonDeleteEnable));
    }


    public ICommand DropFileCommand { get; set; }

    private void DropFile(object obj)
    {
        var a = obj.ToString();
    }

    private RelayCommand dragFileCommand;
    public ICommand DragFileCommand => dragFileCommand ??= new RelayCommand(DragFile);

    private void DragFile()
    {

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


    private double opacityMainGrid =1;

    public double OpacityMainGrid { get => opacityMainGrid; set => SetProperty(ref opacityMainGrid, value); }

    private Visibility dragDropGridVisibility = Visibility.Hidden;

    public Visibility DragDropGridVisibility { get => dragDropGridVisibility; set => SetProperty(ref dragDropGridVisibility, value); }
    //private RelayCommand navigateCommand;

    //public ICommand NavigateCommand => navigateCommand ??= new RelayCommand(Navigate);

    //private void Navigate()
    //{
    //    _navigationService.NavigateTo(typeof(RotateViewModel).FullName);
    //}
}
