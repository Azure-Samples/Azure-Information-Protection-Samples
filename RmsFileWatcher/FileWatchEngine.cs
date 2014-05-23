using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RmsFileWatcher
{
    enum WatchState
    {
        Watching,           // engine is actively watching for changes
        Suspended           // engine has suspended watching for changes
    };

    public enum EngineNotificationType
    {
        Watching,           // engine is now watching for file changes
        Suspended,          // engine is not watching for file changes
        Processing,         // engine is requesting file processing
        Failed              // engine failed during file processing
    };

    /// <summary>
    /// Engine event arguments
    /// </summary>
    public class EngineEventArgs : EventArgs
    {
        public EngineEventArgs(EngineNotificationType t, string f)
        {
            NotificationType = t;
            FullPath = f;
        }

        public EngineNotificationType   NotificationType { get; set; }
        public string                   FullPath { get; set; }
    }

    /// <summary>
    /// Watch for file change events in one or more watched directories and
    /// delegate processing of changed files.
    /// </summary>
    class FileWatchEngine
    {
        List<ChangeNotification>    fileChangeList;
        List<FileSystemWatcher>     fileSystemWatchers;

        public int              MillisecondsBeforeProcessing { get; set; }
        public                  WatchState WatchState { get; set; }

        public FileWatchEngine()
        {
            fileChangeList = new List<ChangeNotification>();
            fileSystemWatchers = new List<FileSystemWatcher>();
            WatchState = WatchState.Suspended;
        }

        public event EventHandler<EngineEventArgs> EngineEvent;

        /// <summary>
        /// Add a directory to the watched list.
        /// </summary>
        public void AddWatchedDirectory(string fullPath)
        {
            FileSystemWatcher   existingWatcher;

            existingWatcher = findWatcherForPath(fullPath);
            if (existingWatcher == null)
            {
                FileSystemWatcher newWatcher;

                newWatcher = new FileSystemWatcher(fullPath);
                newWatcher.Filter = "";
                newWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.LastWrite;
                newWatcher.Changed += OnFileChange;
                newWatcher.Created += OnFileChange;
                newWatcher.EnableRaisingEvents = (WatchState == WatchState.Watching);
                fileSystemWatchers.Add(newWatcher);
            }
        }

        /// <summary>
        /// Remove a directory from the watched list.
        /// </summary>
        public void RemoveWatchedDirectory(string fullPath)
        {
            foreach (FileSystemWatcher w in fileSystemWatchers)
            {
                if (w.Path == fullPath)
                {
                    w.EnableRaisingEvents = false;
                    fileSystemWatchers.Remove(w);
                    w.Dispose();

                    break;
                }
            }
        }

        /// <summary>
        /// Start watching all registered directories.
        /// </summary>
        public void StartWatching()
        {
            setWatchState(WatchState.Watching);
            OnRaiseEngineEvent(new EngineEventArgs(EngineNotificationType.Watching, null));
        }

        /// <summary>
        /// Suspend watching of all registered directories.
        /// </summary>
        public void SuspendWatching()
        {
            setWatchState(WatchState.Suspended);
            OnRaiseEngineEvent(new EngineEventArgs(EngineNotificationType.Suspended, null));
        }

        /// <summary>
        /// Processes all changes, ensuring that no change less than MillisecondsBeforeProcessing
        /// old is handled.  This is to try to make sure we don't break up a file system transaction
        /// by processing the file in between multiple change notifications related to the same
        /// change.
        /// </summary>
        public void ProcessWatchedChanges()
        {
            DateTime startTime;

            startTime = DateTime.Now;

            // Work with a copy of the change list as more changes may be coming in

            foreach (ChangeNotification cn in fileChangeList.ToList<ChangeNotification>())
            {
                try
                {
                    // assume the file is processed either successfully, or resulting in 
                    // a failure in which case we want to stop trying to process it

                    cn.Processed = true;

                    if (File.Exists(cn.FullPath))
                    {
                        TimeSpan delta;

                        delta = startTime - cn.ChangeTime;
                        if (delta.TotalMilliseconds > MillisecondsBeforeProcessing)
                        {
                            OnRaiseEngineEvent(new EngineEventArgs(EngineNotificationType.Processing, cn.FullPath));
                        }
                        else
                        {
                            // too soon to process it, wait for next time

                            cn.Processed = false;
                        }
                    }
                }
                catch (Exception)
                {
                    OnRaiseEngineEvent(new EngineEventArgs(EngineNotificationType.Failed, cn.FullPath));
                }
                finally
                {
                    if (cn.Processed)
                    {
                        fileChangeList.Remove(cn);
                    }
                }
            }
        }

        protected virtual void OnRaiseEngineEvent(EngineEventArgs e)
        {
            EventHandler<EngineEventArgs> handler;

            handler = EngineEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Catches all file changes and coalesces them, removing duplicates, as we 
        /// may receive multiple notifications for a single file and a modification 
        /// to it.
        /// </summary>
        private void OnFileChange(object source, FileSystemEventArgs e)
        {
            ChangeNotification existingChange;

            existingChange = findExistingChange(e.FullPath);
            if (existingChange == null)
            {
                fileChangeList.Add(new ChangeNotification(e.FullPath));
            }
            else
            {
                existingChange.ChangeTime = DateTime.Now;
            }
        }

        /// <summary>
        /// Returns the FileSystemWatcher given a watched directory path, returns
        /// null if the path isn't being watched.
        /// </summary>
        private FileSystemWatcher findWatcherForPath(string fullPath)
        {
            foreach (FileSystemWatcher w in fileSystemWatchers)
            {
                if (w.Path == fullPath)
                {
                    return w;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds a change notification for a file path, returns null
        /// if the file has no existing change notification currently.
        /// </summary>
        private ChangeNotification findExistingChange(string fullPath)
        {
            foreach (ChangeNotification cn in fileChangeList)
            {
                if (cn.FullPath == fullPath)
                {
                    return cn;
                }
            }

            return null;
        }

        /// <summary>
        /// Enables or disables all FileSystemWatchers.
        /// </summary>
        private void setWatchState(WatchState state)
        {
            foreach (FileSystemWatcher w in fileSystemWatchers)
            {
                w.EnableRaisingEvents = (state == WatchState.Watching);
            }

            WatchState = state;
        }
    }
}
