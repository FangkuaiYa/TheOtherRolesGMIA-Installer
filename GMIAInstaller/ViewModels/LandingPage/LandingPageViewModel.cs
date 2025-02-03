using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using GMIAInstaller.Extensions;
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
    public ICommand CleanInstall { get; }
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

        CleanInstall = ReactiveCommand.CreateFromTask(async () =>
        {
            var gmiaInstallPath = new GameInstall
            {
                Location = Context.ModdedAmongUsLocation
            };

            if (!GameIntegrityService.AmongUsGameExists(gmiaInstallPath))
            {
                await WarnDialog.Handle(GameInstall.isChinese() ? "笨蛋，你还没有安装GMIA！" : "You haven't installed GMIA yet!");
            }
            else if (GameIntegrityService.AmongUsGameExists(gmiaInstallPath))
            {
                if (Directory.Exists(Context.ModdedAmongUsLocation))
                    Directory.Delete(Context.ModdedAmongUsLocation, true);

                await WarnDialog.Handle(GameInstall.isChinese() ? "为什么要卸载GMIA呢？\n是不喜欢吗，欢迎来反馈哦！" : "GMIA has left you...");
            }
        });


        InstallGame = ReactiveCommand.CreateFromTask(async () =>
        {
            var ausPath = new GameInstall
            {
                Location = AmongUsLocation
            };
            var gmiaInstallPath = new GameInstall
            {
                Location = Context.ModdedAmongUsLocation
            };

            IsInstallProgressing = true;
            if (!GameIntegrityService.AmongUsGameExists(ausPath))
            {
                IsInstallProgressing = false;
                await WarnDialog.Handle(GameInstall.isChinese() ? $"\"{ausPath.Location}\" 不是一个有效的Among Us位置" : $"\"{ausPath.Location}\"  is not a valid install location");
                return;
            }
            if (!GameIntegrityService.AmongUsGameExists(gmiaInstallPath) || FileExtensions.Sha256Hash(gmiaInstallPath.GameAssemblyDll) != FileExtensions.Sha256Hash(ausPath.GameAssemblyDll))
            {
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

                if (GameIntegrityService.AmongUsVersion(ausPath) == ModAmongUsVersion && HasGMIAAmongUsVersion)
                {
                    try
                    {
                        try
                        {
                            await WarnDialog.Handle(GameInstall.isChinese() ? "即将复制Among Us本体文件\n此过程可能卡死一段时间\n点击\"OK\"将复制文件" : "The Among Us ontology file is about to be copied.\n This process may freeze for a while.\n Click 'OK' to copy the file");
                            await ausPath.CopyTo(gmiaInstallPath);
                        }
                        catch (Exception)
                        {
                            IsInstallProgressing = false;
                            await WarnDialog.Handle(GameInstall.isChinese() ? "复制失败\n请以管理员身份运行后重试！" : "Couldn't setup modded install directory.");
                            return;
                        }

                        try
                        {
                            await DownloadService.DownloadBepInEx(gmiaInstallPath);
                        }
                        catch (Exception)
                        {
                            IsInstallProgressing = false;
                            await WarnDialog.Handle(
                                $"Couldn't install BepInEx patcher to \n \"{gmiaInstallPath.Location}\"");
                            return;
                        }


                        try
                        {
                            await using var stream = await DownloadService.DownloadGMIAPlugin();
                            await stream.CopyToAsync(File.OpenWrite(Path.Combine(gmiaInstallPath.Location, "BepInEx", "plugins",
                                "TheOtherRoles.dll")));
                        }
                        catch (Exception)
                        {
                            IsInstallProgressing = false;
                            await WarnDialog.Handle($"Couldn't install TheOtherRoles.dll patcher to \n \"{gmiaInstallPath.Location}\"");
                            return;
                        }

                        try
                        {
                            await using var stream = await DownloadService.DownloadReactorPlugin();
                            await stream.CopyToAsync(File.OpenWrite(
                                Path.Combine(gmiaInstallPath.Location, "BepInEx", "plugins", "Reactor.dll")
                            ));
                        }
                        catch (Exception)
                        {
                            IsInstallProgressing = false;
                            await WarnDialog.Handle($"Couldn't install Reactor.dll patcher to \n \"{gmiaInstallPath.Location}\"");
                            return;
                        }

                        try
                        {
                            await using var stream = await DownloadService.DownloadServerPlugin();
                            await stream.CopyToAsync(File.OpenWrite(
                                Path.Combine(gmiaInstallPath.Location, "BepInEx", "plugins", "Mini.RegionInstall.dll")
                            ));
                        }
                        catch (Exception)
                        {
                            IsInstallProgressing = false;
                            await WarnDialog.Handle($"Couldn't install Mini.RegionInstall.dll patcher to \n \"{gmiaInstallPath.Location}\"");
                            return;
                        }
                        try
                        {
                            await DownloadService.DownloadTheOtherHats(gmiaInstallPath);
                        }
                        catch (Exception)
                        {
                            await GameLaunchService.LaunchGame(gmiaInstallPath);
                            IsInstallProgressing = false;
                            await WarnDialog.Handle(GameInstall.isChinese() ? "安装TheOtherHats时出错！" : "Could not install TheOtherHats!");
                            await WarnDialog.Handle(GameInstall.isChinese() ? "正在启动Among Us\n请等到2-3分钟\n若长时间未打开请加群787132035获得技术支持" : "Starting Among Us \n Please wait for 2-3 minutes \n If not opened for a long time, please join in \n https://discord.gg/w7msq53dq7 to ask");
                            return;
                        }

                        await GameLaunchService.LaunchGame(gmiaInstallPath);
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
                    await WarnDialog.Handle(GameInstall.isChinese() ? $"请使用Among Us v{ModAmongUsVersion}安装" : $"Pleas user Among Us v{ModAmongUsVersion}");
                    IsInstallProgressing = false;
                }
                else
                {
                    await WarnDialog.Handle("Error");
                }
            }
            else
            {
                if (Process.GetProcessesByName("Among Us").Length > 0)
                {
                    IsInstallProgressing = false;
                    await WarnDialog.Handle(GameInstall.isChinese() ? "笨蛋，已经有一个Among Us在运行了！" : "Among Us is already running!");
                    return;
                }

                await GameLaunchService.LaunchGame(gmiaInstallPath);
                IsInstallProgressing = false;
            }
        });
    }
}