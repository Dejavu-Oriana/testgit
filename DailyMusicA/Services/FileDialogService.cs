using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using DailyMusicA.Library.Services;

namespace DailyMusicA.Services;

public class FileDialogService : IFileDialogService
{
    public async Task<string[]?> OpenFileDialogAsync(string title, string filters, bool allowMultiple = false)
    {
        if (GetMainWindow() is not Window window) return null;
        
        var dialog = new OpenFileDialog
        {
            Title = title,
            Filters = ParseFilters(filters),
            AllowMultiple = allowMultiple
        };
        
        return await dialog.ShowAsync(window);
    }

    public async Task<string?> SaveFileDialogAsync(string title, string fileName, string filters)
    {
        if (GetMainWindow() is not Window window) return null;
        
        var dialog = new SaveFileDialog
        {
            Title = title,
            InitialFileName = fileName,
            Filters = ParseFilters(filters)
        };
        
        return await dialog.ShowAsync(window);
    }
    
    private List<FileDialogFilter> ParseFilters(string filters)
    {
        var filterList = new List<FileDialogFilter>();
        string[] filterParts = filters.Split('|');
        
        for (int i = 0; i < filterParts.Length; i += 2)
        {
            if (i + 1 < filterParts.Length)
            {
                string filterName = filterParts[i];
                string[] extensionParts = filterParts[i + 1].Split(';');
                List<string> extensions = new List<string>();
                
                foreach (string ext in extensionParts)
                {
                    string cleanedExt = ext.Trim();
                    if (string.IsNullOrWhiteSpace(cleanedExt))
                        continue;
                    
                    // 处理"所有文件"过滤器
                    if (cleanedExt == "*.*" || cleanedExt == "*")
                    {
                        // 在Avalonia中，我们可以使用单个"*"作为扩展名
                        extensions.Add("*");
                        continue;
                    }
                    
                    // 移除通配符
                    cleanedExt = cleanedExt.TrimStart('*');
                    
                    // 移除点
                    if (cleanedExt.StartsWith("."))
                    {
                        cleanedExt = cleanedExt.Substring(1);
                    }
                    
                    // 只添加非空扩展名
                    if (!string.IsNullOrWhiteSpace(cleanedExt))
                    {
                        extensions.Add(cleanedExt);
                    }
                }
                
                // 只添加有有效扩展名的过滤器
                if (extensions.Count > 0)
                {
                    var filter = new FileDialogFilter
                    {
                        Name = filterName,
                        Extensions = extensions
                    };
                    
                    filterList.Add(filter);
                }
            }
        }
        
        return filterList;
    }
    
    private Window? GetMainWindow()
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            return desktop.MainWindow;
        }
        return null;
    }
}