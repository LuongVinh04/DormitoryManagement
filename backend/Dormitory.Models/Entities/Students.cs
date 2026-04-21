using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dormitory.Models.Entities
{
    public class Students : BaseEntity
    {
        public string StudentCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Faculty { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string EmergencyContact { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public int? RoomId { get; set; }

        public Rooms? Room { get; set; }
        public ICollection<Registrations> Registrations { get; set; } = new List<Registrations>();
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
        public ICollection<Invoices> Invoices { get; set; } = new List<Invoices>();
    }
}
