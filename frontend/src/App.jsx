import { useEffect, useMemo, useState } from 'react'
import {
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  Line,
  LineChart,
  Pie,
  PieChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts'
import './App.css'
import { ActionCard, CrudPanel, MetricCard, ModalCard, Panel, SummaryBlock } from './components'
import { API_ENDPOINTS, ENTITY_CONFIGS, NAVIGATION } from './constants'
import { currencyFormat, formatDateInput, numberFormat, readError, normalizePayload, localizeValue } from './helpers'

const PANEL_LAYOUT_STORAGE_KEY = 'dormitory-hub-panel-layouts'

function App() {
  const [section, setSection] = useState('overview')
  const [dashboard, setDashboard] = useState(null)
  const [financeSummary, setFinanceSummary] = useState(null)
  const [lookups, setLookups] = useState({ buildings: [], rooms: [], students: [], roles: [], utilities: [] })
  const [data, setData] = useState({
    buildings: [],
    rooms: [],
    students: [],
    registrations: [],
    contracts: [],
    utilities: [],
    roomFeeProfiles: [],
    roomFinances: [],
    invoices: [],
    roles: [],
    users: [],
  })
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [notice, setNotice] = useState('')
  const [modal, setModal] = useState(null)
  const [selectedRoomId, setSelectedRoomId] = useState('')
  const [roomOverview, setRoomOverview] = useState(null)
  const [roomActions, setRoomActions] = useState({
    assignStudentId: '',
    transferStudentId: '',
    transferToRoomId: '',
    removeStudentId: '',
  })
  const [panelLayouts, setPanelLayouts] = useState(() => {
    if (typeof window === 'undefined') return {}

    try {
      const storedLayouts = window.localStorage.getItem(PANEL_LAYOUT_STORAGE_KEY)
      return storedLayouts ? JSON.parse(storedLayouts) : {}
    } catch {
      return {}
    }
  })

  useEffect(() => {
    loadAll()
  }, [])

  useEffect(() => {
    if (!selectedRoomId && data.rooms.length > 0) {
      setSelectedRoomId(String(data.rooms[0].id))
    }
  }, [data.rooms, selectedRoomId])

  useEffect(() => {
    if (selectedRoomId) {
      loadRoomOverview(selectedRoomId)
    }
  }, [selectedRoomId])

  useEffect(() => {
    if (typeof window === 'undefined') return
    window.localStorage.setItem(PANEL_LAYOUT_STORAGE_KEY, JSON.stringify(panelLayouts))
  }, [panelLayouts])

  const waitingStudents = useMemo(
    () => data.students.filter((student) => !student.roomId || ['Waiting', 'PendingMoveIn', 'Pending'].includes(student.status)),
    [data.students],
  )

  function togglePanelCollapse(panelKey) {
    setPanelLayouts((current) => {
      const panelLayout = current[panelKey] ?? { collapsed: false, expanded: false }
      const nextCollapsed = !panelLayout.collapsed

      return {
        ...current,
        [panelKey]: {
          collapsed: nextCollapsed,
          expanded: nextCollapsed ? false : panelLayout.expanded,
        },
      }
    })
  }

  function togglePanelExpand(panelKey) {
    setPanelLayouts((current) => {
      const panelLayout = current[panelKey] ?? { collapsed: false, expanded: false }

      return {
        ...current,
        [panelKey]: {
          collapsed: false,
          expanded: !panelLayout.expanded,
        },
      }
    })
  }

  function getPanelProps(panelKey) {
    const panelLayout = panelLayouts[panelKey] ?? { collapsed: false, expanded: false }

    return {
      collapsed: panelLayout.collapsed,
      expanded: panelLayout.expanded,
      onToggleCollapse: () => togglePanelCollapse(panelKey),
      onToggleExpand: () => togglePanelExpand(panelKey),
    }
  }

  async function loadAll() {
    try {
      setLoading(true)
      setError('')

      const [dashboardRes, financeSummaryRes, lookupRes, ...entityResponses] = await Promise.all([
        fetch('/api/dashboard'),
        fetch('/api/operations/room-finances/summary'),
        fetch('/api/lookups'),
        ...Object.values(API_ENDPOINTS).map((url) => fetch(url)),
      ])

      if (!dashboardRes.ok || !financeSummaryRes.ok || !lookupRes.ok || entityResponses.some((res) => !res.ok)) {
        throw new Error('Không thể tải dữ liệu hệ thống.')
      }

      const [dashboardJson, financeSummaryJson, lookupJson, ...entityJson] = await Promise.all([
        dashboardRes.json(),
        financeSummaryRes.json(),
        lookupRes.json(),
        ...entityResponses.map((res) => res.json()),
      ])

      const nextData = {}
      Object.keys(API_ENDPOINTS).forEach((key, index) => {
        nextData[key] = entityJson[index]
      })

      setDashboard(dashboardJson)
      setFinanceSummary(financeSummaryJson)
      setLookups(lookupJson)
      setData(nextData)
    } catch (loadError) {
      setError(loadError.message)
    } finally {
      setLoading(false)
    }
  }

  async function loadRoomOverview(roomId) {
    try {
      const response = await fetch(`/api/facilities/rooms/${roomId}/overview`)
      if (!response.ok) {
        throw new Error('Không thể tải chi tiết phòng.')
      }

      setRoomOverview(await response.json())
    } catch (roomError) {
      setError(roomError.message)
    }
  }

  async function refreshData() {
    await loadAll()
    if (selectedRoomId) {
      await loadRoomOverview(selectedRoomId)
    }
  }

  function openCreate(entityKey) {
    const defaults = {}
    ENTITY_CONFIGS[entityKey].fields.forEach((field) => {
      if (field.type === 'checkbox') defaults[field.name] = true
      else if (field.type === 'select') defaults[field.name] = field.options[0]?.value ?? ''
      else defaults[field.name] = ''
    })

    setModal({ entityKey, mode: 'create', id: null, values: defaults })
  }

  function openEdit(entityKey, item) {
    const values = {}
    ENTITY_CONFIGS[entityKey].fields.forEach((field) => {
      const rawValue = item[field.name]
      values[field.name] = field.type === 'date' ? (rawValue ? formatDateInput(rawValue) : '') : (rawValue ?? (field.type === 'checkbox' ? false : ''))
    })

    setModal({ entityKey, mode: 'edit', id: item.id, values })
  }

  function updateModalField(name, value) {
    setModal((current) => ({ ...current, values: { ...current.values, [name]: value } }))
  }

  async function saveEntity() {
    if (!modal) return

    const { entityKey, mode, id, values } = modal
    const endpoint = mode === 'create' ? API_ENDPOINTS[entityKey] : `${API_ENDPOINTS[entityKey]}/${id}`

    try {
      setSaving(true)
      setError('')

      const response = await fetch(endpoint, {
        method: mode === 'create' ? 'POST' : 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(normalizePayload(ENTITY_CONFIGS[entityKey].fields, values)),
      })

      if (!response.ok) {
        throw new Error(await readError(response))
      }

      setNotice(mode === 'create' ? 'Đã thêm dữ liệu mới.' : 'Đã cập nhật dữ liệu.')
      setModal(null)
      await refreshData()
    } catch (saveError) {
      setError(saveError.message)
    } finally {
      setSaving(false)
    }
  }

  async function deleteEntity(entityKey, item) {
    const label = item.name ?? item.roomNumber ?? item.studentCode ?? item.invoiceCode ?? item.username ?? 'bản ghi'
    if (!window.confirm(`Xóa ${label}?`)) return

    try {
      const response = await fetch(`${API_ENDPOINTS[entityKey]}/${item.id}`, { method: 'DELETE' })
      if (!response.ok) {
        throw new Error(await readError(response))
      }

      setNotice('Đã xóa bản ghi.')
      await refreshData()
    } catch (deleteError) {
      setError(deleteError.message)
    }
  }

  async function executeAction(factory, message) {
    try {
      setSaving(true)
      setError('')
      const response = await factory()
      if (!response.ok) {
        throw new Error(await readError(response))
      }

      setNotice(message)
      await refreshData()
    } catch (actionError) {
      setError(actionError.message)
    } finally {
      setSaving(false)
    }
  }

  if (loading || !dashboard) {
    return (
      <div className="loading-screen">
        <div className="loading-card">
          <span className="badge">Dormitory Hub</span>
          <h1>Đang tải trung tâm vận hành ký túc xá</h1>
          <p>Hệ thống đang tổng hợp dữ liệu vận hành, lưu trú và tài chính.</p>
        </div>
      </div>
    )
  }

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand-panel">
          <span className="brand-kicker">Dormitory Hub</span>
          <h2>Điều hành lưu trú thông minh</h2>
          <p>Một bảng điều khiển đủ dùng cho quản lý cơ sở vật chất, vận hành sinh viên và tài chính nội trú.</p>
        </div>

        <div className="sidebar-summary">
          <SummaryBlock label="Phòng đang ở" value={numberFormat.format(dashboard.summary.occupiedRooms)} />
          <SummaryBlock label="Giường còn trống" value={numberFormat.format(dashboard.summary.availableBeds)} />
          <SummaryBlock label="Chờ duyệt" value={numberFormat.format(dashboard.summary.waitingStudents)} />
        </div>

        <nav className="nav-grid">
          {NAVIGATION.map((item) => (
            <button
              key={item.key}
              className={section === item.key ? 'nav-link active' : 'nav-link'}
              onClick={() => setSection(item.key)}
            >
              {item.label}
            </button>
          ))}
        </nav>
      </aside>

      <main className="main-area">
        <header className="page-header">
          <div>
            <p className="eyebrow">Hệ thống quản trị ký túc xá</p>
            <h1>Trang quản trị vận hành thực tế cho khu nội trú sinh viên</h1>
            <p className="page-description">
              Theo dõi công suất phòng, xử lý hồ sơ, điều phối chỗ ở và kiểm soát dòng tiền trên một giao diện đẹp, dễ dùng và responsive.
            </p>
          </div>
          <div className="header-actions">
            <button className="primary-button" onClick={refreshData}>Làm mới toàn hệ thống</button>
            <button className="secondary-button" onClick={() => openCreate('students')}>Tiếp nhận sinh viên mới</button>
          </div>
        </header>

        {error ? <div className="feedback error">{error}</div> : null}
        {notice ? <div className="feedback success">{notice}</div> : null}

        <section className="top-metrics">
          <MetricCard label="Tổng sinh viên" value={numberFormat.format(dashboard.summary.totalStudents)} accent="blue" />
          <MetricCard label="Hợp đồng hiệu lực" value={numberFormat.format(dashboard.summary.activeContracts)} accent="emerald" />
          <MetricCard label="Hóa đơn chưa thu" value={numberFormat.format(dashboard.summary.unpaidInvoices)} accent="amber" />
          <MetricCard label="Quá hạn thanh toán" value={numberFormat.format(dashboard.summary.overdueInvoices)} accent="rose" />
          <MetricCard label="Doanh thu tháng" value={currencyFormat.format(dashboard.summary.revenueThisMonth)} accent="violet" large />
        </section>

        {section === 'overview' && <OverviewSection dashboard={dashboard} getPanelProps={getPanelProps} />}

        {section === 'operations' && (
          <OperationsSection
            data={data}
            roomOverview={roomOverview}
            selectedRoomId={selectedRoomId}
            setSelectedRoomId={setSelectedRoomId}
            roomActions={roomActions}
            setRoomActions={setRoomActions}
            waitingStudents={waitingStudents}
            executeAction={executeAction}
            openCreate={openCreate}
            openEdit={openEdit}
            deleteEntity={deleteEntity}
            saving={saving}
            getPanelProps={getPanelProps}
          />
        )}

        {section === 'facilities' && (
          <>
            <CrudPanel entityKey="buildings" rows={data.buildings} onCreate={openCreate} onEdit={openEdit} onDelete={deleteEntity} panelProps={getPanelProps('facilities-buildings')} />
            <CrudPanel entityKey="rooms" rows={data.rooms} onCreate={openCreate} onEdit={openEdit} onDelete={deleteEntity} panelProps={getPanelProps('facilities-rooms')} />
          </>
        )}

        {section === 'students' && (
          <>
            <CrudPanel entityKey="students" rows={data.students} onCreate={openCreate} onEdit={openEdit} onDelete={deleteEntity} panelProps={getPanelProps('students-students')} />
            <CrudPanel entityKey="contracts" rows={data.contracts} onCreate={openCreate} onEdit={openEdit} onDelete={deleteEntity} panelProps={getPanelProps('students-contracts')} />
          </>
        )}

        {section === 'finance' && (
          <>
            <CrudPanel
              entityKey="utilities"
              rows={data.utilities}
              onCreate={openCreate}
              onEdit={openEdit}
              onDelete={deleteEntity}
              panelProps={getPanelProps('finance-utilities')}
              extraRowActions={(item) => [
                {
                  label: 'Tạo hóa đơn',
                  kind: 'approve',
                  onClick: () => executeAction(() => fetch(`/api/operations/utilities/${item.id}/generate-invoices`, { method: 'POST' }), 'Đã tạo hóa đơn từ kỳ điện nước.'),
                },
              ]}
            />
            <CrudPanel
              entityKey="invoices"
              rows={data.invoices}
              onCreate={openCreate}
              onEdit={openEdit}
              onDelete={deleteEntity}
              panelProps={getPanelProps('finance-invoices')}
              extraRowActions={(item) => [
                item.status !== 'Paid'
                  ? {
                      label: 'Thu tiền',
                      kind: 'approve',
                      onClick: () =>
                        executeAction(
                          () =>
                            fetch(`/api/operations/invoices/${item.id}/mark-paid`, {
                              method: 'POST',
                              headers: { 'Content-Type': 'application/json' },
                              body: JSON.stringify({ paidDate: new Date().toISOString() }),
                            }),
                          'Đã ghi nhận thanh toán hóa đơn.',
                        ),
                    }
                  : null,
              ]}
            />
            <FinanceSection
              data={data}
              financeSummary={financeSummary}
              executeAction={executeAction}
              openCreate={openCreate}
              openEdit={openEdit}
              deleteEntity={deleteEntity}
              saving={saving}
              getPanelProps={getPanelProps}
            />
          </>
        )}

        {section === 'admin' && (
          <>
            <CrudPanel entityKey="users" rows={data.users} onCreate={openCreate} onEdit={openEdit} onDelete={deleteEntity} panelProps={getPanelProps('admin-users')} />
            <CrudPanel entityKey="roles" rows={data.roles} onCreate={openCreate} onEdit={openEdit} onDelete={deleteEntity} panelProps={getPanelProps('admin-roles')} />
          </>
        )}

        {modal ? (
          <ModalCard
            title={`${modal.mode === 'create' ? 'Thêm mới' : 'Cập nhật'} ${ENTITY_CONFIGS[modal.entityKey].title}`}
            subtitle="Điền đầy đủ dữ liệu để hệ thống cập nhật chính xác."
            onClose={() => setModal(null)}
            footer={
              <>
                <button className="secondary-button" onClick={() => setModal(null)}>Đóng</button>
                <button className="primary-button" onClick={saveEntity} disabled={saving}>
                  {saving ? 'Đang lưu...' : 'Lưu thay đổi'}
                </button>
              </>
            }
          >
            <EntityForm modal={modal} lookups={lookups} updateModalField={updateModalField} />
          </ModalCard>
        ) : null}
      </main>
    </div>
  )
}

