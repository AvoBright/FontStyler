using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace AvoBright.FontStyler
{
    internal static class FontFile
    {
        [DllImport("FreeTypeWrapper.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe sbyte* GetFontFamilyName(sbyte* fontFilePath);
        
        public static string GetFontFamilyName(string fontFilePath)
        {
            string extension = Path.GetExtension(fontFilePath).ToLower().TrimStart('.');

            if (extension != "ttf" && extension != "woff")
            {
                throw new Exception("Only TTF and WOFF font files are supported.");
            }

            var utf8 = Encoding.UTF8;
            byte[] byteArr = utf8.GetBytes(fontFilePath);

            sbyte[] sbyteArr = new sbyte[byteArr.Length + 1];

            for (int i = 0; i < byteArr.Length; ++i)
            {
                sbyteArr[i] = (sbyte)byteArr[i];
            }

            sbyteArr[sbyteArr.Length - 1] = 0;

            unsafe
            {
                fixed (sbyte* ptr = sbyteArr)
                {
                    sbyte* name = GetFontFamilyName(ptr);
                    string result = new string(name);

                    return result;
                }
            }
        }

    }
}
