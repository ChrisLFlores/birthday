using Birthday;
using Birthday.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Birthday.DataAccess
{
    public class BirthdayRepo : Lib.IBirthdayRepo
    {
        private readonly BirthdayContext _dbContext;

        public BirthdayRepo(BirthdayContext dbContext) =>
               _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        public IEnumerable<Lib.Employees> GetEmployees(string search)
        {
            var strings = search.Split(' ');
            if (search == "")
            {
                var employees = _dbContext.Employees;
                return employees.Select(Mapper.Map);
            }
            else if (strings.Length >= 3)
            {
                var employees = _dbContext.Employees.Where(x => x.FirstName == strings[0] && x.LastName == strings[strings.Length - 1]);
                return employees.Select(Mapper.Map);
            }
            else if (strings.Length == 2)
            {
                var employees = _dbContext.Employees.Where(x => x.FirstName == strings[0] && x.LastName == strings[1]);
                return employees.Select(Mapper.Map);
            }
            else
            {
                var employees = _dbContext.Employees.Where(x => x.FirstName == strings[0] || x.LastName == strings[0]);
                return employees.Select(Mapper.Map);
            }
        }

        public Lib.Employees GetEmployeeById(int id)
        {
            if (id == 0)
            {
                throw new KeyNotFoundException();
            }
            var employee = _dbContext.Employees.Where(x => x.EmployeeId == id).FirstOrDefault();
            return Mapper.Map(employee);
        }

        public void AddEmployee(Lib.Employees employee)
        {
            var entity = Mapper.Map(employee);
            entity.EmployeeId = 0;
            _dbContext.Add(entity);
        }

        public void EditEmployee(Lib.Employees employee)
        {
            var entity = Mapper.Map(employee);
            var current = _dbContext.Employees.First(x=>x.EmployeeId == entity.EmployeeId);
            _dbContext.Entry(current).CurrentValues.SetValues(entity);
        }

        public void DeleteEmployee(int id)
        {
            var entity = _dbContext.Employees.FirstOrDefault(x=>x.EmployeeId == id);
            _dbContext.Remove(entity);
        }

        public void Save()
        {
            //_logger.Info("Saving changes to the database");
            _dbContext.SaveChanges();
        }

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _dbContext.Dispose();
                }

                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }

    }
}