function OverviewSection({ dashboard, getPanelProps }) {
  return (
    <>
      <section className="section-grid">
        <Panel title="Cảnh báo vận hành" description="Những đầu việc cần ưu tiên xử lý trong ngày." {...getPanelProps('overview-alerts')}>
          <div className="alert-list">
            {dashboard.alerts.map((alert) => (
              <div key={alert.title} className={`alert-card ${alert.level}`}>
                <div>
                  <strong>{alert.title}</strong>
                  <p>{alert.description}</p>
                </div>
                <span>{numberFormat.format(alert.value)}</span>
              </div>
            ))}
          </div>
        </Panel>

        <Panel title="Snapshot phòng" description="Trạng thái nhanh của một số phòng nổi bật." {...getPanelProps('overview-snapshots')}>
          <div className="snapshot-grid">
            {dashboard.roomSnapshots.map((room) => (
              <article key={room.id} className="snapshot-card">
                <strong>{room.roomNumber}</strong>
                <span>{room.building}</span>
                <p>{localizeValue(room.status)}</p>
                <small>{room.currentOccupancy}/{room.capacity} chỗ đang sử dụng</small>
              </article>
            ))}
          </div>
        </Panel>
      </section>

      <section className="chart-grid">
        <Panel title="Công suất theo tòa" description="So sánh mức sử dụng và công suất của từng khu nhà." {...getPanelProps('overview-occupancy-chart')}>
          <ResponsiveContainer width="100%" height={280}>
            <BarChart data={dashboard.occupancyByBuilding}>
              <CartesianGrid strokeDasharray="3 3" vertical={false} />
              <XAxis dataKey="building" />
              <YAxis />
              <Tooltip formatter={(value) => numberFormat.format(value)} />
              <Bar dataKey="occupied" fill="#0f7b6c" radius={[10, 10, 0, 0]} />
              <Bar dataKey="capacity" fill="#9bd6cf" radius={[10, 10, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </Panel>

        <Panel title="Trạng thái hóa đơn" description="Tỷ lệ hóa đơn đã thanh toán, chưa thanh toán và quá hạn." {...getPanelProps('overview-invoice-chart')}>
          <ResponsiveContainer width="100%" height={280}>
            <PieChart>
              <Pie data={dashboard.invoiceStatus} dataKey="count" nameKey="status" outerRadius={96}>
                {dashboard.invoiceStatus.map((entry, index) => (
                  <Cell key={entry.status} fill={['#0f7b6c', '#f59e0b', '#e11d48'][index % 3]} />
                ))}
              </Pie>
              <Tooltip formatter={(value) => numberFormat.format(value)} />
            </PieChart>
          </ResponsiveContainer>
        </Panel>

        <Panel title="Dòng doanh thu" description="Theo dõi tổng thu, đã thu và còn phải thu theo tháng." {...getPanelProps('overview-revenue-chart')}>
          <ResponsiveContainer width="100%" height={300}>
            <LineChart data={dashboard.monthlyRevenue}>
              <CartesianGrid strokeDasharray="3 3" vertical={false} />
              <XAxis dataKey="month" />
              <YAxis />
              <Tooltip formatter={(value) => currencyFormat.format(value)} />
              <Line type="monotone" dataKey="total" stroke="#1d4ed8" strokeWidth={3} />
              <Line type="monotone" dataKey="paid" stroke="#0f7b6c" strokeWidth={3} />
              <Line type="monotone" dataKey="unpaid" stroke="#e11d48" strokeWidth={3} />
            </LineChart>
          </ResponsiveContainer>
        </Panel>
      </section>
    </>
  )
}

function OperationsSection({
  data,
  roomOverview,
  selectedRoomId,
  setSelectedRoomId,
  roomActions,
  setRoomActions,
  waitingStudents,
  executeAction,
  openCreate,
  openEdit,
  deleteEntity,
  saving,
  getPanelProps,
}) {
  return (
    <>
      <section className="room-ops-grid">
        <Panel title="Điều phối sinh viên theo phòng" description="Xếp phòng, chuyển phòng và trả phòng ngay trên một luồng xử lý." {...getPanelProps('operations-room-workflow')}>
          <div className="room-toolbar">
            <label className="field">
              <span>Chọn phòng</span>
              <select value={selectedRoomId} onChange={(event) => setSelectedRoomId(event.target.value)}>
                {data.rooms.map((room) => (
                  <option key={room.id} value={room.id}>{room.roomNumber} - {room.buildingName}</option>
                ))}
              </select>
            </label>
            {roomOverview?.room ? (
              <div className="room-summary-box">
                <strong>{roomOverview.room.roomNumber}</strong>
                <span>{roomOverview.room.buildingName}</span>
                <p>{localizeValue(roomOverview.room.status)} · {roomOverview.room.currentOccupancy}/{roomOverview.room.capacity} chỗ</p>
              </div>
            ) : null}
          </div>

          <div className="operation-cards">
            <ActionCard title="Xếp phòng" description="Bố trí sinh viên đang chờ vào phòng hiện tại." actionLabel="Xếp vào phòng" disabled={!roomActions.assignStudentId || saving} onAction={() => executeAction(
              () => fetch(`/api/facilities/rooms/${selectedRoomId}/assign-student`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ studentId: Number(roomActions.assignStudentId), status: 'Active', note: 'Xếp phòng từ trung tâm điều phối.' }) }),
              'Đã xếp sinh viên vào phòng.',
            )}>
              <select value={roomActions.assignStudentId} onChange={(event) => setRoomActions((current) => ({ ...current, assignStudentId: event.target.value }))}>
                <option value="">Chọn sinh viên chờ xếp</option>
                {waitingStudents.map((student) => <option key={student.id} value={student.id}>{student.studentCode} - {student.name}</option>)}
              </select>
            </ActionCard>

            <ActionCard title="Chuyển phòng" description="Điều phối sinh viên sang phòng khác khi cần tối ưu công suất." actionLabel="Chuyển phòng" disabled={!roomActions.transferStudentId || !roomActions.transferToRoomId || saving} onAction={() => executeAction(
              () => fetch('/api/facilities/rooms/transfer-student', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ studentId: Number(roomActions.transferStudentId), toRoomId: Number(roomActions.transferToRoomId), status: 'Active', note: 'Điều chuyển phòng từ trung tâm điều phối.' }) }),
              'Đã chuyển phòng cho sinh viên.',
            )}>
              <select value={roomActions.transferStudentId} onChange={(event) => setRoomActions((current) => ({ ...current, transferStudentId: event.target.value }))}>
                <option value="">Chọn sinh viên đang ở</option>
                {(roomOverview?.students ?? []).map((student) => <option key={student.id} value={student.id}>{student.studentCode} - {student.name}</option>)}
              </select>
              <select value={roomActions.transferToRoomId} onChange={(event) => setRoomActions((current) => ({ ...current, transferToRoomId: event.target.value }))}>
                <option value="">Chọn phòng đích</option>
                {data.rooms.filter((room) => String(room.id) !== selectedRoomId).map((room) => <option key={room.id} value={room.id}>{room.roomNumber} - {room.buildingName}</option>)}
              </select>
            </ActionCard>

            <ActionCard title="Trả phòng" description="Đưa sinh viên ra khỏi phòng và chuyển về trạng thái chờ sắp xếp." actionLabel="Xác nhận trả phòng" disabled={!roomActions.removeStudentId || saving} onAction={() => executeAction(
              () => fetch(`/api/facilities/rooms/${selectedRoomId}/remove-student`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ studentId: Number(roomActions.removeStudentId), status: 'Waiting', note: 'Đã trả phòng, chờ sắp xếp lại.' }) }),
              'Đã trả phòng cho sinh viên.',
            )}>
              <select value={roomActions.removeStudentId} onChange={(event) => setRoomActions((current) => ({ ...current, removeStudentId: event.target.value }))}>
                <option value="">Chọn sinh viên cần trả phòng</option>
                {(roomOverview?.students ?? []).map((student) => <option key={student.id} value={student.id}>{student.studentCode} - {student.name}</option>)}
              </select>
            </ActionCard>
          </div>
        </Panel>

        <Panel title="Sinh viên trong phòng" description="Danh sách cư trú hiện tại của phòng đang chọn." {...getPanelProps('operations-room-students')}>
          <div className="occupant-list">
            {(roomOverview?.students ?? []).length === 0 ? (
              <div className="empty-state">Chưa có sinh viên nào trong phòng này.</div>
            ) : (
              roomOverview.students.map((student) => (
                <article key={student.id} className="occupant-card">
                  <strong>{student.name}</strong>
                  <span>{student.studentCode}</span>
                  <p>{student.faculty} · {student.className}</p>
                  <small>{student.phone}</small>
                </article>
              ))
            )}
          </div>
        </Panel>
      </section>

      <CrudPanel
        entityKey="registrations"
        rows={data.registrations}
        onCreate={openCreate}
        onEdit={openEdit}
        onDelete={deleteEntity}
        panelProps={getPanelProps('operations-registrations')}
        extraRowActions={(item) => [
          item.status === 'Pending' ? { label: 'Duyệt', kind: 'approve', onClick: () => executeAction(() => fetch(`/api/operations/registrations/${item.id}/approve`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ note: 'Duyệt từ giao diện vận hành.' }) }), 'Đã duyệt đăng ký và xếp phòng.') } : null,
          item.status === 'Pending' ? { label: 'Từ chối', kind: 'danger', onClick: () => executeAction(() => fetch(`/api/operations/registrations/${item.id}/reject`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ note: 'Từ chối từ giao diện vận hành.' }) }), 'Đã từ chối đăng ký.') } : null,
        ]}
      />
    </>
  )
}

