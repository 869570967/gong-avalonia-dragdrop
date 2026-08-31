using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Xunit;

namespace GongSolutions.Avalonia.DragDrop.Tests;

public class DefaultDropHandlerTests
{
    [Fact]
    public void ReordersItemsWithinSameCollection()
    {
        var items = new List<object> { "a", "b", "c", "d" };
        var info = CreateInfo(items, items, new object[] { "b", "c" }, 4);

        new DefaultDropHandler().Drop(info);

        Assert.Equal(new[] { "a", "d", "b", "c" }, items);
    }

    [Fact]
    public void MovesItemsAcrossCollections()
    {
        var source = new List<object> { "a", "b" };
        var target = new List<object> { "c" };
        var info = CreateInfo(source, target, new object[] { "b" }, 0);

        new DefaultDropHandler().Drop(info);

        Assert.Equal(new[] { "a" }, source);
        Assert.Equal(new[] { "b", "c" }, target);
    }

    [Fact]
    public void ExternalDataIsCopiedIntoTarget()
    {
        var target = new List<object>();
        var info = new TestDropInfo(target, new object[] { "one", "two" }, 0)
        {
            IsExternal = true,
            Effects = DragDropEffects.Copy
        };

        new DefaultDropHandler().Drop(info);

        Assert.Equal(new[] { "one", "two" }, target);
    }

    [Fact]
    public void UsesSorterAndResolvedSourceIndex()
    {
        var source = new List<object> { "b", "a" };
        var target = new List<object> { "existing" };
        var visualTarget = new Border();
        DragDrop.SetDropTargetItemsSorter(visualTarget, new AlphabeticalSorter());
        DragDrop.SetDropIndexResolver(visualTarget, new FixedIndexResolver(1));
        var info = CreateInfo(source, target, source.ToArray(), 0, visualTarget);

        new DefaultDropHandler().Drop(info);

        Assert.Equal(new[] { "existing", "a", "b" }, target);
    }

    [Fact]
    public void RejectsIncompatibleTargetElementType()
    {
        var source = new List<object> { "text" };
        var info = CreateInfo(source, new List<int>(), source, 0);

        Assert.False(DefaultDropHandler.CanAcceptData(info));
    }

    private static TestDropInfo CreateInfo(
        IList source,
        IList target,
        IReadOnlyList<object> data,
        int insertIndex,
        Control? visualTarget = null)
    {
        return new TestDropInfo(target, data, insertIndex, visualTarget)
        {
            DragInfo = new TestDragInfo(source, data),
            Effects = DragDropEffects.Move
        };
    }

    private sealed class TestDropInfo(
        IEnumerable targetCollection,
        object data,
        int insertIndex,
        Control? visualTarget = null) : IDropInfo
    {
        public bool AcceptChildItem { get; set; } = true;
        public object? Data { get; set; } = data;
        public IDataTransfer DataTransfer { get; } = null!;
        public IDragInfo? DragInfo { get; set; }
        public Point DropPosition => default;
        public DragDropEffects Effects { get; set; }
        public KeyModifiers KeyModifiers => KeyModifiers.None;
        public bool IsCopyRequested { get; set; }
        public bool IsHorizontal => false;
        public int InsertIndex { get; } = insertIndex;
        public int UnfilteredInsertIndex => DragDrop.GetDropIndexResolver(VisualTarget)?.ResolveSourceInsertIndex(this) ?? InsertIndex;
        public IEnumerable? TargetCollection { get; } = targetCollection;
        public object? TargetItem => null;
        public object? TargetGroup => DragDrop.GetDropGroupResolver(VisualTarget)?.ResolveTargetGroup(this);
        public ScrollViewer? TargetScrollViewer => null;
        public Control VisualTarget { get; } = visualTarget ?? new Border();
        public Control? VisualTargetItem => null;
        public bool IsExternal { get; set; }
        public bool IsSameDragDropContextAsSource => true;
        public RelativeInsertPosition InsertPosition => RelativeInsertPosition.None;
    }

    private sealed class TestDragInfo(IEnumerable sourceCollection, IReadOnlyList<object> sourceItems) : IDragInfo
    {
        public object? Data { get; set; } = sourceItems;
        public IDataTransfer? DataTransfer { get; set; }
        public DragDropEffects Effects { get; set; }
        public Point DragStartPosition => default;
        public IEnumerable SourceCollection { get; } = sourceCollection;
        public IReadOnlyList<object> SourceItems { get; } = sourceItems;
        public Control VisualSource { get; } = new Border();
        public Control? VisualSourceItem => null;
    }

    private sealed class AlphabeticalSorter : IDropTargetItemsSorter
    {
        public IEnumerable SortDropTargetItems(IEnumerable items) => items.Cast<object>().OrderBy(item => item);
    }

    private sealed class FixedIndexResolver(int index) : IDropIndexResolver
    {
        public int ResolveSourceInsertIndex(IDropInfo dropInfo) => index;
    }
}
