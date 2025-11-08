using System.Linq.Expressions;
using AvaloniaInfiniteScrolling;
using DailyPoetryA.Library.Models;
using DailyPoetryA.Library.Services;

namespace DailyPoetryA.Library.ViewModels;

public class ResultViewModel : ViewModelBase {
    private Expression<Func<Poetry, bool>> _where =
        Expression.Lambda<Func<Poetry, bool>>(Expression.Constant(true),
            Expression.Parameter(typeof(Poetry), "p"));

    public ResultViewModel(IPoetryStorage poetryStorage) {
        PoetryCollection = new AvaloniaInfiniteScrollCollection<Poetry> {
            OnCanLoadMore = () => _canLoadMore,
            OnLoadMore = async () => {
                Status = Loading;
                var poetries = await poetryStorage.GetPoetriesAsync(_where,
                    PoetryCollection.Count, PageSize);
                Status = "";
                
                if (poetries.Count < PageSize) {
                    _canLoadMore = false;
                    Status = NoMoreResult;
                }

                if (PoetryCollection.Count == 0 && poetries.Count == 0) {
                    Status = NoResult;
                }
                
                return poetries;
            }
        };
    }

    private bool _canLoadMore = true;

    public AvaloniaInfiniteScrollCollection<Poetry> PoetryCollection { get; }

    private string _status;

    public string Status {
        get => _status;
        private set => SetProperty(ref _status, value);
    }

    public const string Loading = "正在载入";

    public const string NoResult = "没有满足条件的结果";

    public const string NoMoreResult = "没有更多结果";
    
    public const int PageSize = 20;
}