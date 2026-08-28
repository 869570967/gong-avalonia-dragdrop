using Avalonia.Controls;
using Avalonia.Input;

namespace GongSolutions.Avalonia.DragDrop;

public interface IDragInfoBuilder
{
    IDragInfo CreateDragInfo(Control source, PointerPressedEventArgs eventArgs);
}