using FolderAnalyzer.ViewModels;
using System.Windows;

namespace FolderAnalyzer;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}