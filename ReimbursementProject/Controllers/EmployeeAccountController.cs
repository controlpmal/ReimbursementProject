using Microsoft.AspNetCore.Mvc;
using ReimbursementProject.Data;
using ReimbursementProject.Models;
using System;

namespace ReimbursementProject.Controllers
{
    public class EmployeeAccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeeAccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /EmployeeAccount/Create
        public IActionResult Create()
        {
            return View();
        }
     



        [HttpGet]
        public IActionResult getToatalamount( string empid)
        {
            var data = _context.EmployeeAccount.Where(e=>e.EMP_ID==empid)
                .GroupBy(e => e.EMP_ID)
                .Select(g => new
                {
                    EMP_ID = g.Key,
                    EMP_NAME = g.First().EMP_NAME, // or g.Max(e => e.EMP_NAME)
                    TotalAdvance = g.Sum(x => x.ADVANCE_AMOUNT ?? 0),
                    LastTransaction = g.Max(x => x.DATETIME)
                })
                .ToList();

            return View(data);
        }

        [HttpGet]

        public IActionResult getEmployeeName(string empid)
        {
            var data = _context.EmployeeDetails.Where(e => e.EmpID == empid)
             
                .Select(g=> g.EmpName)
                .ToList();

            return View(data);
        }


        //// POST: /EmployeeAccount/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult Create(EmployeeAccount model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        model.DATETIME = DateTime.Now;
        //        _context.EmployeeAccount.Add(model);
        //        _context.SaveChanges();
        //        TempData["Success"] = "Advance entry saved successfully.";
        //        return RedirectToAction("Create");
        //    }
        //    return View(model);
        //}


        [HttpGet]
        public IActionResult Create(string empid)
        {
            var model = new EmployeeAccount();

            if (!string.IsNullOrEmpty(empid))
            {
                // Fetch employee name
                var emp = _context.EmployeeDetails.FirstOrDefault(e => e.EmpID == empid);
                if (emp != null)
                {
                    model.EMP_ID = emp.EmpID;
                    model.EMP_NAME = emp.EmpName;
                }

                // Fetch total advance from EmployeeAccount
                var totalData = _context.EmployeeAccount
                    .Where(e => e.EMP_ID == empid)
                    .GroupBy(e => e.EMP_ID)
                    .Select(g => new
                    {
                        TotalAdvance = g.Sum(x => x.ADVANCE_AMOUNT ?? 0)
                    })
                    .FirstOrDefault();

                if (totalData != null)
                {
                    model.ADVANCE_AMOUNT = totalData.TotalAdvance;
                }
            }

            model.DATETIME = DateTime.Now;

            return View(model);
        }

        [HttpPost]
        public IActionResult Create(EmployeeAccount model)
        {
            if (ModelState.IsValid)
            {
                // Negative for IMPRESS or BILL APPROVAL
                if (model.PAYMENT_TYPE == "IMPRESS" || model.PAYMENT_TYPE == "BILL APPROVAL")
                {
                    model.ADVANCE_AMOUNT = -(Math.Abs(model.ADVANCE_AMOUNT ?? 0));
                }
                else if (model.PAYMENT_TYPE == "BILL")
                {
                    model.ADVANCE_AMOUNT = Math.Abs(model.ADVANCE_AMOUNT ?? 0);
                }

                model.DATETIME = DateTime.Now;

                _context.EmployeeAccount.Add(model);
                _context.SaveChanges();

                return RedirectToAction("Create", new { empid = model.EMP_ID });
            }
            return View(model);
        }
    }
}
