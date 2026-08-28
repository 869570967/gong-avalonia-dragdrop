using Avalonia.Controls;
using Avalonia.Input;

namespace GongSolutions.Avalonia.DragDrop;

public interface IDropInfoBuilder
{
    IDropInfo CreateDropInfo(Control target, DragEventArgs eventArgs, IDragInfo? dragInfo);
}