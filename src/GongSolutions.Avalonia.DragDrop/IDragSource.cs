using System;
using Avalonia.Input;

namespace GongSolutions.Avalonia.DragDrop;

public interface IDragSource
{
    void StartDrag(IDragInfo dragInfo);
    bool CanStartDrag(IDragInfo dragInfo);
    void Dropped(IDropInfo dropInfo);
    void DragDropOperationFinished(DragDropEffects operationResult, IDragInfo dragInfo);
    void DragCancelled();
    bool TryCatchOccurredException(Exception exception);
}