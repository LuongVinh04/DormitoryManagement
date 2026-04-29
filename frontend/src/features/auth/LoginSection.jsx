import { useState } from 'react'
import { useAuth } from '../../hooks/useAuth'

export function LoginSection() {
  const { login } = useAuth()
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  async function handleSubmit(event) {
    event.preventDefault()
    if (!username || !password) {
      setError('Vui lòng nhập tài khoản và mật khẩu.')
      return
    }

    try {
      setLoading(true)
      setError('')
      await login(username, password)

      if (window.location.pathname === '/login') {
        window.location.href = '/'
      }
    } catch (loginError) {
      setError(loginError.message)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="login-wrapper">
      <div className="login-card">
        <div className="login-header">
          <img src="/dormitory-hub-logo.svg" alt="Dormitory Hub" className="login-logo" />
          <span className="login-kicker">Dormitory Hub</span>
          <h2>Đăng nhập</h2>
          <p>Hệ thống quản lý nội trú thông minh Dormitory Hub.</p>
        </div>

        {error ? <div className="feedback error">{error}</div> : null}

        <form onSubmit={handleSubmit} className="login-form">
          <div className="form-group">
            <label>Tài khoản</label>
            <input
              type="text"
              value={username}
              onChange={(event) => setUsername(event.target.value)}
              disabled={loading}
              placeholder="Nhập tài khoản"
              autoFocus
            />
          </div>

          <div className="form-group">
            <label>Mật khẩu</label>
            <input
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              disabled={loading}
              placeholder="Nhập mật khẩu"
            />
          </div>

          <button type="submit" disabled={loading} className="primary-button login-button">
            {loading ? 'Đang xác thực...' : 'Đăng nhập'}
          </button>
        </form>
      </div>
    </div>
  )
}
