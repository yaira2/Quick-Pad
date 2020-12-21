using QuickPad.Data.Interfaces;
using QuickPad.Standard.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace QuickPad.Data
{
    public class StorageFileDataProvider : IDataProvider<StorageFile>
    {
        public async Task<byte[]> LoadDataAsync(StorageFile file)
        {
            var buffer = await FileIO.ReadBufferAsync(file);

            using var reader = DataReader.FromBuffer(buffer);

            var bytes = new byte[buffer.Length];
            reader.ReadBytes(bytes);
            return bytes;
        }

        public async Task<string> SaveDataAsync(StorageFileWrapper<StorageFile> file, IWriter writer, Encoding encoding)
        {
            try
            {
                List<byte> allBytes = new List<byte>();

                if (file.BOM != null)
                {
                    allBytes = new List<byte>(file.BOM);
                    allBytes.AddRange(writer.GetBytes(encoding));
                }
                else
                {
                    allBytes = new List<byte>(writer.GetBytes(encoding));
                }

                var bytes = allBytes.ToArray();

                // Create sample file; replace if exists.
                await PathIO.WriteBytesAsync(file.Path, bytes);

                return $"{file.Name} was saved.";
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}