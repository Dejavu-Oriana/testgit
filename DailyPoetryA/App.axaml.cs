using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using DailyPoetryA.Views;
using System;
using System.IO;
using System.Text;

namespace DailyPoetryA;

public partial class App : Application {
    private static readonly string LogFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "app_log.txt");

    public override void Initialize() {
        try {
            Log("Initializing app...");
            AvaloniaXamlLoader.Load(this);
            Log("XAML loaded successfully");
        } catch (Exception ex) {
            Log($"Error during initialization: {ex.Message}\n{ex.StackTrace}");
            throw;
        }
    }

    public override void OnFrameworkInitializationCompleted() {
        try {
            Log("Framework initialization completed");
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                // 移除Avalonia数据验证，避免重复验证
                Log("Removing data validator");
                BindingPlugins.DataValidators.RemoveAt(0);
                Log("Creating MainWindow...");
                desktop.MainWindow = new MainWindow();
                Log("MainWindow created");
                
                // 应用启动时设置初始内容为HomeViewModel
                try {
                    Log("Setting initial content to HomeViewModel...");
                    var mainViewModel = ServiceLocator.Current.MainViewModel;
                    var homeViewModel = ServiceLocator.Current.HomeViewModel;
                    mainViewModel.SetMenuAndContent("HomeView", homeViewModel);
                    Log("Initial content set successfully");
                } catch (Exception ex) {
                    Log($"Error setting initial content: {ex.Message}\n{ex.StackTrace}");
                }
            }

            base.OnFrameworkInitializationCompleted();
            Log("App initialization completed");
        } catch (Exception ex) {
            Log($"Error during framework initialization: {ex.Message}\n{ex.StackTrace}");
            throw;
        }
    }

    private static void Log(string message) {
        try {
            string logEntry = $"[{DateTime.Now}] {message}\n";
            File.AppendAllText(LogFilePath, logEntry, Encoding.UTF8);
        } catch {}
    }
}