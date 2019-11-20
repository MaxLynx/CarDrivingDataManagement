using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarDrivingDataManagement.Utils
{
    public class Record<T>
    where T : IRecordable<T>, IComparable<T>, new()
        
    {
        public Int32 Size { get; set; }
        public byte[] ByteArray { get; set; }
        public Boolean Used { get; set; }

        public T Data { get; set; }

        public Block<T> Block { get; set; }

        public Record() { }

        public Record(byte[] bytes)
        {
            ByteArray = bytes;
            Size = ByteArray.Length;
            Data = new T().newInstance(ByteArray);
        }

        public Record(T data)
        {
            Data = data;
            ByteArray = data.GetBytes();
            Size = ByteArray.Length;
            Used = true;
        }

        public int CompareTo(T other)
        {
            return Data.CompareTo(other);
        }
    }
}
