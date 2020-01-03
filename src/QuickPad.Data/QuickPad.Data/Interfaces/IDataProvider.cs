using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace QuickPad.Data.Interfaces
{
    public interface IDataProvider
    {
        Task<byte[]> LoadDataAsync(StorageFile file);

        Task<string> SaveDataAsync(StorageFile file, IWriter writer, Encoding encoding);
    }
}
