using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dormitory.Models.Entities
{
    public class Users : BaseEntity
    {
        public string Username { get; set; }    
        public string PasswordHash { get; set; }
        public int RoleId { get; set; }
    }
}
