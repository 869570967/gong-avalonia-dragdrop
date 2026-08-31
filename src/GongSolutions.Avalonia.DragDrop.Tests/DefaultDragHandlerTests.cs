using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Xunit;

namespace GongSolutions.Avalonia.DragDrop.Tests;

public class DefaultDragHandlerTests
{
    [Fact]
    public void EmptySelectionCannotStartDrag()
    {
        var dragInfo = new TestDragInfo(Array.Empty<object>(), new List<object>());

        Assert.False(new DefaultDragHandler().CanStartDrag(dragInfo));
    }

    [Fact]
    public void SingleEnumerableRemainsOneDraggedItem()
    {
        var item = new[] { 1, 2 };
        var dragInfo = new TestDragInfo(new object[] { item }, new List<object> { item });

        new DefaultDragHandler().StartDrag(dragInfo);

        Assert.Same(item, Assert.Single(Assert.IsAssignableFrom<IEnumerable<object>>(dragInfo.Data)));
        Assert.Equal(DragDropEffects.Copy | DragDropEffects.Move, dragInfo.Effects);
    }

    [Fact]
    public void MultipleItemsKeepTheirOrder()
    {
        var dragInfo = new TestDragInfo(new object[] { "second", "first" }, new List<object>());

        new DefaultDragHandler().StartDrag(dragInfo);

        Assert.Equal(new[] { "second", "first" }, Assert.IsAssignableFrom<IEnumerable<object>>(dragInfo.Data));
    }

    private sealed class TestDragInfo(IReadOnlyList<object> sourceItems, IEnumerable sourceCollection) : IDragInfo
    {
        public object? Data { get; set; }
        public IDataTransfer? DataTransfer { get; set; }
        public DragDropEffects Effects { get; set; }
        public Point DragStartPosition => default;
        public IEnumerable SourceCollection { get; } = sourceCollection;
        public IReadOnlyList<object> SourceItems { get; } = sourceItems;
        public Control VisualSource { get; } = new Border();
        public Control? VisualSourceItem => null;
    }
}
