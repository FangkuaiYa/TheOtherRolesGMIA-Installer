using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using GMIAInstaller.Models;
using GMIAInstaller.Services;
using GMIAInstaller.Services.GameLocator;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GMIAInstaller.ViewModels.LandingPage;

public class LandingPageViewModel : ViewModelBase
{
    public string AmongUsLocation
    {
        get => Context.Configuration.AmongUsLocation;
        set => this.RaiseAndSetIfChanged(ref Context.Configuration.AmongUsLocation, value);
    }

    public string[] AutodetectPaths
    {
        get => Context.Configuration.AutodetectedPaths;
        set => this.RaiseAndSetIfChanged(ref Context.Configuration.AutodetectedPaths, value);
    }

    public static string ModAmongUsVersion = string.Empty;

    [Reactive] public bool IsInstallProgressing { get; set; }
    public bool HasGMIAAmongUsVersion = true;

    public ICommand ChooseFileCommand { get; }
    public ICommand Autodetect { get; }
    public ICommand InstallGame { get; }
    public Interaction<Unit, string?> ShowDialog { get; }
    public Interaction<string, Unit> WarnDialog { get; }

    public LandingPageViewModel()
    {
        ShowDialog = new Interaction<Unit, string?>();
        WarnDialog = new Interaction<string, Unit>();

        ChooseFileCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var amongUsLocation = await ShowDialog.Handle(Unit.Default).FirstAsync() ?? AmongUsLocation;

            var gameExists = GameIntegrityService.AmongUsGameExists(new GameInstall
            {
                Location = amongUsLocation
            });

            if (gameExists)
                AmongUsLocation = amongUsLocation;
            else if (amongUsLocation != "")
                await WarnDialog.Handle(GameInstall.isChinese() ? $"\"{amongUsLocation}\" 不是一个有效的Among Us位置" : $"\"{amongUsLocation}\"  is not a valid install location");
        });

        Autodetect = ReactiveCommand.Create(() =>
        {
            AutodetectPaths = GameAutoDetectService.LocateGameInstallsAsync().Select(x => x.Location).ToArray();

            var location = AutodetectPaths.FirstOrDefault(x =>
                GameIntegrityService.AmongUsGameExists(new GameInstall { Location = x }));
            if (location is not null)
                AmongUsLocation = Path.GetFullPath(location);
        });

        InstallGame = ReactiveCommand.CreateFromTask(async () =>
        {
            var install = new GameInstall
            {
                Location = AmongUsLocation
            };

            IsInstallProgressing = true;
            if (!GameIntegrityService.AmongUsGameExists(install))
            {
                IsInstallProgressing = false;
                await WarnDialog.Handle(GameInstall.isChinese() ? $"\"{install.Location}\" 不是一个有效的Among Us位置" : $"\"{install.Location}\"  is not a valid install location");
                return;
            }

            try
            {
                ModAmongUsVersion = await DownloadService.DownloadTextAsync(Context.AmongUsVersionTxt);
            }
            catch (Exception ex)
            {
                await WarnDialog.Handle(GameInstall.isChinese() ?
                    $"获取GMIA适配的Among Us版本时出错: {ex.Message}\n请检查网络连接" : $"Error occurred while obtaining the Among Us version adapted to GMIA: {ex.Message} \n Please check the network connection");
                HasGMIAAmongUsVersion = false;
            }

            if (GameIntegrityService.AmongUsVersion(install) == ModAmongUsVersion && HasGMIAAmongUsVersion)
            {
                try
                {
                    try
                    {
                        await DownloadService.DownloadBepInEx(install);
                    }
                    catch (Exception)
                    {
                        IsInstallProgressing = false;
                        await WarnDialog.Handle(
                            $"Couldn't install BepInEx patcher to  \"{install.Location}\"");
                        return;
                    }


                    try
                    {
                        await using var stream = await DownloadService.DownloadGMIAPlugin();
                        await stream.CopyToAsync(File.OpenWrite(Path.Combine(install.Location, "BepInEx", "plugins",
                            "TheOtherRoles.dll")));
                    }
                    catch (Exception)
                    {
                        IsInstallProgressing = false;
                        await WarnDialog.Handle($"Couldn't install TheOtherRoles.dll patcher to  \"{install.Location}\"");
                        return;
                    }

                    try
                    {
                        await using var stream = await DownloadService.DownloadReactorPlugin();
                        await stream.CopyToAsync(File.OpenWrite(
                            Path.Combine(install.Location, "BepInEx", "plugins", "Reactor.dll")
                        ));
                    }
                    catch (Exception)
                    {
                        IsInstallProgressing = false;
                        await WarnDialog.Handle($"Couldn't install Reactor.dll patcher to  \"{install.Location}\"");
                        return;
                    }

                    try
                    {
                        await using var stream = await DownloadService.DownloadServerPlugin();
                        await stream.CopyToAsync(File.OpenWrite(
                            Path.Combine(install.Location, "BepInEx", "plugins", "Mini.RegionInstall.dll")
                        ));
                    }
                    catch (Exception)
                    {
                        IsInstallProgressing = false;
                        await WarnDialog.Handle($"Couldn't install Mini.RegionInstall.dll patcher to  \"{install.Location}\"");
                        return;
                    }

                    await GameLaunchService.LaunchGame(install);
                    IsInstallProgressing = false;
                    await WarnDialog.Handle(GameInstall.isChinese() ? "正在启动Among Us\n请等到2-3分钟\n若长时间未打开请加群787132035获得技术支持" : "Starting Among Us \n Please wait for 2-3 minutes \n If not opened for a long time, please join in \n https://discord.gg/w7msq53dq7 to ask");
                }
                catch (Exception e)
                {
                    await WarnDialog.Handle(GameInstall.isChinese() ? $"启动游戏时出错: {e.Message}, {e.StackTrace}" : $"Caught exception when launching game: {e.Message}, {e.StackTrace}");
                    IsInstallProgressing = false;
                }
                /*finally
                {
                    IsInstallProgressing = false;
                }*/
            }
            else if (!HasGMIAAmongUsVersion)
            {
                IsInstallProgressing = false;
            }
            else if (HasGMIAAmongUsVersion)
            {
                await WarnDialog.Handle($"��ʹ��Among Us v{ModAmongUsVersion} �汾����װ");
                IsInstallProgressing = false;
            }
            else
            {
                await WarnDialog.Handle("��������ϵ����Ա");
            }
        });
    }
}