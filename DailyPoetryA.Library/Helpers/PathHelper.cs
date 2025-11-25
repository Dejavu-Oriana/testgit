using System.IO;

namespace DailyPoetryA.Library.Helpers;

public class PathHelper {
    private static string _localFolder = string.Empty;
    private static string _albumCoverFolder = string.Empty;

    private static string localFolder {
        get {
            if (!string.IsNullOrEmpty(_localFolder)) {
                return _localFolder;
            }

            _localFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder
                    .LocalApplicationData), nameof(DailyPoetryA));

            if (!Directory.Exists(_localFolder)) {
                Directory.CreateDirectory(_localFolder);
            }

            return _localFolder;
        }
    }

    private static string albumCoverFolder {
        get {
            if (!string.IsNullOrEmpty(_albumCoverFolder)) {
                return _albumCoverFolder;
            }

            _albumCoverFolder = Path.Combine(localFolder, "album_cover");

            if (!Directory.Exists(_albumCoverFolder)) {
                Directory.CreateDirectory(_albumCoverFolder);
            }

            return _albumCoverFolder;
        }
    }

    public static string GetLocalFilePath(string fileName) {
        return Path.Combine(localFolder, fileName);
    }

    public static string GetAlbumCoverFilePath(string fileName) {
        return Path.Combine(albumCoverFolder, fileName);
    }
}