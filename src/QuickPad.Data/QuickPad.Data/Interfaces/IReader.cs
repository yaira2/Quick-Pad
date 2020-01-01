using System.Text;

namespace QuickPad.Data.Interfaces
{
    public interface IReader
    {
        string Read(EncodingProvider encodingProvider, string encodingName);
        string Read(Encoding encoding);

        int AddBytes(byte[] data);
    }
}