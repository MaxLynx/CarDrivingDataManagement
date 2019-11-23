using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarDrivingDataManagement.Utils
{
    public class Block<T>
            where T : IRecordable<T>, IComparable<T>, new()
    {
        public Int32 ID { get; set; }
        public Int32 Size { get; set; }
        public byte[] ByteArray { get; set; }
        public Record<T>[] Records { get; set; }

        public Block(int id, int recordsCount)
        {
            ID = id;
            Records = new Record<T>[recordsCount];
        }


        
        public void WriteToFile(String filename)
        {
            if (Records[0] != null)
            {
                Size = Records[0].ByteArray.Length * Records.Length;
                ByteArray = new byte[Size];
                for (int i = 0; i < Records.Length; i++)
                {
                    if (Records[i] == null)
                    {
                        break;
                    }
                    for (int j = 0; j < Records[i].ByteArray.Length; j++)
                    {
                        ByteArray[i * Records[i].ByteArray.Length + j] = Records[i].ByteArray[j];
                    }
                }
                int start = Size * ID;
                int end = Size * (ID + 1) - 1;
                BinaryWriter binaryWriter = new BinaryWriter(File.OpenWrite(filename));
                binaryWriter.Seek(start, SeekOrigin.Begin);
                for (int i = 0; i <= ByteArray.Length; i++)
                {
                    if (start + i > end)
                    {
                        break;
                    }
                    binaryWriter.Write(ByteArray[i]);
                }
                binaryWriter.Dispose();
            }
        }
    }
}
