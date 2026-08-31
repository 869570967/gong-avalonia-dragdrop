using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Themes.Fluent;
using Xunit;

[assembly: AvaloniaTestApplication(typeof(GongSolutions.Avalonia.DragDrop.Tests.TestApplicationBuilder))]
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace GongSolutions.Avalonia.DragDrop.Tests;

public sealed class TestApplication : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
    }
}

public static class TestApplicationBuilder
{
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<TestApplication>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
    }
}
