using System;
using System.Collections.Generic;
using System.Text;

namespace Birthday.Lib
{
    public interface IBirthdayRepo
    {
        IEnumerable<Employees> GetEmployees(string search);
        Lib.Employees GetEmployeeById(int id);
        void AddEmployee(Employees employee);
        void EditEmployee(Employees employee);
        void DeleteEmployee(int id);
        void Save();
        void Dispose();

    }
}
