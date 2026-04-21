using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dormitory.Models.Entities
{
    public class Registrations : BaseEntity
    {
        public int StudentId { get; set; }
        public int RoomId { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string Status { get; set; }
    }
}
