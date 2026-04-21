using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dormitory.Models.Entities
{
    public class Buildings : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string GenderPolicy { get; set; } = "Mixed";
        public int NumberOfFloors { get; set; }
        public string ManagerName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public ICollection<Rooms> Rooms { get; set; } = new List<Rooms>();
    }
}
