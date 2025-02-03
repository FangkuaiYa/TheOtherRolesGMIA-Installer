using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GMIAInstaller.Models;

namespace GMIAInstaller.Services;

public static class GameLaunchService
{
    public static async ValueTask LaunchGame(GameInstall install)
    {
        await Task.Yield();
        Process.Start(Path.Combine(install.Location, "Among Us.exe"));
        await Task.Delay(5000);
    }
    public static ValueTask CopyTo(this GameInstall install, string newInstallPath)
    {
        if (File.Exists(newInstallPath))
            File.Delete(newInstallPath);

        if (Directory.Exists(newInstallPath))
            Directory.Delete(newInstallPath, true);

        var basePath = Path.GetFullPath(install.Location);
        foreach (var filePath in Directory.EnumerateFiles(install.Location, "*", SearchOption.AllDirectories).Select(Path.GetFullPath))
        {
            if (Directory.Exists(filePath) && Path.GetFileName(filePath) != "Among Us_Data")
                continue;

            var rebasedPath = filePath.Replace(basePath, "").TrimStart('/').TrimStart('\\');
            rebasedPath = Path.Join(newInstallPath, rebasedPath);

            var baseDir = Path.GetDirectoryName(rebasedPath);
            if (baseDir is not null)
                Directory.CreateDirectory(baseDir);

            File.Copy(filePath, rebasedPath, true);
        }

        return new ValueTask();
    }

    public static async ValueTask CopyTo(this GameInstall install, GameInstall otherInstall)
    {
        await install.CopyTo(otherInstall.Location);
    }
}