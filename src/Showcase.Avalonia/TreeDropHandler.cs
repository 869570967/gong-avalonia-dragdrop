using GongSolutions.Avalonia.DragDrop;

namespace Showcase.Avalonia.DragDrop;

public sealed class TreeDropHandler : DefaultDropHandler
{
    public override void DragOver(IDropInfo dropInfo)
    {
        dropInfo.AcceptChildItem = dropInfo.TargetItem is not TreeNodeModel { CanAcceptChildren: false };
        base.DragOver(dropInfo);
    }
}