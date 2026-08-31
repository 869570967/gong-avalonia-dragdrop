using Avalonia.Controls;

namespace GongSolutions.Avalonia.DragDrop;

public interface IDropTargetAdorner
{
    Control Visual { get; }
    void Update(IDropInfo dropInfo);
}

public interface IDropTargetAdornerFactory
{
    IDropTargetAdorner Create(Control target);
}