function FinanceSection({
  data,
  financeSummary,
  executeAction,
  openCreate,
  openEdit,
  deleteEntity,
  saving,
  getPanelProps,
}) {
  const totalExpected = data.roomFinances.reduce((sum, item) => sum + Number(item.total ?? 0), 0)
  const totalCollected = data.roomFinances.reduce((sum, item) => sum + Number(item.paidAmount ?? 0), 0)
  const totalOutstanding = Math.max(0, totalExpected - totalCollected)
  const paidRooms = data.roomFinances.filter((item) => item.status === 'Paid').length
  const partialRooms = data.roomFinances.filter((item) => item.status === 'PartiallyPaid').length
  const lateRooms = data.roomFinances.filter((item) => item.status === 'Late').length

  return (
    <>
      <Panel title="Tổng quan tài chính theo phòng" description="Doanh thu tháng đang thể hiện tổng tiền phải thu của các kỳ tài chính hiện có. Khối này giúp nhìn rõ tiền đã thu, còn nợ và trạng thái từng phòng." {...getPanelProps('finance-summary')}>
        <div className="finance-caption">
          {financeSummary?.billingMonth ? `Kỳ theo dõi: ${new Intl.DateTimeFormat('vi-VN', { month: '2-digit', year: 'numeric' }).format(new Date(financeSummary.billingMonth))}` : 'Theo dõi tổng hợp công nợ theo từng phòng.'}
        </div>
        <div className="finance-highlight-grid">
          <article className="finance-highlight-card blue">
            <span>Tổng phải thu</span>
            <strong>{currencyFormat.format(totalExpected)}</strong>
          </article>
          <article className="finance-highlight-card emerald">
            <span>Đã thu</span>
            <strong>{currencyFormat.format(totalCollected)}</strong>
          </article>
          <article className="finance-highlight-card amber">
            <span>Còn phải thu</span>
            <strong>{currencyFormat.format(totalOutstanding)}</strong>
          </article>
          <article className="finance-highlight-card rose">
            <span>Phòng quá hạn</span>
            <strong>{numberFormat.format(lateRooms)}</strong>
          </article>
        </div>
        <div className="finance-status-grid">
          <div className="summary-block">
            <span>Phòng đã thanh toán đủ</span>
            <strong>{numberFormat.format(paidRooms)}</strong>
          </div>
          <div className="summary-block">
            <span>Phòng thanh toán một phần</span>
            <strong>{numberFormat.format(partialRooms)}</strong>
          </div>
          <div className="summary-block">
            <span>Phòng còn công nợ</span>
            <strong>{numberFormat.format(data.roomFinances.filter((item) => item.status !== 'Paid').length)}</strong>
          </div>
        </div>
      </Panel>

      <CrudPanel
        entityKey="roomFeeProfiles"
        rows={data.roomFeeProfiles}
        onCreate={openCreate}
        onEdit={openEdit}
        onDelete={deleteEntity}
        panelProps={getPanelProps('finance-room-fee-profiles')}
      />

      <CrudPanel
        entityKey="roomFinances"
        rows={data.roomFinances}
        onCreate={openCreate}
        onEdit={openEdit}
        onDelete={deleteEntity}
        panelProps={getPanelProps('finance-room-finances')}
        extraRowActions={(item) => [
          item.utilityId
            ? {
                label: 'Đồng bộ từ điện nước',
                kind: 'approve',
                onClick: () =>
                  executeAction(
                    () => fetch(`/api/operations/room-finances/generate-from-utility/${item.utilityId}`, { method: 'POST' }),
                    'Đã cập nhật công nợ phòng từ dữ liệu điện nước.',
                  ),
              }
            : null,
          item.status !== 'Paid'
            ? {
                label: 'Đánh dấu đã thu',
                kind: 'approve',
                onClick: () =>
                  executeAction(
                    () =>
                      fetch(`/api/operations/room-finances/${item.id}/mark-paid`, {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({
                          paidAmount: Number(item.remainingAmount ?? item.total ?? 0),
                          paidDate: new Date().toISOString(),
                          paymentMethod: 'Cash',
                          paymentNote: 'Đã thu tiền phòng từ giao diện tài chính.',
                          recordedBy: 'Kế toán',
                        }),
                      }),
                    'Đã ghi nhận phòng đã nộp tiền.',
                  ),
              }
            : null,
        ]}
      />
    </>
  )
}

