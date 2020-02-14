using System;

namespace QuickPad.Mvvm.ViewModels
{
    public abstract class StorageFileWrapper<TStorageFile>
    {
        public TStorageFile File { get; set; }
        public abstract string FileType { get; }
        public abstract string DisplayType { get; }
        public abstract string DisplayName { get; }
        public abstract string Path { get; }
        public abstract string Name { get; }
        public string OriginalLineEndings { get; set; } = Environment.NewLine;
        public string TargetLineEndings { get; set; } = Environment.NewLine;
    }
}