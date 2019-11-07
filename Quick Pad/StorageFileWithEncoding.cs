using Windows.Storage;
using static QuickPad.App;

namespace QuickPad
{
    public class StorageFileWithEncoding
    {
        public StorageFile StorageFile { get; set; }
        public Encoding FileEncoding { get; set; } 

        public static implicit operator StorageFileWithEncoding(StorageFile fs) => new StorageFileWithEncoding() { StorageFile = fs, FileEncoding = Encoding.UTF8};
    }
}
