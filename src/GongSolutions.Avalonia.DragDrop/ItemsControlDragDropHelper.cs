using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace GongSolutions.Avalonia.DragDrop;

internal static class ItemsControlDragDropHelper
{
    private static readonly ConditionalWeakTable<ItemsControl, ContainerCache> ContainerCaches = new();
    private static readonly ConditionalWeakTable<DataGrid, DataGridRowCache> DataGridRowCaches = new();

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
        return FindClosest(GetRealizedRows(dataGrid), dataGrid, position, Orientation.Vertical);
    }

    public static Control? FindClosestContainer(ItemsControl itemsControl, Point position, Orientation orientation)
    {
        return FindClosest(GetRealizedContainers(itemsControl), itemsControl, position, orientation);
    }

    public static bool IsDataGridHeader(Visual? source)
    {
        return source?.FindAncestorOfType<DataGridColumnHeader>(true) is not null;
    }

    public static bool IsTabHeader(Visual? source, TabItem tabItem)
    {
         return ReferenceEquals(source, tabItem)
             || source?.GetVisualAncestors().Contains(tabItem) == true;
    }

    public static Control? GetTreeViewItemHeader(TreeViewItem treeViewItem)
    {
        var headerPresenter = treeViewItem.HeaderPresenter;
        return headerPresenter?.GetVisualParent() as Control ?? headerPresenter;
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

    private static IReadOnlyList<Control> GetRealizedContainers(ItemsControl itemsControl)
    {
        var cache = ContainerCaches.GetValue(itemsControl, static _ => new ContainerCache());
        if (cache.ItemCount != itemsControl.ItemCount
            || cache.Controls.Any(control => !ReferenceEquals(ItemsControl.ItemsControlFromItemContainer(control), itemsControl)))
        {
            cache.ItemCount = itemsControl.ItemCount;
            cache.Controls = itemsControl.GetVisualDescendants()
                .OfType<Control>()
                .Where(control => ReferenceEquals(ItemsControl.ItemsControlFromItemContainer(control), itemsControl))
                .ToArray();
        }

        return cache.Controls;
    }

    private static T? FindClosest<T>(IEnumerable<T> controls, Visual relativeTo, Point position, Orientation orientation)
        where T : Control
    {
        T? closest = null;
        var closestDistance = double.MaxValue;
        foreach (var control in controls)
        {
            var distance = DistanceFromCenter(control, relativeTo, position, orientation);
            if (distance < closestDistance)
            {
                closest = control;
                closestDistance = distance;
            }
        }

        return closest;
    }

    private static IReadOnlyList<DataGridRow> GetRealizedRows(DataGrid dataGrid)
    {
        var cache = DataGridRowCaches.GetValue(dataGrid, static _ => new DataGridRowCache());
        var itemCount = dataGrid.ItemsSource?.Cast<object>().Count() ?? 0;
        if (cache.ItemCount != itemCount
            || cache.Rows.Any(row => !row.GetVisualAncestors().Contains(dataGrid)))
        {
            cache.ItemCount = itemCount;
            cache.Rows = dataGrid.GetVisualDescendants().OfType<DataGridRow>().ToArray();
        }

        return cache.Rows;
    }

    private sealed class ContainerCache
    {
        public int ItemCount { get; set; } = -1;
        public IReadOnlyList<Control> Controls { get; set; } = Array.Empty<Control>();
    }

    private sealed class DataGridRowCache
    {
        public int ItemCount { get; set; } = -1;
        public IReadOnlyList<DataGridRow> Rows { get; set; } = Array.Empty<DataGridRow>();
    }
}