using System.Collections.ObjectModel;

namespace Showcase.Avalonia.DragDrop;

public sealed class MainViewModel
{
    public MainViewModel()
    {
        for (var index = 1; index <= 12; index++)
        {
            SourceItems.Add(new ItemModel(index, $"Source item {index:00}"));
        }

        for (var index = 1; index <= 5; index++)
        {
            TargetItems.Add(new ItemModel(100 + index, $"Target item {index:00}"));
        }

        IsolatedItems.Add(new ItemModel(201, "Isolated context"));
    }

    public ObservableCollection<ItemModel> SourceItems { get; } = new();
    public ObservableCollection<ItemModel> TargetItems { get; } = new();
    public ObservableCollection<ItemModel> IsolatedItems { get; } = new();
}