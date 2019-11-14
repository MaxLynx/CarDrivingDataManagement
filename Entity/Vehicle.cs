using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarDrivingDataManagement.Entity
{
    public class Vehicle
    {
        public String ID { get; set; }
        public String VIN { get; set; }
        public Int32 RepairingsCount { get; set; }
        public Int32 DriveWeight { get; set; }
        public Boolean Searched { get; set; }
        public DateTime STKEndDate { get; set; }
        public DateTime EKEndDate { get; set; }

        public Vehicle() { }

        public Vehicle(String id, String vin, int repairingsCount, int driveWeight, bool searched,
            DateTime stkEndDate, DateTime ekEndDate)
        {
            ID = id;
            VIN = vin;
            RepairingsCount = repairingsCount;
            DriveWeight = driveWeight;
            Searched = searched;
            STKEndDate = stkEndDate;
            EKEndDate = ekEndDate;
        }
    }
}
