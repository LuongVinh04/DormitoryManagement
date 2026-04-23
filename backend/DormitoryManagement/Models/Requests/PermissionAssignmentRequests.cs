using System.Collections.Generic;

namespace DormitoryManagement.Models
{
    public class UpdateRolePermissionsRequest
    {
        public List<int> PermissionIds { get; set; } = new List<int>();
    }

    public class UpdateUserPermissionsRequest
    {
        public List<int> AllowedPermissionIds { get; set; } = new List<int>();
        public List<int> DeniedPermissionIds { get; set; } = new List<int>();
    }

    public class UpdateUserRoleRequest
    {
        public int RoleId { get; set; }
    }
}
