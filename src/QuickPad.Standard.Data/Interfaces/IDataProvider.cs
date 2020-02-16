using System.Text;
using System.Threading.Tasks;

namespace QuickPad.Data.Interfaces
{
    public interface IDataProvider<TFileDefinition>
    {
        Task<byte[]> LoadDataAsync(TFileDefinition file);

        Task<string> SaveDataAsync(TFileDefinition file, IWriter writer, Encoding encoding);
    }
}
