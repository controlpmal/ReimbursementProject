using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using ReimbursementProject.Data;
using ReimbursementProject.Models;
using System.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ReimbursementProject.Controllers
{
    //[Authorize(AuthenticationSchemes = "EmployeeAuth")]
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        //  pending count get from here 
        [HttpGet("pending")]
        public IActionResult GetPendingCount(string empid, string designation)
        {
            var employee = _context.ExpenseLogBook.FirstOrDefault(e => e.IRB == empid);
            IQueryable<ExpenseLogBook> query = _context.ExpenseLogBook.Where(e => e.Rejection != "reject");
            if (empid != null && empid!="")
            {

               

               
                 if (employee!=null )
                {
                    query = query.Where(e => (e.IRB == empid && e.Status == "0")||(e.EmpID == empid && e.Status != "4"));
                }
                else if (designation == "HR")
                {
                    query = query.Where(e => (e.Status == "1" )|| (e.EmpID == empid && e.Status != "4"));
                }
                else if (designation == "AGM")
                {
                    query = query.Where(e => (e.Status == "2") || (e.EmpID == empid && e.Status != "4"));
                }
                else if (designation == "ACCOUNTS")
                {
                    query = query.Where(e => (e.Status == "3") || (e.EmpID == empid && e.Status != "4"));
                }
                else
                {
                    query = query.Where(e => e.EmpID == empid && e.Status!="4");
                }

                    var count = query
                        .GroupBy(e => new { e.EmpID, e.SubmissionDate, e.ProjectCode,e.ExpenseID })
                        .Count();

                return Ok(count);
            }
            else {  return Ok(0); };
        }

        // approved count get from here 
        [HttpGet("approved")]
        public IActionResult GetApprovedCount(string empid, string designation)
        {
            var employee = _context.ExpenseLogBook.FirstOrDefault(e => e.IRB == empid);
            IQueryable<ExpenseLogBook> query = _context.ExpenseLogBook.Where(e => e.Rejection != "reject");
            if (empid != null && empid != "")
            {



              
                 if (employee != null)
                {
                    query = query.Where(e => (e.IRB == empid && e.Status != "0")||(e.EmpID == empid && e.Status == "4"));
                }
                else if (designation == "HR")
                {
                    query = query.Where(e => (e.Status != "0"&&e.Status!="1") || (e.EmpID == empid && e.Status == "4"));
                }
                else if (designation == "AGM")
                {
                    query = query.Where(e => (e.Status != "0" && e.Status != "1"&&e.Status!="2" ) || (e.EmpID == empid && e.Status == "4"));
                }
                else if (designation == "ACCOUNTS")
                {
                    query = query.Where(e => (e.Status == "4") || (e.EmpID == empid && e.Status != "4"));
                }
                else
                {
                    query = query.Where(e => e.EmpID == empid && e.Status == "4" );
                }

                    var count = query
                        .GroupBy(e => new { e.EmpID, e.SubmissionDate, e.ProjectCode, e.ExpenseID })
                        .Count();
                return Ok(count);
            }
            else
            {
                return Ok(0);
            }
           
        }






        // rejected count get form here 
        [HttpGet("rejected")]
        public IActionResult GetRejectedCount(string empid, string designation)
        {
            var employee = _context.ExpenseLogBook.FirstOrDefault(e => e.IRB == empid);
            IQueryable<ExpenseLogBook> query = _context.ExpenseLogBook.Where(e => e.Rejection == "reject");
            if (empid != "null")
            {
                if (employee != null)
                {
                    query = query.Where(e =>(e.IRB == empid && e.Status == "0")||(e.EmpID == empid));
                }
                else if (designation == "HR")
                {
                    query = query.Where(e => (e.Status == "1") || (e.EmpID == empid));
                }
                else if (designation == "AGM")
                {
                    query = query.Where(e => (e.Status == "2") || (e.EmpID == empid));
                }
                else if (designation == "ACCOUNTS")
                {
                    query = query.Where(e => (e.Status == "3") || (e.EmpID == empid)); 
                }
                else
                {
                    query = query.Where(e => e.EmpID == empid );
                }

                var count = query
                    .GroupBy(e => new { e.EmpID, e.SubmissionDate, e.ProjectCode, e.ExpenseID })
                    .Count();
                return Ok(count);
            }
            else
            {
                return Ok(0);
            }



        }



 // inside DashboardController
 [HttpGet("details")]
    public IActionResult GetGroupedDetails(string empid, string designation, string type)
    {
            // base filter
            var employee = _context.ExpenseLogBook.FirstOrDefault(e => e.IRB == empid);
            IQueryable<ExpenseLogBook> baseQuery = _context.ExpenseLogBook;

            // rejection logic:
            if (type == "rejected")
            {


                baseQuery = baseQuery.Where(e => e.Rejection == "reject");

                if (!string.IsNullOrEmpty(empid))
                {



                    if (employee != null)
                    {
                        baseQuery = baseQuery.Where(e => (e.IRB == empid && e.Status == "0")||(e.EmpID == empid));
                    }
                    else if (designation == "HR")
                    {
                        baseQuery = baseQuery.Where(e => (e.Status == "1") || (e.EmpID == empid));
                    }   
                    else if (designation == "AGM")
                    {
                        baseQuery = baseQuery.Where(e => (e.Status == "2") || (e.EmpID == empid));
                    }
                    else if (designation == "ACCOUNTS")
                    {
                        baseQuery = baseQuery.Where(e => (e.Status == "3") || (e.EmpID == empid));
                    }
                    else
                    {
                        baseQuery = baseQuery.Where(e => e.EmpID == empid);
                    }
                }
                else
                {


                    return Ok(new List<DashboardGroupDto>());

                }

            }

            else if (type == "approved")
            {
                baseQuery = baseQuery.Where(e => e.Rejection != "reject");


                if (!string.IsNullOrEmpty(empid))
                {



                    if (employee != null)
                    {
                        baseQuery = baseQuery.Where(e => (e.IRB == empid && e.Status != "0")||(e.EmpID == empid && e.Status == "4"));
                    }
                    else if (designation == "HR")
                    {
                        baseQuery = baseQuery.Where(e => (e.Status != "0" && e.Status!="1") || (e.EmpID == empid && e.Status == "4"));
                    }
                    else if (designation == "AGM")
                    {
                        baseQuery = baseQuery.Where(e => (e.Status != "0" && e.Status != "1" && e.Status != "2") || (e.EmpID == empid && e.Status == "4"));
                    }
                    else if (designation == "ACCOUNTS")
                    {
                        baseQuery = baseQuery.Where(e => (e.Status == "4") || (e.EmpID == empid && e.Status == "4"));
                    }
                    else
                    {
                        baseQuery = baseQuery.Where(e => e.EmpID == empid && e.Status == "4");
                    }
                }
                else
                {


                    return Ok(new List<DashboardGroupDto>());

                }

            }
            else if (type == "pending")
            {


                baseQuery = baseQuery.Where(e => e.Rejection != "reject" && e.Status != "4");

                if (!string.IsNullOrEmpty(empid))
                {



                    if (employee != null)
                    {
                        baseQuery = baseQuery.Where(e => (e.IRB == empid && e.Status == "0")||( e.EmpID==empid && e.Status!="4"));
                    }
                    else if (designation == "HR")
                    {
                        baseQuery = baseQuery.Where(e => (e.Status == "1") || (e.EmpID == empid && e.Status != "4"));
                    }
                    else if (designation == "AGM")
                    {
                        baseQuery = baseQuery.Where(e => (e.Status == "2") || (e.EmpID == empid && e.Status != "4"));
                    }
                    else if (designation == "ACCOUNTS")
                    {
                        baseQuery = baseQuery.Where(e => (e.Status == "3") || (e.EmpID == empid && e.Status != "4"));
                    }
                    else
                    {
                        baseQuery = baseQuery.Where(e => e.EmpID == empid && e.Status != "4");
                    }
                }
                else
                {


                    return Ok(new List<DashboardGroupDto>());

                }




            }

        // role-specific filtering (same logic as your counts)
   

            // group by EmpID + SubmissionDate (date only) + ProjectCode + SiteName
         
 
           var groups = (
        from e in baseQuery
        join emp in _context.EmployeeDetails on e.EmpID equals emp.EmpID into empJoin
        from emp in empJoin.DefaultIfEmpty()
        join irbEmp in _context.EmployeeDetails on e.IRB equals irbEmp.EmpID into irbJoin
        from irbEmp in irbJoin.DefaultIfEmpty()
        group new { e, emp, irbEmp } by new {
            e.EmpID,
            e.ExpenseID,
            SubmissionDate = e.SubmissionDate.HasValue ? e.SubmissionDate.Value.Date : (DateTime?)null,
            e.ProjectCode,
            e.SiteName,
            e.IRB,
            e.Status,
            e.Rejection
        } into g
        select new DashboardGroupDto {
            EmpID = g.Key.EmpID,
            ExpenseId = g.Key.ExpenseID,
            SubmissionDate = g.Key.SubmissionDate,
            ProjectCode = g.Key.ProjectCode,
            SiteName = g.Key.SiteName,
            IRB = g.Key.IRB,
            Status = g.Key.Status,
            Rejection = g.Key.Rejection,
            TotalClaimAmount = g.Sum(x => x.e.ClaimAmount ?? 0),
            TotalSanctionedAmount = g.Sum(x => x.e.SanctionedAmount ?? 0),
            EmpName = g.Select(x => x.emp.EmpName).FirstOrDefault() ?? "",
            IRBName = g.Select(x => x.irbEmp.EmpName).FirstOrDefault() ?? ""
        }
    ).OrderByDescending(x => x.SubmissionDate)
     .ToList();

        return Ok(groups);
    }

        [HttpGet("groupitems")]
        public async Task<IActionResult> GetGroupItems(string empid, DateTime submissionDate, long expenseId)
        {
            // Normalize empid
            empid = empid.Trim();

            var items = await _context.ExpenseLogBook
                .Where(e => e.EmpID == empid
                            && e.ExpenseID == expenseId
                            && e.SubmissionDate.HasValue
                            && e.SubmissionDate.Value.Date == submissionDate.Date)
                .OrderBy(e => e.DateofExpense) // ✅ ascending order by date
                                               // .OrderByDescending(e => e.DateofExpense) // 👈 use this if you want latest first
                .Select(e => new ExpenseItemDto
                {
                    ID = e.ID,
                    DateOfExpense = e.DateofExpense,
                    TypeOfExpense = e.TypeOfExpense,
                    TravelLocation = e.TravelLocation,
                    Quantity = e.Quantity,
                    FellowMembers = e.FellowMembers,
                    ClaimAmount = e.ClaimAmount,
                    SanctionedAmount = e.SanctionedAmount,
                    BillDocument = e.BillDocument,
                    RequireSpecialApproval = e.RequireSpecialApproval,
                    Status = e.Status,
                    Rejection = e.Rejection
                })
                .ToListAsync();

            return Ok(items);
        }




        //        public class UpdateGroupActionDto
        //    {
        //        public string EmpID { get; set; } = "";
        //        public DateTime SubmissionDate { get; set; }
        //        public string ProjectCode { get; set; } = "";
        //        public string ActionBy { get; set; } = "";
        //            public long ExpenseId { get; set; } = 0;// EmpID who clicked accept/reject
        //}

        //// Accept (finalize) group - increments status to next stage or to 4 for accounts final
        //[HttpPost("accept-group")]
        //        public IActionResult AcceptGroup([FromBody] UpdateGroupActionDto dto)
        //        {
        //            var rows = _context.ExpenseLogBook
        //                .Where(e => e.EmpID == dto.EmpID
        //                         && e.ProjectCode == dto.ProjectCode
        //                         && e.SubmissionDate.HasValue
        //                         && e.SubmissionDate.Value.Date == dto.SubmissionDate.Date)
        //                .ToList();

        //            if (!rows.Any()) return NotFound();

        //            foreach (var r in rows)
        //            {
        //                // increment status number (stored as string)
        //                int st = 0;
        //                int.TryParse(r.Status ?? "0", out st);
        //                r.Status = (st + 1).ToString();
        //                r.Rejection = null; // clear rejection when accepted
        //            }
        //            _context.SaveChanges();
        //            return Ok();
        //        }

        //        // Reject group - set Rejection = "reject" and optionally increment status
        //        [HttpPost("reject-group")]
        //        public IActionResult RejectGroup([FromBody] UpdateGroupActionDto dto)
        //        {
        //            var rows = _context.ExpenseLogBook
        //                .Where(e => e.EmpID == dto.EmpID
        //                         && e.ProjectCode == dto.ProjectCode
        //                         && e.ExpenseID==dto.ExpenseId
        //                         && e.SubmissionDate.HasValue
        //                         && e.SubmissionDate.Value.Date == dto.SubmissionDate.Date)
        //                .ToList();

        //            if (!rows.Any()) return NotFound();

        //            foreach (var r in rows)
        //            {
        //                r.Rejection = "reject";
        //                // optionally increment status:
        //                int st = 0;
        //                int.TryParse(r.Status ?? "0", out st);
        //                r.Status = (st ).ToString();
        //            }
        //            _context.SaveChanges();
        //            return Ok();
        //        }


        [HttpPost("accept-group")]
        public IActionResult AcceptGroup([FromBody] AcceptGroupRequest req)
        {
            var items = _context.ExpenseLogBook
                .Where(e => e.EmpID == req.EmpID
                         && e.SubmissionDate == req.SubmissionDate.Date
                         && e.ProjectCode == req.ProjectCode
                         && e.ExpenseID == req.ExpenseId)
                .ToList();

            foreach (var row in req.Rows)
            {
                var item = items.FirstOrDefault(i => i.ID == row.ItemId);
                if (item != null)
                {
                    item.SanctionedAmount = row.NewSanctionedAmount;
                    item.RequireSpecialApproval = row.IsSpecial ? "false" :$"{row.IsSpecial}";
                    item.Rejection = row.IsRejected ? "reject" : null;

                    // Status progression (example)
                    if (!row.IsRejected)
                    {
                        
                        if (req.Designation == "HR") { item.Status = "2"; item.HRApprovelDate = DateTime.Now; }
                        else if (req.Designation == "AGM") { item.Status = "3"; item.AGMApprovelDate = DateTime.Now; }
                        else if (req.Designation == "ACCOUNTS") { item.Status = "4"; item.AccountApprovelDate = DateTime.Now; }
                        else  { item.Status = "1"; item.IRBApprovedDate = DateTime.Now; } 
                    }
                }
            }

            _context.SaveChanges();
            return Ok(new { success = true });
        }

        // Reject all
        [HttpPost("reject-group")]
        public IActionResult RejectGroup([FromBody] RejectGroupRequest req)
        {
            var items = _context.ExpenseLogBook
                .Where(e => e.EmpID == req.EmpID
                         && e.SubmissionDate == req.SubmissionDate.Date
                         && e.ProjectCode == req.ProjectCode
                         && e.ExpenseID == req.ExpenseId)
                .ToList();

            foreach (var item in items)
            {
                item.Rejection = "reject";
            }

            _context.SaveChanges();
            return Ok(new { success = true });
        }

        public class SpecialApproveDto
    {
        public long ItemId { get; set; }
        public double NewSanctionedAmount { get; set; }
        public string ActionBy { get; set; } = "";
}

// Special approve single item: replace sanctioned amount with claim amount (or given amount)
[HttpPost("special-approve")]
        public IActionResult SpecialApprove([FromBody] SpecialApproveDto dto)
        {
            var item = _context.ExpenseLogBook.FirstOrDefault(e => e.ID == dto.ItemId);
            if (item == null) return NotFound();

            // validate permission on server side if required (based on ActionBy)
            item.SanctionedAmount = dto.NewSanctionedAmount;
            item.RequireSpecialApproval = "true";
            _context.SaveChanges();
            return Ok();
        }



        [HttpGet]
        [Route("/Expenses/ViewBill/{id:long}")]
        public IActionResult ViewBill(int id)
        {
            var item = _context.ExpenseLogBook.FirstOrDefault(e => e.ID == id);
            if (item == null || item.BillDocument == null) return NotFound();

            string contentType = item.BillContentType ?? "application/octet-stream";
            string fileName = item.BillFileName ?? "document";

            // return as File so browser handles it properly
            return File(item.BillDocument, contentType, fileName);
        }


        // DELETE: api/dashboard/delete-expense/5
        [HttpDelete("delete-expense/{id:long}")]
        public async Task<IActionResult> DeleteExpense(long id)
        {
            var expense = await _context.ExpenseBillBook.FindAsync(id);
            if (expense == null)
                return NotFound(new { message = "Expense not found" });

            _context.ExpenseBillBook.Remove(expense);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Expense deleted successfully" });
        }



    }

    public class AcceptGroupRequest
    {
        public string EmpID { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string ProjectCode { get; set; }
        public string ActionBy { get; set; }
        public long ExpenseId { get; set; }
        public string Designation { get; set; }
        public List<RowUpdate> Rows { get; set; }
    }

    public class RowUpdate
    {
        public long ItemId { get; set; }
        public bool IsRejected { get; set; }
        public bool IsSpecial { get; set; }
        public double NewSanctionedAmount { get; set; }
    }

    public class RejectGroupRequest
    {
        public string EmpID { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string ProjectCode { get; set; }
        public string ActionBy { get; set; }
        public long ExpenseId { get; set; }
    }
}
