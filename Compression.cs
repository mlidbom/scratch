using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;

namespace Scratch
{
    class Compression
    {
        public static byte[] Compress(byte[] data)
        {
            var ms = new MemoryStream();
            var ds = new DeflateStream(ms, CompressionMode.Compress);
            ds.Write(data, 0, data.Length);
            ds.Flush();
            ds.Close();
            return ms.ToArray();
        }
        public static byte[] Decompress(byte[] data)
        {
            const int BUFFER_SIZE = 256;
            byte[] tempArray = new byte[BUFFER_SIZE];
            List<byte[]> tempList = new List<byte[]>();
            int count = 0, length = 0;

            MemoryStream ms = new MemoryStream(data);
            DeflateStream ds = new DeflateStream(ms, CompressionMode.Decompress);

            while ((count = ds.Read(tempArray, 0, BUFFER_SIZE)) > 0)
            {
                if (count == BUFFER_SIZE)
                {
                    tempList.Add(tempArray);
                    tempArray = new byte[BUFFER_SIZE];
                }
                else
                {
                    byte[] temp = new byte[count];
                    Array.Copy(tempArray, 0, temp, 0, count);
                    tempList.Add(temp);
                }
                length += count;
            }

            byte[] retVal = new byte[length];

            count = 0;
            foreach (byte[] temp in tempList)
            {
                Array.Copy(temp, 0, retVal, count, temp.Length);
                count += temp.Length;
            }

            return retVal;
        }
    }
}