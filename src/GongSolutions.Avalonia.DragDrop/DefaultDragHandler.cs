using System;
using System.Collections;
using System.Linq;
using Avalonia.Input;

namespace GongSolutions.Avalonia.DragDrop;

public class DefaultDragHandler : IDragSource
{
    public virtual void StartDrag(IDragInfo dragInfo)
    {
        var singleItem = dragInfo.SourceItems.Count == 1 ? dragInfo.SourceItems[0] : null;
        dragInfo.Data = singleItem is IEnumerable and not string
            ? new[] { singleItem }
            : singleItem ?? dragInfo.SourceItems.ToList();
        dragInfo.Effects = dragInfo.Data is null
            ? DragDropEffects.None
            : DragDropEffects.Copy | DragDropEffects.Move;
    }

    public virtual bool CanStartDrag(IDragInfo dragInfo) => dragInfo.SourceItems.Count > 0;
    public virtual void Dropped(IDropInfo dropInfo) { }
    public virtual void DragDropOperationFinished(DragDropEffects operationResult, IDragInfo dragInfo) { }
    public virtual void DragCancelled() { }
    public virtual bool TryCatchOccurredException(Exception exception) => false;
}