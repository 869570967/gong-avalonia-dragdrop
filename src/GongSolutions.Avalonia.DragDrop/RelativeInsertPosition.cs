using System;

namespace GongSolutions.Avalonia.DragDrop;

[Flags]
public enum RelativeInsertPosition
{
    None = 0,
    BeforeTargetItem = 1,
    TargetItemCenter = 2,
    AfterTargetItem = 4
}