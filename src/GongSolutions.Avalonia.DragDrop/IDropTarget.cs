namespace GongSolutions.Avalonia.DragDrop;

public interface IDropTarget
{
    void DragEnter(IDropInfo dropInfo) { }
    void DragOver(IDropInfo dropInfo);
    void DragLeave(IDropInfo dropInfo) { }
    void Drop(IDropInfo dropInfo);
}