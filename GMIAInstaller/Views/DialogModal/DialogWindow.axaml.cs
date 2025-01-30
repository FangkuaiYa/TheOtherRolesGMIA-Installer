using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace GMIAInstaller.Views.DialogModal;

public class DialogWindow : Window
{
    public DialogWindow()
    {
        AvaloniaXamlLoader.Load(this);
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void CloseDialog(object? sender, RoutedEventArgs e)
    {
        Close(Unit.Default);
    }
}