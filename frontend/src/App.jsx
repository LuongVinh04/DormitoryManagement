import { lazy, Suspense } from 'react'
import { Navigate, Route, Routes, useLocation, useNavigate } from 'react-router-dom'
import './App.css'
import { MetricCard, ModalCard, SectionBanner, SummaryBlock } from './components'
import { NAVIGATION, ENTITY_CONFIGS } from './constants'
import { EntityForm } from './forms/EntityForm'
import { StatusToast } from './components/StatusToast'
import { useDashboardViewModel } from './hooks/useDashboardViewModel'
import { useDormitoryData } from './hooks/useDormitoryData'
import { usePanelLayouts } from './hooks/usePanelLayouts'
import { useSidebarLayout } from './hooks/useSidebarLayout'
import { useAuth } from './hooks/useAuth'
import { LoginSection } from './features/auth/LoginSection'
import { currencyFormat, numberFormat } from './helpers'

const AdminSection = lazy(() => import('./features/admin/AdminSection').then((module) => ({ default: module.AdminSection })))
const CatalogSection = lazy(() => import('./features/catalog/CatalogSection').then((module) => ({ default: module.CatalogSection })))
const FacilitiesSection = lazy(() => import('./features/facilities/FacilitiesSection').then((module) => ({ default: module.FacilitiesSection })))
const FinanceSection = lazy(() => import('./features/finance/FinanceSection').then((module) => ({ default: module.FinanceSection })))
const OperationsSection = lazy(() => import('./features/operations/OperationsSection').then((module) => ({ default: module.OperationsSection })))
const OverviewSection = lazy(() => import('./features/overview/OverviewSection').then((module) => ({ default: module.OverviewSection })))
const StudentsSection = lazy(() => import('./features/students/StudentsSection').then((module) => ({ default: module.StudentsSection })))

