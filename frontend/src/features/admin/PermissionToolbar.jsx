export function PermissionToolbar({
  users,
  selectedUserId,
  setSelectedUserId,
  permissionQuery,
  setPermissionQuery,
  filterMode,
  setFilterMode,
}) {
  const selectedUser = users.find(u => String(u.id) === String(selectedUserId))

  return (
    <div className="permission-toolbar">
      <div className="permission-user-select-group">
        <select value={selectedUserId} onChange={(e) => setSelectedUserId(e.target.value)}>
          <option value="">-- Chọn tài khoản cần phân quyền --</option>
          {users.filter(u => u.username !== 'admin').map((u) => (
            <option key={u.id} value={u.id}>
              {u.username} ({u.fullName})
            </option>
          ))}
        </select>
        {selectedUser && (
          <div className="permission-user-card">
            Đang cấu hình: <strong>{selectedUser.fullName || selectedUser.username}</strong> — Vai trò gốc: <strong>{selectedUser.roleName}</strong>
          </div>
        )}
      </div>

      {selectedUserId && (
        <div className="permission-filters">
          <input
            className="search-input"
            placeholder="Tìm quyền theo tên hoặc mã..."
            value={permissionQuery}
            onChange={(e) => setPermissionQuery(e.target.value)}
          />
          <select value={filterMode} onChange={(e) => setFilterMode(e.target.value)}>
            <option value="all">Tất cả trạng thái</option>
            <option value="enabled">Chỉ quyền đang bật</option>
            <option value="granted">Chỉ quyền được cấp thêm</option>
            <option value="denied">Chỉ quyền bị chặn</option>
          </select>
        </div>
      )}
    </div>
  )
}
