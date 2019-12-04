using CarDrivingDataManagement.Entity;
using CarDrivingDataManagement.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarDrivingDataManagement
{
    public class CarController
    {
        BTree<AddressedID> IDCarTree { get; set; }
        BTree<AddressedID> VINCarTree { get; set; }
        HeapFile<Vehicle> CarHeapFile { get; set; }

        public DriverController(String driverFilename, int driverRecordsCountPerBlock)
        {
            DriverTree = new BTree<Driver>(driverRecordsCountPerBlock, driverFilename);
        }
        public DriverController(String driverFilename)
        {
            DriverTree = new BTree<Driver>(driverFilename);
        }
        public String DescribeDriverDBStructure()
        {
            return DriverTree.DescribeFileStructure();
        }

        public void GenerateData(int addCount)
        {
            Random random = new Random();
            for (int i = 0; i < addCount; i++)
            {
                int randomNumber = random.Next(1, addCount * 100);

                DriverTree.Add(new Driver("Name" + i, "Surname" + i,
                    randomNumber, DateTime.Now.AddDays(randomNumber), false, randomNumber % 3
                    ));


            }
            File.WriteAllText("C:/Users/User/source/repos/CarDrivingDataManagementTest/driverTreeLevelOrder.txt",
                    DriverTree.TraceLevelOrder());
        }
        public bool AddDriver(String name, String surname, int id, DateTime endDate, bool drivingForbidden,
            int ruleViolationsCount)
        {
            return DriverTree.Add(new Driver(name, surname, id, endDate, drivingForbidden, ruleViolationsCount));
        }
        public bool UpdateDriver(String name, String surname, int id, DateTime endDate, bool drivingForbidden,
            int ruleViolationsCount)
        {
            Driver searchObj = new Driver();
            searchObj.CardID = id;
            Block<Driver> block = DriverTree.Find(searchObj);
            if (block != null)
            {
                Record<Driver>[] records = block.RecordsArray.Records;
                for (int i = 0; i < records.Length; i++)
                {

                    if (records[i].Data.CompareTo(searchObj) == 0)
                    {
                        searchObj.Name = name;
                        searchObj.Surname = surname;
                        searchObj.CardEndDate = endDate;
                        searchObj.DrivingForbidden = drivingForbidden;
                        searchObj.RuleViolationsCount = ruleViolationsCount;
                        records[i] = new Record<Driver>(searchObj);
                        DriverTree.WriteBlock(block);
                        return true;
                    }
                }
            }
            return false;
        }

        public String[] GetDriverByID(int id)
        {
            String[] result = new string[6];
            Driver driver = GetDriverByIDAsObject(id);
            if (driver != null)
            {
                result[0] = driver.Name;
                result[1] = driver.Surname;
                result[2] = driver.CardID.ToString();
                result[3] = driver.CardEndDate.ToString();
                result[4] = driver.DrivingForbidden.ToString();
                result[5] = driver.RuleViolationsCount.ToString();
            }

            return result;
        }
        private Driver GetDriverByIDAsObject(int id)
        {
            Driver searchObj = new Driver();
            searchObj.CardID = id;
            Block<Driver> block = DriverTree.Find(searchObj);
            if (block != null)
            {
                Record<Driver>[] records = block.RecordsArray.Records;

                foreach (Record<Driver> record in records)
                {
                    if (record.Data.CompareTo(searchObj) == 0)
                    {
                        return record.Data;
                    }
                }
            }
            return null;
        }
        public void Close()
        {
            DriverTree.CloseFileInteraction();
        }
    }
}
