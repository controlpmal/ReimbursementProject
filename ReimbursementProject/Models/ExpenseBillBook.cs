using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ReimbursementProject.Models
{
    [Table("ExpenseBillBook")]
    public class ExpenseBillBook
    {
        [Key]
        public long ExpenseID { get; set; }

        [MaxLength(100)] // adjust based on actual column length in DB
        public string? ExpenseBillNumber { get; set; }

        public DateTime? SubmissionDate { get; set; }

        [MaxLength(50)] // adjust based on actual column length in DB
        public string? Status { get; set; }

        // Navigation property for related ExpenseLogBook rows
        public ICollection<ExpenseLogBook> ExpenseLogBooks { get; set; } = new List<ExpenseLogBook>();
    }
}
