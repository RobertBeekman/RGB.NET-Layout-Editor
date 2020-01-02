using System;
using System.IO;

namespace LayoutEditor.UI
{
    public static class FileChangedWatcher
    {
        private static FileSystemWatcher _watcher;

        public static event EventHandler<string> FileChanged;

        public static void SetWatchDirectory(string directory)
        {
            _watcher?.Dispose();

            if (Directory.Exists(directory))
            {
                _watcher = new FileSystemWatcher(directory);
                _watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.Size;
                _watcher.Changed += WatcherHit;
                _watcher.Created += WatcherHit;
                _watcher.Deleted += WatcherHit;
                _watcher.Renamed += WatcherHit;
                _watcher.EnableRaisingEvents = true;
            }
            else
            {
                _watcher = null;
            }
        }

        private static void WatcherHit(object sender, FileSystemEventArgs e)
        {
            FileChanged?.Invoke(null, e.Name);
        }
    }
}
