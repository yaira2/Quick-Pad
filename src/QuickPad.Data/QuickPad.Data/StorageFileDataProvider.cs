using System;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using QuickPad.Data.Interfaces;
using Windows.Storage.Streams;
using QuickPad.Mvvm.ViewModels;
using Microsoft.Toolkit.Uwp.UI.Controls.TextToolbarSymbols;
using System.Collections.Generic;
using QuickPad.Standard.Data;

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

        public static async Task<byte[]> GetBytesAsync(StorageFile file)
        {
            byte[] fileBytes = null;
            if (file == null) return null;
            using (var stream = await file.OpenReadAsync())
            {
                fileBytes = new byte[stream.Size];
                using (var reader = new DataReader(stream))
                {
                    await reader.LoadAsync((uint)stream.Size);
                    reader.ReadBytes(fileBytes);
                }
            }
            return fileBytes;
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
                await FileIO.WriteBytesAsync(file.File, bytes);
                
                return $"{file.Name} was saved.";
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
