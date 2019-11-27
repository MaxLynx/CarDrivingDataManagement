using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CarDrivingDataManagement.Utils
{
    public class BTree<T>
            where T : IRecordable<T>, IComparable<T>, new()
    {
        Block<T> Root { get; set; }

        public Int32 RootID { get; set; }

        Block<T> FreeBlocksRoot { get; set; }

        String Filename { get; set; }

        Int32 RecordsPerBlockCount { get; set; }

        Int32 NextBlockID { get; set; }

        BinaryReader BinaryReader { get; set; }
        BinaryWriter BinaryWriter { get; set; }

        public int GetBlocksCount()
        {
            return NextBlockID;
        }
        public int GetBlockSize()
        {
            return Root.RecordsArray.Size;
        }
        public int GetRecordSize()
        {
            return new Record<T>(new T()).Size;
        }
        public BTree() { }

        public BTree(int recordsPerBlockCount, String filename)
        {
            RecordsPerBlockCount = recordsPerBlockCount;
            Filename = filename;
            NextBlockID = 1;
            OpenFileInteraction();
        }

        public BTree(int recordsPerBlockCount, String filename, int rootID, int nextBlockID)
        {
            RecordsPerBlockCount = recordsPerBlockCount;
            Filename = filename;
            RootID = rootID;
            NextBlockID = nextBlockID;
            OpenFileInteraction();
            Root = ReadBlock(RootID);
        }

        public Block<T> Find(T searchParameter)
        {
            if(Root == null)
            {
                return default;
            }
            else
            {
                Block<T> currentNode = ReadBlock(RootID);
                while(currentNode != null)
                {
                    for(int i = 0; i < RecordsPerBlockCount; i++)
                    {
                        if (currentNode.RecordsArray.Records[i] == null)
                        {
                            continue;
                        }
                        else
                        {
                            

                            if (currentNode.RecordsArray.Records[i].CompareTo(searchParameter) == 0)
                            {
                                return currentNode;
                            }
                            if (currentNode.RecordsArray.Records[i].CompareTo(searchParameter) > 0 &&
                                (i == 0 || currentNode.RecordsArray.Records[i - 1].CompareTo(searchParameter) < 0))
                            {
                                currentNode = ReadBlock(currentNode.Pointers[i]);
                                break;
                            }
                            if (currentNode.RecordsArray.Records[i].CompareTo(searchParameter) < 0 &&
                                (i + 1 == RecordsPerBlockCount ||
                                currentNode.RecordsArray.Records[i + 1] == null ||
                                currentNode.RecordsArray.Records[i + 1].CompareTo(searchParameter) > 0))
                            {
                                currentNode = ReadBlock(currentNode.Pointers[i + 1]);
                                break;
                            }
                        }
                    }
                }
                return default;
            }
        }

        public bool Delete(T data)
        {
            Block<T> currentBlock = Find(data);
            if(currentBlock == null)
            {
                return false;
            }
            else
            {
                while (currentBlock != null)
                {
                    if (IsLeaf(currentBlock) &&
                    currentBlock.Pointers[(int)Math.Ceiling((RecordsPerBlockCount + 1) * 1.0 / 2)] != 0) //minimal accepted records count
                    {
                        foreach (Record<T> record in currentBlock.RecordsArray.Records)
                        {
                            if (record.Data.CompareTo(data) == 0)
                            {
                                record.Used = false;
                                WriteBlock(currentBlock);
                                return true;
                            }
                        }
                    }
                    else
                    if (!IsLeaf(currentBlock))
                    {
                        for (int i = 0; i < RecordsPerBlockCount; i++)
                        {
                            Record<T> record = currentBlock.RecordsArray.Records[i];
                            if (record.Data.CompareTo(data) == 0)
                            {
                                Block<T> successor = ReadBlock(currentBlock.Pointers[i + 1]); //could it also be a left son?
                                while (!IsLeaf(successor))
                                {
                                    successor = ReadBlock(currentBlock.Pointers[0]);
                                }
                                currentBlock.RecordsArray.Records[i] = successor.RecordsArray.Records[0];
                                WriteBlock(currentBlock);
                                currentBlock = successor;
                                data = successor.RecordsArray.Records[0].Data;
                            }
                        }
                    }
                    else
                    if (IsLeaf(currentBlock) &&
                    currentBlock.Pointers[(int)Math.Ceiling((RecordsPerBlockCount + 1) * 1.0 / 2)] == 0)
                    {
                        Block<T> parent = ReadBlock(currentBlock.Parent);
                        int i = 0;
                        for (i = 0; i < RecordsPerBlockCount; i++)
                        {
                            if (i == currentBlock.RecordsArray.ID)
                            {
                                break;
                            }
                        }
                        Block<T> left = ReadBlock(parent.Pointers[i]);
                        Block<T> right = null;
                        if (i + 1 <= RecordsPerBlockCount)
                        {
                            right = ReadBlock(parent.Pointers[i + 1]);
                        }
                        if (i + 1 <= RecordsPerBlockCount && parent.Pointers[i + 1] != 0
                            && right.Pointers[(int)Math.Ceiling((RecordsPerBlockCount + 1) * 1.0 / 2)] != 0)
                        {
                            for (int j = 0; j < RecordsPerBlockCount; j++)
                            {
                                if (currentBlock.RecordsArray.Records[j].Data.CompareTo(data) == 0)
                                {
                                    currentBlock.RecordsArray.Records[j] = parent.RecordsArray.Records[i];
                                }
                            }
                            WriteBlock(currentBlock);
                            parent.RecordsArray.Records[i] = right.RecordsArray.Records[0];
                            WriteBlock(parent);
                            currentBlock = right;
                            data = right.RecordsArray.Records[0].Data;
                        }
                        else
                            if (parent.Pointers[i] != 0
                            && left.Pointers[(int)Math.Ceiling((RecordsPerBlockCount + 1) * 1.0 / 2)] != 0)
                        {
                            for (int j = 0; j < RecordsPerBlockCount; j++)
                            {
                                if (currentBlock.RecordsArray.Records[j].Data.CompareTo(data) == 0)
                                {
                                    currentBlock.RecordsArray.Records[j] = parent.RecordsArray.Records[i];
                                }
                            }
                            WriteBlock(currentBlock);
                            parent.RecordsArray.Records[i] = left.RecordsArray.Records[0];
                            WriteBlock(parent);
                            currentBlock = left;
                            data = left.RecordsArray.Records[0].Data;
                        }
                        else
                        {
                            int start = (int)Math.Ceiling((RecordsPerBlockCount + 1) * 1.0 / 2);
                            
                                left.RecordsArray.Records[start] = parent.RecordsArray.Records[i];
                                int k = 0;
                                for (int j = start + 1; j < RecordsPerBlockCount; j++)
                                {
                                    left.RecordsArray.Records[j] = currentBlock.RecordsArray.Records[k];
                                    k++;
                                }
                                WriteBlock(left);
                                for(int j = i; j < RecordsPerBlockCount - 1; j++)
                                {
                                    parent.RecordsArray.Records[j] = parent.RecordsArray.Records[j + 1];
                                    parent.Pointers[j] = parent.Pointers[j + 1];
                                }
                                WriteBlock(parent);
                            //cyclic movement to the root
                        }
                    }
                }
            }
            return false;
        }

        public bool Contains(T searchParameter)
        {
            return Find(searchParameter) != null;
        }

        public bool Add(T newData)
        {
            if (Contains(newData))
                return false;
            if(Root == null)
            {
                Root = new Block<T>(NextBlockID, RecordsPerBlockCount);
                NextBlockID++;
                Root.RecordsArray.Records[0] = new Record<T>(newData);
                WriteBlock(Root);
                RootID = Root.RecordsArray.ID;
                return true;
            }
            else
            {
                Block<T> currentNode = ReadBlock(RootID);
                while (!IsLeaf(currentNode))
                {
                    if (currentNode.RecordsArray.Records[0].CompareTo(newData) > 0)
                    {
                        currentNode = ReadBlock(currentNode.Pointers[0]);
                    }
                    else
                    {
                        bool found = false;
                        for (int i = 1; i < RecordsPerBlockCount; i++)
                        {
                            if (currentNode.RecordsArray.Records[i] == null
                                || currentNode.RecordsArray.Records[i].CompareTo(newData) > 0)
                            {
                                currentNode = ReadBlock(currentNode.Pointers[i]);
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            if (currentNode.RecordsArray.Records[RecordsPerBlockCount - 1] != null &&
                                currentNode.RecordsArray.Records[RecordsPerBlockCount - 1].CompareTo(newData) < 0)
                            {
                                currentNode = ReadBlock(currentNode.Pointers[RecordsPerBlockCount]);
                            }
                        }
                    }

                }
                int pointer1 = 0;
                int pointer2 = 0;
                while (currentNode != null)
                {
                    
                    if (currentNode.RecordsArray.Records[RecordsPerBlockCount - 1] == null) // overflow check
                    {
                        
                        int i = 0;
                        while (currentNode.RecordsArray.Records[i] != null && currentNode.RecordsArray.Records[i].CompareTo(newData) < 0)
                        {
                            i++;
                        }
                        Record<T> tmp = currentNode.RecordsArray.Records[i];
                        int pointerTmp = currentNode.Pointers[i + 1];
                        currentNode.RecordsArray.Records[i] = new Record<T>(newData);
                        
                        currentNode.Pointers[i] = pointer1;
                        if(pointer1 != 0)
                        {
                            Block<T> node = ReadBlock(pointer1);
                            node.Parent = currentNode.RecordsArray.ID;
                            WriteBlock(node);
                        }
                        if (pointer2 != 0)
                        {
                            Block<T> node = ReadBlock(pointer2);
                            node.Parent = currentNode.RecordsArray.ID;
                            WriteBlock(node);
                        }
                        
                        currentNode.Pointers[i + 1] = pointer2;

                        
                        for (int j = i + 1; j < RecordsPerBlockCount; j++)
                        {

                            Record<T> tmp2 = currentNode.RecordsArray.Records[j];
                            int pointerTmp2 = currentNode.Pointers[j + 1];
                            currentNode.Pointers[j + 1] = pointerTmp;
                            pointerTmp = pointerTmp2;
                            currentNode.RecordsArray.Records[j] = tmp;
                            tmp = tmp2;
                            if (tmp == null)
                            {
                                break;
                            }

                        }

                        
                        WriteBlock(currentNode);
                        return true;

                    }
                    else //overflow
                    {
                        List<Record<T>> records = new List<Record<T>>();
                        List<Int32> pointers = new List<Int32>();

                        int i = 0;
                            while (i < RecordsPerBlockCount && currentNode.RecordsArray.Records[i].CompareTo(newData) < 0)
                            {
                                records.Add(currentNode.RecordsArray.Records[i]);
                                pointers.Add(currentNode.Pointers[i]);
                                i++;
                            }
                        pointers.Add(pointer1);
                        records.Add(new Record<T>(newData));
                        pointers.Add(pointer2);
                        for (int j = i; j < RecordsPerBlockCount; j++)
                        {
                            records.Add(currentNode.RecordsArray.Records[j]);
                            pointers.Add(currentNode.Pointers[j + 1]);
                        }
                        int medium = (RecordsPerBlockCount + 1) / 2 + (RecordsPerBlockCount + 1) % 2 - 1;

                        Block<T> newNode = new Block<T>(NextBlockID, RecordsPerBlockCount);
                        NextBlockID++;
                        int k = 0;
                        newNode.Pointers[k] = pointers[medium+1];
                        if (newNode.Pointers[k] != 0)
                        {
                            Block<T> node = ReadBlock(newNode.Pointers[k]);
                            node.Parent = newNode.RecordsArray.ID;
                            WriteBlock(node);
                        }
                        for (int j = medium + 1; j < RecordsPerBlockCount + 1; j++)
                        {

                            newNode.RecordsArray.Records[k] = records[j];
                            newNode.Pointers[k+1] = pointers[j + 1];
                            if (newNode.Pointers[k + 1] != 0)
                            {
                                Block<T> node = ReadBlock(newNode.Pointers[k + 1]);
                                node.Parent = newNode.RecordsArray.ID;
                                WriteBlock(node);
                            }
                            k++;

                        }
                        if (currentNode.Parent != 0)
                        {
                            pointer1 = currentNode.RecordsArray.ID;
                            pointer2 = newNode.RecordsArray.ID;
                        }
                        else
                        {
                            pointer1 = 0;
                            pointer2 = 0;
                        }
                        for (int j = 0; j < medium; j++)
                        {

                            currentNode.RecordsArray.Records[j] = records[j];

                        }
                        newData = records[medium].Data;
                        bool newRootCreated = false;
                        if (currentNode.Parent == 0)
                        {
                            Root = new Block<T>(NextBlockID, RecordsPerBlockCount);
                            NextBlockID++;
                            Root.RecordsArray.Records[0] = new Record<T>(records[medium].Data);
                            Root.Pointers[0] = currentNode.RecordsArray.ID;
                            currentNode.Parent = Root.RecordsArray.ID;
                            newNode.Parent = Root.RecordsArray.ID;
                            Root.Pointers[1] = newNode.RecordsArray.ID;
                            WriteBlock(Root);
                            RootID = Root.RecordsArray.ID;
                            newRootCreated = true;

                        }
                        currentNode.Pointers[0] = pointers[0];
                        if (pointers[0] != 0)
                        {
                            Block<T> node = ReadBlock(pointers[0]);
                            node.Parent = currentNode.RecordsArray.ID;
                            WriteBlock(node);
                        }
                        for (i = 0; i < currentNode.RecordsArray.Records.Length; i++)
                        {
                            if (currentNode.RecordsArray.Records[i].CompareTo(records[medium].Data) >= 0)
                            {
                                
                                currentNode.RecordsArray.Records[i] = null;
                                currentNode.Pointers[i + 1] = 0;
                                
                            }
                            else
                            {
                                currentNode.Pointers[i + 1] = pointers[i + 1];
                                if (pointers[i + 1] != 0)
                                {
                                    Block<T> node = ReadBlock(pointers[i+1]);
                                    node.Parent = currentNode.RecordsArray.ID;
                                    WriteBlock(node);
                                }
                            }
                        }
                        WriteBlock(currentNode);
                        WriteBlock(newNode);
                        currentNode = ReadBlock(currentNode.Parent);
                        if(newRootCreated)
                        {
                            currentNode = null;
                        }
                        
                    }

                }
                return true;
            }

        }

        
        


        private void WriteBlock(Block<T> node)
        {
            node.WriteToFile(BinaryWriter, Root.RecordsArray.Records[0].Size);
        }

        public Block<T> ReadBlock(int id)
        {
            if(id == 0)
            {
                return null;
            }
            int blockSize = 4 + GetRecordSize() * RecordsPerBlockCount
                + 4 * (RecordsPerBlockCount + 1) + 4;
            Block<T> node = new Block<T>(id, RecordsPerBlockCount);
            BinaryReader.BaseStream.Seek(blockSize * id, SeekOrigin.Begin);
            byte[] bytes = BinaryReader.ReadBytes(blockSize);

            node.Parent = BitConverter.ToInt32(bytes.Skip(4).Take(4).ToArray(), 0);
            for (int i = 1; i <= RecordsPerBlockCount + 1; i++)
            {
                node.Pointers[i - 1] = BitConverter.ToInt32(bytes.Skip(4 + 4*i).Take(4).ToArray(), 0);
            }
            for(int i = 0; i < RecordsPerBlockCount; i++)
            {
                node.RecordsArray.Records[i] =
                    new Record<T>(bytes.Skip(4 + 4 + 4 * (RecordsPerBlockCount + 1) + i * GetRecordSize()
                    ).Take(GetRecordSize()).ToArray());
                if (node.RecordsArray.Records[i].Data.IsNull() || !node.RecordsArray.Records[i].Used)
                {
                    node.RecordsArray.Records[i] = null;
                }
            }
            return node;
        }


        private bool IsLeaf(Block<T> node)
        {
            
            foreach(int pointer in node.Pointers)
            {
                if (pointer != 0)
                    return false;
            }
            return true;
        }

        public String TraceLevelOrder()
        {
            String result = "";
            Queue<Block<T>> queue = new Queue<Block<T>>();
            Block<T> currentNode = ReadBlock(RootID);

            int currentLevelNodesCount = 1;
            int nextLevelNodesCount = 0;

            queue.Enqueue(currentNode);
            while (queue.Count > 0)
            {
                for (int i = 0; i < currentLevelNodesCount; i++)
                {
                    currentNode = queue.Dequeue();
                    result += "Block " + currentNode.RecordsArray.ID;
                    if (currentNode.Parent != 0)
                    {
                        result += "(Parent: " + currentNode.Parent + ")\r\n";
                    }
                    else
                    {
                        result += "(Parent: null)\r\n";
                    }
                    foreach (Record<T> record in currentNode.RecordsArray.Records)
                    {
                        if (record != null)
                        {
                            result += record.Data + "\r\n";
                        }
                        else
                        {
                            result += "null\r\n";
                        }
                    }
                    result += "References: ";
                    foreach (int pointer in currentNode.Pointers)
                    {
                        if (pointer != 0)
                        {
                            result += pointer + "; ";
                        }
                        else
                        {
                            result += "null; ";
                        }
                    }
                    result += "\r\n***\r\n";
                    foreach(int pointer in currentNode.Pointers)
                    {
                        if(pointer != 0)
                        {
                            nextLevelNodesCount++;
                            queue.Enqueue(ReadBlock(pointer));
                        }
                    }
                    
                }

                result += "LEVEL END\r\n";
                currentLevelNodesCount = nextLevelNodesCount;
                nextLevelNodesCount = 0;
            }

            return result;
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
            BinaryReader.Dispose();
            BinaryWriter.Dispose();
        }

    }
}
