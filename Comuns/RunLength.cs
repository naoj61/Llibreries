using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Comuns
{
    public class RunLength
    {
        public static string Compress(string buffer)
        {
            byte[] bytes = new byte[buffer.Length * sizeof(char)];
            Buffer.BlockCopy(buffer.ToCharArray(), 0, bytes, 0, bytes.Length);
            byte[] enc = Compress(bytes);

            char[] chars = new char[enc.Length / sizeof(char)];
            Buffer.BlockCopy(enc, 0, chars, 0, enc.Length);
            return new string(chars);
        }

        public static byte[] Compress(byte[] buffer)
        {
            MemoryStream ms = new MemoryStream();
            GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true);
            zip.Write(buffer, 0, buffer.Length);
            zip.Close();
            ms.Position = 0;

            byte[] compressed = new byte[ms.Length];
            ms.Read(compressed, 0, compressed.Length);

            byte[] gzBuffer = new byte[compressed.Length + 4];
            Buffer.BlockCopy(compressed, 0, gzBuffer, 4, compressed.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzBuffer, 0, 4);
            return gzBuffer;
        }

        public static byte[] Decompress(byte[] gzBuffer)
        {
            MemoryStream ms = new MemoryStream();
            int msgLength = BitConverter.ToInt32(gzBuffer, 0);
            ms.Write(gzBuffer, 4, gzBuffer.Length - 4);

            byte[] buffer = new byte[msgLength];

            ms.Position = 0;
            GZipStream zip = new GZipStream(ms, CompressionMode.Decompress);
            zip.Read(buffer, 0, buffer.Length);

            return buffer;
        }


        #region Wiki
        public static string Encode(string input)
        {
            return Regex.Replace(input, @"(.)\1*", delegate(Match m)
            {
                return string.Concat(m.Value.Length, m.Groups[1].Value);
            });
        }

        public static string Decode(string input)
        {
            return Regex.Replace(input, @"(\d+)(\D)", delegate(Match m)
            {
                return new string(m.Groups[2].Value[0], int.Parse(m.Groups[1].Value));
            });
        }
        #endregion

        private const char Escape = '\\';

        public static string RunLengthEncode(string s)
        {
            string srle = string.Empty;
            int ccnt = 1; //char counter
            for (int i = 0; i < s.Length - 1; i++)
            {
                if (s[i] != s[i + 1] || i == s.Length - 2) //..a break in character repetition or the end of the string
                {
                    if (s[i] == s[i + 1] && i == s.Length - 2) //end of string condition
                        ccnt++;
                    srle += ccnt + ("1234567890".Contains(s[i]) ? "" + Escape : "") + s[i]; //escape digits
                    if (s[i] != s[i + 1] && i == s.Length - 2) //end of string condition
                        srle += ("1234567890".Contains(s[i + 1]) ? "1" + Escape : "") + s[i + 1];
                    ccnt = 1; //reset char repetition counter
                }
                else
                {
                    ccnt++;
                }

            }
            return srle;
        }

        public static string RunLengthDecode(string s)
        {
            string dsrle = string.Empty;
            string ccnt = string.Empty; //char counter
            for (int i = 0; i < s.Length; i++)
            {
                if ("1234567890".Contains(s[i])) //extract repetition counter
                {
                    ccnt += s[i];
                }
                else
                {
                    if (s[i] == Escape)
                    {
                        i++;
                    }
                    dsrle += new String(s[i], int.Parse(ccnt));
                    ccnt = "";
                }

            }
            return dsrle;
        }
    }
}