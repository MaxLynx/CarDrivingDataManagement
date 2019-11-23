using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarDrivingDataManagement.Utils
{
    public class TreeNode<T>
            where T : IRecordable<T>, IComparable<T>, new()
    {
        public Block<T> Block { get; set; }

        public Int32[] Pointers { get; set; }

        public Int32 Parent { get; set; }

        public TreeNode(int blockID, int recordsPerBlockCount)
        {
            Block = new Block<T>(blockID, recordsPerBlockCount);
            Pointers = new Int32[recordsPerBlockCount + 1];
        }

        public void WriteToFile(String filename, int recordSize)
        {
            if (Block.Records[0] != null)
            {
                Block.Size = 4 + recordSize * Block.Records.Length + 4 * Pointers.Length + 4;
                Block.ByteArray = new byte[recordSize * Block.Records.Length];
                int start = Block.Size * Block.ID;
                int end = Block.Size * (Block.ID + 1) - 1;

                BinaryWriter binaryWriter = new BinaryWriter(File.OpenWrite(filename));
                binaryWriter.Seek(start, SeekOrigin.Begin);

                // Block ID
                byte[] bytes = new byte[4];
                byte[] idBytes = BitConverter.GetBytes(Block.ID);
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
                for (int i = 0; i < Block.Records.Length; i++)
                {
                    if (Block.Records[i] == null)
                    {
                        break;
                    }
                    for (int j = 0; j < Block.Records[i].ByteArray.Length; j++)
                    {
                        Block.ByteArray[i * Block.Records[i].ByteArray.Length + j] = Block.Records[i].ByteArray[j];
                    }
                }
                
                for (int i = 0; i < Block.ByteArray.Length; i++)
                {
                    if (start + i > end)
                    {
                        break;
                    }
                    binaryWriter.Write(Block.ByteArray[i]);
                }
                binaryWriter.Dispose();
            }
            
        }
    }
}
