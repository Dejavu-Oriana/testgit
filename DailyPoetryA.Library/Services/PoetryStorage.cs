using System.Linq.Expressions;
using DailyPoetryA.Library.Helpers;
using DailyPoetryA.Library.Models;
using SQLite;

namespace DailyPoetryA.Library.Services;

public class PoetryStorage : IPoetryStorage {
    private IPreferenceStorage _preferenceStorage;

    public PoetryStorage(IPreferenceStorage preferenceStorage) {
        _preferenceStorage = preferenceStorage;
    }

    public const string DbName = "poetrydb.sqlite3";

    public static readonly string PoetryDbPath =
        PathHelper.GetLocalFilePath(DbName);

    private SQLiteAsyncConnection _connection;

    private SQLiteAsyncConnection connection =>
        _connection ??= new SQLiteAsyncConnection(PoetryDbPath);

    public const int NumberPoetry = 30;

    public bool IsInitialized =>
        _preferenceStorage.Get(PoetryStorageConstant.VersionKey, 
            default(int)) == PoetryStorageConstant.Version;

    public async Task InitializeAsync() {
        await using var dbFileStream =
            new FileStream(PoetryDbPath, FileMode.OpenOrCreate);
        await using var dbAssertStream =
            typeof(Poetry).Assembly.GetManifestResourceStream(DbName);
        await dbAssertStream.CopyToAsync(dbFileStream);

        _preferenceStorage.Set(PoetryStorageConstant.VersionKey,
            PoetryStorageConstant.Version);
    }

    public async Task<Poetry> GetPoetryAsync(int id) =>
        await connection.Table<Poetry>().FirstOrDefaultAsync(
            p => p.Id == id);

    public async Task<IList<Poetry>> GetPoetriesAsync(
        Expression<Func<Poetry, bool>> where, int skip, int take) =>
        await connection.Table<Poetry>()
            .Where(where).Skip(skip).Take(take).ToListAsync();

    public async Task CloseAsync() => await connection.CloseAsync();
}

public static class PoetryStorageConstant {
    public const string VersionKey = 
        nameof(PoetryStorageConstant) + "." + nameof(Version);
    // PoetryStorageConstant.Version

    public const int Version = 1;
}