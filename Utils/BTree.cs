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

        public BTree() { }

        public BTree(int recordsPerBlockCount, String filename)
        {
            RecordsPerBlockCount = recordsPerBlockCount;
            Filename = filename;
            NextBlockID = 0;
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
                                i == RecordsPerBlockCount - 1 || currentNode.Block.Records[i + 1].CompareTo(searchParameter) > 0))
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
                        for (int i = 1; i < RecordsPerBlockCount; i++)
                        {
                            if (currentNode.Block.Records[i] == null
                                || currentNode.Block.Records[i].CompareTo(newData) < 0)
                            {
                                currentNode = currentNode.Pointers[i];
                                break;
                            }
                        }
                    }

                }
                if (currentNode.Block.Records[RecordsPerBlockCount - 1] == null) //leaf overflow check
                {
                    int i = 0;
                    while(currentNode.Block.Records[i] != null && currentNode.Block.Records[i].CompareTo(newData) < 0)
                    {
                        i++;
                    }
                    Record<T> tmp = currentNode.Block.Records[i];
                    currentNode.Block.Records[i] = new Record<T>(newData);
                    for (int j = i + 1; j < RecordsPerBlockCount; j++)
                    {

                        Record<T> tmp2 = currentNode.Block.Records[j];
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
                else //leaf overflow
                {
                    GrowTree(currentNode, newData);
                    return true;
                }
            }
        }

        private void GrowTree(TreeNode<T> node, T newData)
        {
            List<Record<T>> records = new List<Record<T>>();
            List<TreeNode<T>> pointers = new List<TreeNode<T>>();

            int i = 0;
            while(i < RecordsPerBlockCount && node.Block.Records[i].CompareTo(newData) < 0)
            {
                records.Add(node.Block.Records[i]);
                pointers.Add(node.Pointers[i + 1]);
                i++;
            }
            records.Add(new Record<T>(newData));
            pointers.Add(null);
            for (int j = i; j < RecordsPerBlockCount; j++)
            {
                records.Add(node.Block.Records[j]);
                pointers.Add(node.Pointers[j + 1]);
            }
            int medium = (RecordsPerBlockCount + 1) / 2 + (RecordsPerBlockCount + 1) % 2 - 1;
            
            TreeNode<T> newNode = new TreeNode<T>(NextBlockID, RecordsPerBlockCount);
            NextBlockID++;
            int k = 0;
            for (int j = medium + 1; j < RecordsPerBlockCount + 1; j++)
            {
                
                newNode.Block.Records[k] = records[j];
                newNode.Pointers[k+1] = pointers[j];
                k++;
                
            }
            if (node.Parent != null)
            {
                for (int j = 0; j < RecordsPerBlockCount + 1; j++)
                {
                    if (node.Parent.Pointers[j] == null)
                    {
                        node.Parent.Pointers[j] = newNode;
                        newNode.Parent = node.Parent;
                        break;
                    }
                }
            }
            for (int j = 0; j < medium; j++)
            {

                node.Block.Records[j] = records[j];

            }

            if (node.Parent != null)
            {

                AddToNode(records[medium].Data, node.Parent);
            }
            else
            {
                Root = new TreeNode<T>(NextBlockID, RecordsPerBlockCount);
                NextBlockID++;
                Root.Block.Records[0] = new Record<T>(records[medium].Data);
                Root.Pointers[0] = node;
                node.Parent = Root;
                newNode.Parent = Root;
                Root.Pointers[1] = newNode;
                WriteBlock(Root.Block);
                
            }

            for (i = 0; i < node.Block.Records.Length; i++)
            {
                if(node.Block.Records[i].CompareTo(records[medium].Data) >= 0)
                {
                    node.Block.Records[i] = null;
                    node.Pointers[i + 1] = null;
                }
            }
            WriteBlock(node.Block);
            WriteBlock(newNode.Block);
        }

        public bool AddToNode(T newData, TreeNode<T> node)
        {

            if (node.Block.Records[RecordsPerBlockCount - 1] == null) //leaf overflow check
            {
                int i = 0;
                while (node.Block.Records[i] != null && node.Block.Records[i].CompareTo(newData) < 0)
                {
                    i++;
                }
                Record<T> tmp = node.Block.Records[i];
                node.Block.Records[i] = new Record<T>(newData);
                for (int j = i + 1; j < RecordsPerBlockCount; j++)
                {

                    Record<T> tmp2 = node.Block.Records[j];
                    node.Block.Records[j] = tmp;
                    tmp = tmp2;
                    if (tmp == null)
                    {
                        break;
                    }

                }
                WriteBlock(node.Block);
                return true;

            }
            else //leaf overflow
            {
                GrowTree(node, newData);
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
                    result += "Block " + currentNode.Block.ID + "\r\n";
                    foreach (Record<T> record in currentNode.Block.Records)
                    {
                        if (record != null)
                        {
                            result += record.Data + "\r\n";
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
