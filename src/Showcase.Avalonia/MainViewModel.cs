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

        for (var index = 1; index <= 18; index++)
        {
            DataGridSourceItems.Add(new ItemModel(300 + index, $"Grid source {index:00}"));
        }

        for (var index = 1; index <= 5; index++)
        {
            DataGridTargetItems.Add(new ItemModel(400 + index, $"Grid target {index:00}"));
        }

        TabSourceItems.Add(new ItemModel(501, "Overview"));
        TabSourceItems.Add(new ItemModel(502, "Details"));
        TabSourceItems.Add(new ItemModel(503, "History"));
        TabTargetItems.Add(new ItemModel(504, "Output"));
        TabTargetItems.Add(new ItemModel(505, "Preview"));

        for (var index = 1; index <= 8; index++)
        {
            PlainItems.Add(new ItemModel(600 + index, $"Plain item {index:00}"));
        }

        TreeItems.Add(new TreeNodeModel(
            "Projects",
            new TreeNodeModel(
                "Avalonia",
                new TreeNodeModel("Controls") { CanAcceptChildren = false },
                new TreeNodeModel("Themes") { CanAcceptChildren = false }),
            new TreeNodeModel(
                "Libraries",
                new TreeNodeModel("Drag and drop") { CanAcceptChildren = false },
                new TreeNodeModel("Utilities") { CanAcceptChildren = false })));
        TreeItems.Add(new TreeNodeModel(
            "Documents",
            new TreeNodeModel("Notes"),
            new TreeNodeModel("Archive")));
        TreeItems.Add(new TreeNodeModel("Inbox"));
    }

    public ObservableCollection<ItemModel> SourceItems { get; } = new();
    public ObservableCollection<ItemModel> TargetItems { get; } = new();
    public ObservableCollection<ItemModel> IsolatedItems { get; } = new();
    public ObservableCollection<ItemModel> DataGridSourceItems { get; } = new();
    public ObservableCollection<ItemModel> DataGridTargetItems { get; } = new();
    public ObservableCollection<ItemModel> TabSourceItems { get; } = new();
    public ObservableCollection<ItemModel> TabTargetItems { get; } = new();
    public ObservableCollection<ItemModel> PlainItems { get; } = new();
    public ObservableCollection<TreeNodeModel> TreeItems { get; } = new();
}