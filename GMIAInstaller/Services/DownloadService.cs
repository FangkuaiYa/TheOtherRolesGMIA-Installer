using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using GMIAInstaller.Models;

namespace GMIAInstaller.Services;

public static class DownloadService
{
    private static readonly string[] BEPINEX_FILES =
    {
        "winhttp.dll",
        "doorstop_config.ini",
        "mono",
        Path.Combine("BepInEx", "core"),
        Path.Combine("BepInEx", "unity-libs"),
        Path.Combine("BepInEx", "unhollowed")
    };

    public static async Task<string> DownloadTextAsync(string url)
    {
        using (var client = new HttpClient())
        {
            //client.DefaultRequestHeaders.Add("User-Agent", "C# App");

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }
    }

    public static async ValueTask DownloadBepInEx(GameInstall install)
    {
        using var httpClient = new HttpClient();
        await using var stream = await httpClient.GetStreamAsync(
            "https://dl.fungle.icu/ModFiles/TheOtherRolesGMIA/BepInEx-Unity.IL2CPP-GMIA-win-x86-6.0.0-be.725.zip");
        var downloadPath = await DownloadStreamToTempFile(stream, "BepInEx-Unity.IL2CPP-GMIA-win-x86-6.0.0-be.725");

        CleanInstalledBepInEx(install);

        ZipFile.ExtractToDirectory(downloadPath, install.Location);
    }
    public static async ValueTask DownloadTheOtherHats(GameInstall install)
    {
        using var httpClient = new HttpClient();
        await using var stream = await httpClient.GetStreamAsync(
            "https://dl.fungle.icu/ModFiles/TheOtherRolesGMIA/TheOtherHats.zip");
        var downloadPath = await DownloadStreamToTempFile(stream, "TheOtherHats.zip");

        ZipFile.ExtractToDirectory(downloadPath, install.Location);
    }

    private static void CleanInstalledBepInEx(GameInstall install)
    {
        foreach (var file in BEPINEX_FILES)
        {
            var fullPath = Path.Combine(install.Location, file);
            if (File.Exists(fullPath))
                File.Delete(fullPath);

            if (Directory.Exists(fullPath))
                Directory.Delete(fullPath, true);
        }
    }

    public static async Task<Stream> DownloadGMIAPlugin()
    {
        var req = WebRequest.CreateHttp("https://dl.fungle.icu/ModFiles/TheOtherRolesGMIA/TheOtherRoles.dll");
        return (await req.GetResponseAsync()).GetResponseStream();
    }

    public static async Task<Stream> DownloadReactorPlugin()
    {
        var req = WebRequest.CreateHttp("https://dl.fungle.icu/ModFiles/TheOtherRolesGMIA/Reactor.dll");
        return (await req.GetResponseAsync()).GetResponseStream();
    }

    public static async Task<Stream> DownloadServerPlugin()
    {
        var req = WebRequest.CreateHttp("https://dl.fungle.icu/ModFiles/TheOtherRolesGMIA/Mini.RegionInstall.dll");
        return (await req.GetResponseAsync()).GetResponseStream();
    }

    private static async Task<string> DownloadStreamToTempFile(Stream inStream, string fileName)
    {
        var tempPathRoot = Path.Combine(Path.GetTempPath(), "fangkuaifun-client-launcher");
        if (File.Exists(tempPathRoot))
            File.Delete(tempPathRoot);

        if (!Directory.Exists(tempPathRoot))
            Directory.CreateDirectory(tempPathRoot);

        var tempPath = Path.Combine(tempPathRoot, fileName);
        if (File.Exists(tempPath))
            File.Delete(tempPath);

        await using var file = File.OpenWrite(tempPath);
        await inStream.CopyToAsync(file);

        return tempPath;
    }
}