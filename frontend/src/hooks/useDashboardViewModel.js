import { useMemo } from 'react'
import { NAVIGATION } from '../constants'
import { currencyFormat, numberFormat, repairText } from '../helpers'

export function useDashboardViewModel({ dashboard, data, financeSummary, section }) {
  const waitingStudents = useMemo(
    () => data.students.filter((student) => !student.roomId || ['Waiting', 'PendingMoveIn', 'Pending'].includes(student.status)),
    [data.students],
  )

  const activeNavigation = useMemo(
    () => NAVIGATION.find((item) => item.key === section) ?? NAVIGATION[0],
    [section],
  )

  const occupancyRate = useMemo(() => {
    if (!dashboard?.summary.totalRooms) return 0
    return Math.round((dashboard.summary.occupiedRooms / dashboard.summary.totalRooms) * 100)
  }, [dashboard])

  const focusCards = useMemo(() => ([
    {
      label: 'Quá hạn cần xử lý',
      value: numberFormat.format(dashboard?.summary?.overdueInvoices ?? 0),
      tone: 'rose',
      route: '/finance',
      panelKey: 'finance-invoices',
      panelId: 'panel-finance-invoices'
    },
    {
      label: 'Hồ sơ đang chờ',
      value: numberFormat.format(dashboard?.summary?.waitingStudents ?? 0),
      tone: 'amber',
      route: '/operations',
      panelKey: 'operations-registrations',
      panelId: 'panel-operations-registrations'
    },
    {
      label: 'Công suất sử dụng',
      value: `${occupancyRate}%`,
      tone: 'teal',
      route: '/overview',
      panelKey: 'overview-occupancy-chart',
      panelId: 'panel-overview-occupancy-chart'
    },
  ]), [dashboard, occupancyRate])

  const sectionStats = useMemo(() => {
    if (!dashboard?.summary) {
      return []
    }

    switch (section) {
      case 'operations':
        return [
          { label: 'Sinh viên chờ xếp', value: numberFormat.format(waitingStudents.length) },
          { label: 'Hồ sơ chờ duyệt', value: numberFormat.format(data.registrations.filter((item) => item.status === 'Pending').length) },
          { label: 'Phòng còn trống', value: numberFormat.format(data.rooms.filter((item) => Number(item.availableSlots) > 0).length) },
        ]
      case 'catalog':
        return [
          { label: 'Loại phòng', value: numberFormat.format(data.roomCategories.length) },
          { label: 'Phân khu', value: numberFormat.format(data.roomZones.length) },
          { label: 'Hình thức thu', value: numberFormat.format(data.paymentMethods.length) },
        ]
      case 'facilities':
        return [
          { label: 'Tòa nhà', value: numberFormat.format(data.buildings.length) },
          { label: 'Phòng đang dùng', value: numberFormat.format(data.rooms.filter((item) => item.status === 'Occupied' || item.status === 'Full').length) },
          { label: 'Giường trống', value: numberFormat.format(dashboard.summary.availableBeds) },
        ]
      case 'students':
        return [
          { label: 'Sinh viên đang ở', value: numberFormat.format(data.students.filter((item) => item.status === 'Active').length) },
          { label: 'Chờ nhận phòng', value: numberFormat.format(data.students.filter((item) => item.status === 'PendingMoveIn').length) },
          { label: 'Hợp đồng hiệu lực', value: numberFormat.format(dashboard.summary.activeContracts) },
        ]
      case 'finance':
        return [
          { label: 'Tổng phải thu', value: currencyFormat.format(financeSummary?.totalExpected ?? 0) },
          { label: 'Đã thu', value: currencyFormat.format(financeSummary?.collected ?? 0) },
          { label: 'Còn lại', value: currencyFormat.format(financeSummary?.outstanding ?? 0) },
        ]
      case 'admin':
        return [
          { label: 'Tài khoản', value: numberFormat.format(data.users.length) },
          { label: 'Đang hoạt động', value: numberFormat.format(data.users.filter((item) => item.isActive).length) },
          { label: 'Vai trò', value: numberFormat.format(data.roles.length) },
        ]
      default:
        return [
          { label: 'Phòng hoạt động', value: numberFormat.format(dashboard.summary.occupiedRooms) },
          { label: 'Giường còn trống', value: numberFormat.format(dashboard.summary.availableBeds) },
          { label: 'Doanh thu tháng', value: currencyFormat.format(dashboard.summary.revenueThisMonth) },
        ]
    }
  }, [data, dashboard, financeSummary, section, waitingStudents.length])

  const safeFocusCards = useMemo(
    () => focusCards.map((item) => ({ ...item, label: repairText(item.label) })),
    [focusCards],
  )

  const safeSectionStats = useMemo(
    () => sectionStats.map((item) => ({ ...item, label: repairText(item.label) })),
    [sectionStats],
  )

  const sectionBannerContent = useMemo(() => ({
    eyebrow: `Ph\u00e2n h\u1ec7 ${activeNavigation.label}`,
    title: section === 'overview'
      ? 'T\u1ed5ng quan v\u1eadn h\u00e0nh v\u00e0 t\u00e0i ch\u00ednh theo ng\u00e0y'
      : `Kh\u00f4ng gian l\u00e0m vi\u1ec7c cho ${activeNavigation.label.toLowerCase()}`,
    description: section === 'overview'
      ? '\u01afu ti\u00ean quan s\u00e1t nh\u1eefng bi\u1ebfn \u0111\u1ed9ng l\u1edbn: c\u00f4ng su\u1ea5t ph\u00f2ng, c\u1ea3nh b\u00e1o v\u00e0 d\u00f2ng ti\u1ec1n.'
      : 'M\u1ed7i ph\u00e2n h\u1ec7 \u0111\u01b0\u1ee3c gom theo nhi\u1ec7m v\u1ee5 ch\u00ednh \u0111\u1ec3 gi\u1ea3m thao t\u00e1c chuy\u1ec3n qua l\u1ea1i.',
  }), [activeNavigation.label, section])

  return {
    activeNavigation,
    safeFocusCards,
    safeSectionStats,
    sectionBannerContent,
    waitingStudents,
  }
}
