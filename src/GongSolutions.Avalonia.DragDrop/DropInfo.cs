using System;
using System.Collections;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace GongSolutions.Avalonia.DragDrop;

public sealed class DropInfo : IDropInfo
{
    private readonly DragEventArgs eventArgs;

    public DropInfo(Control target, DragEventArgs eventArgs, IDragInfo? dragInfo)
    {
        this.eventArgs = eventArgs;
        VisualTarget = target;
        DragInfo = dragInfo;
        DataTransfer = eventArgs.DataTransfer;
        Data = dragInfo?.Data ?? GetExternalData(DataTransfer);
        DropPosition = eventArgs.GetPosition(target);
        KeyModifiers = eventArgs.KeyModifiers;
        var copyModifiers = dragInfo is null
            ? DragDrop.GetDragDropCopyKeyModifiers(target)
            : DragDrop.GetDragDropCopyKeyModifiers(dragInfo.VisualSource);
        IsCopyRequested = copyModifiers != KeyModifiers.None
                          && (KeyModifiers & copyModifiers) == copyModifiers;
        Effects = DragDropEffects.None;
        TargetScrollViewer = target.FindDescendantOfType<ScrollViewer>();
        acceptChildItem = true;

        Update();
    }

    private bool acceptChildItem;

    public bool AcceptChildItem
    {
        get => acceptChildItem;
        set
        {
            if (acceptChildItem == value)
            {
                return;
            }

            acceptChildItem = value;
            Update();
        }
    }

    private void Update()
    {
        InsertIndex = 0;
        InsertPosition = RelativeInsertPosition.None;
        TargetCollection = null;
        TargetItem = null;
        VisualTargetItem = null;
        IsHorizontal = false;

        if (VisualTarget is DataGrid dataGrid)
        {
            if (ItemsControlDragDropHelper.IsDataGridHeader(eventArgs.Source as Visual))
            {
                return;
            }

            TargetCollection = dataGrid.ItemsSource as IEnumerable;
            VisualTargetItem = ItemsControlDragDropHelper.FindDataGridRow(eventArgs.Source as Visual);
            VisualTargetItem ??= ItemsControlDragDropHelper.FindClosestDataGridRow(dataGrid, DropPosition);
            if (VisualTargetItem is DataGridRow { DataContext: { } rowItem } row)
            {
                TargetItem = rowItem;
                InsertIndex = ItemsControlDragDropHelper.IndexOf(TargetCollection ?? Array.Empty<object>(), rowItem);
                SetLinearInsertPosition(row, Orientation.Vertical);
            }
            else
            {
                InsertIndex = TargetCollection?.Cast<object>().Count() ?? 0;
            }
        }
        else if (VisualTarget is TreeView treeView)
        {
            VisualTargetItem = (eventArgs.Source as Visual)?.FindAncestorOfType<TreeViewItem>(true);
            if (VisualTargetItem is TreeViewItem treeViewItem)
            {
                var itemsParent = ItemsControl.ItemsControlFromItemContainer(treeViewItem);
                TargetCollection = itemsParent is null ? null : ItemsControlDragDropHelper.GetItems(itemsParent);
                TargetItem = itemsParent?.ItemFromContainer(treeViewItem) ?? treeViewItem.DataContext;
                InsertIndex = itemsParent?.IndexFromContainer(treeViewItem) ?? 0;

                var header = ItemsControlDragDropHelper.GetTreeViewItemHeader(treeViewItem);
                if (header is not null)
                {
                    var position = eventArgs.GetPosition(header);
                    if (position.Y >= header.Bounds.Height * 0.25
                        && position.Y <= header.Bounds.Height * 0.75
                        && AcceptChildItem)
                    {
                        TargetCollection = ItemsControlDragDropHelper.GetItems(treeViewItem);
                        InsertIndex = treeViewItem.ItemCount;
                        InsertPosition = RelativeInsertPosition.TargetItemCenter;
                    }
                    else if (position.Y > header.Bounds.Height / 2)
                    {
                        InsertIndex++;
                        InsertPosition = RelativeInsertPosition.AfterTargetItem;
                    }
                    else
                    {
                        InsertPosition = RelativeInsertPosition.BeforeTargetItem;
                    }
                }
            }
            else
            {
                TargetCollection = ItemsControlDragDropHelper.GetItems(treeView);
                InsertIndex = treeView.ItemCount;
            }
        }
        else if (VisualTarget is ItemsControl itemsControl)
        {
            var orientation = ItemsControlDragDropHelper.GetOrientation(itemsControl);
            IsHorizontal = orientation == Orientation.Horizontal;
            VisualTargetItem = ItemsControlDragDropHelper.FindContainer(itemsControl, eventArgs.Source as Visual);

            if (itemsControl is TabControl
                && (VisualTargetItem is not TabItem tabItem
                    || !ItemsControlDragDropHelper.IsTabHeader(eventArgs.Source as Visual, tabItem)))
            {
                return;
            }

            VisualTargetItem ??= ItemsControlDragDropHelper.FindClosestContainer(itemsControl, DropPosition, orientation);

            if (VisualTargetItem is { } item
                && ItemsControl.ItemsControlFromItemContainer(item) is { } itemsParent)
            {
                orientation = ItemsControlDragDropHelper.GetOrientation(itemsParent);
                IsHorizontal = orientation == Orientation.Horizontal;
                TargetCollection = ItemsControlDragDropHelper.GetItems(itemsParent);
                TargetItem = itemsParent.ItemFromContainer(item) ?? item.DataContext;
                InsertIndex = itemsParent.IndexFromContainer(item);
                SetLinearInsertPosition(item, orientation);
            }
            else
            {
                TargetCollection = ItemsControlDragDropHelper.GetItems(itemsControl);
                InsertIndex = itemsControl.ItemCount;
            }
        }
    }

