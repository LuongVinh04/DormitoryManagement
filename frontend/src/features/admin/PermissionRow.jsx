export function PermissionRow({ permission, state, onToggle }) {
  return (
    <label className="permission-row">
      <div className="ui-switch" style={{ marginTop: '2px' }}>
        <input
          type="checkbox"
          checked={state.isChecked}
          onChange={() => onToggle(permission.id)}
        />
        <span className="slider"></span>
      </div>
      <div className="permission-row-main">
        <strong>{permission.name}</strong>
        <span>{permission.id}</span>
      </div>
      <div className="permission-row-meta">
        {state.source === 'grantedOverride' && <span className="permission-badge granted">+ Cấp thêm</span>}
        {state.source === 'deniedOverride' && <span className="permission-badge denied">- Đã chặn</span>}
        {state.source === 'role' && <span className="permission-badge role">Theo vai trò</span>}
      </div>
    </label>
  )
}
