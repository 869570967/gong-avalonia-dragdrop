using System.Collections;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Xunit;

namespace GongSolutions.Avalonia.DragDrop.Tests;

public sealed class DropTargetAdornerTests
{
    [AvaloniaFact]
    public void DisabledVisualSourceItemPreview_HidesFloatingPreview()
    {
        var source = new Border();
        DragDrop.SetUseVisualSourceItemPreview(source, false);
        var target = new Border();
        var adorner = new DropTargetAdorner(target);

        adorner.Update(new TestDropInfo(target, new TestDragInfo(source)));

        Assert.False(adorner.Children[1].IsVisible);
    }

    private sealed class TestDragInfo(Control source) : IDragInfo
    {
        public object? Data { get; set; } = "item";
        public IDataTransfer? DataTransfer { get; set; }
        public DragDropEffects Effects { get; set; } = DragDropEffects.Move;
        public Point DragStartPosition => default;
        public IEnumerable SourceCollection { get; } = new[] { "item" };
        public IReadOnlyList<object> SourceItems { get; } = new object[] { "item" };
        public Control VisualSource { get; } = source;
        public Control? VisualSourceItem => null;
    }

    private sealed class TestDropInfo(Control target, IDragInfo dragInfo) : IDropInfo
    {
        public bool AcceptChildItem { get; set; }
        public object? Data { get; set; } = dragInfo.Data;
        public IDataTransfer DataTransfer { get; } = new DataTransfer();
        public IDragInfo? DragInfo { get; } = dragInfo;
        public Point DropPosition => default;
        public DragDropEffects Effects { get; set; } = DragDropEffects.Move;
        public KeyModifiers KeyModifiers => KeyModifiers.None;
        public bool IsCopyRequested => false;
        public bool IsHorizontal => false;
        public int InsertIndex => 0;
        public int UnfilteredInsertIndex => 0;
        public RelativeInsertPosition InsertPosition => RelativeInsertPosition.None;
        public IEnumerable? TargetCollection => null;
        public object? TargetItem => null;
        public object? TargetGroup => null;
        public ScrollViewer? TargetScrollViewer => null;
        public Control VisualTarget { get; } = target;
        public Control? VisualTargetItem => null;
        public bool IsExternal => false;
        public bool IsSameDragDropContextAsSource => true;
    }
}