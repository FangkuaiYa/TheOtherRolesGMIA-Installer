using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.VisualTree;
using GMIAInstaller.ViewModels.DialogModal;
using GMIAInstaller.ViewModels.LandingPage;
using GMIAInstaller.Views.DialogModal;
using ReactiveUI;

namespace GMIAInstaller.Views.LandingPage;

public class LandingPageView : ReactiveUserControl<LandingPageViewModel>
{
    public LandingPageView()
    {
        this.WhenActivated(block =>
        {
            ViewModel.ShowDialog.RegisterHandler(DoShowDialogAsync).DisposeWith(block);
            ViewModel.WarnDialog.RegisterHandler(DoWarnDialogAsync).DisposeWith(block);
        });
        AvaloniaXamlLoader.Load(this);
    }

    private async Task DoShowDialogAsync(InteractionContext<Unit, string?> context)
    {
        var dialog = new OpenFolderDialog();
        var result = await dialog.ShowAsync((Window)this.GetVisualRoot());
        context.SetOutput(result);
    }

    private async Task DoWarnDialogAsync(InteractionContext<string, Unit> context)
    {
        var dialog = new DialogWindow()
        {
            DataContext = new DialogWindowViewModel
            {
                DialogString = context.Input
            },
            SystemDecorations = SystemDecorations.None,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        await dialog.ShowDialog<Unit>((Window)this.GetVisualRoot());
        context.SetOutput(Unit.Default);
    }
}