using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace GongSolutions.Avalonia.DragDrop;

public static class DragDrop
{
    private static readonly DataFormat<object> GongDataFormat = global::Avalonia.Input.DataFormat.CreateInProcessFormat<object>("GongSolutions.Avalonia.DragDrop");
    private static IDragInfo? activeDragInfo;
    private static PointerPressedEventArgs? triggerEvent;
    private static bool dragInProgress;
    private static DropTargetAdorner? dropTargetAdorner;
    private static Control? adornedTarget;
    private static TreeViewItem? pendingExpandTarget;
    private static CancellationTokenSource? expandCancellation;

    public static readonly AttachedProperty<bool> IsDragSourceProperty =
        AvaloniaProperty.RegisterAttached<Control, Control, bool>("IsDragSource");

    public static readonly AttachedProperty<bool> IsDropTargetProperty =
        AvaloniaProperty.RegisterAttached<Control, Control, bool>("IsDropTarget");

    public static readonly AttachedProperty<IDragSource?> DragHandlerProperty =
        AvaloniaProperty.RegisterAttached<Control, Control, IDragSource?>("DragHandler");

    public static readonly AttachedProperty<IDropTarget?> DropHandlerProperty =
        AvaloniaProperty.RegisterAttached<Control, Control, IDropTarget?>("DropHandler");

    public static readonly AttachedProperty<string?> DragDropContextProperty =
        AvaloniaProperty.RegisterAttached<Control, Control, string?>("DragDropContext");

    public static readonly AttachedProperty<IDragInfoBuilder?> DragInfoBuilderProperty =
        AvaloniaProperty.RegisterAttached<Control, Control, IDragInfoBuilder?>("DragInfoBuilder");

    public static readonly AttachedProperty<IDropInfoBuilder?> DropInfoBuilderProperty =
        AvaloniaProperty.RegisterAttached<Control, Control, IDropInfoBuilder?>("DropInfoBuilder");

    public static readonly AttachedProperty<bool> SelectDroppedItemsProperty =
        AvaloniaProperty.RegisterAttached<Control, Control, bool>("SelectDroppedItems", true);

    public static readonly AttachedProperty<ScrollingMode> ScrollingModeProperty =
        AvaloniaProperty.RegisterAttached<Control, Control, ScrollingMode>("ScrollingMode", ScrollingMode.Both);

    public static IDragSource DefaultDragHandler { get; } = new DefaultDragHandler();
    public static IDropTarget DefaultDropHandler { get; } = new DefaultDropHandler();
    internal static bool ShouldCopy { get; private set; }

    static DragDrop()
    {
        IsDragSourceProperty.Changed.AddClassHandler<Control>(OnIsDragSourceChanged);
        IsDropTargetProperty.Changed.AddClassHandler<Control>(OnIsDropTargetChanged);
    }

    public static bool GetIsDragSource(Control element) => element.GetValue(IsDragSourceProperty);
    public static void SetIsDragSource(Control element, bool value) => element.SetValue(IsDragSourceProperty, value);
    public static bool GetIsDropTarget(Control element) => element.GetValue(IsDropTargetProperty);
    public static void SetIsDropTarget(Control element, bool value) => element.SetValue(IsDropTargetProperty, value);
    public static IDragSource? GetDragHandler(Control element) => element.GetValue(DragHandlerProperty);
    public static void SetDragHandler(Control element, IDragSource? value) => element.SetValue(DragHandlerProperty, value);
    public static IDropTarget? GetDropHandler(Control element) => element.GetValue(DropHandlerProperty);
    public static void SetDropHandler(Control element, IDropTarget? value) => element.SetValue(DropHandlerProperty, value);
    public static string? GetDragDropContext(Control element) => element.GetValue(DragDropContextProperty);
    public static void SetDragDropContext(Control element, string? value) => element.SetValue(DragDropContextProperty, value);
    public static IDragInfoBuilder? GetDragInfoBuilder(Control element) => element.GetValue(DragInfoBuilderProperty);
    public static void SetDragInfoBuilder(Control element, IDragInfoBuilder? value) => element.SetValue(DragInfoBuilderProperty, value);
    public static IDropInfoBuilder? GetDropInfoBuilder(Control element) => element.GetValue(DropInfoBuilderProperty);
    public static void SetDropInfoBuilder(Control element, IDropInfoBuilder? value) => element.SetValue(DropInfoBuilderProperty, value);
    public static bool GetSelectDroppedItems(Control element) => element.GetValue(SelectDroppedItemsProperty);
    public static void SetSelectDroppedItems(Control element, bool value) => element.SetValue(SelectDroppedItemsProperty, value);
    public static ScrollingMode GetScrollingMode(Control element) => element.GetValue(ScrollingModeProperty);
    public static void SetScrollingMode(Control element, ScrollingMode value) => element.SetValue(ScrollingModeProperty, value);

