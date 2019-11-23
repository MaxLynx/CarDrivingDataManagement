using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarDrivingDataManagement.Utils
{
    public interface IRecordable<T>
    {
        byte[] GetBytes();

        T newInstance(byte[] bytes);

        bool IsNull();
    }
}
