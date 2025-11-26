using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DailyPoetryA.Library.ViewModels;

namespace DailyPoetryA.Views;

public partial class StatsView : UserControl
{
    public StatsView()
    {
        InitializeComponent();
        DataContext = ServiceLocator.Current.StatsViewModel;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

