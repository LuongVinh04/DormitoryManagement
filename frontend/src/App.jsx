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
import { currencyFormat, numberFormat } from './helpers'

const AdminSection = lazy(() => import('./features/admin/AdminSection').then((module) => ({ default: module.AdminSection })))
const CatalogSection = lazy(() => import('./features/catalog/CatalogSection').then((module) => ({ default: module.CatalogSection })))
const FacilitiesSection = lazy(() => import('./features/facilities/FacilitiesSection').then((module) => ({ default: module.FacilitiesSection })))
const FinanceSection = lazy(() => import('./features/finance/FinanceSection').then((module) => ({ default: module.FinanceSection })))
const OperationsSection = lazy(() => import('./features/operations/OperationsSection').then((module) => ({ default: module.OperationsSection })))
const OverviewSection = lazy(() => import('./features/overview/OverviewSection').then((module) => ({ default: module.OverviewSection })))
const StudentsSection = lazy(() => import('./features/students/StudentsSection').then((module) => ({ default: module.StudentsSection })))

function App() {
  const location = useLocation()
  const navigate = useNavigate()
  const activeRoute = NAVIGATION.find((item) => item.path === location.pathname) ?? NAVIGATION[0]
  const section = activeRoute.key
  const { getPanelProps } = usePanelLayouts()
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
                <article key={item.label} className={`hero-focus-item ${item.tone}`}>
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
            <EntityForm modal={modal} lookups={lookups} updateModalField={updateModalField} errors={formErrors} />
          </ModalCard>
        ) : null}
      </main>
    </div>
  )
}


export default App
