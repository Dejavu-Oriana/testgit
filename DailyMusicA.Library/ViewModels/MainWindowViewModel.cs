namespace DailyMusicA.Library.ViewModels;

public class MainWindowViewModel : ViewModelBase {
    private ViewModelBase? _content;

    public ViewModelBase? Content {
        get => _content;
        set => SetProperty(ref _content, value);
    }
}