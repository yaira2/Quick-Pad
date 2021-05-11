using QuickPad.Data.Interfaces;
using QuickPad.Standard.Data;
using System;
using System.Collections.Generic;
using System.IO;
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

        public static bool IsFileReadOnly(StorageFile file)
        {
            return (file.Attributes & Windows.Storage.FileAttributes.ReadOnly) != 0;
        }

        public static async Task<bool> IsFileWritable(StorageFile file)
        {
            try
            {
                using (var stream = await file.OpenStreamForWriteAsync()) { }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<string> SaveDataAsync(StorageFileWrapper<StorageFile> file, IWriter writer, Encoding encoding)
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


            if (IsFileReadOnly(file.File) || !await IsFileWritable(file.File))
            {
                // For file(s) dragged into Quick Pad, they are read-only
                // StorageFile API won't work but can be overwritten by Win32 PathIO API
                // In case the file is actually read-only, WriteBytesAsync will throw UnauthorizedAccessException exception
                await PathIO.WriteBytesAsync(file.Path, bytes);
            }
            else // Use FileIO API to save
            {
                // Create file; replace if exists.
                await FileIO.WriteBytesAsync(file.File, bytes);
            }

            return $"{file.Name} was saved.";
        }
    }
}