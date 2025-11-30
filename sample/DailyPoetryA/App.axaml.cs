using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using DailyPoetryA.Library.Services;
using DailyPoetryA.Library.ViewModels;
using DailyPoetryA.ViewModels;
using DailyPoetryA.Views;

namespace DailyPoetryA;

public partial class App : Application {
    public override void Initialize() {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted() {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime
            desktop) {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);
            desktop.MainWindow = new MainWindow();

            // TODO delete this
            ServiceLocator.Current.RootNavigationService.NavigateTo(
                RootNavigationConstant.MainView);
            ServiceLocator.Current.MainViewModel.PushContent(ServiceLocator
                .Current.TodayViewModel);
        }

        base.OnFrameworkInitializationCompleted();
    }
}