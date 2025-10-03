using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReimbursementProject.Models
{
    public class ExpenseLogBook
    {
        [Key]
        public long ID { get; set; }   // bigint

        [StringLength(100)]
        public string? TypeOfExpense { get; set; }  // varchar(100)
        [StringLength(500)]
        public string? TravelLocation { get; set; }

        public int? Quantity { get; set; }  // int

        [StringLength(250)]
        public string? FellowMembers { get; set; }  // varchar(250)

        public double? ClaimAmount { get; set; }  // float

        public byte[]? BillDocument { get; set; }  // varbinary(MAX)

        [StringLength(255)]
        public string? BillFileName { get; set; }

        [StringLength(100)]
        public string? BillContentType { get; set; }

        public double? SanctionedAmount { get; set; }  // float
    

        [StringLength(20)]
        public string? EmpID { get; set; }  // varchar(20)

        [StringLength(100)]
        public string? SiteName { get; set; }  // varchar(100)

        [StringLength(50)]
        public string? ProjectCode { get; set; }  // varchar(50)

        public DateTime? SubmissionDate { get; set; }  // date

        public DateTime? DateofExpense { get; set; }  // datetime

        [StringLength(20)]
        public string? IRB { get; set; }  // varchar(20)

        public DateTime? IRBApprovedDate { get; set; }  // datetime

        [StringLength(20)]
        public string? HRApprovel { get; set; }  // varchar(20)

        public DateTime? HRApprovelDate { get; set; }  // datetime

        [StringLength(20)]
        public string? AGMApprovel { get; set; }  // varchar(20)

        public DateTime? AGMApprovelDate { get; set; }  // datetime

        [StringLength(20)]
        public string? AccountApprovel { get; set; }  // varchar(20)

        public DateTime? AccountApprovelDate { get; set; }  // datetime

        [StringLength(10)]
        public string? Status { get; set; }  // varchar(10)

        [StringLength(10)]
        public string? RequireSpecialApproval { get; set; }  // varchar(10)

        [StringLength(10)]
        public string? Rejection { get; set; }  // varchar(10)
        [ForeignKey("ExpenseBillBook")]
        public long ExpenseID { get; set; }

        public ExpenseBillBook ExpenseBillBook { get; set; }

    }
}
