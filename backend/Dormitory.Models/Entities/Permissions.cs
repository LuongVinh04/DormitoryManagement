using System;
using System.Collections.Generic;

namespace Dormitory.Models.Entities
{
    public class Permissions : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;

        public ICollection<RolePermissions> RolePermissions { get; set; } = new List<RolePermissions>();
        public ICollection<UserPermissions> UserPermissions { get; set; } = new List<UserPermissions>();
    }
}
