using Avalonia.Controls;

namespace Showcase.Avalonia.DragDrop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}