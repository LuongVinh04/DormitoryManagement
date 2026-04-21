using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dormitory.Models.Entities
{
    public class Invoices : BaseEntity
    {
        public string InvoiceCode { get; set; } = string.Empty;
        public int StudentId { get; set; }
        public int RoomId { get; set; }
        public int? UtilityId { get; set; }
        public decimal RoomFee { get; set; }
        public decimal ElectricityFee { get; set; }
        public decimal WaterFee { get; set; }
        public decimal ServiceFee { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; } = "Unpaid";
        public DateTime BillingMonth { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaidDate { get; set; }

        public Students? Student { get; set; }
        public Rooms? Room { get; set; }
        public Utilities? Utility { get; set; }
    }
}
