using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarDrivingDataManagement.Utils
{
    public class Block
    {
        public Int32 Size { get; set; }
        public byte[] ByteArray { get; set; }
        public List<Record> Records { get; set; }

        public Block()
        {
            Records = new List<Record>();
        }
    }
}
