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
                                (currentNode.Block.Records[i + 1] == null ||
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
                    File.WriteAllText("C:/Users/User/source/repos/CarDrivingDataManagementTest/add" + i + ".txt",
                                currentNode.Block.Records[i].Data.ToString());
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
            
            //TODO
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
        
    }
}
