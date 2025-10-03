using System;
using System.ComponentModel.DataAnnotations;

namespace ReimbursementProject.Models
{
    public class EmployeeAccount
    {
        [Key]
        public int? ID { get; set; }   // int, nullable

        [StringLength(20)]
        public string? EMP_ID { get; set; }   // varchar(20), nullable

        [StringLength(100)]
        public string? EMP_NAME { get; set; }  // varchar(100), nullable

        public double? ADVANCE_AMOUNT { get; set; }  // float, nullable

        public DateTime? DATETIME { get; set; }  // datetime, nullable

        [StringLength(50)]
        public string? PAYMENT_TYPE { get; set; }  // varchar(50), nullable

        [StringLength(100)]
        public string? APPROVAL_EMP { get; set; }  // varchar(100), nullable
    }
}
