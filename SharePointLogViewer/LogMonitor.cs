﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SharePointLogViewer
{
    class LogEntryDiscoveredEventArgs : EventArgs
    {
        public LogEntry LogEntry { get; set; }
    }

    class LogMonitor: IDisposable
    {
        FileTail fileTail;
        string folderPath;
        LogDirectoryWatcher watcher;

        public event EventHandler<LogEntryDiscoveredEventArgs> LogEntryDiscovered = delegate { };

        public LogMonitor(string folderPath)
        {
            this.folderPath = folderPath;
            fileTail = new FileTail();
            fileTail.LineDiscovered += new EventHandler<LineDiscoveredEventArgs>(fileTail_LineDiscovered);
            watcher = new LogDirectoryWatcher(folderPath);
            watcher.FileCreated += new EventHandler<FileCreatedEventArgs>(watcher_FileCreated);            
        }

        public void Start()
        {
            watcher.Start();
            string filePath = GetLastAccessedFile(folderPath);
            if (filePath != null)
                fileTail.Start(filePath);
        }

        public void Stop()
        {
            watcher.Stop();
            fileTail.Stop();
        }

        void watcher_FileCreated(object sender, FileCreatedEventArgs e)
        {
            fileTail.Stop();
            fileTail.Start(e.Filename);
        }

        void fileTail_LineDiscovered(object sender, LineDiscoveredEventArgs e)
        {
            var entry = LogEntry.Parse(e.Line);
            LogEntryDiscovered(this, new LogEntryDiscoveredEventArgs() { LogEntry = entry });
        }

        string GetLastAccessedFile(string folderPath)
        {
            var dirInfo = new DirectoryInfo(folderPath);
            var file = dirInfo.GetFiles().OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
            if (file != null)
                return file.FullName;
            return null;
        }

        #region IDisposable Members

        public void Dispose()
        {
            fileTail.Stop();
            watcher.Dispose();
        }

        #endregion
    }
}