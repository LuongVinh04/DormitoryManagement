using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dormitory.Models.Entities
{
    public class Invoices : BaseEntity
    {     
        public int StudentId { get; set; }
        public int RoomId { get; set; }
        public int RoomFee { get; set; }
        public int ElectricityFee { get; set; }
        public int WaterFee { get; set; }
        public int Total { get; set; }
        public string Status { get; set; }
        public DateTime BillingDate { get; set; }
    }
}
