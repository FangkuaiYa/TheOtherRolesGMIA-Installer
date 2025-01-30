using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using Microsoft.Win32;

namespace GMIAInstaller.Services.GameLocator;

public class EpicGamesLocatorService
{
    public static bool TryGetGameLocation(out string gameLocation)
    {
        gameLocation = @"C:\Program Files\Epic Games\Among Us";
        return true;
    }
}