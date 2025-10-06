using Microsoft.AspNetCore.Mvc;

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ReimbursementProject.Data;
using ReimbursementProject.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using MimeKit;
using MailKit.Net.Smtp;

namespace ReimbursementProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EmployeeController(ApplicationDbContext context)
        {
            _context = context;
        }



        // ✅ GET: /api/MasterData/employees
        [HttpGet("employees1")]
        public IActionResult GetEmployees1()
        {
            var employees = _context.EmployeeDetails.ToList();
         
            return Ok(employees);
        }



        [HttpPost("DoLogin")]
        public async Task<IActionResult> DoLogin([FromBody] LoginDto dto)
        {
            if (dto == null) return BadRequest(new { message = "Invalid request" });

            // Look up user server-side (do NOT send all users to the client)
            var employee = _context.EmployeeDetails.SingleOrDefault(e => e.EmpID == dto.EmpId);
            if (employee == null) return BadRequest(new { message = "Employee ID not found." });

            // TODO: Replace plain-text compare with hashed password compare
            if (employee.Password != dto.Password) return BadRequest(new { message = "Password is incorrect." });

            if (string.IsNullOrWhiteSpace(employee.Status)) return BadRequest(new { message = "Pending approval from HR." });
            if (employee.Status.ToUpper() != "OK") return BadRequest(new { message = "Not approved from HR." });

            // Create claims (only store non-sensitive info)
            var claims = new List<Claim>
{
    new Claim("EmpID", employee.EmpID),
    new Claim("EmpName", employee.EmpName ?? string.Empty),
    new Claim("Level", employee.Level ?? string.Empty),
    new Claim("Designation", employee.Designation ?? string.Empty),
    new Claim("IRB", employee.IRB ?? string.Empty)
};

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                new AuthenticationProperties { IsPersistent = false, ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30) });
            
            // Optionally also set small server session values (not required):
            HttpContext.Session.SetString("EmpID", employee.EmpID);
            
         

            return Ok(new { success = true, redirectUrl = Url.Action("Index", "Home") });
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return Ok(new { success = true });
        }


        [HttpGet("employees")]
        public IActionResult GetEmployees()
        {
            var employees = _context.EmployeeDetails
                .Select(e => new { EmpID = e.EmpID, EmpName = e.EmpName, Designation = e.Designation, Level = e.Level })
                .ToList();
            return Ok(employees);
        }



        [HttpGet("totalAmount")]
        public IActionResult getToatalamount(string empid)
        {
            var data = _context.EmployeeAccount.Where(e => e.EMP_ID == empid)
                .GroupBy(e => e.EMP_ID)
                .Select(g => new
                {
                    EMP_ID = g.Key,
                    EMP_NAME = g.First().EMP_NAME, // or g.Max(e => e.EMP_NAME)
                    TotalAdvance = g.Sum(x => x.ADVANCE_AMOUNT ?? 0),
                    LastTransaction = g.Max(x => x.DATETIME)
                })
                .ToList();

            return Ok(data);
        }

        [HttpGet("employeeName")]

        public IActionResult getEmployeeName(string empid)
        {
            var data = _context.EmployeeDetails.Where(e => e.EmpID == empid)

                .Select(g => g.EmpName)
                .ToList();

            return Ok(data);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] string empid)
        {
            if (string.IsNullOrEmpty(empid))
                return BadRequest("empid is required");

            var history = await _context.EmployeeAccount
                .Where(e => e.EMP_ID == empid)
                .OrderByDescending(e => e.DATETIME)
                .ToListAsync();

            if (history == null || history.Count == 0)
                return NotFound("No history found for this empid");

            return Ok(history);
        }


        // ✅ GET: /api/MasterData/expenseTypes
        //[HttpGet("expenseTypes")]
        //public IActionResult GetExpenseTypes()
        //{
        //    var expenses = _context.TypeOfExpense.ToList();
        //    return Ok(expenses);
        //}
        [HttpGet("expenseTypes")]
        public IActionResult GetExpenseTypes()
        {
            var expenses = _context.TypeOfExpense
                .Select(t => new { t.ID, TypeOfExpense = t.TypeOfExpense })
                .ToList();
            return Ok(expenses);
        }




        //// ✅ GET: /api/MasterData/expenseLimits
        //[HttpGet("expenseLimits")]
        //public IActionResult GetExpenseLimits()
        //{
        //    var limits = _context.ExpenseLimitDetails.ToList();
        //    return Ok(limits);
        //}


        [HttpGet("expenseLimits")]
        public IActionResult GetExpenseLimits(string level, string type, bool withBill = true)
        {
            // find the matching limit row (Level + TypeOfExpense)
            var row = _context.ExpenseLimitDetails
                        .Where(x => x.Level == level && x.TypeOfExpense == type)
                        .FirstOrDefault();

            double maxLimit = 0;
            if (row != null)
            {
                maxLimit = withBill ? (row.MaxLimitWithBill ?? 0) : (row.MaxLimitWOBill ?? 0);
            }

            return Ok(new { maxLimit });
        }




        [HttpPost("save")]
        public IActionResult SaveExpenses([FromBody] List<ExpenseLogBook> expenses)
        {
            if (expenses == null || !expenses.Any())
                return BadRequest("No expenses provided.");
            var lastId = _context.ExpenseLogBook
                     .OrderByDescending(e => e.ID)
                     .Select(e => e.ID)
                     .FirstOrDefault();


            foreach (var exp in expenses)
            {
                exp.SubmissionDate = DateTime.Now;  // current datetime
                exp.Status = "0";
                exp.Quantity = exp.Quantity == 0 ? 1 : exp.Quantity;

                // default pending
            }

            _context.ExpenseLogBook.AddRange(expenses);
            _context.SaveChanges();

            return Ok(new { message = "Expenses saved successfully!" });
        }
        [HttpGet("pendingCount")]
        public IActionResult GetPendingCount()
        {
            var count = _context.ExpenseLogBook
                .Where(e => e.Status == "0")
                .GroupBy(e => new { e.EmpID, e.SubmissionDate, e.ProjectCode })
                .Count();

            return Ok(count);
        }

        // ✅ Register API
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] EmployeeDetails emp)
        {
            if (await _context.EmployeeDetails.AnyAsync(e => e.EmpID == emp.EmpID))
                return BadRequest(new { message = "EmpID already exists" });

            emp.Status = null; // Default: waiting for HR approval
            _context.EmployeeDetails.Add(emp);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Registration successful. Waiting for HR approval" });
        }

        // GET api/email/send?toEmail=someone@example.com
        [HttpGet("send")]
        public IActionResult SendOtp(string toEmail)
        {
            try
            {
                // 1. Generate 4-digit random OTP
                Random random = new Random();
                string otp = random.Next(1000, 9999).ToString();

                // 2. Create email message
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("PMAL Control", "pmalcontrol@gmail.com"));
                emailMessage.To.Add(new MailboxAddress("", toEmail));
                emailMessage.Subject = "Your OTP Code";
                emailMessage.Body = new TextPart("plain")
                {
                    Text = $"Hello,\n\nYour OTP is: {otp}\n\nRegards,\nPMAL Control"
                };

                // 3. Send email via Gmail SMTP
                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                    client.Authenticate("5302akashsingh@gmail.com", "ivhj gjrm shvu nytq");
                    client.Send(emailMessage);
                    client.Disconnect(true);
                }

                // 4. Return OTP in API response (for testing only)
                return Ok(new { Success = true, OTP = otp, Message = "OTP sent successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }









        public class ExpenseRowDto
        {
            public string DateOfExpense { get; set; }
            public string TypeOfExpense { get; set; }
            public string? TravelLocation { get; set; }
            public double? KM { get; set; }
            public int? Quantity { get; set; }
            public string? FellowMembers { get; set; }
            public string BillType { get; set; } // "with" or "without"
            public double? SanctionedAmount { get; set; }
            public double? ClaimAmount { get; set; }
            public int? FileIndex { get; set; } // maps to file_i in form data
        }


        [HttpPost("submit")]
        [RequestSizeLimit(20 * 1024 * 1024)] // allow up to 20MB total
        public IActionResult Submit()
        {
            var form = Request.Form;
            var empId = form["EmpID"].ToString();
            var site = form["SiteName"].ToString();
            var project = form["ProjectCode"].ToString();
            var level = form["Level"].ToString();
            var irb = form["IRB"].ToString();
            var rowsJson = form["Rows"].ToString();

            if (string.IsNullOrWhiteSpace(empId) || string.IsNullOrWhiteSpace(rowsJson))
                return BadRequest(new { success = false, message = "Missing data" });

            List<ExpenseRowDto> rows;
            try
            {
                rows = JsonSerializer.Deserialize<List<ExpenseRowDto>>(rowsJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch
            {
                return BadRequest(new { success = false, message = "Invalid rows JSON" });
            }

            // ✅ Create ExpenseBillBook (header entry)
            var now = DateTime.Now;
            var expenseBill = new ExpenseBillBook
            {
                ExpenseBillNumber = $"EBL{empId}{now:yyyyMMddHHmmss}", // EBL + EmpID + timestamp till seconds
                SubmissionDate = now,
                Status = "Pending"
            };

            _context.ExpenseBillBook.Add(expenseBill);
            _context.SaveChanges(); // save once so ExpenseID is generated

            foreach (var r in rows)
            {
                var el = new ExpenseLogBook
                {
                    SiteName = site,
                    ProjectCode = project,
                    EmpID = empId,
                    SubmissionDate = now,
                    DateofExpense = DateTime.TryParse(r.DateOfExpense, out var dt) ? dt : (DateTime?)null,
                    TypeOfExpense = r.TypeOfExpense,
                    Quantity = r.Quantity,
                    TravelLocation=r.TravelLocation,
                    FellowMembers = r.FellowMembers,
                    ClaimAmount = r.ClaimAmount,
                    IRB = irb,
                    Status = "0", // pending
                    RequireSpecialApproval = (r.ClaimAmount > (r.SanctionedAmount ?? 0)) ? "true" : "false",
                    Rejection = null,
                    ExpenseID = expenseBill.ExpenseID  // ✅ link with parent
                };

                // Handle file if exists
                if (r.FileIndex != null)
                {
                    var fileKey = $"file_{r.FileIndex}";
                    var file = Request.Form.Files.FirstOrDefault(f => f.Name == fileKey);
                    if (file != null && file.Length > 0)
                    {
                        using var ms = new MemoryStream();
                        file.CopyTo(ms);
                        el.BillDocument = ms.ToArray();
                        el.BillFileName = file.FileName;
                        el.BillContentType = file.ContentType;
                    }
                }

                // Recompute sanctioned amount (server-side safe check)
                double computedSanctioned = 0;
                var limitRow = _context.ExpenseLimitDetails
                                       .FirstOrDefault(x => x.Level == level && x.TypeOfExpense == r.TypeOfExpense);
                double maxLimit = 0;
                if (limitRow != null)
                {
                    maxLimit = (r.BillType?.ToLower() == "with")
                        ? (limitRow.MaxLimitWithBill ?? 0)
                        : (limitRow.MaxLimitWOBill ?? 0);
                }

                bool isTravel = (r.TypeOfExpense ?? "").ToUpper().Contains("TRAVEL BIKE")
                                || (r.TypeOfExpense ?? "").ToUpper().Contains("TRAVEL CAR");
                if (isTravel) computedSanctioned = (r.KM ?? 0) * maxLimit;
                else computedSanctioned = (r.Quantity ?? 0) * maxLimit;

                el.SanctionedAmount = computedSanctioned;

                _context.ExpenseLogBook.Add(el);
            }

            _context.SaveChanges();

            return Ok(new { success = true, billNo = expenseBill.ExpenseBillNumber });
        }


        [HttpGet("new")]
        public IActionResult GetNewEmployees()
        {
            var employees = _context.EmployeeDetails
                .Where(e => e.Status != "OK")
                .Select(e => new {
                    empId = e.EmpID,
                    empName = e.EmpName,
                    empLevel=e.Level,
                    empDesignation=e.Designation,
                    empIrb=e.IRB,
                    empDepartment=e.Dept,
                    empMail = e.MailID, // ⬅️ Add this line
                    photoUrl = "/uploads/employees/" + e.EmpID + ".jpg" // store photos with EmpID.jpg
                })
                .ToList();

            return Ok(employees);
        }

        // Approve employee
        [HttpPost("approve/{empId}")]
        public IActionResult ApproveEmployee(string empId, [FromBody] EmployeeDetails updatedEmp)
        {
            var emp = _context.EmployeeDetails.FirstOrDefault(e => e.EmpID == empId);
            if (emp == null) return NotFound(new { success = false, message = "Employee not found" });

            // Update editable fields
            emp.IRB = updatedEmp.IRB;
            emp.Level = updatedEmp.Level;
            emp.Designation = updatedEmp.Designation;
            emp.Dept = updatedEmp.Dept;
            emp.MailID = updatedEmp.MailID; // ⬅️ Add this line

            // Approve employee
            emp.Status = "OK";

            _context.SaveChanges();
            return Ok(new { success = true, message = "Employee approved and updated successfully" });
        }


        // Get all employees
        [HttpGet("all")]
        public IActionResult GetAllEmployees()
        {
            return Ok(_context.EmployeeDetails.ToList());
        }

        // Delete employee
        [HttpDelete("{empId}")]
        public IActionResult DeleteEmployee(string empId)
        {
            var emp = _context.EmployeeDetails.FirstOrDefault(e => e.EmpID == empId);
            if (emp == null) return NotFound();

            _context.EmployeeDetails.Remove(emp);
            _context.SaveChanges();

            return Ok(new { success = true });
        }

        // Edit employee
        [HttpPut("{empId}")]
        public IActionResult EditEmployee(string empId, [FromBody] EmployeeDetails updated)
        {
            var emp = _context.EmployeeDetails.FirstOrDefault(e => e.EmpID == empId);
            if (emp == null) return NotFound();

            emp.EmpName = updated.EmpName;
            emp.Designation = updated.Designation;
            emp.Level = updated.Level;
            emp.IRB = updated.IRB;
            emp.Dept = updated.Dept;
            emp.AdvanceAmount = updated.AdvanceAmount;
            emp.Password = updated.Password;
            emp.MailID = updated.MailID;

            _context.SaveChanges();

            return Ok(new { success = true });
        }


        [HttpGet("check-duplicate")]
        public IActionResult CheckDuplicate(string empId, string typeOfExpense, DateTime dateOfExpense)
        {
            if (string.IsNullOrEmpty(empId) || string.IsNullOrEmpty(typeOfExpense))
                return BadRequest(new { success = false, message = "Invalid input" });

            var duplicate = _context.ExpenseLogBook
                .Where(e => e.DateofExpense.HasValue
                            && e.DateofExpense.Value.Date == dateOfExpense.Date
                            && e.TypeOfExpense == typeOfExpense
                            && (
                                e.EmpID == empId ||
                                (e.FellowMembers != null && e.FellowMembers.Contains(empId))
                               )
                       )
                .FirstOrDefault();

            if (duplicate != null)
                return Ok(new { success = true, exists = true, message = "Duplicate expense found for this date and type." });

            return Ok(new { success = true, exists = false });
        }



        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword([FromBody] string empId)
        {
            var employee = _context.EmployeeDetails.FirstOrDefault(e => e.EmpID == empId);
            if (employee == null || string.IsNullOrWhiteSpace(employee.MailID))
                return NotFound(new { success = false, message = "Employee not found or email not available." });

            // Reuse your SendOtp logic
            Random random = new Random();
            string otp = random.Next(1000, 9999).ToString();

            try
            {
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("PMAL Control", "pmalcontrol@gmail.com"));
                emailMessage.To.Add(new MailboxAddress("", employee.MailID));
                emailMessage.Subject = "Your OTP Code";
                emailMessage.Body = new TextPart("plain")
                {
                    Text = $"Hello {employee.EmpName},\n\nYour OTP is: {otp}\n\nRegards,\nPMAL Control"
                };

                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                    client.Authenticate("5302akashsingh@gmail.com", "ivhj gjrm shvu nytq");
                    client.Send(emailMessage);
                    client.Disconnect(true);
                }

                // Store OTP in TempData / Memory / Cache / or return it for now (not safe for production)
                HttpContext.Session.SetString("ResetOtp", otp);
                HttpContext.Session.SetString("ResetEmpId", empId);

                return Ok(new { success = true, message = "OTP sent to your registered email." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }



        [HttpPost("verify-otp")]
        public IActionResult VerifyOtp([FromBody] string enteredOtp)
        {
            var storedOtp = HttpContext.Session.GetString("ResetOtp");
            if (storedOtp == enteredOtp)
            {
                return Ok(new { success = true, message = "OTP verified" });
            }

            return BadRequest(new { success = false, message = "Invalid OTP" });
        }


        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] PasswordResetModel model)
        {
            var empId = HttpContext.Session.GetString("ResetEmpId");
            if (string.IsNullOrEmpty(empId)) return Unauthorized();

            var emp = _context.EmployeeDetails.FirstOrDefault(e => e.EmpID == empId);
            if (emp == null) return NotFound(new { success = false, message = "Employee not found" });

            emp.Password = model.NewPassword;
            _context.SaveChanges();

            HttpContext.Session.Remove("ResetEmpId");
            HttpContext.Session.Remove("ResetOtp");

            return Ok(new { success = true, message = "Password updated successfully" });
        }

        public class PasswordResetModel
        {
            public string NewPassword { get; set; }
        }



    }
}
