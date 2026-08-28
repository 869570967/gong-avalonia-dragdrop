using System.Collections;
using Avalonia.Controls;
using Avalonia.Input;

namespace GongSolutions.Avalonia.DragDrop;

public interface IDropInfo
{
    object? Data { get; set; }
    IDragInfo? DragInfo { get; }
    DragDropEffects Effects { get; set; }
    int InsertIndex { get; }
    IEnumerable? TargetCollection { get; }
    Control VisualTarget { get; }
    Control? VisualTargetItem { get; }
    bool IsSameDragDropContextAsSource { get; }
}