using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace GongSolutions.Avalonia.DragDrop;

internal static class ItemsControlDragDropHelper
{
    public static IEnumerable GetItems(ItemsControl itemsControl)
    {
        return itemsControl.ItemsSource as IEnumerable ?? itemsControl.Items;
    }

    public static Control? FindContainer(ItemsControl itemsControl, Visual? source)
    {
        if (source is null)
        {
            return null;
        }

        var visuals = new[] { source }.Concat(source.GetVisualAncestors());
        foreach (var visual in visuals)
        {
            if (visual is not Control control)
            {
                continue;
            }

            var owner = ItemsControl.ItemsControlFromItemContainer(control);
            if (ReferenceEquals(owner, itemsControl))
            {
                return control;
            }

            if (ReferenceEquals(control, itemsControl))
            {
                break;
            }
        }

        return null;
    }

    public static DataGridRow? FindDataGridRow(Visual? source)
    {
        return source?.FindAncestorOfType<DataGridRow>(true);
    }

    public static DataGridRow? FindClosestDataGridRow(DataGrid dataGrid, Point position)
    {
        return dataGrid.GetVisualDescendants()
            .OfType<DataGridRow>()
            .OrderBy(row => DistanceFromCenter(row, dataGrid, position, Orientation.Vertical))
            .FirstOrDefault();
    }

    public static Control? FindClosestContainer(ItemsControl itemsControl, Point position, Orientation orientation)
    {
        return itemsControl.GetVisualDescendants()
            .OfType<Control>()
            .Where(control => ReferenceEquals(ItemsControl.ItemsControlFromItemContainer(control), itemsControl))
            .OrderBy(control => DistanceFromCenter(control, itemsControl, position, orientation))
            .FirstOrDefault();
    }

    public static bool IsDataGridHeader(Visual? source)
    {
        return source?.FindAncestorOfType<DataGridColumnHeader>(true) is not null;
    }

    public static bool IsTabHeader(Visual? source, TabItem tabItem)
    {
        return tabItem.HeaderPresenter is { } header
               && (ReferenceEquals(source, header) || source?.GetVisualAncestors().Contains(header) == true);
    }

    public static IReadOnlyList<object> GetSelectedItems(ItemsControl itemsControl, object clickedItem)
    {
        if (itemsControl is ListBox listBox)
        {
            var selectedItems = listBox.SelectedItems?.Cast<object>().ToList() ?? new List<object>();
            return selectedItems.Contains(clickedItem) ? selectedItems : new[] { clickedItem };
        }

        return new[] { clickedItem };
    }

    public static IReadOnlyList<object> GetSelectedItems(DataGrid dataGrid, object clickedItem)
    {
        var selectedItems = dataGrid.SelectedItems.Cast<object>().ToList();
        return selectedItems.Contains(clickedItem) ? selectedItems : new[] { clickedItem };
    }

    public static Orientation GetOrientation(ItemsControl itemsControl)
    {
        if (itemsControl is TabControl)
        {
            return Orientation.Horizontal;
        }

        return itemsControl.FindDescendantOfType<ItemsPresenter>()?.Panel switch
        {
            StackPanel panel => panel.Orientation,
            VirtualizingStackPanel panel => panel.Orientation,
            WrapPanel panel => panel.Orientation,
            _ => Orientation.Vertical
        };
    }

    public static int IndexOf(IEnumerable collection, object item)
    {
        return collection is IList list
            ? list.IndexOf(item)
            : collection.Cast<object>().ToList().IndexOf(item);
    }

    private static double DistanceFromCenter(Control control, Visual relativeTo, Point position, Orientation orientation)
    {
        if (control.TransformToVisual(relativeTo) is not { } transform)
        {
            return double.MaxValue;
        }

        var origin = transform.Transform(default);
        var center = orientation == Orientation.Horizontal
            ? origin.X + control.Bounds.Width / 2
            : origin.Y + control.Bounds.Height / 2;
        var coordinate = orientation == Orientation.Horizontal ? position.X : position.Y;
        return Math.Abs(center - coordinate);
    }
}