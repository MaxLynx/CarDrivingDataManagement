using CarDrivingDataManagement.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarDrivingDataManagement.Entity
{
    class AddressedID : IComparable<AddressedID>, IRecordable<AddressedID>
    {
        public String ID { get; set; }
        public Int32 IDActualLength { get; set; }
        public Int32 IDMaxLength { get; set; }
        public Int32 Address { get; set; }

        public AddressedID() 
        {
            ID = "Placeholder123456";
            IDMaxLength = ID.Length;
            IDActualLength = ID.Length;
        }
        public AddressedID(String id, int idMaxLength, int address)
        {
            ID = id;
            IDActualLength = id.Length;
            IDMaxLength = idMaxLength;
            Address = address;
        }

        public byte[] GetBytes()
        {
            byte[] maxIDLengthBytes = BitConverter.GetBytes(IDMaxLength);
            byte[] idBytes = new byte[IDMaxLength];
            byte[] idStr = Encoding.ASCII.GetBytes(ID);
            for (int i = 0; i < IDMaxLength; i++)
            {
                if (i < idStr.Length)
                {
                    idBytes[i] = idStr[i];
                }
                else
                {
                    idBytes[i] = 0;
                }
            }
            byte[] actualIDLengthBytes = BitConverter.GetBytes(IDActualLength);
            
            byte[] addressBytes = BitConverter.GetBytes(Address);
            byte[] result = new byte[
                maxIDLengthBytes.Length + 
                IDMaxLength + actualIDLengthBytes.Length
                + addressBytes.Length
                ];
            Buffer.BlockCopy(maxIDLengthBytes, 0, result, 0, maxIDLengthBytes.Length);
            Buffer.BlockCopy(idBytes, 0, result, maxIDLengthBytes.Length, idBytes.Length);
            Buffer.BlockCopy(actualIDLengthBytes, 0, result, maxIDLengthBytes.Length + idBytes.Length, actualIDLengthBytes.Length);
            Buffer.BlockCopy(addressBytes, 0, result, maxIDLengthBytes.Length + idBytes.Length + actualIDLengthBytes.Length
                , addressBytes.Length);
           
            return result;
        }

        public AddressedID newInstance(byte[] bytes)
        {
            int maxIDLength = BitConverter.ToInt32(bytes.Take(4).ToArray(), 0);

            byte[] idStr = bytes.Skip(4).Take(maxIDLength).ToArray();
            byte[] actualIDStrLengthBytes = bytes.Skip(4 + maxIDLength).Take(4).ToArray();
            
            byte[] addressBytes = bytes.Skip(4 + maxIDLength + 4).Take(4).ToArray();

            AddressedID obj = new AddressedID(
                Encoding.ASCII.GetString(idStr, 0, BitConverter.ToInt32(actualIDStrLengthBytes, 0)),
                maxIDLength,
                BitConverter.ToInt32(addressBytes, 0));

            return obj;

        }
        public int CompareTo(AddressedID other)
        {
            return ID.CompareTo(other.ID);
        }

        public override String ToString()
        {
            if (!this.IsNull())
            {
                return "" + ID + " (length: " + IDActualLength + ") " +
                    "address: " + Address;
            }
            else
            {
                return "";
            }
        }

        public bool IsNull()
        {
            return IDActualLength == 0 && Address == 0;
        }
    }
}
