using System.Text;
using System.Threading.Tasks;

using QuickPad.Standard.Data;

namespace QuickPad.Data.Interfaces
{
    public interface IDataProvider<TFileDefinition>
    {
        Task<byte[]> LoadDataAsync(TFileDefinition file);

        Task<string> SaveDataAsync(StorageFileWrapper<TFileDefinition> file, IWriter writer, Encoding encoding);
    }
}
