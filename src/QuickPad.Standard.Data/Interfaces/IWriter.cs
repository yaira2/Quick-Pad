using System.Text;

namespace QuickPad.Data.Interfaces
{
    public interface IWriter
    {
        int Write(string text);

        byte[] GetBytes(EncodingProvider encodingProvider, string encodingName);
        byte[] GetBytes(Encoding encoding);
    }
}