    private static void OnIsDragSourceChanged(Control control, AvaloniaPropertyChangedEventArgs args)
    {
        control.RemoveHandler(InputElement.PointerPressedEvent, OnPointerPressed);
        control.RemoveHandler(InputElement.PointerMovedEvent, OnPointerMoved);
        control.RemoveHandler(InputElement.PointerReleasedEvent, OnPointerReleased);

        if (args.GetNewValue<bool>())
        {
            control.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
            control.AddHandler(InputElement.PointerMovedEvent, OnPointerMoved, RoutingStrategies.Tunnel);
            control.AddHandler(InputElement.PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Tunnel);
        }
    }

    private static void OnIsDropTargetChanged(Control control, AvaloniaPropertyChangedEventArgs args)
    {
        global::Avalonia.Input.DragDrop.SetAllowDrop(control, args.GetNewValue<bool>());
        control.RemoveHandler(global::Avalonia.Input.DragDrop.DragEnterEvent, OnDragEnter);
        control.RemoveHandler(global::Avalonia.Input.DragDrop.DragOverEvent, OnDragOver);
        control.RemoveHandler(global::Avalonia.Input.DragDrop.DragLeaveEvent, OnDragLeave);
        control.RemoveHandler(global::Avalonia.Input.DragDrop.DropEvent, OnDrop);

        if (args.GetNewValue<bool>())
        {
            control.AddHandler(global::Avalonia.Input.DragDrop.DragEnterEvent, OnDragEnter);
            control.AddHandler(global::Avalonia.Input.DragDrop.DragOverEvent, OnDragOver);
            control.AddHandler(global::Avalonia.Input.DragDrop.DragLeaveEvent, OnDragLeave);
            control.AddHandler(global::Avalonia.Input.DragDrop.DropEvent, OnDrop);
        }
    }

    private static void OnPointerPressed(object? sender, PointerPressedEventArgs args)
    {
        if (sender is not Control control || !args.GetCurrentPoint(control).Properties.IsLeftButtonPressed)
        {
            return;
        }

        activeDragInfo = GetDragInfoBuilder(control)?.CreateDragInfo(control, args)
                 ?? new DragInfo(control, args);
        triggerEvent = args;
    }

    private static async void OnPointerMoved(object? sender, PointerEventArgs args)
    {
        if (sender is not Control control || activeDragInfo is null || triggerEvent is null || dragInProgress)
        {
            return;
        }

        var position = args.GetPosition(control);
        var delta = position - activeDragInfo.DragStartPosition;
        if (Math.Abs(delta.X) < 4 && Math.Abs(delta.Y) < 4)
        {
            return;
        }

        var dragInfo = activeDragInfo;
        var handler = GetDragHandler(control) ?? DefaultDragHandler;
        if (!handler.CanStartDrag(dragInfo))
        {
            return;
        }

        handler.StartDrag(dragInfo);
        if (dragInfo.Effects == DragDropEffects.None || dragInfo.Data is null && dragInfo.DataTransfer is null)
        {
            return;
        }

        var transfer = dragInfo.DataTransfer;
        if (transfer is null)
        {
            var item = new DataTransferItem();
            item.Set(GongDataFormat, dragInfo.Data);
            var defaultTransfer = new DataTransfer();
            defaultTransfer.Add(item);
            transfer = defaultTransfer;
        }

        try
        {
            dragInProgress = true;
            var result = await global::Avalonia.Input.DragDrop.DoDragDropAsync(triggerEvent, transfer, dragInfo.Effects);
            if (result == DragDropEffects.None)
            {
                handler.DragCancelled();
            }

            handler.DragDropOperationFinished(result, dragInfo);
        }
        catch (Exception exception) when (handler.TryCatchOccurredException(exception))
        {
        }
        finally
        {
            CancelAutoExpand();
            ClearDropAdorner();
            dragInProgress = false;
            activeDragInfo = null;
            triggerEvent = null;
        }
    }

    private static void OnPointerReleased(object? sender, PointerReleasedEventArgs args)
    {
        if (!dragInProgress)
        {
            activeDragInfo = null;
            triggerEvent = null;
        }
    }

    private static void OnDragOver(object? sender, DragEventArgs args)
    {
        if (sender is not Control control)
        {
            return;
        }

        var dropInfo = CreateDropInfo(control, args);
        ShouldCopy = args.KeyModifiers.HasFlag(KeyModifiers.Control);
        (GetDropHandler(control) ?? DefaultDropHandler).DragOver(dropInfo);
        Scroll(dropInfo, args);
        UpdateAutoExpand(dropInfo);
        UpdateDropAdorner(dropInfo);
        args.DragEffects = dropInfo.Effects;
        args.Handled = true;
    }

    private static void OnDragEnter(object? sender, DragEventArgs args)
    {
        if (sender is not Control control)
        {
            return;
        }

        var dropInfo = CreateDropInfo(control, args);
        ShouldCopy = args.KeyModifiers.HasFlag(KeyModifiers.Control);
        var dropHandler = GetDropHandler(control) ?? DefaultDropHandler;
        dropHandler.DragEnter(dropInfo);
        dropHandler.DragOver(dropInfo);
        UpdateAutoExpand(dropInfo);
        UpdateDropAdorner(dropInfo);
        args.DragEffects = dropInfo.Effects;
        args.Handled = true;
    }

