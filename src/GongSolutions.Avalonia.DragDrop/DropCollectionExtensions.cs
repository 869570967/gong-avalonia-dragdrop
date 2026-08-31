using System.Collections;

namespace GongSolutions.Avalonia.DragDrop;

public interface IDropTargetItemsSorter
{
    IEnumerable SortDropTargetItems(IEnumerable items);
}

public interface IDropIndexResolver
{
    int ResolveSourceInsertIndex(IDropInfo dropInfo);
}

public interface IDropGroupResolver
{
    object? ResolveTargetGroup(IDropInfo dropInfo);
}
