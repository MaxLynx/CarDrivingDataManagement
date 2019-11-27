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
        public RecordsArray<T> RecordsArray { get; set; }

        public Int32[] Pointers { get; set; }

        public Int32 Parent { get; set; }

        public Block(int blockID, int recordsPerBlockCount)
        {
            RecordsArray = new RecordsArray<T>(blockID, recordsPerBlockCount);
            Pointers = new Int32[recordsPerBlockCount + 1];
        }

        public void WriteToFile(BinaryWriter binaryWriter, int recordSize)
        {
            File.WriteAllText("C:/Users/User/source/repos/CarDrivingDataManagementTest/called.txt", "Called");
            if (RecordsArray.Records[0] != null)
            {
                RecordsArray.Size = 4 + recordSize * RecordsArray.Records.Length + 4 * Pointers.Length + 4;
                RecordsArray.ByteArray = new byte[recordSize * RecordsArray.Records.Length];
                int start = RecordsArray.Size * RecordsArray.ID;
                int end = RecordsArray.Size * (RecordsArray.ID + 1) - 1;
                File.WriteAllText("C:/Users/User/source/repos/CarDrivingDataManagementTest/start.txt", ""+start);
                binaryWriter.Seek(start, SeekOrigin.Begin);

                // Block ID
                byte[] bytes = new byte[4];
                byte[] idBytes = BitConverter.GetBytes(RecordsArray.ID);
                for (int i = 0; i < idBytes.Length; i++)
                {
                    bytes[i] = idBytes[i];
                }
                foreach (byte _byte in bytes)
                {
                    binaryWriter.Write(_byte);
                }

                // Parent ID
                bytes = new byte[4];
                idBytes = BitConverter.GetBytes(Parent);
                for (int i = 0; i < idBytes.Length; i++)
                {
                    bytes[i] = idBytes[i];
                }
                foreach (byte _byte in bytes)
                {
                    binaryWriter.Write(_byte);
                }

                // Pointers
                foreach (int pointer in Pointers)
                {
                    bytes = new byte[4];
                    idBytes = BitConverter.GetBytes(pointer);
                    for (int i = 0; i < idBytes.Length; i++)
                    {
                        bytes[i] = idBytes[i];
                    }
                    foreach (byte _byte in bytes)
                    {
                        binaryWriter.Write(_byte);
                    }
                }

                // Records
                for (int i = 0; i < RecordsArray.Records.Length; i++)
                {
                    if (RecordsArray.Records[i] == null)
                    {
                        break;
                    }
                    for (int j = 0; j < RecordsArray.Records[i].ByteArray.Length; j++)
                    {
                        RecordsArray.ByteArray[i * RecordsArray.Records[i].ByteArray.Length + j] = RecordsArray.Records[i].ByteArray[j];
                    }
                }
                
                for (int i = 0; i < RecordsArray.ByteArray.Length; i++)
                {
                    if (start + i > end)
                    {
                        break;
                    }
                    binaryWriter.Write(RecordsArray.ByteArray[i]);
                }
                binaryWriter.Flush();
            }
            
        }
    }
}
