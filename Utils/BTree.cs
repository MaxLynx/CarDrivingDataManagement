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
            return Root.Block.ByteArray.Length;
        }
        public int GetRecordSize()
        {
            return Root.Block.Records[0].Size;
        }
        public BTree() { }

        public BTree(int recordsPerBlockCount, String filename)
        {
            RecordsPerBlockCount = recordsPerBlockCount;
            Filename = filename;
            NextBlockID = 0;
        }

        public BTree(int recordsPerBlockCount, String filename, int nextBlockID)
        {
            RecordsPerBlockCount = recordsPerBlockCount;
            Filename = filename;
            NextBlockID = nextBlockID;
        }

        public Record<T> Find(T searchParameter)
        {
            if(Root == null)
            {
                return default;
            }
            else
            {
                TreeNode<T> currentNode = Root;
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
                                currentNode = currentNode.Pointers[i];
                                break;
                            }
                            if (currentNode.Block.Records[i].CompareTo(searchParameter) < 0 &&
                                (i + 1 == RecordsPerBlockCount ||
                                currentNode.Block.Records[i + 1] == null ||
                                currentNode.Block.Records[i + 1].CompareTo(searchParameter) > 0))
                            {
                                currentNode = currentNode.Pointers[i + 1];
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
                WriteBlock(Root.Block);
                return true;
            }
            else
            {
                TreeNode<T> currentNode = Root;
                while (!IsLeaf(currentNode))
                {
                    if (currentNode.Block.Records[0].CompareTo(newData) > 0)
                    {
                        currentNode = currentNode.Pointers[0];
                    }
                    else
                    {
                        bool found = false;
                        for (int i = 1; i < RecordsPerBlockCount; i++)
                        {
                            if (currentNode.Block.Records[i] == null
                                || currentNode.Block.Records[i].CompareTo(newData) > 0)
                            {
                                currentNode = currentNode.Pointers[i];
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            if (currentNode.Block.Records[RecordsPerBlockCount - 1] != null &&
                                currentNode.Block.Records[RecordsPerBlockCount - 1].CompareTo(newData) < 0)
                            {
                                currentNode = currentNode.Pointers[RecordsPerBlockCount];
                            }
                        }
                    }

                }
                TreeNode<T> pointer1 = null;
                TreeNode<T> pointer2 = null;
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
                        TreeNode<T> pointerTmp = currentNode.Pointers[i + 1];
                        currentNode.Block.Records[i] = new Record<T>(newData);
                        currentNode.Pointers[i] = pointer1;
                        if(pointer1 != null)
                        {
                            pointer1.Parent = currentNode;
                        }
                        if (pointer2 != null)
                        {
                            pointer2.Parent = currentNode;
                        }
                        currentNode.Pointers[i + 1] = pointer2;
                        for (int j = i + 1; j < RecordsPerBlockCount; j++)
                        {

                            Record<T> tmp2 = currentNode.Block.Records[j];
                            TreeNode<T> pointerTmp2 = currentNode.Pointers[j + 1];
                            currentNode.Pointers[j + 1] = pointerTmp;
                            pointerTmp = pointerTmp2;
                            currentNode.Block.Records[j] = tmp;
                            tmp = tmp2;
                            if (tmp == null)
                            {
                                break;
                            }

                        }
                        WriteBlock(currentNode.Block);
                        return true;

                    }
                    else //overflow
                    {
                        List<Record<T>> records = new List<Record<T>>();
                        List<TreeNode<T>> pointers = new List<TreeNode<T>>();

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
                        if (newNode.Pointers[k] != null)
                        {
                            newNode.Pointers[k].Parent = newNode;
                        }
                        for (int j = medium + 1; j < RecordsPerBlockCount + 1; j++)
                        {

                            newNode.Block.Records[k] = records[j];
                            newNode.Pointers[k+1] = pointers[j + 1];
                            if (newNode.Pointers[k + 1] != null)
                            {
                                newNode.Pointers[k + 1].Parent = newNode;
                            }
                            k++;

                        }
                        if (currentNode.Parent != null)
                        {
                            pointer1 = currentNode;
                            pointer2 = newNode;
                        }
                        else
                        {
                            pointer1 = null;
                            pointer2 = null;
                        }
                        for (int j = 0; j < medium; j++)
                        {

                            currentNode.Block.Records[j] = records[j];

                        }
                        newData = records[medium].Data;
                        bool newRootCreated = false;
                        if (currentNode.Parent == null)
                        {
                            Root = new TreeNode<T>(NextBlockID, RecordsPerBlockCount);
                            NextBlockID++;
                            Root.Block.Records[0] = new Record<T>(records[medium].Data);
                            Root.Pointers[0] = currentNode;
                            currentNode.Parent = Root;
                            newNode.Parent = Root;
                            Root.Pointers[1] = newNode;
                            WriteBlock(Root.Block);
                            newRootCreated = true;

                        }
                        currentNode.Pointers[0] = pointers[0];
                        if (pointers[0] != null)
                        {
                            pointers[0].Parent = currentNode;
                        }
                        for (i = 0; i < currentNode.Block.Records.Length; i++)
                        {
                            if (currentNode.Block.Records[i].CompareTo(records[medium].Data) >= 0)
                            {
                                
                                currentNode.Block.Records[i] = null;
                                currentNode.Pointers[i + 1] = null;
                                
                            }
                            else
                            {
                                currentNode.Pointers[i + 1] = pointers[i + 1];
                                if (pointers[i + 1] != null)
                                {
                                    pointers[i + 1].Parent = currentNode;
                                }
                            }
                        }
                        WriteBlock(currentNode.Block);
                        WriteBlock(newNode.Block);
                        currentNode = currentNode.Parent;
                        if(newRootCreated)
                        {
                            currentNode = null;
                        }
                        
                    }

                }
                return true;
            }

        }

        
        


        private void WriteBlock(Block<T> block)
        {
            block.WriteToFile(Filename);
        }

        
        private bool IsLeaf(TreeNode<T> node)
        {
            
            foreach(TreeNode<T> pointer in node.Pointers)
            {
                if (pointer != null)
                    return false;
            }
            return true;
        }

        public String TraceLevelOrder()
        {
            String result = "";
            Queue<TreeNode<T>> queue = new Queue<TreeNode<T>>();
            TreeNode<T> currentNode = Root;

            int currentLevelNodesCount = 1;
            int nextLevelNodesCount = 0;

            queue.Enqueue(Root);
            while (queue.Count > 0)
            {
                for (int i = 0; i < currentLevelNodesCount; i++)
                {
                    currentNode = queue.Dequeue();
                    result += "Block " + currentNode.Block.ID;
                    if (currentNode.Parent != null)
                    {
                        result += "(Parent: " + currentNode.Parent.Block.ID + ")\r\n";
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
                    foreach (TreeNode<T> pointer in currentNode.Pointers)
                    {
                        if (pointer != null)
                        {
                            result += pointer.Block.ID + "; ";
                        }
                        else
                        {
                            result += "null; ";
                        }
                    }
                    result += "\r\n***\r\n";
                    foreach(TreeNode<T> pointer in currentNode.Pointers)
                    {
                        if(pointer != null)
                        {
                            nextLevelNodesCount++;
                            queue.Enqueue(pointer);
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
