using System;
using System.Collections;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.VisualTree;

namespace GongSolutions.Avalonia.DragDrop;

public sealed class DragInfo : IDragInfo
{
    public DragInfo(Control source, PointerPressedEventArgs eventArgs)
    {
        VisualSource = source;
        DragStartPosition = eventArgs.GetPosition(source);
        var sourceVisual = eventArgs.Source as Visual;

        if (source is DataGrid dataGrid)
        {
            SourceCollection = dataGrid.ItemsSource as IEnumerable ?? Array.Empty<object>();
            if (ItemsControlDragDropHelper.IsDataGridHeader(sourceVisual)
                || ItemsControlDragDropHelper.FindDataGridRow(sourceVisual) is not { DataContext: { } rowItem } row)
            {
                SourceItems = Array.Empty<object>();
                return;
            }

            VisualSourceItem = row;
            SourceItems = ItemsControlDragDropHelper.GetSelectedItems(dataGrid, rowItem);
        }
        else if (source is TreeView treeView)
        {
            VisualSourceItem = sourceVisual?.FindAncestorOfType<TreeViewItem>(true);
            if (VisualSourceItem is not TreeViewItem treeViewItem
                || ItemsControl.ItemsControlFromItemContainer(treeViewItem) is not { } itemsParent)
            {
                SourceCollection = ItemsControlDragDropHelper.GetItems(treeView);
                SourceItems = Array.Empty<object>();
                return;
            }

            SourceCollection = ItemsControlDragDropHelper.GetItems(itemsParent);
            var clickedItem = itemsParent.ItemFromContainer(treeViewItem) ?? treeViewItem.DataContext;
            SourceItems = clickedItem is null ? Array.Empty<object>() : new[] { clickedItem };
        }
        else if (source is ItemsControl itemsControl)
        {
            VisualSourceItem = ItemsControlDragDropHelper.FindContainer(itemsControl, sourceVisual);
            if (VisualSourceItem is not { } container
                || itemsControl is TabControl && container is TabItem tabItem
                    && !ItemsControlDragDropHelper.IsTabHeader(sourceVisual, tabItem)
                || ItemsControl.ItemsControlFromItemContainer(container) is not { } itemsParent)
            {
                SourceCollection = ItemsControlDragDropHelper.GetItems(itemsControl);
                SourceItems = Array.Empty<object>();
                return;
            }

            SourceCollection = ItemsControlDragDropHelper.GetItems(itemsParent);
            var clickedItem = itemsParent.ItemFromContainer(container) ?? container.DataContext;
            SourceItems = clickedItem is null
                ? Array.Empty<object>()
                : ItemsControlDragDropHelper.GetSelectedItems(itemsParent, clickedItem);
        }
        else
        {
            SourceCollection = Array.Empty<object>();
            SourceItems = source.DataContext is null ? Array.Empty<object>() : new[] { source.DataContext };
        }
    }

    public object? Data { get; set; }
    public IDataTransfer? DataTransfer { get; set; }
    public DragDropEffects Effects { get; set; }
    public Point DragStartPosition { get; }
    public IEnumerable SourceCollection { get; }
    public IReadOnlyList<object> SourceItems { get; }
    public Control VisualSource { get; }
    public Control? VisualSourceItem { get; }
}