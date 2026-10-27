using FolderAnalyzer.Commands;
using FolderAnalyzer.Models;
using FolderAnalyzer.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using static FolderAnalyzer.Services.FileScannerService;

namespace FolderAnalyzer.ViewModels;

public class MainViewModel : BaseViewModel
{
    private CancellationTokenSource? cts;
    private readonly FileScannerService scanner = new();
    private readonly CsvExportService exporter = new();

    private string selectedFolder = "";

    public string SelectedFolder
    {
        get => selectedFolder;
        set
        {
            selectedFolder = value;
            OnPropertyChanged();
        }
    }

    private int totalFiles;

    public int TotalFiles
    {
        get => totalFiles;
        set
        {
            totalFiles = value;
            OnPropertyChanged();
        }
    }

    private string totalSize = "0 Mo";

    public string TotalSize
    {
        get => totalSize;
        set
        {
            totalSize = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<FileItem> Files { get; }
        = new();

    public RelayCommand BrowseCommand { get; }
    public RelayCommand ScanCommand { get; }
    public RelayCommand ExportCommand { get; }
    public RelayCommand CancelCommand { get; }

    public MainViewModel()
    {
        BrowseCommand = new RelayCommand(_ => Browse());
        ScanCommand = new RelayCommand(_ => Scan());
        ExportCommand = new RelayCommand(_ => Export());
        CancelCommand = new RelayCommand(_ => Cancel());
    }

    private void Browse()
    {
        using var dialog = new FolderBrowserDialog();

        if (dialog.ShowDialog() ==
            DialogResult.OK)
        {
            SelectedFolder = dialog.SelectedPath;
        }
    }

    private string currentFile = "";

    public string CurrentFile
    {
        get => currentFile;
        set
        {
            currentFile = value;
            OnPropertyChanged();
        }
    }

    private async void Scan()
    {
        if (!Directory.Exists(SelectedFolder))
            return;

        IsScanning = true;
        cts = new CancellationTokenSource();
        ProgressValue = 0;

        Files.Clear();
        
        try
            {
        var progress = new Progress<ScanResult>(p =>
            {
                Files.Add(p.File);

                ProgressValue = p.Percentage;
                CurrentFile = p.FileName;

                TotalFiles = Files.Count;
            });

       
        await scanner.ScanAsync(
            SelectedFolder,
            item =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    Files.Add(item);
                });
            },
            progress,
            cts.Token);

        TotalFiles = Files.Count;

        var size = Files.Sum(f => f.Size);

        TotalSize =
        $"{size / 1024d / 1024d / 1024d:F2} Go";
        }
        catch (OperationCanceledException)
        {
            CurrentFile = "Scan annulé";
        }
        finally
        {
            IsScanning = false;
        }

        IsScanning = false;
    }

    private void Export()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "CSV (*.csv)|*.csv"
        };

        if (dialog.ShowDialog() == true)
        {
            exporter.Export(Files, dialog.FileName);
        }
    }

    private int progressValue;

    public int ProgressValue
    {
        get => progressValue;
        set
        {
            progressValue = value;
            OnPropertyChanged();
        }
    }

    private bool isScanning;

    public bool IsScanning
    {
        get => isScanning;
        set
        {
            isScanning = value;
            OnPropertyChanged();
        }
    }

    private void Cancel()
    {
        cts?.Cancel();
    }

}