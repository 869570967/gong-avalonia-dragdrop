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

        TreeItems.Add(new TreeNodeModel(
            "Projects",
            new TreeNodeModel(
                "Avalonia",
                new TreeNodeModel("Controls"),
                new TreeNodeModel("Themes")),
            new TreeNodeModel(
                "Libraries",
                new TreeNodeModel("Drag and drop"),
                new TreeNodeModel("Utilities"))));
        TreeItems.Add(new TreeNodeModel(
            "Documents",
            new TreeNodeModel("Notes"),
            new TreeNodeModel("Archive")));
        TreeItems.Add(new TreeNodeModel("Inbox"));
    }

    public ObservableCollection<ItemModel> SourceItems { get; } = new();
    public ObservableCollection<ItemModel> TargetItems { get; } = new();
    public ObservableCollection<ItemModel> IsolatedItems { get; } = new();
    public ObservableCollection<TreeNodeModel> TreeItems { get; } = new();
}