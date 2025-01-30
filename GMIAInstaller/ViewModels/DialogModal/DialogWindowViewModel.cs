using ReactiveUI.Fody.Helpers;

namespace GMIAInstaller.ViewModels.DialogModal;

public class DialogWindowViewModel
{
    [Reactive] public string DialogString { get; set; } = "";
}