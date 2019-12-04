using CarDrivingDataManagement.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarDrivingDataManagement.Entity
{
    public class Vehicle : IComparable<Vehicle>, IRecordable<Vehicle>
    {
        public String ID { get; set; }
        public Int32 IDActualLength { get; set; }
        public Int32 IDMaxLength { get; set; }
        public String VIN { get; set; }
        public Int32 VINActualLength { get; set; }
        public Int32 VINMaxLength { get; set; }
        public Int32 RepairingsCount { get; set; }
        public Int32 DriveWeight { get; set; }
        public Boolean Searched { get; set; }
        public DateTime STKEndDate { get; set; }
        public DateTime EKEndDate { get; set; }

        public Vehicle() {
            IDMaxLength = 7;
            VINMaxLength = 17;
        }

        public Vehicle(String id, String vin, int repairingsCount, int driveWeight, bool searched,
            DateTime stkEndDate, DateTime ekEndDate)
        {
            IDMaxLength = 7;
            VINMaxLength = 17;
            ID = id;
            IDActualLength = ID.Length;
            VIN = vin;
            VINActualLength = VIN.Length;
            RepairingsCount = repairingsCount;
            DriveWeight = driveWeight;
            Searched = searched;
            STKEndDate = stkEndDate;
            EKEndDate = ekEndDate;
        }

        public byte[] GetBytes()
        {
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

            byte[] vinBytes = new byte[VINMaxLength];
            byte[] vinStr = Encoding.ASCII.GetBytes(VIN);
            for (int i = 0; i < VINMaxLength; i++)
            {
                if (i < vinStr.Length)
                {
                    vinBytes[i] = vinStr[i];
                }
                else
                {
                    vinBytes[i] = 0;
                }
            }
            byte[] actualVINLengthBytes = BitConverter.GetBytes(VINActualLength);

            byte[] repairingsCountBytes = BitConverter.GetBytes(RepairingsCount);
            byte[] driveWeightBytes = BitConverter.GetBytes(DriveWeight);
            byte[] searchedBytes = BitConverter.GetBytes(Searched);
            byte[] stkEndDateBytes = BitConverter.GetBytes(STKEndDate.Ticks);
            byte[] ekEndDateBytes = BitConverter.GetBytes(EKEndDate.Ticks);

            byte[] result = new byte[
                IDMaxLength + actualIDLengthBytes.Length
                + VINMaxLength + actualVINLengthBytes.Length
                + repairingsCountBytes.Length
                + driveWeightBytes.Length
                + searchedBytes.Length
                + stkEndDateBytes.Length
                + ekEndDateBytes.Length
                ];
            Buffer.BlockCopy(idBytes, 0, result, 0, idBytes.Length);
            Buffer.BlockCopy(actualIDLengthBytes, 0, result,idBytes.Length, actualIDLengthBytes.Length);

            
            Buffer.BlockCopy(vinBytes, 0, result, idBytes.Length + actualIDLengthBytes.Length 
                , vinBytes.Length);
            Buffer.BlockCopy(actualVINLengthBytes, 0, result, idBytes.Length + actualIDLengthBytes.Length
                + vinBytes.Length, actualVINLengthBytes.Length);

            Buffer.BlockCopy(repairingsCountBytes, 0, result, idBytes.Length + actualIDLengthBytes.Length
                + vinBytes.Length + actualVINLengthBytes.Length
                , repairingsCountBytes.Length);
            Buffer.BlockCopy(driveWeightBytes, 0, result, idBytes.Length + actualIDLengthBytes.Length
                + vinBytes.Length + actualVINLengthBytes.Length
                + repairingsCountBytes.Length
                , driveWeightBytes.Length);
            Buffer.BlockCopy(searchedBytes, 0, result, idBytes.Length + actualIDLengthBytes.Length
                + vinBytes.Length + actualVINLengthBytes.Length
                + repairingsCountBytes.Length + driveWeightBytes.Length
                , searchedBytes.Length);
            Buffer.BlockCopy(stkEndDateBytes, 0, result, idBytes.Length + actualIDLengthBytes.Length
                + vinBytes.Length + actualVINLengthBytes.Length
                + repairingsCountBytes.Length + driveWeightBytes.Length + searchedBytes.Length
                , stkEndDateBytes.Length);
            Buffer.BlockCopy(ekEndDateBytes, 0, result, idBytes.Length + actualIDLengthBytes.Length
                + vinBytes.Length + actualVINLengthBytes.Length
               + repairingsCountBytes.Length + driveWeightBytes.Length + searchedBytes.Length + stkEndDateBytes.Length
               , ekEndDateBytes.Length);
            return result;
        }

        public Vehicle newInstance(byte[] bytes)
        {

            byte[] idStr = bytes.Skip(4).Take(IDMaxLength).ToArray();
            byte[] actualIDStrLengthBytes = bytes.Skip(4 + IDMaxLength).Take(4).ToArray();


            byte[] vinStr = bytes.Skip(4 + IDMaxLength + 4 + 4).Take(VINMaxLength).ToArray();
            byte[] actualVINStrLengthBytes = bytes.Skip(4 + IDMaxLength + 4 + 4 + VINMaxLength).Take(4).ToArray();

            byte[] repairingsCountBytes = bytes.Skip(4 + IDMaxLength + 4 + 4 + VINMaxLength + 4
                ).Take(4).ToArray();

            byte[] driveWeightBytes = bytes.Skip(4 + IDMaxLength + 4 + 4 + VINMaxLength + 4 + 4
                ).Take(4).ToArray();

            byte[] searchedBytes = bytes.Skip(4 + IDMaxLength + 4 + 4 + VINMaxLength + 4 + 4 + 4
                ).Take(1).ToArray();

            byte[] stkEndDateBytes = bytes.Skip(4 + IDMaxLength + 4 + 4 + VINMaxLength + 4 + 4 + 4 + 1
                ).Take(8).ToArray();

            byte[] ekEndDateBytes = bytes.Skip(4 + IDMaxLength + 4 + 4 + VINMaxLength + 4 + 4 + 4 + 1 + 8
               ).Take(8).ToArray();

            Vehicle obj = new Vehicle(
                Encoding.ASCII.GetString(idStr, 0, BitConverter.ToInt32(actualIDStrLengthBytes, 0)),
                Encoding.ASCII.GetString(vinStr, 0, BitConverter.ToInt32(actualVINStrLengthBytes, 0)),
                BitConverter.ToInt32(repairingsCountBytes, 0),
                BitConverter.ToInt32(driveWeightBytes, 0),
                BitConverter.ToBoolean(searchedBytes, 0),
                new DateTime(BitConverter.ToInt64(stkEndDateBytes, 0)),
                new DateTime(BitConverter.ToInt64(ekEndDateBytes, 0))
                );

            return obj;

        }
        public int CompareTo(Vehicle other)
        {
            return ID.CompareTo(other.ID) + VIN.CompareTo(other.VIN);
        }

        public override String ToString()
        {
            if (!this.IsNull())
            {
                return "ID " + ID + " (length: " + IDActualLength + ") " +
                    "VIN " + VIN + " (length: " + VINActualLength + ") " +
                    "repaired: " + RepairingsCount
                    + ", drive weight: " + DriveWeight
                    + ", searched by police: " + Searched.ToString()
                    + ", STK end date: " + STKEndDate.ToString()
                    + ", EK end date: " + EKEndDate.ToString();


            }
            else
            {
                return "";
            }
        }

        public bool IsNull()
        {
            return IDActualLength == 0 && VINActualLength == 0;
        }
    }
}
