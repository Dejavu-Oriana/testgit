using System.IO;
using System.Linq.Expressions;
using DailyPoetryA.Library.Helpers;
using DailyPoetryA.Library.Models;
using SQLite;

namespace DailyPoetryA.Library.Services;

/// <summary>
/// 诗歌数据库存储服务实现类
/// 负责处理与诗歌数据库相关的所有操作，包括初始化、查询和关闭连接
/// </summary>
public class PoetryStorage : IPoetryStorage {
    /// <summary>
    /// 偏好设置存储服务实例
    /// 用于存储数据库版本信息，实现版本控制
    /// </summary>
    private IPreferenceStorage _preferenceStorage;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="preferenceStorage">偏好设置存储服务实例</param>
    public PoetryStorage(IPreferenceStorage preferenceStorage) {
        _preferenceStorage = preferenceStorage;
    }

    /// <summary>
    /// 数据库文件名
    /// </summary>
    public const string DbName = "poetrydb.sqlite3";

    /// <summary>
    /// 数据库文件完整路径
    /// 结合PathHelper获取本地应用数据目录下的数据库文件路径
    /// </summary>
    public static readonly string PoetryDbPath = 
        PathHelper.GetLocalFilePath(DbName);

    /// <summary>
    /// SQLite异步连接实例
    /// </summary>
    private SQLiteAsyncConnection _connection;

    /// <summary>
    /// 获取SQLite异步连接
    /// 使用懒加载模式，仅在首次使用时创建连接实例
    /// </summary>
    private SQLiteAsyncConnection connection =>
        _connection ??= new SQLiteAsyncConnection(PoetryDbPath);

    /// <summary>
    /// 每页显示的诗歌数量
    /// </summary>
    public const int NumberPoetry = 30;

    /// <summary>
    /// 获取数据库是否已初始化
    /// 通过比较存储的版本号与当前版本号来确定
    /// </summary>
    public bool IsInitialized =>
        _preferenceStorage.Get(PoetryStorageConstant.VersionKey, 
            default(int)) == PoetryStorageConstant.Version;

    /// <summary>
    /// 初始化数据库
    /// 从嵌入式资源中复制数据库文件到本地路径
    /// 设置数据库版本信息
    /// </summary>
    /// <exception cref="ArgumentNullException">当嵌入式资源流为空时可能抛出</exception>
    public async Task InitializeAsync() {
        // 创建或打开本地数据库文件
        await using var dbFileStream = 
            new FileStream(PoetryDbPath, FileMode.OpenOrCreate);
        // 获取嵌入式资源中的数据库文件流
        await using var dbAssertStream = 
            typeof(Poetry).Assembly.GetManifestResourceStream(DbName);
        // 将嵌入式资源复制到本地文件
        await dbAssertStream.CopyToAsync(dbFileStream);

        // 设置数据库版本，标记初始化完成
        _preferenceStorage.Set(PoetryStorageConstant.VersionKey, 
            PoetryStorageConstant.Version);
    }

    /// <summary>
    /// 根据ID获取诗歌
    /// </summary>
    /// <param name="id">诗歌ID</param>
    /// <returns>找到的诗歌对象，如果未找到则返回null</returns>
    public async Task<Poetry> GetPoetryAsync(int id) =>
        await connection.Table<Poetry>().FirstOrDefaultAsync(
            p => p.Id == id);

    /// <summary>
    /// 根据条件查询诗歌列表，支持分页
    /// </summary>
    /// <param name="where">查询条件表达式</param>
    /// <param name="skip">跳过的记录数，用于分页</param>
    /// <param name="take">获取的记录数，用于分页</param>
    /// <returns>符合条件的诗歌列表</returns>
    public async Task<IList<Poetry>> GetPoetriesAsync(
        Expression<Func<Poetry, bool>> where, int skip, int take) =>
        await connection.Table<Poetry>()
            .Where(where).Skip(skip).Take(take).ToListAsync();

    /// <summary>
    /// 关闭数据库连接
    /// 在不需要使用数据库时调用，释放资源
    /// </summary>
    public async Task CloseAsync() => await connection.CloseAsync();
}

/// <summary>
/// 诗歌存储常量类
/// 定义数据库版本和版本键等常量
/// </summary>
public static class PoetryStorageConstant {
    /// <summary>
    /// 数据库版本键
    /// 用于在偏好设置中存储和检索数据库版本
    /// </summary>
    public const string VersionKey = 
        nameof(PoetryStorageConstant) + "." + nameof(Version);

    /// <summary>
    /// 当前数据库版本
    /// 当数据库结构发生变化时，增加此版本号
    /// </summary>
    public const int Version = 1;
}