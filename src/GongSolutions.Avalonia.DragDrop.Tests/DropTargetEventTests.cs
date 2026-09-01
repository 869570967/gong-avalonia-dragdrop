using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Xunit;

namespace GongSolutions.Avalonia.DragDrop.Tests;

public sealed class DropTargetEventTests
{
    [Fact]
    public void AttachedProperties_AreOwnedByDragDrop()
    {
        Assert.Equal(typeof(DragDrop), DragDrop.IsDropTargetProperty.OwnerType);
        Assert.Equal(typeof(DragDrop), DragDrop.DropHandlerProperty.OwnerType);
    }

    [AvaloniaFact]
    public void DropTargetCallbacks_ReceiveEventsThatWereAlreadyHandled()
    {
        var handler = new RecordingDropTarget();
        var target = new Border();
        var child = new Border();
        target.Child = child;
        DragDrop.SetDropHandler(target, handler);
        DragDrop.SetIsDropTarget(target, true);

        RaiseHandledEvent(global::Avalonia.Input.DragDrop.DragEnterEvent, child);
        RaiseHandledEvent(global::Avalonia.Input.DragDrop.DragOverEvent, child);
        RaiseHandledEvent(global::Avalonia.Input.DragDrop.DragLeaveEvent, child);
        RaiseHandledEvent(global::Avalonia.Input.DragDrop.DropEvent, child);

        Assert.Equal(1, handler.DragEnterCount);
        Assert.Equal(3, handler.DragOverCount);
        Assert.Equal(1, handler.DragLeaveCount);
        Assert.Equal(1, handler.DropCount);
    }

    [AvaloniaFact]
    public void DragOver_InvokesOnlyNearestNestedDropTarget()
    {
        var outerHandler = new RecordingDropTarget();
        var innerHandler = new RecordingDropTarget();
        var outer = new Border();
        var inner = new Border();
        var child = new Border();
        outer.Child = inner;
        inner.Child = child;
        DragDrop.SetDropHandler(outer, outerHandler);
        DragDrop.SetIsDropTarget(outer, true);
        DragDrop.SetDropHandler(inner, innerHandler);
        DragDrop.SetIsDropTarget(inner, true);

        child.RaiseEvent(CreateDragEventArgs(global::Avalonia.Input.DragDrop.DragOverEvent, child));

        Assert.Equal(0, outerHandler.DragOverCount);
        Assert.Equal(1, innerHandler.DragOverCount);
    }

    private static DragEventArgs CreateDragEventArgs(RoutedEvent<DragEventArgs> routedEvent, Control source)
    {
        return new DragEventArgs(routedEvent, new DataTransfer(), source, default, KeyModifiers.None);
    }

    private static void RaiseHandledEvent(RoutedEvent<DragEventArgs> routedEvent, Control source)
    {
        var args = CreateDragEventArgs(routedEvent, source);
        args.Handled = true;
        source.RaiseEvent(args);
    }

    private sealed class RecordingDropTarget : IDropTarget
    {
        public int DragEnterCount { get; private set; }
        public int DragOverCount { get; private set; }
        public int DragLeaveCount { get; private set; }
        public int DropCount { get; private set; }

        public void DragEnter(IDropInfo dropInfo)
        {
            DragEnterCount++;
        }

        public void DragOver(IDropInfo dropInfo)
        {
            DragOverCount++;
            dropInfo.Effects = DragDropEffects.Move;
        }

        public void DragLeave(IDropInfo dropInfo)
        {
            DragLeaveCount++;
        }

        public void Drop(IDropInfo dropInfo)
        {
            DropCount++;
        }
    }
}
