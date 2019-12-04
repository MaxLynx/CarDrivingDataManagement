using CarDrivingDataManagement.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarDrivingDataManagement.Entity
{
    public class Driver : IComparable<Driver>, IRecordable<Driver>
    {
        public String Name { get; set; }
        public Int32 NameActualLength { get; set; }
        public Int32 NameMaxLength { get; set; }
        public String Surname { get; set; }
        public Int32 SurnameActualLength { get; set; }
        public Int32 SurnameMaxLength { get; set; }

        public Int64 CardID { get; set; }
        public DateTime CardEndDate { get; set; }
        public Boolean DrivingForbidden { get; set; }

        public Int32 RuleViolationsCount { get; set; }

        public Driver() 
        {
            NameMaxLength = 35;
            Name = "Placeholder";
            NameActualLength = Name.Length;
            SurnameMaxLength = 35;
            Surname = "Placeholder";
            SurnameActualLength = Surname.Length;
            CardID = 0;
            CardEndDate = DateTime.Now;
            DrivingForbidden = false;
            RuleViolationsCount = 0;
        }

        public Driver(String name, String surname, long cardID, DateTime cardEndTime, bool drivingForbidden, int ruleViolationsCount)
        {
            NameMaxLength = 35;
            Name = name;
            NameActualLength = Name.Length;
            SurnameMaxLength = 35;
            Surname = surname;
            SurnameActualLength = Surname.Length;
            CardID = cardID;
            CardEndDate = cardEndTime;
            DrivingForbidden = drivingForbidden;
            RuleViolationsCount = ruleViolationsCount;
        }

        public byte[] GetBytes()
        {
            byte[] nameBytes = new byte[NameMaxLength];
            byte[] nameStr = Encoding.ASCII.GetBytes(Name);
            for (int i = 0; i < NameMaxLength; i++)
            {
                if (i < nameStr.Length)
                {
                    nameBytes[i] = nameStr[i];
                }
                else
                {
                    nameBytes[i] = 0;
                }
            }
            byte[] actualNameLengthBytes = BitConverter.GetBytes(NameActualLength);
            byte[] surnameBytes = new byte[SurnameMaxLength];
            byte[] surnameStr = Encoding.ASCII.GetBytes(Surname);
            for (int i = 0; i < SurnameMaxLength; i++)
            {
                if (i < surnameStr.Length)
                {
                    surnameBytes[i] = surnameStr[i];
                }
                else
                {
                    surnameBytes[i] = 0;
                }
            }
            byte[] actualSurnameLengthBytes = BitConverter.GetBytes(SurnameActualLength);
            byte[] cardIDBytes = BitConverter.GetBytes(CardID);
            byte[] cardEndDateBytes = BitConverter.GetBytes(CardEndDate.Ticks);
            byte[] drivingForbiddenBytes = BitConverter.GetBytes(DrivingForbidden);
            byte[] ruleViolationsCountBytes = BitConverter.GetBytes(RuleViolationsCount);
            byte[] result = new byte[
                NameMaxLength + actualNameLengthBytes.Length
                + SurnameMaxLength + actualSurnameLengthBytes.Length + cardIDBytes.Length 
                + cardEndDateBytes.Length + drivingForbiddenBytes.Length + ruleViolationsCountBytes.Length
                ];
            Buffer.BlockCopy(nameBytes, 0, result, 0, NameMaxLength);
            Buffer.BlockCopy(actualNameLengthBytes, 0, result, NameMaxLength, actualNameLengthBytes.Length);
            Buffer.BlockCopy(surnameBytes, 0, result, NameMaxLength + actualNameLengthBytes.Length, SurnameMaxLength);
            Buffer.BlockCopy(actualSurnameLengthBytes, 0, result, NameMaxLength + actualNameLengthBytes.Length + SurnameMaxLength
                , actualSurnameLengthBytes.Length);
            Buffer.BlockCopy(cardIDBytes, 0, result, NameMaxLength + actualNameLengthBytes.Length + SurnameMaxLength 
                + actualSurnameLengthBytes.Length,
                cardIDBytes.Length);
            Buffer.BlockCopy(cardEndDateBytes, 0, result, NameMaxLength + actualNameLengthBytes.Length + SurnameMaxLength
                + actualSurnameLengthBytes.Length + cardIDBytes.Length,
                cardEndDateBytes.Length);
            Buffer.BlockCopy(drivingForbiddenBytes, 0, result, NameMaxLength + actualNameLengthBytes.Length + SurnameMaxLength
               + actualSurnameLengthBytes.Length + cardIDBytes.Length + cardEndDateBytes.Length,
               drivingForbiddenBytes.Length);
            Buffer.BlockCopy(ruleViolationsCountBytes, 0, result, NameMaxLength + actualNameLengthBytes.Length + SurnameMaxLength
               + actualSurnameLengthBytes.Length + cardIDBytes.Length + cardEndDateBytes.Length + drivingForbiddenBytes.Length,
               ruleViolationsCountBytes.Length);
            return result;
        }

        public Driver newInstance(byte[] bytes)
        {
            byte[] nameStr = bytes.Take(NameMaxLength).ToArray();
            byte[] actualNameStrLengthBytes = bytes.Skip(NameMaxLength).Take(4).ToArray();
            byte[] surnameStr = bytes.Skip(NameMaxLength + 4).Take(SurnameMaxLength).ToArray();
            byte[] actualSurnameStrLengthBytes = bytes.Skip(NameMaxLength + 4 + SurnameMaxLength).Take(4).ToArray();
            byte[] cardIDBytes = bytes.Skip(NameMaxLength + 4 + SurnameMaxLength + 4).Take(8).ToArray();
            byte[] dateBytes = bytes.Skip(NameMaxLength + 4 + SurnameMaxLength + 4 + 8).Take(8).ToArray();
            byte[] drivingForbiddenBytes = bytes.Skip(NameMaxLength + 4 + SurnameMaxLength + 4 + 8 * 2).Take(1).ToArray();
            byte[] ruleViolationsCountBytes = bytes.Skip(NameMaxLength + 4 + SurnameMaxLength + 4 + 8 * 2 + 1).Take(4).ToArray();

            Driver obj = new Driver(
                Encoding.ASCII.GetString(nameStr, 0, BitConverter.ToInt32(actualNameStrLengthBytes, 0)),
                Encoding.ASCII.GetString(surnameStr, 0, BitConverter.ToInt32(actualSurnameStrLengthBytes, 0)),
                BitConverter.ToInt64(cardIDBytes, 0),
                new DateTime(BitConverter.ToInt64(dateBytes, 0)),
                BitConverter.ToBoolean(drivingForbiddenBytes, 0),
                BitConverter.ToInt32(ruleViolationsCountBytes, 0));

            return obj;

        }

        public int CompareTo(Driver other)
        {
            return CardID.CompareTo(other.CardID);
        }

        public override String ToString()
        {
            if (!this.IsNull())
            {
                return "" + Name + " (length: " + NameActualLength + ") " + Surname + " (length: " + SurnameActualLength + ") id "
                    + CardID + " until " + CardEndDate.ToString() + " (driving forbidden: " + DrivingForbidden + ") " +
                    "rule violations: " + RuleViolationsCount;
            }
            else
            {
                return "";
            }
        }

        public bool IsNull()
        {
            return SurnameActualLength == 0 && NameActualLength == 0;
        }
    }
}