    private static void OnDragLeave(object? sender, DragEventArgs args)
    {
        if (sender is not Control control)
        {
            return;
        }

        (GetDropHandler(control) ?? DefaultDropHandler).DragLeave(CreateDropInfo(control, args));
        CancelAutoExpand();
        ClearDropAdorner();
        args.Handled = true;
    }

    private static void OnDrop(object? sender, DragEventArgs args)
    {
        if (sender is not Control control)
        {
            return;
        }

        var dropInfo = CreateDropInfo(control, args);
        ShouldCopy = args.KeyModifiers.HasFlag(KeyModifiers.Control);
        var dropHandler = GetDropHandler(control) ?? DefaultDropHandler;
        dropHandler.DragOver(dropInfo);
        dropHandler.Drop(dropInfo);
        (activeDragInfo is null ? DefaultDragHandler : GetDragHandler(activeDragInfo.VisualSource) ?? DefaultDragHandler).Dropped(dropInfo);
        args.DragEffects = dropInfo.Effects;
        args.Handled = true;
        CancelAutoExpand();
        ClearDropAdorner();
    }

    private static IDropInfo CreateDropInfo(Control control, DragEventArgs args)
    {
        return GetDropInfoBuilder(control)?.CreateDropInfo(control, args, activeDragInfo)
               ?? new DropInfo(control, args, activeDragInfo);
    }

    private static void Scroll(IDropInfo dropInfo, DragEventArgs args)
    {
        if (dropInfo.TargetScrollViewer is not { } scrollViewer)
        {
            return;
        }

        var position = args.GetPosition(scrollViewer);
        var scrollingMode = GetScrollingMode(dropInfo.VisualTarget);
        var verticalMargin = Math.Min(32, scrollViewer.Viewport.Height / 2);
        var horizontalMargin = Math.Min(32, scrollViewer.Viewport.Width / 2);

        if (scrollingMode is ScrollingMode.VerticalOnly or ScrollingMode.Both && position.Y < verticalMargin)
        {
            scrollViewer.LineUp();
        }
        else if (scrollingMode is ScrollingMode.VerticalOnly or ScrollingMode.Both
                 && position.Y > scrollViewer.Viewport.Height - verticalMargin)
        {
            scrollViewer.LineDown();
        }

        if (scrollingMode is ScrollingMode.HorizontalOnly or ScrollingMode.Both && position.X < horizontalMargin)
        {
            scrollViewer.LineLeft();
        }
        else if (scrollingMode is ScrollingMode.HorizontalOnly or ScrollingMode.Both
                 && position.X > scrollViewer.Viewport.Width - horizontalMargin)
        {
            scrollViewer.LineRight();
        }
    }

    private static void UpdateAutoExpand(IDropInfo dropInfo)
    {
        if (dropInfo.Effects == DragDropEffects.None
            || !dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.TargetItemCenter)
            || dropInfo.VisualTargetItem is not TreeViewItem { IsExpanded: false, ItemCount: > 0 } target)
        {
            CancelAutoExpand();
            return;
        }

        if (ReferenceEquals(target, pendingExpandTarget))
        {
            return;
        }

        CancelAutoExpand();
        pendingExpandTarget = target;
        expandCancellation = new CancellationTokenSource();
        _ = ExpandAfterDelayAsync(target, expandCancellation.Token);
    }

    private static async Task ExpandAfterDelayAsync(TreeViewItem target, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(600, cancellationToken);
            Dispatcher.UIThread.Post(() =>
            {
                if (!cancellationToken.IsCancellationRequested && ReferenceEquals(target, pendingExpandTarget))
                {
                    target.IsExpanded = true;
                    CancelAutoExpand();
                }
            });
        }
        catch (OperationCanceledException)
        {
        }
    }

    private static void CancelAutoExpand()
    {
        expandCancellation?.Cancel();
        expandCancellation?.Dispose();
        expandCancellation = null;
        pendingExpandTarget = null;
    }

    private static void UpdateDropAdorner(IDropInfo dropInfo)
    {
        if (dropInfo.Effects == DragDropEffects.None)
        {
            ClearDropAdorner();
            return;
        }

        if (!ReferenceEquals(adornedTarget, dropInfo.VisualTarget))
        {
            ClearDropAdorner();
            adornedTarget = dropInfo.VisualTarget;
            dropTargetAdorner = new DropTargetAdorner();
            AdornerLayer.SetAdorner(adornedTarget, dropTargetAdorner);
        }

        dropTargetAdorner?.Update(dropInfo);
    }

    private static void ClearDropAdorner()
    {
        if (adornedTarget is not null)
        {
            AdornerLayer.SetAdorner(adornedTarget, null);
        }

        adornedTarget = null;
        dropTargetAdorner = null;
    }
}