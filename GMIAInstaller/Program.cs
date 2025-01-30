using System;
using System.IO;
using Avalonia;
using Avalonia.ReactiveUI;
using GMIAInstaller.Models;
using Newtonsoft.Json;
using Octokit;

namespace GMIAInstaller;

internal class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    public static void Main(string[] args)
    {
        CreateConfiguration();

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

        SaveConfiguration();
    }

    private static void CreateConfiguration()
    {
        try
        {
            Context.Configuration = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(Context.ConfigPath));
        }
        catch (Exception)
        {
            /* ignored */
        }
    }

    private static void SaveConfiguration()
    {
        try
        {
            if (!Directory.Exists(Context.DataPath))
                Directory.CreateDirectory(Context.DataPath);

            if (File.Exists(Context.ConfigPath))
                File.Delete(Context.ConfigPath);

            File.WriteAllText(Context.ConfigPath,
                JsonConvert.SerializeObject(Context.Configuration, Formatting.Indented));
        }
        catch (Exception)
        {
            /* ignored */
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();
    }
}