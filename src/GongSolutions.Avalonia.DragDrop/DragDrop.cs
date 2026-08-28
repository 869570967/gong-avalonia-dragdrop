using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;

namespace GongSolutions.Avalonia.DragDrop;

public static class DragDrop
{
    private static readonly DataFormat<object> GongDataFormat = global::Avalonia.Input.DataFormat.CreateInProcessFormat<object>("GongSolutions.Avalonia.DragDrop");
    private static DragInfo? activeDragInfo;
    private static PointerPressedEventArgs? triggerEvent;
    private static bool dragInProgress;

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
        control.RemoveHandler(global::Avalonia.Input.DragDrop.DragOverEvent, OnDragOver);
        control.RemoveHandler(global::Avalonia.Input.DragDrop.DropEvent, OnDrop);

        if (args.GetNewValue<bool>())
        {
            control.AddHandler(global::Avalonia.Input.DragDrop.DragOverEvent, OnDragOver);
            control.AddHandler(global::Avalonia.Input.DragDrop.DropEvent, OnDrop);
        }
    }

    private static void OnPointerPressed(object? sender, PointerPressedEventArgs args)
    {
        if (sender is not Control control || !args.GetCurrentPoint(control).Properties.IsLeftButtonPressed)
        {
            return;
        }

        activeDragInfo = new DragInfo(control, args);
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
        if (dragInfo.Effects == DragDropEffects.None || dragInfo.Data is null)
        {
            return;
        }

        var item = new DataTransferItem();
        item.Set(GongDataFormat, dragInfo.Data);
        var transfer = new DataTransfer();
        transfer.Add(item);

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

        var dropInfo = new DropInfo(control, args, activeDragInfo);
        ShouldCopy = args.KeyModifiers.HasFlag(KeyModifiers.Control);
        (GetDropHandler(control) ?? DefaultDropHandler).DragOver(dropInfo);
        args.DragEffects = dropInfo.Effects;
        args.Handled = true;
    }

    private static void OnDrop(object? sender, DragEventArgs args)
    {
        if (sender is not Control control)
        {
            return;
        }

        var dropInfo = new DropInfo(control, args, activeDragInfo);
        var dropHandler = GetDropHandler(control) ?? DefaultDropHandler;
        dropHandler.DragOver(dropInfo);
        dropHandler.Drop(dropInfo);
        (activeDragInfo is null ? DefaultDragHandler : GetDragHandler(activeDragInfo.VisualSource) ?? DefaultDragHandler).Dropped(dropInfo);
        args.DragEffects = dropInfo.Effects;
        args.Handled = true;
    }
}