using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Showcase.Avalonia.DragDrop;

public sealed class TreeNodeModel : ICloneable
{
    public TreeNodeModel(string caption, params TreeNodeModel[] children)
    {
        Caption = caption;
        Children = new ObservableCollection<TreeNodeModel>(children);
    }

    public string Caption { get; }
    public ObservableCollection<TreeNodeModel> Children { get; }

    public object Clone()
    {
        return new TreeNodeModel(Caption, Children.Select(child => (TreeNodeModel)child.Clone()).ToArray());
    }
}