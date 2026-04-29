import { useMemo, useState } from 'react'
import { ENTITY_CONFIGS } from './constants'
import { renderCell } from './helpers'

export function Panel({
  panelId,
  title,
  description,
  children,
  collapsed = false,
  expanded = false,
  onToggleCollapse,
  onToggleExpand,
}) {
  const panelClassName = [
    'panel-card',
    collapsed ? 'panel-collapsed' : '',
    expanded ? 'panel-expanded' : '',
  ].filter(Boolean).join(' ')

  return (
    <section id={panelId} className={panelClassName}>
      <div className="panel-header">
        <div>
          <h2>{title}</h2>
          <p>{description}</p>
        </div>
        {(onToggleCollapse || onToggleExpand) ? (
          <div className="panel-header-actions">
            {onToggleCollapse ? (
              <button type="button" className={collapsed ? 'panel-control active' : 'panel-control'} onClick={onToggleCollapse}>
                {collapsed ? 'Mở nội dung' : 'Thu gọn'}
              </button>
            ) : null}
            {onToggleExpand ? (
              <button type="button" className={expanded ? 'panel-control active' : 'panel-control'} onClick={onToggleExpand}>
                {expanded ? 'Kích thước chuẩn' : 'Phóng to'}
              </button>
            ) : null}
          </div>
        ) : null}
      </div>
      {collapsed ? (
        <div className="panel-collapsed-note">Khối chức năng đang được thu gọn để tối ưu không gian làm việc.</div>
      ) : (
        <div className="panel-body">{children}</div>
      )}
    </section>
  )
}

export function MetricCard({ label, value, accent, large = false }) {
  return (
    <article className={`metric-tile ${accent} ${large ? 'large' : ''}`}>
      <span>{label}</span>
      <strong>{value}</strong>
    </article>
  )
}

export function SectionBanner({ eyebrow, title, description, stats = [], actions = null }) {
  return (
    <section className="section-banner">
      <div className="section-banner-copy">
        <span className="eyebrow">{eyebrow}</span>
        <h2>{title}</h2>
        <p>{description}</p>
      </div>
      <div className="section-banner-side">
        {stats.length > 0 ? (
          <div className="section-banner-stats">
            {stats.map((stat) => (
              <article key={stat.label} className="section-stat-card">
                <span>{stat.label}</span>
                <strong>{stat.value}</strong>
              </article>
            ))}
          </div>
        ) : null}
        {actions ? <div className="section-banner-actions">{actions}</div> : null}
      </div>
    </section>
  )
}

export function SummaryBlock({ label, value, compact = false }) {
  return (
    <div className={`summary-block ${compact ? 'compact' : ''}`}>
      <span>{label}</span>
      <strong>{value}</strong>
    </div>
  )
}

export function ActionCard({ title, description, actionLabel, disabled, onAction, children }) {
  return (
    <article className="action-card">
      <div>
        <strong>{title}</strong>
        <p>{description}</p>
      </div>
      <div className="action-card-body">{children}</div>
      <button className="primary-button" disabled={disabled} onClick={onAction}>
        {actionLabel}
      </button>
    </article>
  )
}

export function ModalCard({ title, subtitle, children, footer, onClose }) {
  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div
        className="modal-card"
        onClick={(event) => event.stopPropagation()}
        onMouseDown={focusNearestFieldControl}
      >
        <div className="panel-header">
          <div>
            <h2>{title}</h2>
            <p>{subtitle}</p>
          </div>
        </div>
        <div className="modal-body">{children}</div>
        <div className="modal-footer">{footer}</div>
      </div>
    </div>
  )
}

function focusNearestFieldControl(event) {
  if (event.target.closest('input, select, textarea, button, a')) {
    return
  }

  const field = event.target.closest('.field')
  const control = field?.querySelector('input:not([type="hidden"]):not(:disabled), select:not(:disabled), textarea:not(:disabled)')
  if (control) {
    control.focus()
  }
}

export function CrudPanel({ entityKey, rows, onCreate, onEdit, onDelete, extraRowActions = () => [], panelProps }) {
  const [query, setQuery] = useState('')
  const config = ENTITY_CONFIGS[entityKey]

  const filteredRows = useMemo(() => {
    const keyword = query.trim().toLowerCase()
    if (!keyword) return rows

    return rows.filter((row) =>
      Object.values(row).some((value) => String(value ?? '').toLowerCase().includes(keyword)),
    )
  }, [query, rows])

  return (
    <Panel title={config.title} description={config.description} {...panelProps}>
      <div className="panel-toolbar">
        <div className="search-stack">
          <input
            className="search-input"
            value={query}
            onChange={(event) => setQuery(event.target.value)}
            placeholder="Tìm nhanh theo từ khóa..."
          />
          <span className="toolbar-meta">
            {query.trim()
              ? `Hiển thị ${filteredRows.length}/${rows.length} bản ghi`
              : `${rows.length} bản ghi`}
          </span>
        </div>
        <button className="primary-button" onClick={() => onCreate(entityKey)}>Thêm mới</button>
      </div>

      <div className="table-wrap">
        <table className="data-table">
          <thead>
            <tr>
              {config.columns.map(([key, label]) => <th key={key}>{label}</th>)}
              <th>Thao tác</th>
            </tr>
          </thead>
          <tbody>
            {filteredRows.length === 0 ? (
              <tr>
                <td colSpan={config.columns.length + 1}>
                  <div className="empty-state">
                    {query.trim()
                      ? 'Không tìm thấy bản ghi phù hợp với từ khóa hiện tại.'
                      : 'Chưa có dữ liệu trong nhóm chức năng này.'}
                  </div>
                </td>
              </tr>
            ) : filteredRows.map((item) => {
              const actions = extraRowActions(item).filter(Boolean)

              return (
                <tr key={item.id}>
                  {config.columns.map(([key, label]) => (
                    <td key={key} data-label={label}>
                      {renderCell(item[key])}
                    </td>
                  ))}
                  <td data-label="Thao tác">
                    <div className="action-row">
                      {actions.map((action) => (
                        <button
                          key={`${item.id}-${action.label}`}
                          className={action.kind === 'danger' ? 'ghost-button danger' : 'ghost-button approve'}
                          onClick={action.onClick}
                        >
                          {action.label}
                        </button>
                      ))}
                      <button className="ghost-button" onClick={() => onEdit(entityKey, item)}>Sửa</button>
                      <button className="ghost-button danger" onClick={() => onDelete(entityKey, item)}>Xóa</button>
                    </div>
                  </td>
                </tr>
              )
            })}
          </tbody>
        </table>
      </div>
    </Panel>
  )
}
