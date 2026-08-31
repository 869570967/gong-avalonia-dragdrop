# GongSolutions.Avalonia.DragDrop

An Avalonia drag-and-drop library based on
[gong-wpf-dragdrop](https://github.com/punker76/gong-wpf-dragdrop).

The core source/target workflow follows the WPF library, but the public API is
adapted to Avalonia and is not a drop-in replacement for every WPF property or
extension interface.

## Setup

Add the namespace to an Avalonia view:

```xml
xmlns:dd="using:GongSolutions.Avalonia.DragDrop"
```

Enable a control as a source or target:

```xml
<ListBox ItemsSource="{Binding Items}"
		 dd:DragDrop.IsDragSource="True"
		 dd:DragDrop.IsDropTarget="True" />
```

The default handlers support moving, copying, and reordering items in mutable
`IList` collections. `ListBox` and `DataGrid` selections, `TreeView` child
drops, `TabControl`, horizontal panels, RTL panels, automatic scrolling, and
tree auto-expansion are supported.

## Configuration

The main attached properties include:

- `DragHandler`, `DropHandler`, `DragInfoBuilder`, and `DropInfoBuilder`
- `DragDropContext` and `SelectDroppedItems`
- `ScrollingMode`
- `MinimumHorizontalDragDistance` and `MinimumVerticalDragDistance`
- `CanDragWithMouseRightButton` and `DragSourceIgnore`
- `DragDropCopyKeyModifiers`
- `DragPreviewTemplate`, `EffectPreviewTemplate`, and `DropHintText`
- `DropTargetAdornerBrush` and `DropTargetAdornerFactory`
- `DropTargetItemsSorter`, `DropIndexResolver`, and `DropGroupResolver`

Text boxes, buttons, sliders, scroll bars, combo boxes, and menu items inside an
item template do not start a drag by default. Set `DragSourceIgnore` on any
additional interactive region that must retain pointer input.

Implement `IDropTargetAdornerFactory` and `IDropTargetAdorner` to replace the
complete drop visual. Templates and the adorner brush can be used when only the
default preview content or insertion styling needs to change.

## External Drops

External file and text payloads are read from Avalonia's `IDataTransfer` and
copied into a compatible mutable target collection. Other formats remain
available through `IDropInfo.DataTransfer` and can be handled by a custom
`IDropTarget`.

## Grouped And Filtered Views

Avalonia collection views do not expose one universal mapping from a visible
index to a source index. Use `IDropIndexResolver` to map `InsertIndex` to
`UnfilteredInsertIndex`, and `IDropGroupResolver` to expose the destination
group through `IDropInfo.TargetGroup`. This keeps grouped and filtered behavior
explicit when views contain sorting, duplicate values, or projected items.

Use `IDropTargetItemsSorter` when dragged items must be ordered before they are
inserted.

## WPF Compatibility

Existing WPF concepts such as drag/drop handlers, context isolation, insertion
positions, and default collection updates have Avalonia equivalents. WPF-only
APIs based on `System.Windows.Adorner`, `Popup`, `ICollectionView`, routed event
selection, or WPF data templates cannot be reused directly. Use the Avalonia
interfaces and attached properties described above when porting those features.

## Build And Test

```powershell
dotnet build src/GongSolutions.Avalonia.DragDrop.sln
dotnet test src/GongSolutions.Avalonia.DragDrop.sln
```

The Cake `Default`, `package`, and `ci` targets also run the automated test
suite.
