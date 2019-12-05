using CarDrivingDataManagement.Entity;
using CarDrivingDataManagement.Utils;
using System;
using System.Collections.Generic;
using System.IO;
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

        public CarController(String carIDFilename, int carIDRecordsCountPerBlock,
            String carVINFilename, int carVINRecordsCountPerBlock,
            String heapFilename, int heapfileRecordsCountPerBlock
            )
        {
            IDCarTree = new BTree<AddressedID>(carIDRecordsCountPerBlock, carIDFilename);
            VINCarTree = new BTree<AddressedID>(carVINRecordsCountPerBlock, carVINFilename);
            CarHeapFile = new HeapFile<Vehicle>(heapfileRecordsCountPerBlock, heapFilename);
        }
        public CarController(String carIDFilename,
            String carVINFilename,
            String heapFilename)
        {
            IDCarTree = new BTree<AddressedID>(carIDFilename);
            VINCarTree = new BTree<AddressedID>(carVINFilename);
            CarHeapFile = new HeapFile<Vehicle>(heapFilename);
        }
        public String DescribeCarIDDBStructure()
        {
            return IDCarTree.DescribeFileStructure();
        }

        public String DescribeCarHeapFileDBStructure()
        {
            return CarHeapFile.DescribeFileStructure();
        }

        public String DescribeCarVINDBStructure()
        {
            return VINCarTree.DescribeFileStructure();
        }

        public void GenerateData(int addCount)
        {
            Random random = new Random();
            for (int i = 0; i < addCount; i++)
            {
                int randomNumber = random.Next(1, addCount * 100);

                Vehicle vehicle = new Vehicle("ID" + i, "VIN" + (addCount - i), randomNumber % 4,
                    randomNumber,
                    false, DateTime.Now.AddDays(randomNumber), DateTime.Now.AddDays(randomNumber)
                    );

                int address = CarHeapFile.Add(vehicle);

                IDCarTree.Add(new AddressedID(vehicle.ID, vehicle.VINMaxLength, address)); 
                VINCarTree.Add(new AddressedID(vehicle.VIN, vehicle.VINMaxLength, address));


            }
            File.WriteAllText("C:/Users/User/source/repos/CarDrivingDataManagementTest/idCarTreeLevelOrder.txt",
                    IDCarTree.TraceLevelOrder());
            File.WriteAllText("C:/Users/User/source/repos/CarDrivingDataManagementTest/vinCarTreeLevelOrder.txt",
                    VINCarTree.TraceLevelOrder());
        }
        public bool AddCar(String id, String vin, int repairingsCount, int driveWeight, bool searched,
            DateTime stkEndDate, DateTime ekEndDate)
        {
            Vehicle vehicle = new Vehicle(id, vin, repairingsCount,
                    driveWeight,
                    searched, stkEndDate, ekEndDate
                    );

            int address = CarHeapFile.Add(vehicle);

            bool result = IDCarTree.Add(new AddressedID(vehicle.ID, vehicle.VINMaxLength, address));
            result &= VINCarTree.Add(new AddressedID(vehicle.VIN, vehicle.VINMaxLength, address));
            return result;
        }
        public bool UpdateCar(String id, String vin, int repairingsCount, int driveWeight, bool searched,
            DateTime stkEndDate, DateTime ekEndDate)
        {
            AddressedID searchObj = new AddressedID();
            searchObj.ID = id;
            Block<AddressedID> block = IDCarTree.Find(searchObj);
            if (block != null)
            {
                Record<AddressedID>[] records = block.RecordsArray.Records;
                for (int i = 0; i < records.Length; i++)
                {

                    if (records[i].Data.CompareTo(searchObj) == 0)
                    {
                        Block<Vehicle> vehicleBlock = CarHeapFile.FindBlock(records[i].Data.Address);
                        Vehicle vehicle = null;
                        int recordIndex = 0;
                        for(int j = 0; j < vehicleBlock.RecordsArray.Records.Length; j++)
                        {
                            if (vehicleBlock.RecordsArray.Records[j].Data.ID.Equals(id))
                            {
                                vehicle = vehicleBlock.RecordsArray.Records[j].Data;
                                recordIndex = j;
                                break;
                            }
                        }
                        vehicle.RepairingsCount = repairingsCount;
                        vehicle.DriveWeight = driveWeight;
                        vehicle.Searched = searched;
                        vehicle.STKEndDate = stkEndDate;
                        vehicle.EKEndDate = ekEndDate;

                        vehicleBlock.RecordsArray.Records[recordIndex] = new Record<Vehicle>(vehicle);
                        CarHeapFile.WriteBlock(vehicleBlock);
                        return true;
                    }
                }
            }
            return false;
        }

        public String[] GetCarByID(String id)
        {
            String[] result = new string[7];
            Vehicle car = GetCarByIDAsObject(id);
            if (car != null)
            {
                result[0] = car.ID;
                result[1] = car.VIN;
                result[2] = car.RepairingsCount.ToString();
                result[3] = car.DriveWeight.ToString();
                result[4] = car.Searched.ToString();
                result[5] = car.STKEndDate.ToString();
                result[6] = car.EKEndDate.ToString();
            }

            return result;
        }

        public String[] GetCarByVIN(String vin)
        {
            String[] result = new string[7];
            Vehicle car = GetCarByVINAsObject(vin);
            if (car != null)
            {
                result[0] = car.ID;
                result[1] = car.VIN;
                result[2] = car.RepairingsCount.ToString();
                result[3] = car.DriveWeight.ToString();
                result[4] = car.Searched.ToString();
                result[5] = car.STKEndDate.ToString();
                result[6] = car.EKEndDate.ToString();
            }

            return result;
        }
        private Vehicle GetCarByIDAsObject(String id)
        {
            AddressedID searchObj = new AddressedID();
            searchObj.ID = id;
            Block<AddressedID> block = IDCarTree.Find(searchObj);
            
            if (block != null)
            {
                Record<AddressedID>[] records = block.RecordsArray.Records;

                foreach (Record<AddressedID> record in records)
                {
                    if (record.Data.CompareTo(searchObj) == 0)
                    {
                        return CarHeapFile.FindData(record.Data.Address);
                    }
                }
            }
            return null;
        }
        private Vehicle GetCarByVINAsObject(String vin)
        {
            AddressedID searchObj = new AddressedID();
            searchObj.ID = vin;
            Block<AddressedID> block = VINCarTree.Find(searchObj);

            if (block != null)
            {
                Record<AddressedID>[] records = block.RecordsArray.Records;

                foreach (Record<AddressedID> record in records)
                {
                    if (record.Data.CompareTo(searchObj) == 0)
                    {
                        return CarHeapFile.FindData(record.Data.Address);
                    }
                }
            }
            return null;
        }
        public void Close()
        {
            IDCarTree.CloseFileInteraction();
            VINCarTree.CloseFileInteraction();
            CarHeapFile.CloseFileInteraction();
        }
    }
}
