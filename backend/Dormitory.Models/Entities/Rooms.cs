using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dormitory.Models.Entities
{
    public class Rooms : BaseEntity
    {
        public string RoomNumber { get; set; } = string.Empty;
        public int BuildingId { get; set; }
        public int FloorNumber { get; set; }
        public string RoomType { get; set; } = "Standard";
        public int Capacity { get; set; }
        public int CurrentOccupancy { get; set; }
        public decimal PricePerMonth { get; set; }
        public string Status { get; set; } = "Available";

        public Buildings? Building { get; set; }
        public ICollection<Students> Students { get; set; } = new List<Students>();
        public ICollection<Registrations> Registrations { get; set; } = new List<Registrations>();
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
        public ICollection<Utilities> Utilities { get; set; } = new List<Utilities>();
        public ICollection<Invoices> Invoices { get; set; } = new List<Invoices>();
        public ICollection<RoomFeeProfile> FeeProfiles { get; set; } = new List<RoomFeeProfile>();
        public ICollection<RoomFinanceRecord> FinanceRecords { get; set; } = new List<RoomFinanceRecord>();
    }
}
