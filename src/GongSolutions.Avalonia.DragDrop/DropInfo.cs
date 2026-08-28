using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Platform;
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
        Data = dragInfo?.Data;
        DataTransfer = eventArgs.DataTransfer;
        DropPosition = eventArgs.GetPosition(target);
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

        if (VisualTarget is ListBox listBox)
        {
            TargetCollection = GetItems(listBox);
            VisualTargetItem = (eventArgs.Source as Visual)?.FindAncestorOfType<ListBoxItem>(true);
            if (VisualTargetItem is ListBoxItem item)
            {
                var index = listBox.IndexFromContainer(item);
                TargetItem = listBox.ItemFromContainer(item);
                if (eventArgs.GetPosition(item).Y > item.Bounds.Height / 2)
                {
                    InsertIndex = index + 1;
                    InsertPosition = RelativeInsertPosition.AfterTargetItem;
                }
                else
                {
                    InsertIndex = index;
                    InsertPosition = RelativeInsertPosition.BeforeTargetItem;
                }
            }
            else
            {
                InsertIndex = listBox.ItemCount;
            }
        }
        else if (VisualTarget is TreeView treeView)
        {
            VisualTargetItem = (eventArgs.Source as Visual)?.FindAncestorOfType<TreeViewItem>(true);
            if (VisualTargetItem is TreeViewItem treeViewItem)
            {
                var itemsParent = ItemsControl.ItemsControlFromItemContainer(treeViewItem);
                TargetCollection = itemsParent is null ? null : GetItems(itemsParent);
                TargetItem = itemsParent?.ItemFromContainer(treeViewItem) ?? treeViewItem.DataContext;
                InsertIndex = itemsParent?.IndexFromContainer(treeViewItem) ?? 0;

                var header = treeViewItem.HeaderPresenter;
                if (header is not null)
                {
                    var position = eventArgs.GetPosition(header);
                    if (position.Y >= header.Bounds.Height * 0.25
                        && position.Y <= header.Bounds.Height * 0.75
                        && AcceptChildItem)
                    {
                        TargetCollection = GetItems(treeViewItem);
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
                TargetCollection = GetItems(treeView);
                InsertIndex = treeView.ItemCount;
            }
        }
    }

    private static IEnumerable GetItems(ItemsControl itemsControl)
    {
        return itemsControl.ItemsSource as IEnumerable ?? itemsControl.Items;
    }

    public object? Data { get; set; }
    public IDataTransfer DataTransfer { get; }
    public IDragInfo? DragInfo { get; }
    public Point DropPosition { get; }
    public DragDropEffects Effects { get; set; }
    public int InsertIndex { get; private set; }
    public RelativeInsertPosition InsertPosition { get; private set; }
    public IEnumerable? TargetCollection { get; private set; }
    public object? TargetItem { get; private set; }
    public ScrollViewer? TargetScrollViewer { get; }
    public Control VisualTarget { get; }
    public Control? VisualTargetItem { get; private set; }

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