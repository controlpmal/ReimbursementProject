using System.ComponentModel.DataAnnotations;

namespace ReimbursementProject.Models
{
    public class TypeOfExpenses
    {
        [Key]
        public int? ID { get; set; }  // int (nullable in SQL)

        [StringLength(50)]
        public string? TypeOfExpense { get; set; }  // varchar(50), Nullable
    }
}
