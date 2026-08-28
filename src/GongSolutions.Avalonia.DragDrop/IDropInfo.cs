using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;

namespace GongSolutions.Avalonia.DragDrop;

public interface IDropInfo
{
    bool AcceptChildItem { get; set; }
    object? Data { get; set; }
    IDataTransfer DataTransfer { get; }
    IDragInfo? DragInfo { get; }
    Point DropPosition { get; }
    DragDropEffects Effects { get; set; }
    bool IsHorizontal { get; }
    int InsertIndex { get; }
    RelativeInsertPosition InsertPosition { get; }
    IEnumerable? TargetCollection { get; }
    object? TargetItem { get; }
    ScrollViewer? TargetScrollViewer { get; }
    Control VisualTarget { get; }
    Control? VisualTargetItem { get; }
    bool IsSameDragDropContextAsSource { get; }
}