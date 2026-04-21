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
        public int Electricity { get; set; }
        public int Water { get; set; }
        public DateTime BillingDate { get; set; }
    }
}
