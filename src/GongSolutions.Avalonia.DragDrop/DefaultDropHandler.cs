using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace GongSolutions.Avalonia.DragDrop;

public class DefaultDropHandler : IDropTarget
{
    public virtual void DragOver(IDropInfo dropInfo)
    {
        dropInfo.Effects = CanAcceptData(dropInfo)
            ? DragDrop.ShouldCopy ? DragDropEffects.Copy : DragDropEffects.Move
            : DragDropEffects.None;
    }

    public virtual void Drop(IDropInfo dropInfo)
    {
        if (!CanAcceptData(dropInfo) || dropInfo.TargetCollection is not IList destination)
        {
            return;
        }

        var data = ExtractData(dropInfo.Data).ToList();
        var insertIndex = Math.Clamp(dropInfo.InsertIndex, 0, destination.Count);
        var copy = dropInfo.Effects.HasFlag(DragDropEffects.Copy);
        var insertedItems = new List<object>();

        if (!copy && dropInfo.DragInfo?.SourceCollection is IList source)
        {
            if (ReferenceEquals(source, destination))
            {
                foreach (var item in data)
                {
                    var sourceIndex = source.IndexOf(item);
                    if (sourceIndex < 0)
                    {
                        continue;
                    }

                    var destinationIndex = Math.Clamp(insertIndex, 0, destination.Count);
                    if (sourceIndex < destinationIndex)
                    {
                        destinationIndex--;
                    }

                    Move(source, sourceIndex, destinationIndex);
                    insertedItems.Add(item);
                    insertIndex = destinationIndex + 1;
                }

                SelectDroppedItems(dropInfo, insertedItems);
                return;
            }

            foreach (var item in data)
            {
                var sourceIndex = source.IndexOf(item);
                if (sourceIndex < 0)
                {
                    continue;
                }

                source.RemoveAt(sourceIndex);
            }
        }

        foreach (var item in data)
        {
            var itemToInsert = copy && item is ICloneable cloneable
                ? cloneable.Clone()
                : item;
            destination.Insert(insertIndex++, itemToInsert);
            insertedItems.Add(itemToInsert);
        }

        SelectDroppedItems(dropInfo, insertedItems);
    }

    public static bool CanAcceptData(IDropInfo dropInfo)
    {
        if (dropInfo.DragInfo is null
            || dropInfo.Data is null
            || dropInfo.TargetCollection is not IList { IsReadOnly: false, IsFixedSize: false }
            || !dropInfo.IsSameDragDropContextAsSource)
        {
            return false;
        }

        if (!DragDrop.ShouldCopy
            && dropInfo.DragInfo.SourceCollection is IList source
            && (source.IsReadOnly || source.IsFixedSize))
        {
            return false;
        }

        if (dropInfo.DragInfo.VisualSourceItem is TreeViewItem sourceItem
            && dropInfo.VisualTargetItem is TreeViewItem targetItem)
        {
            if (targetItem.GetVisualAncestors().Contains(sourceItem))
            {
                return false;
            }

            if (ReferenceEquals(sourceItem, targetItem)
                && dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.TargetItemCenter))
            {
                return false;
            }
        }

        var targetType = GetCollectionElementType(dropInfo.TargetCollection.GetType());
         var data = ExtractData(dropInfo.Data).ToList();
         return (!DragDrop.ShouldCopy || data.All(item => item is not Control || item is ICloneable))
               && (targetType is null || data.All(item => IsCompatible(item, targetType)));
    }

    private static bool IsCompatible(object? item, Type targetType)
    {
        return item is null
            ? !targetType.IsValueType || Nullable.GetUnderlyingType(targetType) is not null
            : targetType.IsInstanceOfType(item);
    }

    private static Type? GetCollectionElementType(Type collectionType)
    {
        return collectionType.GetInterfaces()
            .Concat(new[] { collectionType })
            .FirstOrDefault(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IList<>))
            ?.GetGenericArguments()[0];
    }

    private static void Move(IList collection, int sourceIndex, int destinationIndex)
    {
        if (sourceIndex == destinationIndex)
        {
            return;
        }

        var moveMethod = collection.GetType().GetMethod("Move", new[] { typeof(int), typeof(int) });
        if (moveMethod is not null)
        {
            moveMethod.Invoke(collection, new object[] { sourceIndex, destinationIndex });
            return;
        }

        var item = collection[sourceIndex];
        collection.RemoveAt(sourceIndex);
        collection.Insert(destinationIndex, item);
    }

    private static void SelectDroppedItems(IDropInfo dropInfo, IReadOnlyList<object> items)
    {
        if (!DragDrop.GetSelectDroppedItems(dropInfo.VisualTarget) || items.Count == 0)
        {
            return;
        }

        var itemsParent = dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.TargetItemCenter)
            ? dropInfo.VisualTargetItem as ItemsControl
            : dropInfo.VisualTargetItem is null
                ? dropInfo.VisualTarget as ItemsControl
                : ItemsControl.ItemsControlFromItemContainer(dropInfo.VisualTargetItem);

        if (itemsParent is null)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (itemsParent is ListBox listBox)
            {
                listBox.SelectedItems?.Clear();
                foreach (var item in items)
                {
                    listBox.SelectedItems?.Add(item);
                    listBox.ScrollIntoView(item);
                }

                return;
            }

            foreach (var item in items)
            {
                if (itemsParent.ContainerFromItem(item) is TreeViewItem treeViewItem)
                {
                    treeViewItem.IsSelected = true;
                    treeViewItem.BringIntoView();
                }
            }
        });
    }

    public static IEnumerable<object> ExtractData(object? data)
    {
        return data switch
        {
            null => Array.Empty<object>(),
            string value => new object[] { value },
            IEnumerable values => values.Cast<object>(),
            _ => new[] { data }
        };
    }
}