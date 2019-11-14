using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarDrivingDataManagement.Entity
{
    public class Driver
    {
        public String Name { get; set; }
        public String Surname { get; set; }
        public Int64 CardID { get; set; }
        public DateTime CardEndDate { get; set; }
        public Boolean DrivingForbidden { get; set; }

        public Int32 RuleViolationsCount { get; set; }

        public Driver() { }

        public Driver(String name, String surname, int cardID, DateTime cardEndTime, bool drivingForbidden, int ruleViolationsCount)
        {
            Name = name;
            Surname = surname;
            CardID = cardID;
            CardEndDate = cardEndTime;
            DrivingForbidden = drivingForbidden;
            RuleViolationsCount = ruleViolationsCount;
        }
    }
}
