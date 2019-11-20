using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarDrivingDataManagement.Utils
{
    public class TreeNode<T>
            where T : IRecordable<T>, IComparable<T>, new()
    {
        public Block<T> Block { get; set; }

        public TreeNode<T>[] Pointers { get; set; }

        public TreeNode<T> Parent { get; set; }

        public TreeNode(int blockID, int recordsPerBlockCount)
        {
            Block = new Block<T>(blockID, recordsPerBlockCount);
            Pointers = new TreeNode<T>[recordsPerBlockCount + 1];
        }
    }
}
