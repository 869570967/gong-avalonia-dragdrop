using System;
using System.Collections;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;

namespace GongSolutions.Avalonia.DragDrop;

internal sealed class DropTargetAdorner : Panel
{
    private readonly TextBlock previewText;
    private readonly Border preview;
    private readonly DropIndicator indicator;
    private IDropInfo? dropInfo;

    public DropTargetAdorner()
    {
        IsHitTestVisible = false;
        indicator = new DropIndicator();
        previewText = new TextBlock
        {
            Foreground = Brushes.White,
            FontSize = 12,
            MaxWidth = 220,
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        preview = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(224, 31, 41, 55)),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(8, 5),
            Child = previewText
        };
        Children.Add(indicator);
        Children.Add(preview);
    }

    public void Update(IDropInfo value)
    {
        dropInfo = value;
        indicator.DropInfo = value;
        previewText.Text = GetPreviewText(value.Data);
        InvalidateMeasure();
        InvalidateVisual();
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
            var left = Math.Clamp(dropInfo.DropPosition.X + 14, 0, Math.Max(0, finalSize.Width - width));
            var top = Math.Clamp(dropInfo.DropPosition.Y + 14, 0, Math.Max(0, finalSize.Height - height));
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
        private IDropInfo? dropInfo;

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
            if (DropInfo?.VisualTargetItem is not Control targetItem
                || targetItem.TransformToVisual(DropInfo.VisualTarget) is not { } transform)
            {
                return;
            }

            var origin = transform.Transform(default);
            var height = targetItem is TreeViewItem { HeaderPresenter: { } header }
                ? header.Bounds.Height
                : targetItem.Bounds.Height;
            var bounds = new Rect(origin, new Size(targetItem.Bounds.Width, height));
            var accent = new SolidColorBrush(Color.FromRgb(37, 99, 235));

            if (DropInfo.InsertPosition.HasFlag(RelativeInsertPosition.TargetItemCenter))
            {
                context.DrawRectangle(new SolidColorBrush(Color.FromArgb(48, 37, 99, 235)), new Pen(accent, 2), bounds);
                return;
            }

            var y = DropInfo.InsertPosition.HasFlag(RelativeInsertPosition.AfterTargetItem)
                ? bounds.Bottom
                : bounds.Top;
            context.DrawLine(new Pen(accent, 2), new Point(bounds.Left, y), new Point(bounds.Right, y));
        }
    }
}