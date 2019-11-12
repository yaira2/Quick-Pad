using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using QuickPad.Data.Interfaces;

namespace QuickPad.Data
{
    public class FileDataProvider : IDataProvider
    {
        public Task<byte[]> LoadDataAsync(Uri uri)
        {
            return File.ReadAllBytesAsync(uri.ToString());
        }

        public Task<byte[]> LoadDataAsync(string path)
        {
            return File.ReadAllBytesAsync(path);
        }

        public async Task<string> SaveDataAsync(StorageFile file, IWriter writer, Encoding encoding)
        {
            try
            {
                var bytes = writer.GetBytes(encoding);

                // Create sample file; replace if exists.
                await FileIO.WriteBytesAsync(file, bytes);
                
                return $"{file.Name} was saved.";
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
