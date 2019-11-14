using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarDrivingDataManagement.Utils
{
    public class Record
    {
        public Int32 Size { get; set; }
        public Int32 TrueSize { get; set; }
        public byte[] ByteArray { get; set; }
        public Boolean Used { get; set; }

        public Record() { }

        public Record(byte[] bytes)
        {
            Size = bytes.Length;
            TrueSize = Size;
            ByteArray = bytes;
            Used = true;
        }
    }
}
