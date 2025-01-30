using System;
using Avalonia.Media;
using GMIAInstaller.ViewModels.LandingPage;
using Material.Colors;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GMIAInstaller.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    [Reactive] public ViewModelBase CurrentView { get; set; }

    [Reactive] public IBrush LaunchButtonColor { get; set; }

    public MainWindowViewModel()
    {
        this.WhenAnyValue(vm => vm.CurrentView)
            .Subscribe(vm => { LaunchButtonColor = ActivationColor<LandingPageViewModel>(vm); });

        CurrentView = new LandingPageViewModel();
        LaunchButtonColor = new SolidColorBrush(SwatchHelper.Lookup[MaterialColor.Grey300]);
    }

    public void OpenViewLandingPage()
    {
        if (CurrentView is not LandingPageViewModel)
            CurrentView = new LandingPageViewModel();
    }

    private SolidColorBrush ActivationColor<T>(ViewModelBase viewModel) where T : ViewModelBase
    {
        return viewModel is T
            ? new SolidColorBrush(SwatchHelper.Lookup[MaterialColor.Grey300])
            : new SolidColorBrush(SwatchHelper.Lookup[MaterialColor.Grey600]);
    }
}