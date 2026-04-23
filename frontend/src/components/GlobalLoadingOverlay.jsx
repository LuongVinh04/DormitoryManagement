import { useEffect, useState } from 'react'
import { subscribeLoading } from '../loadingBus'

export function GlobalLoadingOverlay() {
  const [loadingState, setLoadingState] = useState({ active: false, pendingRequests: 0 })

  useEffect(() => subscribeLoading(setLoadingState), [])

  return (
    <div
      className={loadingState.active ? 'global-loading active' : 'global-loading'}
      aria-hidden={loadingState.active ? 'false' : 'true'}
    >
      <div className="global-loading-backdrop" />
      <div className="global-loading-card" role="status" aria-live="polite">
        <div className="global-loading-brand">
          <img src="/dormitory-hub-logo.svg" alt="Dormitory Hub" className="global-loading-logo" />
          <div>
            <span className="global-loading-kicker">Dormitory Hub</span>
            <strong>Đang đồng bộ dữ liệu</strong>
          </div>
        </div>
        <div className="global-loading-bar">
          <span />
        </div>
        <p>Hệ thống đang tải dữ liệu và hoàn tất thao tác của bạn. Vui lòng chờ trong giây lát.</p>
      </div>
    </div>
  )
}
