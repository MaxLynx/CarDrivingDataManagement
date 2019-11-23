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
        TreeNode<T> Root { get; set; }

        public Int32 RootID { get; set; }

        TreeNode<T> FreeBlocksRoot { get; set; }

        String Filename { get; set; }

        Int32 RecordsPerBlockCount { get; set; }

        Int32 NextBlockID { get; set; }

        public int GetBlocksCount()
        {
            return NextBlockID;
        }
        public int GetBlockSize()
        {
            return Root.Block.Size;
        }
        public int GetRecordSize()
        {
            return new T().GetBytes().Length;
        }
        public BTree() { }

        public BTree(int recordsPerBlockCount, String filename)
        {
            RecordsPerBlockCount = recordsPerBlockCount;
            Filename = filename;
            NextBlockID = 1;
        }

        public BTree(int recordsPerBlockCount, String filename, int rootID, int nextBlockID)
        {
            RecordsPerBlockCount = recordsPerBlockCount;
            Filename = filename;
            RootID = rootID;
            NextBlockID = nextBlockID;
            Root = ReadBlock(RootID);
        }

        public Record<T> Find(T searchParameter)
        {
            if(Root == null)
            {
                return default;
            }
            else
            {
                TreeNode<T> currentNode = ReadBlock(RootID);
                while(currentNode != null)
                {
                    for(int i = 0; i < RecordsPerBlockCount; i++)
                    {
                        if (currentNode.Block.Records[i] == null)
                        {
                            continue;
                        }
                        else
                        {
                            

                            if (currentNode.Block.Records[i].CompareTo(searchParameter) == 0)
                            {
                                return currentNode.Block.Records[i];
                            }
                            if (currentNode.Block.Records[i].CompareTo(searchParameter) > 0 &&
                                (i == 0 || currentNode.Block.Records[i - 1].CompareTo(searchParameter) < 0))
                            {
                                currentNode = ReadBlock(currentNode.Pointers[i]);
                                break;
                            }
                            if (currentNode.Block.Records[i].CompareTo(searchParameter) < 0 &&
                                (i + 1 == RecordsPerBlockCount ||
                                currentNode.Block.Records[i + 1] == null ||
                                currentNode.Block.Records[i + 1].CompareTo(searchParameter) > 0))
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
                Root = new TreeNode<T>(NextBlockID, RecordsPerBlockCount);
                NextBlockID++;
                Root.Block.Records[0] = new Record<T>(newData);
                WriteBlock(Root);
                RootID = Root.Block.ID;
                return true;
            }
            else
            {
                TreeNode<T> currentNode = ReadBlock(RootID);
                while (!IsLeaf(currentNode))
                {
                    if (currentNode.Block.Records[0].CompareTo(newData) > 0)
                    {
                        currentNode = ReadBlock(currentNode.Pointers[0]);
                    }
                    else
                    {
                        bool found = false;
                        for (int i = 1; i < RecordsPerBlockCount; i++)
                        {
                            if (currentNode.Block.Records[i] == null
                                || currentNode.Block.Records[i].CompareTo(newData) > 0)
                            {
                                currentNode = ReadBlock(currentNode.Pointers[i]);
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            if (currentNode.Block.Records[RecordsPerBlockCount - 1] != null &&
                                currentNode.Block.Records[RecordsPerBlockCount - 1].CompareTo(newData) < 0)
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
                    
                    if (currentNode.Block.Records[RecordsPerBlockCount - 1] == null) // overflow check
                    {
                        
                        int i = 0;
                        while (currentNode.Block.Records[i] != null && currentNode.Block.Records[i].CompareTo(newData) < 0)
                        {
                            i++;
                        }
                        Record<T> tmp = currentNode.Block.Records[i];
                        int pointerTmp = currentNode.Pointers[i + 1];
                        currentNode.Block.Records[i] = new Record<T>(newData);
                        
                        currentNode.Pointers[i] = pointer1;
                        if(pointer1 != 0)
                        {
                            TreeNode<T> node = ReadBlock(pointer1);
                            node.Parent = currentNode.Block.ID;
                            WriteBlock(node);
                        }
                        if (pointer2 != 0)
                        {
                            TreeNode<T> node = ReadBlock(pointer2);
                            node.Parent = currentNode.Block.ID;
                            WriteBlock(node);
                        }
                        
                        currentNode.Pointers[i + 1] = pointer2;

                        
                        for (int j = i + 1; j < RecordsPerBlockCount; j++)
                        {

                            Record<T> tmp2 = currentNode.Block.Records[j];
                            int pointerTmp2 = currentNode.Pointers[j + 1];
                            currentNode.Pointers[j + 1] = pointerTmp;
                            pointerTmp = pointerTmp2;
                            currentNode.Block.Records[j] = tmp;
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
                            while (i < RecordsPerBlockCount && currentNode.Block.Records[i].CompareTo(newData) < 0)
                            {
                                records.Add(currentNode.Block.Records[i]);
                                pointers.Add(currentNode.Pointers[i]);
                                i++;
                            }
                        pointers.Add(pointer1);
                        records.Add(new Record<T>(newData));
                        pointers.Add(pointer2);
                        for (int j = i; j < RecordsPerBlockCount; j++)
                        {
                            records.Add(currentNode.Block.Records[j]);
                            pointers.Add(currentNode.Pointers[j + 1]);
                        }
                        int medium = (RecordsPerBlockCount + 1) / 2 + (RecordsPerBlockCount + 1) % 2 - 1;

                        TreeNode<T> newNode = new TreeNode<T>(NextBlockID, RecordsPerBlockCount);
                        NextBlockID++;
                        int k = 0;
                        newNode.Pointers[k] = pointers[medium+1];
                        if (newNode.Pointers[k] != 0)
                        {
                            TreeNode<T> node = ReadBlock(newNode.Pointers[k]);
                            node.Parent = newNode.Block.ID;
                            WriteBlock(node);
                        }
                        for (int j = medium + 1; j < RecordsPerBlockCount + 1; j++)
                        {

                            newNode.Block.Records[k] = records[j];
                            newNode.Pointers[k+1] = pointers[j + 1];
                            if (newNode.Pointers[k + 1] != 0)
                            {
                                TreeNode<T> node = ReadBlock(newNode.Pointers[k + 1]);
                                node.Parent = newNode.Block.ID;
                                WriteBlock(node);
                            }
                            k++;

                        }
                        if (currentNode.Parent != 0)
                        {
                            pointer1 = currentNode.Block.ID;
                            pointer2 = newNode.Block.ID;
                        }
                        else
                        {
                            pointer1 = 0;
                            pointer2 = 0;
                        }
                        for (int j = 0; j < medium; j++)
                        {

                            currentNode.Block.Records[j] = records[j];

                        }
                        newData = records[medium].Data;
                        bool newRootCreated = false;
                        if (currentNode.Parent == 0)
                        {
                            Root = new TreeNode<T>(NextBlockID, RecordsPerBlockCount);
                            NextBlockID++;
                            Root.Block.Records[0] = new Record<T>(records[medium].Data);
                            Root.Pointers[0] = currentNode.Block.ID;
                            currentNode.Parent = Root.Block.ID;
                            newNode.Parent = Root.Block.ID;
                            Root.Pointers[1] = newNode.Block.ID;
                            WriteBlock(Root);
                            RootID = Root.Block.ID;
                            newRootCreated = true;

                        }
                        currentNode.Pointers[0] = pointers[0];
                        if (pointers[0] != 0)
                        {
                            TreeNode<T> node = ReadBlock(pointers[0]);
                            node.Parent = currentNode.Block.ID;
                            WriteBlock(node);
                        }
                        for (i = 0; i < currentNode.Block.Records.Length; i++)
                        {
                            if (currentNode.Block.Records[i].CompareTo(records[medium].Data) >= 0)
                            {
                                
                                currentNode.Block.Records[i] = null;
                                currentNode.Pointers[i + 1] = 0;
                                
                            }
                            else
                            {
                                currentNode.Pointers[i + 1] = pointers[i + 1];
                                if (pointers[i + 1] != 0)
                                {
                                    TreeNode<T> node = ReadBlock(pointers[i+1]);
                                    node.Parent = currentNode.Block.ID;
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

        
        


        private void WriteBlock(TreeNode<T> node)
        {
            node.WriteToFile(Filename, Root.Block.Records[0].Size);
        }

        public TreeNode<T> ReadBlock(int id)
        {
            if(id == 0)
            {
                return null;
            }
            int blockSize = 4 + GetRecordSize() * RecordsPerBlockCount
                + 4 * (RecordsPerBlockCount + 1) + 4;
            TreeNode<T> node = new TreeNode<T>(id, RecordsPerBlockCount);
            BinaryReader binaryReader =
                new BinaryReader(File.Open(Filename,
                FileMode.Open));
            binaryReader.BaseStream.Seek(blockSize * id, SeekOrigin.Begin);
            byte[] bytes = binaryReader.ReadBytes(blockSize);
            node.Parent = BitConverter.ToInt32(bytes.Skip(4).Take(4).ToArray(), 0);
            for (int i = 1; i <= RecordsPerBlockCount + 1; i++)
            {
                node.Pointers[i - 1] = BitConverter.ToInt32(bytes.Skip(4 + 4*i).Take(4).ToArray(), 0);
            }
            for(int i = 0; i < RecordsPerBlockCount; i++)
            {
                node.Block.Records[i] =
                    new Record<T>(bytes.Skip(4 + 4 + 4 * (RecordsPerBlockCount + 1) + i * GetRecordSize()
                    ).Take(GetRecordSize()).ToArray());
                if (node.Block.Records[i].Data.IsNull())
                {
                    node.Block.Records[i] = null;
                }
            }
            binaryReader.Dispose();
            return node;
        }


        private bool IsLeaf(TreeNode<T> node)
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
            Queue<TreeNode<T>> queue = new Queue<TreeNode<T>>();
            TreeNode<T> currentNode = ReadBlock(RootID);

            int currentLevelNodesCount = 1;
            int nextLevelNodesCount = 0;

            queue.Enqueue(currentNode);
            while (queue.Count > 0)
            {
                for (int i = 0; i < currentLevelNodesCount; i++)
                {
                    currentNode = queue.Dequeue();
                    result += "Block " + currentNode.Block.ID;
                    if (currentNode.Parent != 0)
                    {
                        result += "(Parent: " + currentNode.Parent + ")\r\n";
                    }
                    else
                    {
                        result += "(Parent: null)\r\n";
                    }
                    foreach (Record<T> record in currentNode.Block.Records)
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
        
    }
}
