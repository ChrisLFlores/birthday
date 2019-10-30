using System;
using System.Collections.Generic;
using System.Text;

namespace Birthday.DataAccess
{
    public static class Mapper
    {
        public static Lib.Employees Map(Entities.Employees employee) => new Lib.Employees
        {
            EmployeeId = employee.EmployeeId,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Birthday = employee.Birthday
        };

        public static Entities.Employees Map(Lib.Employees employee) => new Entities.Employees
        {
            EmployeeId = employee.EmployeeId,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Birthday = employee.Birthday
        };
    }
}
