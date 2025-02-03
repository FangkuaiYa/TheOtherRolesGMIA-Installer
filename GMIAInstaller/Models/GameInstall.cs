using System;
using System.Globalization;
using System.IO;

namespace GMIAInstaller.Models;

public class GameInstall
{
    public string Location { get; set; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".fangkuaifun");
    public string GameAssemblyDll => Path.Combine(Location, "GameAssembly.dll");
    public static bool isChinese()
    {
        try
        {
            var name = CultureInfo.CurrentUICulture.Name;
            if (name.StartsWith("zh")) return true;
            return false;
        }
        catch
        {
            return false;
        }
    }
}