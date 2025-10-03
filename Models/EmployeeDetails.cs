using System.ComponentModel.DataAnnotations;

namespace ReimbursementProject.Models
{
    public class EmployeeDetails
    {
        [Key]
        [StringLength(20)]
        public string? EmpID { get; set; }  // varchar(20) → Primary Key

        [StringLength(100)]
        public string? EmpName { get; set; }  // varchar(100)

        [StringLength(100)]
        public string? Designation { get; set; }  // varchar(100)

        [StringLength(50)]
        public string? Level { get; set; }  // varchar(50)

        [StringLength(20)]
        public string? IRB { get; set; }  // varchar(20)

        [StringLength(100)]
        public string? Dept { get; set; }  // varchar(100)

        public double? AdvanceAmount { get; set; }  // float

        [StringLength(20)]
        public string? Password { get; set; }  // varchar(200) (assuming password stored as hash)
        public string? Status { get; set; }
    }
}
