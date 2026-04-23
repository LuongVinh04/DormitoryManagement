import { useState, useEffect } from 'react'

export function useSidebarLayout() {
  const [isSidebarCollapsed, setIsSidebarCollapsed] = useState(() => {
    const saved = localStorage.getItem('dormitory-hub-sidebar-layout')
    if (saved) {
      try {
        return JSON.parse(saved).sidebarCollapsed
      } catch {
        return false
      }
    }
    return false
  })

  const [isSidebarSummaryCollapsed, setIsSidebarSummaryCollapsed] = useState(() => {
    const saved = localStorage.getItem('dormitory-hub-sidebar-layout')
    if (saved) {
      try {
        return JSON.parse(saved).summaryCollapsed
      } catch {
        return false
      }
    }
    return false
  })

  const [isSidebarNavCollapsed, setIsSidebarNavCollapsed] = useState(() => {
    const saved = localStorage.getItem('dormitory-hub-sidebar-layout')
    if (saved) {
      try {
        return JSON.parse(saved).navCollapsed
      } catch {
        return false
      }
    }
    return false
  })

  useEffect(() => {
    localStorage.setItem('dormitory-hub-sidebar-layout', JSON.stringify({
      sidebarCollapsed: isSidebarCollapsed,
      summaryCollapsed: isSidebarSummaryCollapsed,
      navCollapsed: isSidebarNavCollapsed
    }))
  }, [isSidebarCollapsed, isSidebarSummaryCollapsed, isSidebarNavCollapsed])

  return {
    isSidebarCollapsed,
    isSidebarSummaryCollapsed,
    isSidebarNavCollapsed,
    toggleSidebarCollapse: () => setIsSidebarCollapsed(prev => !prev),
    toggleSidebarSummary: () => setIsSidebarSummaryCollapsed(prev => !prev),
    toggleSidebarNav: () => setIsSidebarNavCollapsed(prev => !prev)
  }
}
