using System;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace QuickPad.Data.Interfaces
{
    public interface IDataProvider
    {
        Task<byte[]> LoadDataAsync(Uri uri);
        Task<byte[]> LoadDataAsync(string path);

        Task<string> SaveDataAsync(StorageFile file, IWriter writer, Encoding encoding);
    }
}
