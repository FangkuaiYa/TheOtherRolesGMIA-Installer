using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using Microsoft.Win32;

namespace GMIAInstaller.Services.GameLocator;

public class SteamLocatorService : IGameLocator
{
    private static string? GetSteamAppsLocation()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".steam", "steam",
                "steamapps");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library",
                "Application Support", "Steam", "steamapps");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            if (Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Valve\Steam")?.GetValue("SteamPath") is string steamPath)
                return Path.Combine(steamPath, "steamapps");

        return null;
    }

    public static bool TryGetGameLocation(out string gameLocation)
    {
        gameLocation = string.Empty;

        var steamApps = GetSteamAppsLocation();
        if (steamApps is null || !Directory.Exists(steamApps))
            return false;

        var libraries = new List<string> { steamApps };

        var vdf = Path.Combine(steamApps, "libraryfolders.vdf");
        if (File.Exists(vdf))
        {
            var libraryFolders = VdfConvert.Deserialize(File.ReadAllText(vdf));

            foreach (var libraryFolder in libraryFolders.Value.Children<VProperty>())
            {
                if (!int.TryParse(libraryFolder.Key, out _))
                    continue;

                foreach (var libraryFolderKey in libraryFolder.Value.Children<VProperty>())
                    if (string.Equals(libraryFolderKey.Key, "path"))
                        libraries.Add(Path.Combine(libraryFolderKey.Value.Value<string>(), "steamapps"));
            }
        }

        foreach (var library in libraries)
        {
            var path = Path.Combine(library, "common", "Among Us");
            if (Directory.Exists(path))
            {
                gameLocation = path;
                return true;
            }
        }

        return false;
    }
}