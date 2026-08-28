using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
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

        if (!copy && dropInfo.DragInfo?.SourceCollection is IList source)
        {
            foreach (var item in data)
            {
                var sourceIndex = source.IndexOf(item);
                if (sourceIndex < 0)
                {
                    continue;
                }

                source.RemoveAt(sourceIndex);
                if (ReferenceEquals(source, destination) && sourceIndex < insertIndex)
                {
                    insertIndex--;
                }
            }
        }

        foreach (var item in data)
        {
            var itemToInsert = copy && item is ICloneable cloneable
                ? cloneable.Clone()
                : item;
            destination.Insert(insertIndex++, itemToInsert);
        }
    }

    public static bool CanAcceptData(IDropInfo dropInfo)
    {
        if (dropInfo.DragInfo is null
            || dropInfo.Data is null
            || dropInfo.TargetCollection is not IList
            || !dropInfo.IsSameDragDropContextAsSource)
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
                && ReferenceEquals(dropInfo.TargetCollection, targetItem.ItemsSource))
            {
                return false;
            }
        }

        return true;
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