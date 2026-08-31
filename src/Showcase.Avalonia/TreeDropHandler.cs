using Avalonia.Controls;
using GongSolutions.Avalonia.DragDrop;

namespace Showcase.Avalonia.DragDrop;

public sealed class TreeDropHandler : DefaultDropHandler
{
    public override void DragOver(IDropInfo dropInfo)
    {
        dropInfo.AcceptChildItem = dropInfo.TargetItem switch
        {
            TreeNodeModel { CanAcceptChildren: false } => false,
            TreeViewItem { ItemCount: 0 } => false,
            _ => true
        };
        base.DragOver(dropInfo);
    }
}