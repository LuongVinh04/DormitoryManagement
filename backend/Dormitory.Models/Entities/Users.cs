using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dormitory.Models.Entities
{
    public class Users : BaseEntity
    {
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public int? StudentId { get; set; }
        public bool IsActive { get; set; } = true;

        public Roles? Role { get; set; }
        public Students? Student { get; set; }
        public ICollection<UserPermissions> UserPermissions { get; set; } = new List<UserPermissions>();
    }
}
