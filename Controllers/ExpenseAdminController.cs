using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReimbursementProject.Data;
using ReimbursementProject.Models;

namespace ReimbursementProject.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExpenseAdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ExpenseAdminController(ApplicationDbContext context) => _context = context;

        // DTO for grouped results
        public class GroupedSubmissionDto
        {
            public string EmpID { get; set; } = "";
            public string EmpName { get; set; } = "";
            public string IRB { get; set; } = "";
            public string IRBName { get; set; } = "";
            public string SiteName { get; set; } = "";
            public string ProjectCode { get; set; } = "";
            public DateTime? SubmissionDate { get; set; }
            public int RowCount { get; set; }
            public double TotalClaim { get; set; }
            public double TotalSanctioned { get; set; }
            public string Status { get; set; } = "";
        }

        // GET grouped list for dashboard -> role based
        // call: /api/ExpenseAdmin/grouped?empid=E1&designation=IRB&mode=pending
        [HttpGet("grouped")]
        public async Task<IActionResult> GetGrouped(
            string empid,
            string designation,
            string mode = "pending" // pending | approved | rejected
        )
        {
            // base query
            IQueryable<ExpenseLogBook> baseQ = _context.ExpenseLogBook.AsNoTracking();

            if (mode == "rejected")
            {
                baseQ = baseQ.Where(e => e.Rejection == "reject");
            }
            else
            {
                baseQ = baseQ.Where(e => e.Rejection != "reject");
            }
            var employeeirb = _context.EmployeeDetails.FirstOrDefault(e => e.IRB == empid);
            // apply role filters like in dashboard
            if (!string.IsNullOrEmpty(empid))
            {
                if (employeeirb!=null)
                {
                    if (mode == "pending") baseQ = baseQ.Where(e => e.IRB == empid && e.Status == "0");
                    else if (mode == "approved") baseQ = baseQ.Where(e => e.IRB == empid && e.Status != "0");
                }
                else if (designation == "HR")
                {
                    if (mode == "pending") baseQ = baseQ.Where(e => e.Status == "1");
                    else if (mode == "approved") baseQ = baseQ.Where(e => e.Status == "2" || e.Status == "3");
                }
                else if (designation == "AGM")
                {
                    if (mode == "pending") baseQ = baseQ.Where(e => e.Status == "2");
                    else if (mode == "approved") baseQ = baseQ.Where(e => e.Status == "3");
                }
                else if (designation == "ACCOUNTS")
                {
                    if (mode == "pending") baseQ = baseQ.Where(e => e.Status == "3");
                    else if (mode == "approved") baseQ = baseQ.Where(e => e.Status == "4");
                }
                else // normal employee
                {
                    if (mode == "pending") baseQ = baseQ.Where(e => e.EmpID == empid && e.Status == "0");
                    else if (mode == "approved") baseQ = baseQ.Where(e => e.EmpID == empid && e.Status != "0");
                }
            }
            else
            {
                // no empid -> return empty
                return Ok(new List<GroupedSubmissionDto>());
            }

            // group
            var groups = await baseQ
                .GroupBy(e => new { e.EmpID, e.SiteName, e.ProjectCode, e.SubmissionDate, e.IRB, e.Status })
                .Select(g => new
                {
                    g.Key.EmpID,
                    g.Key.SiteName,
                    g.Key.ProjectCode,
                    g.Key.SubmissionDate,
                    g.Key.IRB,
                    g.Key.Status,
                    RowCount = g.Count(),
                    TotalClaim = g.Sum(x => x.ClaimAmount ?? 0),
                    TotalSanctioned = g.Sum(x => x.SanctionedAmount ?? 0)
                })
                .OrderByDescending(x => x.SubmissionDate)
                .ToListAsync();

            // join names from EmployeeDetails
            var empIds = groups.Select(g => g.EmpID).Concat(groups.Select(g => g.IRB)).Distinct().ToList();
            var empDict = _context.EmployeeDetails
                .Where(e => empIds.Contains(e.EmpID))
                .ToDictionary(e => e.EmpID ?? "", e => e.EmpName ?? "");

            var result = groups.Select(g => new GroupedSubmissionDto
            {
                EmpID = g.EmpID,
                EmpName = empDict.ContainsKey(g.EmpID) ? empDict[g.EmpID] : "",
                IRB = g.IRB,
                IRBName = empDict.ContainsKey(g.IRB) ? empDict[g.IRB] : "",
                SiteName = g.SiteName,
                ProjectCode = g.ProjectCode,
                SubmissionDate = g.SubmissionDate,
                RowCount = g.RowCount,
                TotalClaim = g.TotalClaim,
                TotalSanctioned = g.TotalSanctioned,
                Status = g.Status
            }).ToList();

            return Ok(result);
        }

        // GET details for a grouped submission
        // /api/ExpenseAdmin/details?empid=E1&site=SiteA&project=PC1&submissionDate=2025-09-12T00:00:00
        [HttpGet("details")]
        public async Task<IActionResult> GetDetails(string empid, string site, string project, DateTime submissionDate)
        {
            var rows = await _context.ExpenseLogBook
                .Where(e => e.EmpID == empid && e.SiteName == site && e.ProjectCode == project && e.SubmissionDate == submissionDate)
                .Select(e => new {
                    e.ID,
                    e.DateofExpense,
                    e.TypeOfExpense,
                    e.Quantity,
                    e.FellowMembers,
                    HasFile = e.BillDocument != null,
                    e.ClaimAmount,
                    e.SanctionedAmount,
                    e.Status,
                    e.RequireSpecialApproval,
                    e.Rejection
                })
                .OrderBy(e => e.DateofExpense)
                .ToListAsync();

            return Ok(rows);
        }

        // stream bill document for a row id
        [HttpGet("file/{id:long}")]
        public async Task<IActionResult> GetFile(long id)
        {
            var row = await _context.ExpenseLogBook.FindAsync(id);
            if (row == null || row.BillDocument == null) return NotFound();
            // try to detect basic content-type (pdf or image) — fallback to octet-stream
            // For demonstration, we'll return application/octet-stream
            return File(row.BillDocument, "application/octet-stream", $"bill_{id}");
        }

        // Delete grouped submission (all rows)
        [HttpDelete("grouped")]
        public async Task<IActionResult> DeleteGrouped(string empid, string site, string project, DateTime submissionDate)
        {
            var rows = _context.ExpenseLogBook.Where(e => e.EmpID == empid && e.SiteName == site && e.ProjectCode == project && e.SubmissionDate == submissionDate);
            _context.ExpenseLogBook.RemoveRange(rows);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        // Reject grouped submission (set Rejection="reject")
        [HttpPost("reject")]
        public async Task<IActionResult> RejectGrouped([FromBody] GroupActionDto dto)
        {
            var rows = _context.ExpenseLogBook.Where(e => e.EmpID == dto.EmpID && e.SiteName == dto.SiteName && e.ProjectCode == dto.ProjectCode && e.SubmissionDate == dto.SubmissionDate);
            await rows.ForEachAsync(e => {
                e.Rejection = "reject";
            });
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        // Unreject / Accept (remove rejection and optionally advance status)
        [HttpPost("unreject")]
        public async Task<IActionResult> Unreject([FromBody] GroupActionDto dto)
        {
            var rows = _context.ExpenseLogBook.Where(e => e.EmpID == dto.EmpID && e.SiteName == dto.SiteName && e.ProjectCode == dto.ProjectCode && e.SubmissionDate == dto.SubmissionDate);
            await rows.ForEachAsync(e => {
                e.Rejection = null;
                // advance status by one if it makes sense
                if (int.TryParse(e.Status, out var s)) e.Status = (s + 1).ToString();
            });
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        // Approve (role-specific) - updates approver id & datetime and status
        // /api/ExpenseAdmin/approve with body { EmpID, SiteName, ProjectCode, SubmissionDate, Role, ApproverID }
        [HttpPost("approve")]
        public async Task<IActionResult> Approve([FromBody] ApproveDto dto)
        {
            var rows = _context.ExpenseLogBook.Where(e => e.EmpID == dto.EmpID && e.SiteName == dto.SiteName && e.ProjectCode == dto.ProjectCode && e.SubmissionDate == dto.SubmissionDate);
            if (!rows.Any()) return NotFound();

            foreach (var r in rows)
            {
                if (dto.Role == "IRB")
                {
                    r.IRBApprovedDate = DateTime.Now;
                    r.IRB = dto.ApproverID; // store approver id in IRB column? if IRB column originally stored manager mapping, adjust as needed
                    r.Status = "1";
                }
                else if (dto.Role == "HR")
                {
                    r.HRApprovel = dto.ApproverID;
                    r.HRApprovelDate = DateTime.Now;
                    r.Status = "2";
                }
                else if (dto.Role == "AGM")
                {
                    // if AGM accepts special approval, they may also override sanctioned amount — handled separately by AcceptSpecial endpoint
                    r.AGMApprovel = dto.ApproverID;
                    r.AGMApprovelDate = DateTime.Now;
                    r.Status = "3";
                }
                else if (dto.Role == "ACCOUNTS")
                {
                    r.AccountApprovel = dto.ApproverID;
                    r.AccountApprovelDate = DateTime.Now;
                    r.Status = "4";
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        // AGM: Accept special approval on grouped submission -> sets each row sanctioned = claim, RequireSpecialApproval = false
        [HttpPost("accept-special")]
        public async Task<IActionResult> AcceptSpecial([FromBody] GroupActionDto dto)
        {
            var rows = _context.ExpenseLogBook.Where(e => e.EmpID == dto.EmpID && e.SiteName == dto.SiteName && e.ProjectCode == dto.ProjectCode && e.SubmissionDate == dto.SubmissionDate);
            await rows.ForEachAsync(e =>
            {
                if (e.RequireSpecialApproval == "true")
                {
                    e.SanctionedAmount = e.ClaimAmount;
                    e.RequireSpecialApproval = "false";
                }
            });
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        // DTOs used by endpoints
        public class GroupActionDto
        {
            public string EmpID { get; set; } = "";
            public string SiteName { get; set; } = "";
            public string ProjectCode { get; set; } = "";
            public DateTime SubmissionDate { get; set; }
        }

        public class ApproveDto : GroupActionDto
        {
            public string Role { get; set; } = "";
            public string ApproverID { get; set; } = "";
        }
    }
}