function EntityForm({ modal, lookups, updateModalField }) {
  return (
    <div className="form-grid">
      {ENTITY_CONFIGS[modal.entityKey].fields.map((field) => (
        <label key={field.name} className={field.type === 'textarea' ? 'field field-wide' : 'field'}>
          <span>{field.label}</span>
          {renderInputField(field, modal.values[field.name], lookups[field.lookup] ?? [], updateModalField)}
        </label>
      ))}
    </div>
  )
}

function renderInputField(field, value, lookupItems, onChange) {
  if (field.type === 'select') {
    return (
      <select value={value} onChange={(event) => onChange(field.name, event.target.value)}>
        {field.allowEmpty ? <option value="">Chọn</option> : null}
        {field.options.map((option) => <option key={option.value} value={option.value}>{option.label}</option>)}
      </select>
    )
  }

  if (field.type === 'lookup') {
    return (
      <select value={value} onChange={(event) => onChange(field.name, event.target.value)}>
        {field.allowEmpty ? <option value="">Không chọn</option> : null}
        {lookupItems.map((item) => <option key={item.id} value={item.id}>{field.optionLabel(item)}</option>)}
      </select>
    )
  }

  if (field.type === 'textarea') {
    return <textarea rows={4} value={value} onChange={(event) => onChange(field.name, event.target.value)} />
  }

  if (field.type === 'checkbox') {
    return <div className="checkbox-row"><input type="checkbox" checked={Boolean(value)} onChange={(event) => onChange(field.name, event.target.checked)} /><span>Kích hoạt tài khoản</span></div>
  }

  return <input type={field.type} value={value} onChange={(event) => onChange(field.name, event.target.value)} />
}

export default App
