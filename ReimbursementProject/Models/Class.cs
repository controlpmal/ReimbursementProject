public class DashboardGroupDto
{
    public string EmpID { get; set; } = "";
    public string EmpName { get; set; } = "";
    public string? IRB { get; set; }
    public string? IRBName { get; set; }
    public string? SiteName { get; set; }
    public string? ProjectCode { get; set; }
    public DateTime? SubmissionDate { get; set; }
    public double TotalClaimAmount { get; set; }
    public double TotalSanctionedAmount { get; set; }
    public string? Status { get; set; }
    public string? Rejection { get; set; }
    public long? ExpenseId { get; set; }
}

public class ExpenseItemDto
{
    public long ID { get; set; }
    public DateTime? DateOfExpense { get; set; }
    public string? TypeOfExpense { get; set; }
    public string? TravelLocation { get; set; }
    public int? Quantity { get; set; }
    public string? FellowMembers { get; set; }
    public double? ClaimAmount { get; set; }
    public double? SanctionedAmount { get; set; }
    public bool HasBillDocument => BillDocument != null && BillDocument.Length > 0;
    public byte[]? BillDocument { get; set; }
    public string? RequireSpecialApproval { get; set; }
    public string? Status { get; set; }
    public string? Rejection { get; set; }
}


public class GroupSubmitDto
{
    public string EmpID { get; set; } = string.Empty;
    public string SubmissionDate { get; set; } = string.Empty;  // "yyyy-MM-dd"
    public string ProjectCode { get; set; } = string.Empty;
    public List<GroupItemUpdateDto> Updates { get; set; } = new();
}

public class GroupItemUpdateDto
{
    public long ItemId { get; set; }                 // ExpenseLogBook.ID
    public double NewSanctionedAmount { get; set; }  // updated sanctioned amount
    public string Rejection { get; set; } = "";      // "rejected" or empty
    public bool IncrementStatus { get; set; }        // true if status should increment
    public string ActionBy { get; set; } = string.Empty;
}

    public class EmployeeAccountSummary
    {
        public string? EMP_ID { get; set; }
        public string? EMP_NAME { get; set; }
        public double TotalAdvance { get; set; }
        public DateTime? LastTransaction { get; set; }
    }
public class LoginDto
{
    public string EmpId { get; set; }
    public string Password { get; set; }
}
