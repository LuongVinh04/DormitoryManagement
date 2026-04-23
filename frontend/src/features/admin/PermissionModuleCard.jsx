import { useEffect, useRef } from 'react'

export function PermissionModuleCard({
  moduleName,
  meta,
  stats,
  isCollapsed,
  onToggleCollapse,
  onToggleAll,
  children
}) {
  const checkboxRef = useRef(null)

  useEffect(() => {
    if (checkboxRef.current) {
      if (stats.allChecked) {
        checkboxRef.current.checked = true
        checkboxRef.current.indeterminate = false
      } else if (stats.partiallyChecked) {
        checkboxRef.current.checked = false
        checkboxRef.current.indeterminate = true
      } else {
        checkboxRef.current.checked = false
        checkboxRef.current.indeterminate = false
      }
    }
  }, [stats.allChecked, stats.partiallyChecked])

  return (
    <div className={`permission-module-card ${isCollapsed ? 'collapsed' : ''}`}>
      <div className="permission-module-header">
        <div className="permission-module-meta">
          <label className="ui-switch" style={{ marginTop: '2px' }}>
            <input
              type="checkbox"
              ref={checkboxRef}
              onChange={(e) => onToggleAll(e.target.checked)}
              title="Chọn toàn bộ hoặc bỏ chọn toàn bộ quyền nhóm này"
            />
            <span className="slider"></span>
          </label>
          <div className="permission-module-meta-text">
            <h4>{meta?.label || moduleName}</h4>
            {meta?.description && <p>{meta.description}</p>}
          </div>
        </div>
        <div className="permission-module-actions">
          <span className="count">{stats.enabled}/{stats.total} quyền đang bật</span>
          <button className="module-toggle-btn" onClick={onToggleCollapse} title={isCollapsed ? 'Mở rộng' : 'Thu gọn'}>
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              {isCollapsed ? <polyline points="6 9 12 15 18 9"></polyline> : <polyline points="18 15 12 9 6 15"></polyline>}
            </svg>
          </button>
        </div>
      </div>
      <div className="permission-module-body">
        {children}
      </div>
    </div>
  )
}
