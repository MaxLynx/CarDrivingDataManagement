using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarDrivingDataManagement.Utils
{
    public class HeapFile<T>
            where T : IRecordable<T>, IComparable<T>, new()
    {
        
        BinaryReader BinaryReader { get; set; }
        BinaryWriter BinaryWriter { get; set; }

        public Int32 RecordsPerBlockCount { get; set; }

        public Int32 NextBlockID { get; set; }
        
        public int GetBlockSize() //min is 20 bytes
        {
            return ReadBlock(1).RecordsArray.Size;
        }
        public int GetRecordSize()
        {
            return new Record<T>(new T()).Size;
        }

        public String Filename { get; set; }

        public HeapFile() { }
        
        public HeapFile(int recordsPerBlockCount, String filename)
        {
            RecordsPerBlockCount = recordsPerBlockCount;
            Filename = filename;
            NextBlockID = 2;
            OpenFileInteraction();
        }

        public HeapFile(String filename)
        {
            Filename = filename;
            OpenFileInteraction();
            BinaryReader.BaseStream.Seek(0, SeekOrigin.Begin);
            RecordsPerBlockCount = BitConverter.ToInt32(BinaryReader.ReadBytes(4), 0);
            NextBlockID = BitConverter.ToInt32(BinaryReader.ReadBytes(4), 0);
        }
        public int Add(T data)
        {
            Block<T> currentBlock = ReadBlock(NextBlockID - 1);
            if (currentBlock == null) 
            {
                currentBlock = new Block<T>(NextBlockID - 1, RecordsPerBlockCount);
                currentBlock.RecordsArray.Records[0] = new Record<T>(data);
                WriteBlock(currentBlock);
                return (NextBlockID - 1) * RecordsPerBlockCount;
            }
            else
            if(currentBlock.RecordsArray.Records[RecordsPerBlockCount-1] != null)
            {
                currentBlock = new Block<T>(NextBlockID, RecordsPerBlockCount);
                NextBlockID++;
                currentBlock.RecordsArray.Records[0] = new Record<T>(data);
                WriteBlock(currentBlock);
                return (NextBlockID - 1) * RecordsPerBlockCount;
            }
            else
            {
                int i = 0;
                while (currentBlock.RecordsArray.Records[i] != null)
                {
                    i++;
                }

                currentBlock.RecordsArray.Records[i] = new Record<T>(data);
                WriteBlock(currentBlock);
                return (NextBlockID - 1) * RecordsPerBlockCount + i;
            }
        }

        public T FindData(int address)
        {
            int blockIndex = address / RecordsPerBlockCount;
            int recordIndex = address - blockIndex * RecordsPerBlockCount;
            return ReadBlock(blockIndex).RecordsArray.Records[recordIndex].Data;
        }

        public Block<T> FindBlock(int address)
        {
            int blockIndex = address / RecordsPerBlockCount;
            return ReadBlock(blockIndex);
        }

        public void WriteBlock(Block<T> block)
        {
            if (block.RecordsArray.Records[0] != null)
            {
                int recordSize = GetRecordSize();
                block.RecordsArray.Size = recordSize * block.RecordsArray.Records.Length;
                block.RecordsArray.ByteArray = new byte[recordSize * block.RecordsArray.Records.Length];
                int start = block.RecordsArray.Size * block.RecordsArray.ID;
                int end = block.RecordsArray.Size * (block.RecordsArray.ID + 1) - 1;
                BinaryWriter.Seek(start, SeekOrigin.Begin);

                for (int i = 0; i < block.RecordsArray.Records.Length; i++)
                {
                    if (block.RecordsArray.Records[i] == null)
                    {
                        break;
                    }
                    for (int j = 0; j < block.RecordsArray.Records[i].Size; j++)
                    {
                        block.RecordsArray.ByteArray[i * block.RecordsArray.Records[i].Size + j] = 
                            block.RecordsArray.Records[i].ByteArray[j];
                    }
                }

                for (int i = 0; i < block.RecordsArray.ByteArray.Length; i++)
                {
                    if (start + i > end)
                    {
                        break;
                    }
                    BinaryWriter.Write(block.RecordsArray.ByteArray[i]);
                }
                BinaryWriter.Flush();
            }

        }

        public Block<T> ReadBlock(int id)
        {
            if (id == 0)
            {
                return null;
            }
            int blockSize = GetRecordSize() * RecordsPerBlockCount;
            Block<T> node = new Block<T>(id, RecordsPerBlockCount);
                BinaryReader.BaseStream.Seek(blockSize * id, SeekOrigin.Begin);
                byte[] bytes = BinaryReader.ReadBytes(blockSize);
            if(bytes.Length == 0)
            {
                return null;
            }

                for (int i = 0; i < RecordsPerBlockCount; i++)
                {
                node.RecordsArray.Records[i] =
                        new Record<T>(bytes.Skip(i * GetRecordSize())
                        .Take(GetRecordSize()).ToArray());
                    if (node.RecordsArray.Records[i].Data == null ||
                    node.RecordsArray.Records[i].Data.IsNull() || !node.RecordsArray.Records[i].Used)
                    {
                        node.RecordsArray.Records[i] = null;
                    }
                }

                return node;
            
        }
        
        public void OpenFileInteraction()
        {
            BinaryReader =
                new BinaryReader(File.Open(Filename,
                FileMode.Open, FileAccess.Read, FileShare.Write));
            BinaryWriter = new BinaryWriter(File.Open(Filename, FileMode.Open, FileAccess.Write, FileShare.Read));
        }
        public void CloseFileInteraction()
        {
            BinaryWriter.Seek(0, SeekOrigin.Begin);
            BinaryWriter.Write(BitConverter.GetBytes(RecordsPerBlockCount));
            BinaryWriter.Write(BitConverter.GetBytes(NextBlockID));
            BinaryWriter.Flush();

            BinaryReader.Dispose();
            BinaryWriter.Dispose();
        }

        public String DescribeFileStructure()
        {
            String result = "";
            BinaryReader.BaseStream.Seek(0, SeekOrigin.Begin);

            int blockSize = GetBlockSize();
            int recordSize = GetRecordSize();

            for (int i = 0; i < NextBlockID; i++)
            {
                result += "\r\nBlock " + i + "\r\n";
                
                for (int k = 0; k < RecordsPerBlockCount; k++)
                {
                    byte[] instance = new byte[recordSize];
                    String record = "null";
                    try
                    {
                        for (int j = 0; j < recordSize; j++)
                        {

                            instance[j] = BinaryReader.ReadByte();
                        }
                        Record<T> recordObj = new Record<T>(instance);
                        record = recordObj.Data.ToString() + " [USED: " + recordObj.Used + "]";
                    }
                    catch (Exception e)
                    {

                    }

                    result += "Record " + (k + 1) + ": ";
                    result += record + "\r\n";

                }

            }

            return result;
        }

    }
}
