using System;
using System.ComponentModel.DataAnnotations;

namespace ReimbursementProject.Models
{
    public class ExpenseLimitDetails
    {
        [Key]
        public long ID { get; set; }  // bigint → long

        [StringLength(10)]
        public string? Level { get; set; }  // varchar(10)

        [StringLength(100)]
        public string? TypeOfExpense { get; set; }  // varchar(100)

        public double? MaxLimitWithBill { get; set; }  // float → double

        public double? MaxLimitWOBill { get; set; }  // float → double
    }
}
