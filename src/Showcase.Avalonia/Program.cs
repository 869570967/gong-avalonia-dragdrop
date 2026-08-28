using System;
using Avalonia;

namespace Showcase.Avalonia.DragDrop;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            throw;
        }
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
                         .UsePlatformDetect()
                         .WithInterFont()
                         .WithDeveloperTools()
                         .LogToTrace();
    }
}