using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DailyMusicA.Library.ViewModels;

namespace DailyMusicA.Views;

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

