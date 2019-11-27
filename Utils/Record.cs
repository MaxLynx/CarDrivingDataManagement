using System;
using System.Collections.Generic;
using System.IO;
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

        public RecordsArray<T> Block { get; set; }

        public Record() { }

        public Record(byte[] bytes)
        {
            
            ByteArray = bytes;
            Size = ByteArray.Length;
            if (BitConverter.ToInt32(ByteArray.Take(4).ToArray(), 0) == 1)
            {
                Used = true;
            }
            else
            {
                Used = false;
            }
            Data = new T().newInstance(ByteArray.Skip(4).Take(Size-4).ToArray());
            
        }

        public Record(T data)
        {
            Data = data;
            byte[] dataBytes = data.GetBytes();
            ByteArray = new byte[4 + dataBytes.Length];
            System.Buffer.BlockCopy(BitConverter.GetBytes(1), 0, ByteArray, 0, 4);
            System.Buffer.BlockCopy(dataBytes, 0, ByteArray, 4, dataBytes.Length); // record is used
            Size = ByteArray.Length;
            Used = true;
        }

        public int CompareTo(T other)
        {
            return Data.CompareTo(other);
        }
    }
}
