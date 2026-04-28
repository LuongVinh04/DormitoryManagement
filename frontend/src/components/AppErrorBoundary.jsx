import { Component } from 'react'

export class AppErrorBoundary extends Component {
  constructor(props) {
    super(props)
    this.state = { hasError: false }
  }

  static getDerivedStateFromError() {
    return { hasError: true }
  }

  render() {
    if (this.state.hasError) {
      return (
        <div className="loading-screen">
          <div className="route-error-card">
            <strong>Giao diện gặp lỗi khi tải.</strong>
            <p>Vui lòng tải lại trang. Nếu lỗi vẫn lặp lại, hãy kiểm tra console để lấy thông tin lỗi chi tiết.</p>
            <button className="primary-button" onClick={() => window.location.reload()}>Tải lại trang</button>
          </div>
        </div>
      )
    }

    return this.props.children
  }
}
