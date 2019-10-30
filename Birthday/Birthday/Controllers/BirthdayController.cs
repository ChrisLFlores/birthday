using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Birthday.Lib;
using Birthday.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace Birthday.Controllers
{
    public class BirthdayController : Controller
    {
        public IBirthdayRepo birthdayRepo { get; }

        public BirthdayController(IBirthdayRepo repo) =>
               birthdayRepo = repo ?? throw new ArgumentNullException(nameof(repo));

        // GET: Birthday
        public IActionResult Index([FromQuery]string search = "")
        {
            try
            {
                var employees = birthdayRepo.GetEmployees(search);
                var employeeModels = employees.Select(MapEToE);
                return View(employeeModels);
            }
            catch (Exception)
            {

                return View();
            }
            
        }

        // GET: Birthday/Details/5
        public ActionResult Details(int id)
        {
            var employee = birthdayRepo.GetEmployeeById(id);
            var employeeModel = MapEToE(employee);
            return View(employeeModel);
        }

        // GET: Birthday/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Birthday/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EmployeeBirthday employeeModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var employee = MapEToE(employeeModel);
                    birthdayRepo.AddEmployee(employee);
                    birthdayRepo.Save();

                    return RedirectToAction(nameof(Index));
                }
                //Add messeage to say why it failed
                return View(employeeModel);
            }
            catch
            {
                return View();
            }
        }

        // GET: Birthday/Edit/5
        public ActionResult Edit(int id)
        {
            var employeeModel = MapEToE(birthdayRepo.GetEmployeeById(id));
            return View(employeeModel);
        }

        // POST: Birthday/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, EmployeeBirthday employeeModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var employee = MapEToE(employeeModel);
                    birthdayRepo.EditEmployee(employee);
                    birthdayRepo.Save();

                    return RedirectToAction(nameof(Index));
                }
                //Add messeage to say why it failed
                return View(employeeModel);
            }
            catch
            {
                return View();
            }
        }

        // GET: Birthday/Delete/5
        public ActionResult Delete(int id)
        {
            var employeeModel = MapEToE(birthdayRepo.GetEmployeeById(id));
            return View(employeeModel);
        }

        // POST: Birthday/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection employeeModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    birthdayRepo.DeleteEmployee(id);
                    birthdayRepo.Save();

                    return RedirectToAction(nameof(Index));
                }
                //Add messeage to say why it failed
                return View(employeeModel);
            }
            catch
            {
                return View();
            }
        }

        public EmployeeBirthday MapEToE(Lib.Employees employee) => new EmployeeBirthday
        {
            EmployeeId = employee.EmployeeId,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            //Birthday = employee.Birthday
            Month = employee.Birthday.Value.Month,
            Day = employee.Birthday.Value.Day,
        };

        public Lib.Employees MapEToE(EmployeeBirthday employee) => new Lib.Employees
        {
            EmployeeId = employee.EmployeeId,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            //Birthday = employee.Birthday,
            Birthday = Convert.ToDateTime(employee.Month.ToString() + "/" + employee.Day.ToString() + "/1111")
        };

    }
}