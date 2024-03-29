﻿using System;
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

        public Int32 FreeBlocksRoot { get; set; }

        public Int32 FreeBlocksEnd { get; set; }


        String Filename { get; set; }

        Int32 RecordsPerBlockCount { get; set; }

        Int32 NextBlockID { get; set; }

        BinaryReader BinaryReader { get; set; }
        BinaryWriter BinaryWriter { get; set; }

        public int GetBlocksCount()
        {
            return NextBlockID;
        }
        public int GetBlockSize() //min is 20 bytes
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
            FreeBlocksRoot = 0;
            FreeBlocksEnd = 0;
            OpenFileInteraction();
        }

        public BTree(String filename)
        {
            Filename = filename;
            OpenFileInteraction();
            BinaryReader.BaseStream.Seek(0, SeekOrigin.Begin);
            RecordsPerBlockCount = BitConverter.ToInt32(BinaryReader.ReadBytes(4), 0);
            RootID = BitConverter.ToInt32(BinaryReader.ReadBytes(4), 0);
            NextBlockID = BitConverter.ToInt32(BinaryReader.ReadBytes(4), 0);
            FreeBlocksRoot = BitConverter.ToInt32(BinaryReader.ReadBytes(4), 0);
            FreeBlocksEnd = BitConverter.ToInt32(BinaryReader.ReadBytes(4), 0);
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
                /*
                File.WriteAllText("C:/Users/User/source/repos/CarDrivingDataManagementTest/deletion.txt",
                    "" + currentBlock.RecordsArray.ID + "\r\n");*/
                while (currentBlock != null)
                {
                    if (IsLeaf(currentBlock) &&
                    currentBlock.RecordsArray.Records[(int)Math.Ceiling((RecordsPerBlockCount + 1) * 1.0 / 2)-1] != null) //minimal accepted records count
                    {
                        /*File.AppendAllText("C:/Users/User/source/repos/CarDrivingDataManagementTest/deletion.txt",
                    "First case" + "\r\n");*/
                        bool done = false;
                        for (int i = 0; i < RecordsPerBlockCount; i++)
                        {
                            
                            Record<T> record = currentBlock.RecordsArray.Records[i];
                            if (!done && record.Data.CompareTo(data) == 0)
                            {
                               /* File.AppendAllText("C:/Users/User/source/repos/CarDrivingDataManagementTest/deletion.txt",
                    "Record found in its block at position " + i + "\r\n");*/
                                record.Used = false;
                                done = true;
                            }
                            else
                                if (done)
                            {
                                currentBlock.RecordsArray.Records[i - 1] = record;
                            }
                        }
                        WriteBlock(currentBlock);
                        return true;

                    }
                    else
                    if (!IsLeaf(currentBlock))
                    {
                        return false; //TODO

                        File.AppendAllText("C:/Users/User/source/repos/CarDrivingDataManagementTest/deletion.txt",
                    "Second case" + "\r\n");
                        for (int i = 0; i < RecordsPerBlockCount; i++)
                        {
                            
                            Record<T> record = currentBlock.RecordsArray.Records[i];
                            if (record.Data.CompareTo(data) == 0)
                            {
                                File.AppendAllText("C:/Users/User/source/repos/CarDrivingDataManagementTest/deletion.txt",
                    "Record found in its block at position " + i + "\r\n");
                                Block<T> successor = ReadBlock(currentBlock.Pointers[i + 1]); //could it also be a left son?
                                File.AppendAllText("C:/Users/User/source/repos/CarDrivingDataManagementTest/deletion.txt",
                    "First successor candidate: " + successor.RecordsArray.ID + "\r\n");
                                while (!IsLeaf(successor))
                                {
                                    successor = ReadBlock(successor.Pointers[0]);
                                }
                                File.AppendAllText("C:/Users/User/source/repos/CarDrivingDataManagementTest/deletion.txt",
                    "Successor found: " + successor.RecordsArray.ID + "\r\n");
                                currentBlock.RecordsArray.Records[i] = successor.RecordsArray.Records[0];
                                WriteBlock(currentBlock);
                                currentBlock = successor;
                                data = successor.RecordsArray.Records[0].Data;
                                break;
                            }
                        }
                    }
                    else
                    if (IsLeaf(currentBlock) &&
                    currentBlock.RecordsArray.Records[(int)Math.Ceiling((RecordsPerBlockCount + 1) * 1.0 / 2)-1] == null)
                    {
                        return false; //TODO
                        File.AppendAllText("C:/Users/User/source/repos/CarDrivingDataManagementTest/deletion.txt",
                    "The hardest case" + "\r\n");

                        Block<T> parent = ReadBlock(currentBlock.Parent);
                        
                        bool firstTime = true;
                        int extraPointer = 0;
                        while (parent != null)
                        {
                            
                            int i = 0;
                            for (i = 0; i < RecordsPerBlockCount; i++)
                            {
                                if (parent.Pointers[i] == currentBlock.RecordsArray.ID)
                                {
                                    break;
                                }
                            }
                            File.AppendAllText("C:/Users/User/source/repos/CarDrivingDataManagementTest/deletion.txt",
                    i + "th pointer of block " + parent.RecordsArray.ID + " points to " + parent.Pointers[i] + "\r\n");
                            Block<T> left = null;
                            if (i != 0 && parent.Pointers[i - 1] != 0)
                            {
                                left = ReadBlock(parent.Pointers[i - 1]);
                                File.AppendAllText("C:/Users/User/source/repos/CarDrivingDataManagementTest/deletion.txt",
                       "left neighbour: " + parent.Pointers[i - 1] + "\r\n");
                            }
                            Block<T> right = null;
                            if (i + 1 <= RecordsPerBlockCount && parent.Pointers[i + 1] != 0)
                            {
                                right = ReadBlock(parent.Pointers[i + 1]);
                                File.AppendAllText("C:/Users/User/source/repos/CarDrivingDataManagementTest/deletion.txt",
                       "right neighbour: " + parent.Pointers[i + 1] + "\r\n");
                            }
                            if (i + 1 <= RecordsPerBlockCount && parent.Pointers[i + 1] != 0
                                && right.RecordsArray.Records[(int)Math.Ceiling((RecordsPerBlockCount + 1) * 1.0 / 2)-1] != null)
                            {
                                File.AppendAllText("C:/Users/User/source/repos/CarDrivingDataManagementTest/deletion.txt",
                       "Taking element from the left neighbour\r\n");
                                bool found = false;
                                for (int j = 0; j < RecordsPerBlockCount - 1; j++)
                                {
                                    if (!found && firstTime)
                                    {
                                        if (currentBlock.RecordsArray.Records[j].Data.CompareTo(data) == 0)
                                        {
                                            found = true;
                                            if (currentBlock.RecordsArray.Records[j + 1] != null)
                                            {
                                                currentBlock.RecordsArray.Records[j] = currentBlock.RecordsArray.Records[j + 1];

                                            }
                                            else
                                            {
                                                currentBlock.RecordsArray.Records[j] =
                                                    parent.RecordsArray.Records[i];
                                            }
                                        }
                                    }
                                    else
                                    if (found && firstTime)
                                    {
                                        if (currentBlock.RecordsArray.Records[j + 1] != null)
                                        {
                                            currentBlock.RecordsArray.Records[j] = currentBlock.RecordsArray.Records[j + 1];

                                        }
                                        else
                                        {
                                            currentBlock.RecordsArray.Records[j] =
                                                parent.RecordsArray.Records[i];
                                        }
                                        
                                    }
                                    else
                                    {
                                        if (currentBlock.RecordsArray.Records[j] == null)
                                        {
                                            currentBlock.RecordsArray.Records[j] = parent.RecordsArray.Records[i];
                                            while (j > 0 && currentBlock.RecordsArray.Records[j].Data.
                                                CompareTo(currentBlock.RecordsArray.Records[j - 1].Data) < 0)
                                            {
                                                Record<T> tmp = currentBlock.RecordsArray.Records[j];
                                                currentBlock.RecordsArray.Records[j] = currentBlock.RecordsArray.Records[j - 1];
                                                currentBlock.RecordsArray.Records[j - 1] = tmp;
                                            }

                                            break;
                                        }

                                    }
                                }
                                WriteBlock(currentBlock);
                                parent.RecordsArray.Records[i] = right.RecordsArray.Records[0];
                                WriteBlock(parent);
                                currentBlock = null;
                                for(i = 0; i < RecordsPerBlockCount - 1; i++)
                                {
                                    right.RecordsArray.Records[i] = right.RecordsArray.Records[i + 1];
                                }
                                File.WriteAllText("C:/Users/User/source/repos/CarDrivingDataManagementTest/treeAfterHardDeletion.txt",
                    TraceLevelOrder());
                                return true;
                            }
                            else
                                if (i != 0
                                && left.RecordsArray.Records[(int)Math.Ceiling((RecordsPerBlockCount + 1) * 1.0 / 2)-1] != null)
                            {
                                File.AppendAllText("C:/Users/User/source/repos/CarDrivingDataManagementTest/deletion.txt",
                       "Taking element from the left neighbour\r\n");
                                bool found = false;
                                for (int j = RecordsPerBlockCount - 1; j > 0; j--)
                                {
                                    
                                    if (!found && firstTime)
                                    {
                                        if (currentBlock.RecordsArray.Records[j] != null &&
                                            currentBlock.RecordsArray.Records[j].Data.CompareTo(data) == 0)
                                        {
                                            found = true;
                                            currentBlock.RecordsArray.Records[j] = currentBlock.RecordsArray.Records[j - 1];

                                            
                                        }
                                    }
                                    else
                                    if (found && firstTime)
                                    {
                                        currentBlock.RecordsArray.Records[j] = currentBlock.RecordsArray.Records[j - 1];

                                    }
                                    
                                }
                                currentBlock.RecordsArray.Records[0] = parent.RecordsArray.Records[i];
                                WriteBlock(currentBlock);
                                parent.RecordsArray.Records[i] = left.RecordsArray.Records[0];
                                WriteBlock(parent);
                                currentBlock = null;
                                for (i = 0; i < RecordsPerBlockCount - 1; i++)
                                {
                                    left.RecordsArray.Records[i] = left.RecordsArray.Records[i + 1];
                                }
                                File.WriteAllText("C:/Users/User/source/repos/CarDrivingDataManagementTest/treeAfterHardDeletion.txt",
                    TraceLevelOrder());
                                return true;
                            }
                            else
                            {
                                int start = (int)Math.Ceiling((RecordsPerBlockCount + 1) * 1.0 / 2) - 1;


                                Block<T> onlyOneStaysAliveNode = null;
                                Block<T> freeBlock = null;
                                if (left != null)
                                {
                                    File.AppendAllText("C:/Users/User/source/repos/CarDrivingDataManagementTest/deletion.txt",
                       "Moving all to the left block\r\n");
                                    if (parent.RecordsArray.ID == RootID)
                                    {
                                        RootID = left.RecordsArray.ID;
                                    }
                                    onlyOneStaysAliveNode = left;
                                    freeBlock = currentBlock;
                                    onlyOneStaysAliveNode.RecordsArray.Records[start] = parent.RecordsArray.Records[i];
                                    int k = 0;
                                    for (int j = start + 1; j < RecordsPerBlockCount; j++)
                                    {
                                        if (currentBlock.RecordsArray.Records[k].Data.CompareTo(data) != 0)
                                        {
                                            onlyOneStaysAliveNode.RecordsArray.Records[j] = currentBlock.RecordsArray.Records[k];
                                        }
                                        k++;
                                    }
                                    for (int j = i; j < RecordsPerBlockCount - 1; j++)
                                    {
                                        parent.RecordsArray.Records[j] = parent.RecordsArray.Records[j + 1];
                                        parent.Pointers[j] = parent.Pointers[j + 1];
                                    }
                                    
                                }
                                else
                                {
                                    File.AppendAllText("C:/Users/User/source/repos/CarDrivingDataManagementTest/deletion.txt",
                       "Moving all to the right block\r\n");
                                    if (parent.RecordsArray.ID == RootID)
                                    {
                                        RootID = right.RecordsArray.ID;
                                    }
                                    
                                    onlyOneStaysAliveNode = right;
                                    freeBlock = currentBlock;

                                    List<Record<T>> tmpRecords = new List<Record<T>>();
                                    for (int j = 0; j < start; j++)
                                    {
                                        tmpRecords.Add(onlyOneStaysAliveNode.RecordsArray.Records[j]);
                                        if (currentBlock.RecordsArray.Records[j].Data.CompareTo(data) != 0)
                                        {
                                            onlyOneStaysAliveNode.RecordsArray.Records[j] = currentBlock.RecordsArray.Records[j];
                                        }
                                    }
                                    onlyOneStaysAliveNode.RecordsArray.Records[start] = parent.RecordsArray.Records[i + 1];
                                    for (int j = start + 1; j < RecordsPerBlockCount; j++)
                                    {
                                        if (tmpRecords.Count > 0)
                                        {
                                            onlyOneStaysAliveNode.RecordsArray.Records[j] = tmpRecords[0];
                                            tmpRecords.RemoveAt(0);
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    for (int j = i + 1; j < RecordsPerBlockCount - 1; j++)
                                    {
                                        parent.RecordsArray.Records[j] = parent.RecordsArray.Records[j + 1];
                                        parent.Pointers[j] = parent.Pointers[j + 1];
                                    }
                                }
                                File.AppendAllText("C:/Users/User/source/repos/CarDrivingDataManagementTest/deletion.txt",
                       "Free block: " + freeBlock.RecordsArray.ID + "\r\n");
                                if (FreeBlocksEnd == 0)
                                {
                                    FreeBlocksRoot = freeBlock.RecordsArray.ID;
                                    FreeBlocksEnd = freeBlock.RecordsArray.ID;

                                    for (int j = 0; j <= RecordsPerBlockCount; j++)
                                    {
                                        freeBlock.Pointers[j] = 0;
                                    }
                                }
                                else
                                {
                                    Block<T> freeBlocksEnd = ReadBlock(FreeBlocksEnd);
                                    FreeBlocksEnd = freeBlock.RecordsArray.ID;
                                    freeBlocksEnd.Pointers[0] = FreeBlocksEnd;
                                    WriteBlock(freeBlocksEnd);
                                }
                                foreach (Record<T> record in freeBlock.RecordsArray.Records)
                                {
                                    if (record != null)
                                    {
                                        record.Used = false;
                                    }
                                }
                                WriteBlock(freeBlock);
                                WriteBlock(onlyOneStaysAliveNode);

                                WriteBlock(parent);

                                if (parent.RecordsArray.Records[(int)Math.Ceiling((RecordsPerBlockCount + 1) * 1.0 / 2) - 1] != null)
                                {
                                    File.WriteAllText("C:/Users/User/source/repos/CarDrivingDataManagementTest/treeAfterHardDeletion.txt",
                    TraceLevelOrder());
                                    return true;
                                }
                                else
                                {
                                    extraPointer = currentBlock.RecordsArray.ID;
                                    currentBlock = parent;
                                    parent = ReadBlock(parent.Parent);
                                    firstTime = false;
                                }

                            }
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
            
            if(Root == null)
            {
                if (FreeBlocksRoot == 0)
                {
                    Root = new Block<T>(NextBlockID, RecordsPerBlockCount);
                    NextBlockID++;
                }
                else
                {
                    Root = new Block<T>(FreeBlocksRoot, RecordsPerBlockCount);
                    if (FreeBlocksRoot != FreeBlocksEnd)
                    {
                        FreeBlocksRoot = ReadBlock(FreeBlocksRoot).Pointers[0];
                    }
                    else
                    {
                        FreeBlocksRoot = 0;
                        FreeBlocksEnd = 0;
                    }
                }
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
                        if(currentNode.RecordsArray.Records[0].CompareTo(newData) == 0)
                        {
                            return false;
                        }
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
                            else
                            {
                                if(currentNode.RecordsArray.Records[i] != null
                                && currentNode.RecordsArray.Records[i].CompareTo(newData) == 0)
                                {
                                    return false;
                                }
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
                bool firstTime = true;
                while (currentNode != null)
                {
                    
                    if (currentNode.RecordsArray.Records[RecordsPerBlockCount - 1] == null) // overflow check
                    {
                        
                        int i = 0;
                        while (currentNode.RecordsArray.Records[i] != null && currentNode.RecordsArray.Records[i].CompareTo(newData) < 0)
                        {
                            i++;
                        }
                        if(firstTime && currentNode.RecordsArray.Records[i] != null
                            &&
                            currentNode.RecordsArray.Records[i].CompareTo(newData) == 0)
                        {
                            return false;
                        }
                        firstTime = false;
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
                        if (i < RecordsPerBlockCount && currentNode.RecordsArray.Records[i].CompareTo(newData) == 0)
                        {
                            return false;
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

                        Block<T> newNode = null;
                        if (FreeBlocksRoot == 0)
                        {
                            newNode = new Block<T>(NextBlockID, RecordsPerBlockCount);
                            NextBlockID++;
                        }
                        else
                        {
                            newNode = new Block<T>(FreeBlocksRoot, RecordsPerBlockCount);
                            if (FreeBlocksRoot != FreeBlocksEnd)
                            {
                                FreeBlocksRoot = ReadBlock(FreeBlocksRoot).Pointers[0];
                            }
                            else
                            {
                                FreeBlocksRoot = 0;
                                FreeBlocksEnd = 0;
                            }
                        }
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
                            if (FreeBlocksRoot == 0)
                            {
                                Root = new Block<T>(NextBlockID, RecordsPerBlockCount);
                                NextBlockID++;
                            }
                            else
                            {
                                Root = new Block<T>(FreeBlocksRoot, RecordsPerBlockCount);
                                if (FreeBlocksRoot != FreeBlocksEnd)
                                {
                                    FreeBlocksRoot = ReadBlock(FreeBlocksRoot).Pointers[0];
                                }
                                else
                                {
                                    FreeBlocksRoot = 0;
                                    FreeBlocksEnd = 0;
                                }
                            }
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

        
        


        public void WriteBlock(Block<T> node)
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
                if (node.RecordsArray.Records[i].Data == null ||
                    node.RecordsArray.Records[i].Data.IsNull() || !node.RecordsArray.Records[i].Used)
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

            if (RootID != 0)
            {

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
                                result += record.Data + " [";
                                if (record.Used)
                                {
                                    result += "USED]\r\n";
                                }
                                else
                                {
                                    result += "NOT USED]\r\n";
                                }
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
                        foreach (int pointer in currentNode.Pointers)
                        {
                            if (pointer != 0)
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
            BinaryWriter.Seek(0, SeekOrigin.Begin);
            BinaryWriter.Write(BitConverter.GetBytes(RecordsPerBlockCount));
            BinaryWriter.Write(BitConverter.GetBytes(RootID));
            BinaryWriter.Write(BitConverter.GetBytes(NextBlockID));
            BinaryWriter.Write(BitConverter.GetBytes(FreeBlocksRoot));
            BinaryWriter.Write(BitConverter.GetBytes(FreeBlocksEnd));
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

            for (int i = 0; i < GetBlocksCount(); i++)
            {
                result += "\r\nBlock " + i + "\r\n";
                for (int k = 0; k < 4; k++)
                {
                    BinaryReader.ReadByte();
                }
                byte[] parentID = new byte[4];

                for (int k = 0; k < 4; k++)
                {
                    try
                    {
                        parentID[k] = BinaryReader.ReadByte();
                    }
                    catch (Exception)
                    {
                        parentID[k] = 0;
                    }
                }

                result += "Parent block: " + BitConverter.ToInt32(parentID, 0) + "\r\n";

                result += "Pointers: ";
                for (int m = 0; m < RecordsPerBlockCount + 1; m++)
                {
                    byte[] ptrID = new byte[4];
                    for (int k = 0; k < 4; k++)
                    {
                        try
                        {
                            ptrID[k] = BinaryReader.ReadByte();
                        }
                        catch (Exception)
                        {
                            ptrID[k] = 0;
                        }
                    }
                    result += BitConverter.ToInt32(ptrID, 0) + ";";
                }
                result += "\r\n";
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
