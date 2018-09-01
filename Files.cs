using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ElectionBot
{
    class Files
    {
        public static async Task DictArrayToFile(Dictionary<ulong, string[]> dict, string keyFile, string valueFile)
        {
            await Task.Run(() =>
            {
                BinaryWriter keyWriter = new BinaryWriter(File.Open(keyFile, FileMode.Create));
                BinaryWriter valueWriter = new BinaryWriter(File.Open(valueFile, FileMode.Create));

                foreach (ulong str in dict.Keys)
                {
                    keyWriter.Write(str.ToString());
                    valueWriter.Write(dict[str][0]);
                    valueWriter.Write(dict[str][1]);
                }

                keyWriter.Close();
                valueWriter.Close();
            });
        }

        public static async Task<Dictionary<ulong, string[]>> FileToDictArray(string keyFile, string valueFile)
        {
            Dictionary<ulong, string[]> dict = new Dictionary<ulong, string[]>();

            List<ulong> keys = new List<ulong>();
            List<string[]> values = new List<string[]>();

            string key, value;
            string[] str = new string[2];

            BinaryReader keyReader = new BinaryReader(File.Open(keyFile, FileMode.OpenOrCreate));
            BinaryReader valueReader = new BinaryReader(File.Open(valueFile, FileMode.OpenOrCreate));

            for (int i = 0; i < keyReader.BaseStream.Length; i += key.Length + 1)
            {
                key = keyReader.ReadString();
                keys.Add(Convert.ToUInt64(key));
            }
            keyReader.Close();

            int arrPos = 0;
            for (int i = 0; i < valueReader.BaseStream.Length; i += value.Length + 1)
            {
                value = valueReader.ReadString();
                str[arrPos] = value;

                if (arrPos == 0)
                {
                    arrPos = 1;
                }
                else
                {
                    values.Add(str);
                    arrPos = 0;
                }
            }

            for (int i = 0; i < keys.Count; i++)
            {
                dict.Put(keys[i], values[i]);
            }

            return await Task.Run(() =>
            {
                return dict;
            });
        }
    }
}
