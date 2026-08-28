using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace GongSolutions.Avalonia.DragDrop;

public sealed class DropInfo : IDropInfo
{
    public DropInfo(Control target, DragEventArgs eventArgs, IDragInfo? dragInfo)
    {
        VisualTarget = target;
        DragInfo = dragInfo;
        Data = dragInfo?.Data;
        Effects = DragDropEffects.None;

        if (target is ListBox listBox)
        {
            TargetCollection = listBox.ItemsSource as IEnumerable;
            var position = eventArgs.GetPosition(listBox);
            VisualTargetItem = (eventArgs.Source as Visual)?.FindAncestorOfType<ListBoxItem>(true);
            if (VisualTargetItem is ListBoxItem item)
            {
                var index = listBox.IndexFromContainer(item);
                var itemOrigin = item.TransformToVisual(listBox)?.Transform(default).Y ?? 0;
                InsertIndex = position.Y > itemOrigin + item.Bounds.Height / 2
                    ? index + 1
                    : index;
            }
            else
            {
                InsertIndex = listBox.ItemCount;
            }
        }
    }

    public object? Data { get; set; }
    public IDragInfo? DragInfo { get; }
    public DragDropEffects Effects { get; set; }
    public int InsertIndex { get; }
    public IEnumerable? TargetCollection { get; }
    public Control VisualTarget { get; }
    public Control? VisualTargetItem { get; }

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