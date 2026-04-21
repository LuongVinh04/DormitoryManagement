using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dormitory.Models.Entities
{
    public class Rooms : BaseEntity
    { 
        public string RoomNumber { get; set; }
        public int BuildingId { get; set; }
        public int Capacity { get; set; }
        public int CurrentOccupancy { get; set; }
        public string Status { get; set; }
    }
}