    private void SetLinearInsertPosition(Control item, Orientation orientation)
    {
        var position = eventArgs.GetPosition(item);
        var after = orientation == Orientation.Horizontal
            ? position.X > item.Bounds.Width / 2
            : position.Y > item.Bounds.Height / 2;
        if (orientation == Orientation.Horizontal && item.FlowDirection == FlowDirection.RightToLeft)
        {
            after = !after;
        }
        if (after)
        {
            InsertIndex++;
            InsertPosition = RelativeInsertPosition.AfterTargetItem;
        }
        else
        {
            InsertPosition = RelativeInsertPosition.BeforeTargetItem;
        }
    }

    public object? Data { get; set; }
    public IDataTransfer DataTransfer { get; }
    public IDragInfo? DragInfo { get; }
    public Point DropPosition { get; }
    public DragDropEffects Effects { get; set; }
    public KeyModifiers KeyModifiers { get; }
    public bool IsCopyRequested { get; }
    public bool IsHorizontal { get; private set; }
    public int InsertIndex { get; private set; }
    public int UnfilteredInsertIndex => DragDrop.GetDropIndexResolver(VisualTarget)?.ResolveSourceInsertIndex(this) ?? InsertIndex;
    public RelativeInsertPosition InsertPosition { get; private set; }
    public IEnumerable? TargetCollection { get; private set; }
    public object? TargetItem { get; private set; }
    public object? TargetGroup => DragDrop.GetDropGroupResolver(VisualTarget)?.ResolveTargetGroup(this);
    public ScrollViewer? TargetScrollViewer { get; }
    public Control VisualTarget { get; }
    public Control? VisualTargetItem { get; private set; }
    public bool IsExternal => DragInfo is null;

    private static object? GetExternalData(IDataTransfer dataTransfer)
    {
        var files = dataTransfer.TryGetFiles()?.Cast<object>().ToList();
        return files is { Count: > 0 } ? files : dataTransfer.TryGetText();
    }

    public bool IsSameDragDropContextAsSource
    {
        get
        {
            if (DragInfo is null)
            {
                return true;
            }

            var sourceContext = DragDrop.GetDragDropContext(DragInfo.VisualSource);
            var targetContext = DragDrop.GetDragDropContext(VisualTarget);
            return string.IsNullOrEmpty(targetContext) || sourceContext == targetContext;
        }
    }
}