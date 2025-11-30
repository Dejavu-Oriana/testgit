using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using DailyMusicA.Library.ViewModels;

namespace DailyMusicA;

public class ViewLocator : IDataTemplate {
    public Control? Build(object? data) {
        if (data is null)
            return null;

        var viewModelType = data.GetType();
        var name = viewModelType.FullName!.Replace("ViewModel", "View",
            StringComparison.Ordinal);
        
        // 处理Library命名空间下的ViewModel
        if (name.Contains("DailyMusicA.Library.Views")) {
            name = name.Replace("DailyMusicA.Library.Views", "DailyMusicA.Views");
        }
        
        var type = Type.GetType(name);

        if (type != null) {
            var control = (Control)Activator.CreateInstance(type)!;
            control.DataContext = data;
            return control;
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data) {
        return data is DailyMusicA.Library.ViewModels.ViewModelBase;
    }
}