function App() {
  const { user, loading: authLoading, logout, hasPermission, hasAnyPermission } = useAuth()
  const {
    isSidebarCollapsed,
    isSidebarSummaryCollapsed,
    isSidebarNavCollapsed,
    toggleSidebarCollapse,
    toggleSidebarSummary,
    toggleSidebarNav
  } = useSidebarLayout()
  const location = useLocation()
  const navigate = useNavigate()

  const allowedNavs = NAVIGATION.filter((item) => {
    switch (item.key) {
      case 'overview': return hasPermission('dashboard.view')
      case 'catalog': return true
      case 'operations': return hasAnyPermission(['registrations.view', 'room.assign'])
      case 'facilities': return hasAnyPermission(['buildings.view', 'rooms.view'])
      case 'students': return hasPermission('students.view')
      case 'finance': return hasAnyPermission(['roomFinance.view', 'invoices.view', 'utilities.view'])
      case 'admin': return hasAnyPermission(['users.view', 'roles.view', 'permissions.manage'])
      default: return true
    }
  })

  const activeRoute = allowedNavs.find((item) => item.path === location.pathname) ?? allowedNavs[0]
  const section = activeRoute?.key ?? 'overview'
  const { getPanelProps, expandPanel } = usePanelLayouts()
  const {
    activeRoomId,
    dashboard,
    data,
    deleteEntity,
    error,
    executeAction,
    financeSummary,
    formErrors,
    loading,
    lookups,
    modal,
    notice,
    openCreate,
    openEdit,
    refreshData,
    roomActions,
    roomOverview,
    saveEntity,
    saving,
    setModal,
    setRoomActions,
    setSelectedRoomId,
    updateModalField,
  } = useDormitoryData()
  const {
    activeNavigation,
    safeFocusCards,
    safeSectionStats,
    sectionBannerContent,
    waitingStudents,
  } = useDashboardViewModel({ dashboard, data, financeSummary, section })

  const handleFocusCardClick = (item) => {
    if (item.route) {
      if (location.pathname !== item.route) {
        navigate(item.route)
      }
      setTimeout(() => {
        if (item.panelKey) expandPanel(item.panelKey)
        if (item.panelId) {
          const el = document.getElementById(item.panelId)
          if (el) el.scrollIntoView({ behavior: 'smooth', block: 'start' })
        }
      }, 50)
    }
  }

  if (authLoading) {
    return <div className="loading-screen"><div className="loading-card"><h1>Đang kiểm tra phiên làm việc...</h1></div></div>
  }

  if (!user) {
    return (
      <Routes>
        <Route path="/login" element={<LoginSection />} />
        <Route path="*" element={<Navigate to="/login" replace />} />
      </Routes>
    )
  }

  if (loading || !dashboard) {
    return (
      <div className="loading-screen">
        <div className="loading-card">
          <img src="/dormitory-hub-logo.svg" alt="Dormitory Hub" className="loading-brand-logo" />
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
        <div className={isSidebarCollapsed ? 'brand-panel compact' : 'brand-panel'}>
          <div className="brand-panel-header">
            <div className="brand-panel-title">
              <img src="/dormitory-hub-logo.svg" alt="Dormitory Hub" className="brand-logo" />
              <span className="brand-kicker">DORMITORY HUB</span>
            </div>
            <button className="sidebar-toggle" onClick={toggleSidebarCollapse} title={isSidebarCollapsed ? "Mở rộng" : "Thu gọn"}>
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
                {isSidebarCollapsed ? <polyline points="9 18 15 12 9 6"></polyline> : <polyline points="15 18 9 12 15 6"></polyline>}
              </svg>
            </button>
          </div>
          <h2>Điều hành lưu trú thông minh</h2>
          <div className="brand-panel-user-row">
            <p>
              <span className="greeting-text">Xin chào ! </span>
              <strong>{user.fullName || user.username}</strong>
            </p>
            <button className="secondary-button" onClick={logout}>Đăng xuất</button>
          </div>
        </div>

        <div className={isSidebarSummaryCollapsed ? 'sidebar-summary compact' : 'sidebar-summary'}>
          <div className="sidebar-summary-header" onClick={toggleSidebarSummary}>
            <span>Thống kê</span>
            <button title={isSidebarSummaryCollapsed ? "Mở rộng" : "Thu gọn"}>
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                {isSidebarSummaryCollapsed ? <polyline points="6 9 12 15 18 9"></polyline> : <polyline points="18 15 12 9 6 15"></polyline>}
              </svg>
            </button>
          </div>
          <SummaryBlock label="Phòng đang ở" value={numberFormat.format(dashboard.summary.occupiedRooms)} compact={isSidebarSummaryCollapsed} />
          <SummaryBlock label="Giường còn trống" value={numberFormat.format(dashboard.summary.availableBeds)} compact={isSidebarSummaryCollapsed} />
          <SummaryBlock label="Chờ duyệt" value={numberFormat.format(dashboard.summary.waitingStudents)} compact={isSidebarSummaryCollapsed} />
        </div>

        <nav className={isSidebarNavCollapsed ? 'nav-grid collapsed' : 'nav-grid'}>
          <div className="nav-section-header" onClick={toggleSidebarNav}>
            <span>Danh mục</span>
            <button title={isSidebarNavCollapsed ? "Mở rộng" : "Thu gọn"}>
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                {isSidebarNavCollapsed ? <polyline points="6 9 12 15 18 9"></polyline> : <polyline points="18 15 12 9 6 15"></polyline>}
              </svg>
            </button>
          </div>
          {allowedNavs.map((item) => (
            <button
              key={item.key}
              className={section === item.key ? 'nav-link active' : 'nav-link'}
              onClick={() => navigate(item.path)}
            >
              {item.label}
            </button>
          ))}
        </nav>
      </aside>

      <main className="main-area">
        <header className="page-header">
          <div className="page-hero-copy">
            <p className="eyebrow">Hệ thống quản trị ký túc xá</p>
            <h1>Trang quản trị vận hành thực tế cho khu nội trú sinh viên</h1>
            <p className="page-description">
              Theo dõi công suất phòng, xử lý hồ sơ, điều phối chỗ ở và kiểm soát dòng tiền trên một giao diện đẹp, dễ dùng và responsive.
            </p>
          </div>
          <div className="hero-focus-card">
            <div className="hero-focus-header">
              <span className="hero-focus-label">Focus</span>
              <strong>{activeNavigation.label}</strong>
            </div>
            <div className="hero-focus-grid">
              {safeFocusCards.map((item) => (
                <article
                  key={item.label}
                  className={`hero-focus-item ${item.tone}`}
                  style={{ cursor: item.route ? 'pointer' : 'default' }}
                  onClick={() => handleFocusCardClick(item)}
                >
                  <span>{item.label}</span>
                  <strong>{item.value}</strong>
                </article>
              ))}
            </div>
            <div className="header-actions">
              <button className="primary-button" onClick={refreshData}>{'L\u00e0m m\u1edbi to\u00e0n h\u1ec7 th\u1ed1ng'}</button>
              <button className="secondary-button" onClick={() => openCreate('students')}>{'Ti\u1ebfp nh\u1eadn sinh vi\u00ean m\u1edbi'}</button>
            </div>
          </div>
        </header>

        <StatusToast error={error} notice={notice} saving={saving} />

        <section className="top-metrics">
          <MetricCard label={'T\u1ed5ng sinh vi\u00ean'} value={numberFormat.format(dashboard.summary.totalStudents)} accent="blue" />
          <MetricCard label={'H\u1ee3p \u0111\u1ed3ng hi\u1ec7u l\u1ef1c'} value={numberFormat.format(dashboard.summary.activeContracts)} accent="emerald" />
          <MetricCard label={'H\u00f3a \u0111\u01a1n ch\u01b0a thu'} value={numberFormat.format(dashboard.summary.unpaidInvoices)} accent="amber" />
          <MetricCard label={'Qu\u00e1 h\u1ea1n thanh to\u00e1n'} value={numberFormat.format(dashboard.summary.overdueInvoices)} accent="rose" />
          <MetricCard label={'Doanh thu th\u00e1ng'} value={currencyFormat.format(dashboard.summary.revenueThisMonth)} accent="violet" large />
        </section>

        <SectionBanner
          eyebrow={sectionBannerContent.eyebrow}
          title={sectionBannerContent.title}
          description={sectionBannerContent.description}
          stats={safeSectionStats}
          actions={section === 'overview' ? (
            <>
              <button className="secondary-button" onClick={() => navigate('/operations')}>{'M\u1edf \u0111i\u1ec1u ph\u1ed1i'}</button>
              <button className="secondary-button" onClick={() => navigate('/finance')}>{'M\u1edf t\u00e0i ch\u00ednh'}</button>
            </>
          ) : null}
        />

        <Suspense fallback={<div className="page-loading">Đang mở phân hệ...</div>}>
          <Routes>
            <Route path="/" element={<Navigate to="/overview" replace />} />
            <Route path="/overview" element={<OverviewSection dashboard={dashboard} getPanelProps={getPanelProps} />} />
            <Route
              path="/catalog"
              element={<CatalogSection data={data} openCreate={openCreate} openEdit={openEdit} deleteEntity={deleteEntity} getPanelProps={getPanelProps} />}
            />
            <Route
              path="/operations"
              element={(
                <OperationsSection
                  data={data}
                  roomOverview={roomOverview}
                  selectedRoomId={activeRoomId}
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
            />
            <Route
              path="/facilities"
              element={<FacilitiesSection data={data} openCreate={openCreate} openEdit={openEdit} deleteEntity={deleteEntity} getPanelProps={getPanelProps} />}
            />
            <Route
              path="/students"
              element={<StudentsSection data={data} openCreate={openCreate} openEdit={openEdit} deleteEntity={deleteEntity} getPanelProps={getPanelProps} />}
            />
            <Route
              path="/finance"
              element={<FinanceSection data={data} financeSummary={financeSummary} executeAction={executeAction} openCreate={openCreate} openEdit={openEdit} deleteEntity={deleteEntity} getPanelProps={getPanelProps} />}
            />
            <Route
              path="/admin"
              element={<AdminSection data={data} openCreate={openCreate} openEdit={openEdit} deleteEntity={deleteEntity} getPanelProps={getPanelProps} />}
            />
            <Route path="*" element={<Navigate to="/overview" replace />} />
          </Routes>
        </Suspense>

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
            <EntityForm modal={modal} lookups={{ ...lookups, users: data.users || [] }} updateModalField={updateModalField} errors={formErrors} />
          </ModalCard>
        ) : null}
      </main>
    </div>
  )
}


export default App
