using System.Collections.Generic;
using System.IO;
using System.Linq;
using GMIAInstaller.Models;
using ModClientPreloader.Services;

namespace GMIAInstaller.Services;

public static class GameIntegrityService
{
    public static bool AmongUsGameExists(GameInstall install)
    {
        return File.Exists(Path.Combine(install.Location, "Among Us.exe")) &&
               File.Exists(Path.Combine(install.Location, "GameAssembly.dll")) &&
               Directory.EnumerateFiles(Path.Combine(install.Location, "Among Us_Data")).Any();
    }

    public static string AmongUsVersion(GameInstall install)
    {
        if (AmongUsGameExists(install))
            return GameVersionParser.Parse(Path.Combine(install.Location, "Among Us_Data", "globalgamemanagers"));
        return "";
    }

   /* public static IEnumerable<string> FindGMIAModFiles(GameInstall install)
    {
        var pluginPath = Path.Combine(install.Location, "BepInEx", "plugins");
        if (!Directory.Exists(pluginPath))
            Directory.CreateDirectory(pluginPath);

        return Directory.EnumerateFiles(pluginPath)
            .Where(fileName => fileName.ToLower().Contains(Context.GMIAModPrefix))
            .Select(file => Path.Combine(pluginPath, file));
    }*/
}