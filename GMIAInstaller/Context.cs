using System;
using System.IO;
using GMIAInstaller.Models;
using Octokit;

namespace GMIAInstaller;

public static class Context
{
    public static string AmongUsVersionTxt = "https://dl.fangkuai.fun/ModFiles/TheOtherRolesGMIA/AmongUsVersion.txt";
    public static string GMIAModPrefix { get; } = "fangkuaifun";

    public static string DataPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        ".fangkuaifun-client-launcher");

    public static string ConfigPath => Path.Combine(DataPath, "config.json");
    public static string ModdedAmongUsLocation => Path.Combine(DataPath, "GMIAFiles");
    public static Configuration Configuration { get; set; } = new();
}