export function PermissionSummaryBar({ addedCount, removedCount, saving, savePermissions, onReset }) {
  if (addedCount === 0 && removedCount === 0) return null

  return (
    <div className="permission-summary-bar">
      <span>
        Chưa lưu:
        {addedCount > 0 && <strong>{addedCount} quyền cấp thêm</strong>}
        {addedCount > 0 && removedCount > 0 && <span>, </span>}
        {removedCount > 0 && <strong>{removedCount} quyền bị chặn</strong>}
      </span>
      <button className="secondary-button" onClick={onReset} disabled={saving}>Hủy thay đổi</button>
      <button className="primary-button" onClick={savePermissions} disabled={saving}>
        {saving ? 'Đang lưu...' : 'Lưu lại thay đổi'}
      </button>
    </div>
  )
}
