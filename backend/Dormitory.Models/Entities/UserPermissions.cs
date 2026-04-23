using System;

namespace Dormitory.Models.Entities
{
    public class UserPermissions : BaseEntity
    {
        public int UserId { get; set; }
        public int PermissionId { get; set; }
        public bool IsGranted { get; set; } = true;

        public Users? User { get; set; }
        public Permissions? Permission { get; set; }
    }
}
