using System;
using System.IO;
using System.Text;

namespace QuickPad
{
    public static class Extensions
    {
        public static System.Text.Encoding GetEncoding(this byte[] data)
        {
            //var bom = new byte[4];
            //try
            //{
            //    using (var file = new FileStream(fullFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            //    {
            //        file.Read(bom, 0, 4);
            //    }
            //}
            //catch(Exception ex)
            //{
            //    var m = ex.Message;
            //    var i = 0;
            //}

            if (data[0] == 0x2B && data[1] == 0x2F && data[2] == 0x76) return System.Text.Encoding.UTF7;
            if (data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF) return System.Text.Encoding.UTF8;
            if (data[0] == 0xFF && data[1] == 0xFE) return System.Text.Encoding.Unicode;          //UTF-16 LE
            if (data[0] == 0xFE && data[1] == 0xFF) return System.Text.Encoding.BigEndianUnicode; //UTF-16 BE
            if (data[0] == 0xFF && data[1] == 0xFE && data[2] == 0x0 && data[3] == 0x0) return System.Text.Encoding.UTF32;

            return System.Text.Encoding.Default;
        }
    }
}