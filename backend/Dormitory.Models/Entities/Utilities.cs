using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dormitory.Models.Entities
{
    public class Utilities : BaseEntity
    {
        public int RoomId { get; set; }
        public int ElectricityOld { get; set; }
        public int ElectricityNew { get; set; }
        public int WaterOld { get; set; }
        public int WaterNew { get; set; }
        public decimal ElectricityUnitPrice { get; set; }
        public decimal WaterUnitPrice { get; set; }
        public DateTime BillingMonth { get; set; }

        public Rooms? Room { get; set; }
        public ICollection<Invoices> Invoices { get; set; } = new List<Invoices>();
    }
}
