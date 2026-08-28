using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace GongSolutions.Avalonia.DragDrop;

public sealed class DragInfo : IDragInfo
{
    public DragInfo(Control source, PointerPressedEventArgs eventArgs)
    {
        VisualSource = source;
        DragStartPosition = eventArgs.GetPosition(source);
        var sourceVisual = eventArgs.Source as Visual;
        VisualSourceItem = sourceVisual?.FindAncestorOfType<ListBoxItem>(true)
                   ?? (Control?)sourceVisual?.FindAncestorOfType<TreeViewItem>(true);

        if (source is ListBox listBox)
        {
            SourceCollection = listBox.ItemsSource as IEnumerable ?? Array.Empty<object>();
            var selectedItems = listBox.SelectedItems?.Cast<object>().ToList() ?? new List<object>();
            var clickedItem = VisualSourceItem?.DataContext;
            SourceItems = selectedItems.Count > 0 && clickedItem is not null && selectedItems.Contains(clickedItem)
                ? selectedItems
                : clickedItem is null ? Array.Empty<object>() : new[] { clickedItem };
        }
            else if (VisualSourceItem is TreeViewItem treeViewItem)
            {
                var itemsParent = ItemsControl.ItemsControlFromItemContainer(treeViewItem);
                SourceCollection = itemsParent?.ItemsSource as IEnumerable ?? Array.Empty<object>();
                SourceItems = treeViewItem.DataContext is null
                ? Array.Empty<object>()
                : new[] { treeViewItem.DataContext };
            }
        else
        {
            SourceCollection = Array.Empty<object>();
            SourceItems = source.DataContext is null ? Array.Empty<object>() : new[] { source.DataContext };
        }
    }

    public object? Data { get; set; }
    public DragDropEffects Effects { get; set; }
    public Point DragStartPosition { get; }
    public IEnumerable SourceCollection { get; }
    public IReadOnlyList<object> SourceItems { get; }
    public Control VisualSource { get; }
    public Control? VisualSourceItem { get; }
}