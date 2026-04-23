import { useState } from 'react'
import { useAuth } from '../../hooks/useAuth'

export function LoginSection() {
  const { login } = useAuth()
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  async function handleSubmit(e) {
    e.preventDefault()
    if (!username || !password) {
      setError('Vui lòng nhập tài khoản và mật khẩu.')
      return
    }

    try {
      setLoading(true)
      setError('')
      await login(username, password)
      // will trigger redirect based on AuthContext update
      if (window.location.pathname === '/login') {
        window.location.href = '/'
      }
    } catch (err) {
      setError(err.message)
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
        <form onSubmit={handleSubmit} className="login-form">
          {error && <div className="login-error">{error}</div>}
          <div className="form-group">
            <label>Tài khoản</label>
            <input
              type="text"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              disabled={loading}
              autoFocus
            />
          </div>
          <div className="form-group">
            <label>Mật khẩu</label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              disabled={loading}
            />
          </div>
          <button type="submit" disabled={loading} className="primary-button login-button">
            {loading ? 'Đang xác thực...' : 'Đăng nhập'}
          </button>
        </form>
      </div>
      <style dangerouslySetInnerHTML={{ __html: `
        .login-wrapper {
          display: flex;
          align-items: center;
          justify-content: center;
          min-height: 100vh;
          background: var(--bg-neutral);
        }
        .login-card {
          background: white;
          width: 100%;
          max-width: 400px;
          border-radius: var(--radius-lg);
          box-shadow: var(--shadow-md);
          padding: 2rem;
          border: 1px solid var(--border-light);
        }
        .login-header {
          text-align: center;
          margin-bottom: 2rem;
        }
        .login-logo {
          width: 72px;
          height: 72px;
          margin-bottom: 0.9rem;
          filter: drop-shadow(0 18px 35px rgba(29, 78, 216, 0.18));
        }
        .login-kicker {
          display: inline-flex;
          margin-bottom: 0.65rem;
          padding: 0.35rem 0.75rem;
          border-radius: 999px;
          background: rgba(15, 118, 110, 0.1);
          color: #0f766e;
          font-size: 0.78rem;
          font-weight: 800;
          letter-spacing: 0.08em;
          text-transform: uppercase;
        }
        .login-header h2 {
          margin: 0 0 0.5rem 0;
          font-size: 1.5rem;
          color: var(--text-heading);
        }
        .login-header p {
          margin: 0;
          color: var(--text-muted);
          font-size: 0.875rem;
        }
        .login-form .form-group {
          margin-bottom: 1.25rem;
        }
        .login-form label {
          display: block;
          margin-bottom: 0.5rem;
          font-size: 0.875rem;
          font-weight: 500;
          color: var(--text-heading);
        }
        .login-form input {
          width: 100%;
          padding: 0.625rem;
          border: 1px solid var(--border-light);
          border-radius: var(--radius-md);
          box-sizing: border-box;
          outline: none;
        }
        .login-form input:focus {
          border-color: var(--blue-500);
          box-shadow: 0 0 0 2px var(--blue-100);
        }
        .login-button {
          width: 100%;
          padding: 0.75rem;
          font-size: 1rem;
          margin-top: 0.5rem;
        }
        .login-error {
          padding: 0.75rem;
          background: var(--rose-50);
          border: 1px solid var(--rose-200);
          color: var(--rose-700);
          border-radius: var(--radius-md);
          margin-bottom: 1rem;
          font-size: 0.875rem;
        }
      `}} />
    </div>
  )
}
