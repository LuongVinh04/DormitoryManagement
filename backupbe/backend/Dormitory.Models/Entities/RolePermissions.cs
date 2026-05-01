using System;

namespace Dormitory.Models.Entities
{
    public class RolePermissions : BaseEntity
    {
        public int RoleId { get; set; }
        public int PermissionId { get; set; }

        public Roles? Role { get; set; }
        public Permissions? Permission { get; set; }
    }
}
