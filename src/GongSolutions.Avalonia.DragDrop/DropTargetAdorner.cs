using System;
using System.Collections;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;

namespace GongSolutions.Avalonia.DragDrop;

public sealed class DropTargetAdorner : Panel, IDropTargetAdorner
{
    private static readonly IBrush PreviewBackground = new SolidColorBrush(Color.FromArgb(224, 31, 41, 55));
    private readonly ContentControl dataPreview;
    private readonly ContentControl effectPreview;
    private readonly TextBlock hintText;
    private readonly Border preview;
    private readonly DropIndicator indicator;
    private Control? defaultDataPreview;
    private IDropInfo? dropInfo;

    public DropTargetAdorner(Control target)
    {
        IsHitTestVisible = false;
        indicator = new DropIndicator(DragDrop.GetDropTargetAdornerBrush(target));
        dataPreview = new ContentControl
        {
            ContentTemplate = DragDrop.GetDragPreviewTemplate(target)
        };
        effectPreview = new ContentControl
        {
            ContentTemplate = DragDrop.GetEffectPreviewTemplate(target)
        };
        hintText = new TextBlock
        {
            Text = DragDrop.GetDropHintText(target),
            Foreground = Brushes.White,
            FontSize = 11
        };
        preview = new Border
        {
            Background = PreviewBackground,
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(8, 5),
            Child = new StackPanel
            {
                Spacing = 2,
                Children =
                {
                    dataPreview,
                    effectPreview,
                    hintText
                }
            }
        };
        Children.Add(indicator);
        Children.Add(preview);
    }

    public Control Visual => this;

    public void Update(IDropInfo value)
    {
        dropInfo = value;
        indicator.DropInfo = value;
        if (value.DragInfo is { } dragInfo)
        {
            dataPreview.ContentTemplate ??= DragDrop.GetDragPreviewTemplate(dragInfo.VisualSource);
            effectPreview.ContentTemplate ??= DragDrop.GetEffectPreviewTemplate(dragInfo.VisualSource);
        }
        var defaultPreview = dataPreview.ContentTemplate is null ? GetDefaultPreview(value) : null;
        dataPreview.Content = dataPreview.ContentTemplate is not null
            ? value.Data
            : (object?)defaultPreview ?? GetPreviewText(value.Data);
        effectPreview.Content = value.Effects.HasFlag(global::Avalonia.Input.DragDropEffects.Copy) ? "Copy" : "Move";
        var showOnlyItem = defaultPreview is not null;
        preview.Background = showOnlyItem ? null : PreviewBackground;
        preview.Padding = showOnlyItem ? default : new Thickness(8, 5);
        effectPreview.IsVisible = !showOnlyItem;
        hintText.IsVisible = !showOnlyItem;
        InvalidateMeasure();
        InvalidateVisual();
    }

    private Control? GetDefaultPreview(IDropInfo value)
    {
        if (value.DragInfo is not { } dragInfo
            || !DragDrop.GetUseVisualSourceItemPreview(dragInfo.VisualSource))
        {
            return null;
        }

        if (defaultDataPreview is not null)
        {
            return defaultDataPreview;
        }

        if (dragInfo.SourceItems.Count != 1
            || dragInfo.VisualSourceItem is not { } sourceItem
            || sourceItem.Bounds.Width <= 0
            || sourceItem.Bounds.Height <= 0)
        {
            return null;
        }

        defaultDataPreview = new Border
        {
            Width = sourceItem.Bounds.Width,
            Height = sourceItem.Bounds.Height,
            Background = new VisualBrush(sourceItem),
            Opacity = 0.85
        };
        return defaultDataPreview;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        preview.Measure(availableSize);
        return availableSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (dropInfo is not null)
        {
            indicator.Arrange(new Rect(finalSize));
            var width = preview.DesiredSize.Width;
            var height = preview.DesiredSize.Height;
            var position = dropInfo.VisualTarget.TransformToVisual(this)?.Transform(dropInfo.DropPosition)
                           ?? dropInfo.DropPosition;
            var left = Math.Clamp(position.X + 14, 0, Math.Max(0, finalSize.Width - width));
            var top = Math.Clamp(position.Y + 14, 0, Math.Max(0, finalSize.Height - height));
            preview.Arrange(new Rect(left, top, width, height));
        }

        return finalSize;
    }

    private static string GetPreviewText(object? data)
    {
        if (data is IEnumerable values and not string)
        {
            var items = values.Cast<object>().Take(2).ToList();
            return items.Count > 1 ? $"{items[0]} (+more)" : items.FirstOrDefault()?.ToString() ?? string.Empty;
        }

        return data?.ToString() ?? string.Empty;
    }

    private sealed class DropIndicator : Control
    {
        private const double TriangleSize = 5;
        private readonly IBrush accent;
        private IDropInfo? dropInfo;

        public DropIndicator(IBrush accent)
        {
            this.accent = accent;
        }

        public IDropInfo? DropInfo
        {
            get => dropInfo;
            set
            {
                dropInfo = value;
                InvalidateVisual();
            }
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            if (DropInfo?.VisualTargetItem is not Control targetItem)
            {
                return;
            }

            Rect bounds;
            if (targetItem is TreeViewItem treeViewItem
                && ItemsControlDragDropHelper.GetTreeViewItemHeader(treeViewItem) is { } header
                && header.TransformToVisual(this) is { } headerTransform)
            {
                var headerOrigin = headerTransform.Transform(default);
                bounds = new Rect(headerOrigin, header.Bounds.Size);
            }
            else if (targetItem.TransformToVisual(this) is { } transform)
            {
                bounds = new Rect(transform.Transform(default), targetItem.Bounds.Size);
            }
            else
            {
                return;
            }
            if (DropInfo.InsertPosition.HasFlag(RelativeInsertPosition.TargetItemCenter))
            {
                context.DrawRectangle(null, new Pen(accent, 2), bounds);
                return;
            }

            Point start;
            Point end;
            if (DropInfo.IsHorizontal)
            {
                var x = DropInfo.InsertPosition.HasFlag(RelativeInsertPosition.AfterTargetItem)
                    ? bounds.Right
                    : bounds.Left;
                start = new Point(x, bounds.Top);
                end = new Point(x, bounds.Bottom);
            }
            else
            {
                var y = DropInfo.InsertPosition.HasFlag(RelativeInsertPosition.AfterTargetItem)
                    ? bounds.Bottom
                    : bounds.Top;
                start = new Point(bounds.Left, y);
                end = new Point(bounds.Right, y);
            }

            context.DrawLine(new Pen(accent, 2), start, end);
            DrawTriangle(context, start, end);
            DrawTriangle(context, end, start);
        }

        private void DrawTriangle(DrawingContext context, Point origin, Point opposite)
        {
            var vector = opposite - origin;
            var length = Math.Sqrt((vector.X * vector.X) + (vector.Y * vector.Y));
            if (length == 0)
            {
                return;
            }

            var direction = vector / length;
            var perpendicular = new Vector(-direction.Y, direction.X);
            var geometry = new StreamGeometry();
            using (var geometryContext = geometry.Open())
            {
                geometryContext.BeginFigure(origin + (direction * TriangleSize), true);
                geometryContext.LineTo(origin + (perpendicular * TriangleSize));
                geometryContext.LineTo(origin - (perpendicular * TriangleSize));
                geometryContext.EndFigure(true);
            }

            context.DrawGeometry(accent, null, geometry);
        }
    }
}