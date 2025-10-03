namespace locationget.Models
{
    public class BillEntry
    {
        
        public DateTime Date { get; set; }
        public string TypeOfExpense { get; set; }
        public string TravelMode { get; set; } // Car, Cab, etc.
        public int? KmDriven { get; set; }
        public string Purpose { get; set; }
        public string BillType { get; set; } // Bill / Without Bill
        public decimal ActualBill { get; set; }
        public decimal ClaimAmount { get; set; }
        public decimal SectionedAmount { get; set; } = 200; // default
    }
}
