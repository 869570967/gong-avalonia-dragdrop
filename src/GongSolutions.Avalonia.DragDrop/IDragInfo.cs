using System.Collections;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace GongSolutions.Avalonia.DragDrop;

public interface IDragInfo
{
    object? Data { get; set; }
    DragDropEffects Effects { get; set; }
    Point DragStartPosition { get; }
    IEnumerable SourceCollection { get; }
    IReadOnlyList<object> SourceItems { get; }
    Control VisualSource { get; }
    Control? VisualSourceItem { get; }
}