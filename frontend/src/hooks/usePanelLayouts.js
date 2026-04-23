import { useEffect, useState } from 'react'

const PANEL_LAYOUT_STORAGE_KEY = 'dormitory-hub-panel-layouts'

export function usePanelLayouts() {
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
    if (typeof window === 'undefined') return
    window.localStorage.setItem(PANEL_LAYOUT_STORAGE_KEY, JSON.stringify(panelLayouts))
  }, [panelLayouts])

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

  function expandPanel(panelKey) {
    setPanelLayouts((current) => ({
      ...current,
      [panelKey]: {
        ...(current[panelKey] ?? { collapsed: false, expanded: false }),
        collapsed: false,
      },
    }))
  }

  return { getPanelProps, expandPanel }
}
