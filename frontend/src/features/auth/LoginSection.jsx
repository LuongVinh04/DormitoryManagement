import { useState } from 'react'
import { useAuth } from '../../hooks/useAuth'
import { apiFetch, readError } from '../../helpers'

const INITIAL_REGISTER_FORM = {
  studentCode: '',
  email: '',
  username: '',
  password: '',
  confirmPassword: '',
}

export function LoginSection() {
  const { login } = useAuth()
  const [mode, setMode] = useState('login')
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [registerForm, setRegisterForm] = useState(INITIAL_REGISTER_FORM)
  const [error, setError] = useState('')
  const [notice, setNotice] = useState('')
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
      setNotice('')
      await login(username, password)

      if (window.location.pathname === '/login') {
        window.location.href = '/'
      }
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading(false)
    }
  }

  async function handleStudentRegister(e) {
    e.preventDefault()
    if (!registerForm.studentCode || !registerForm.email || !registerForm.username || !registerForm.password) {
      setError('Vui lòng nhập đầy đủ mã sinh viên, email, tài khoản và mật khẩu.')
      return
    }

    if (registerForm.password !== registerForm.confirmPassword) {
      setError('Mật khẩu xác nhận chưa khớp.')
      return
    }

    try {
      setLoading(true)
      setError('')
      setNotice('')
      const response = await apiFetch('/api/auth/student-register', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          studentCode: registerForm.studentCode,
          email: registerForm.email,
          username: registerForm.username,
          password: registerForm.password,
        }),
      })

      if (!response.ok) {
        throw new Error(await readError(response))
      }

      setNotice('Đăng ký tài khoản sinh viên thành công. Bạn có thể đăng nhập ngay.')
      setUsername(registerForm.username)
      setPassword('')
      setRegisterForm(INITIAL_REGISTER_FORM)
      setMode('login')
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading(false)
    }
  }

  function updateRegisterField(name, value) {
    setRegisterForm((current) => ({ ...current, [name]: value }))
  }

  return (
    <div className="login-wrapper">
      <div className="login-card">
        <div className="login-header">
          <img src="/dormitory-hub-logo.svg" alt="Dormitory Hub" className="login-logo" />
          <span className="login-kicker">Dormitory Hub</span>
          <h2>{mode === 'login' ? 'Đăng nhập' : 'Đăng ký sinh viên'}</h2>
          <p>
            {mode === 'login'
              ? 'Hệ thống quản lý nội trú thông minh Dormitory Hub.'
              : 'Sinh viên dùng mã sinh viên và email đã có trong hồ sơ để tự tạo tài khoản.'}
          </p>
        </div>

        <div className="auth-tabs">
          <button type="button" className={mode === 'login' ? 'active' : ''} onClick={() => { setMode('login'); setError(''); setNotice('') }}>
            Đăng nhập
          </button>
          <button type="button" className={mode === 'register' ? 'active' : ''} onClick={() => { setMode('register'); setError(''); setNotice('') }}>
            Tạo tài khoản sinh viên
          </button>
        </div>

        {error && <div className="feedback error">{error}</div>}
        {notice && <div className="feedback success">{notice}</div>}

        {mode === 'login' ? (
          <form onSubmit={handleSubmit} className="login-form">
            <div className="form-group">
              <label>Tài khoản</label>
              <input
                type="text"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
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
                onChange={(e) => setPassword(e.target.value)}
                disabled={loading}
                placeholder="Nhập mật khẩu"
              />
            </div>

            <button type="submit" disabled={loading} className="primary-button login-button">
              {loading ? 'Đang xác thực...' : 'Đăng nhập'}
            </button>
          </form>
        ) : (
          <form onSubmit={handleStudentRegister} className="login-form">
            <div className="form-group">
              <label>Mã sinh viên</label>
              <input
                value={registerForm.studentCode}
                onChange={(e) => updateRegisterField('studentCode', e.target.value)}
                disabled={loading}
                placeholder="Ví dụ: SV001"
                autoFocus
              />
            </div>

            <div className="form-group">
              <label>Email trong hồ sơ</label>
              <input
                type="email"
                value={registerForm.email}
                onChange={(e) => updateRegisterField('email', e.target.value)}
                disabled={loading}
                placeholder="email@sinhvien.edu.vn"
              />
            </div>

            <div className="form-group">
              <label>Tài khoản muốn tạo</label>
              <input
                value={registerForm.username}
                onChange={(e) => updateRegisterField('username', e.target.value)}
                disabled={loading}
                placeholder="Tên đăng nhập"
              />
            </div>

            <div className="form-grid compact-auth-grid">
              <div className="form-group">
                <label>Mật khẩu</label>
                <input
                  type="password"
                  value={registerForm.password}
                  onChange={(e) => updateRegisterField('password', e.target.value)}
                  disabled={loading}
                  placeholder="Mật khẩu"
                />
              </div>
              <div className="form-group">
                <label>Xác nhận</label>
                <input
                  type="password"
                  value={registerForm.confirmPassword}
                  onChange={(e) => updateRegisterField('confirmPassword', e.target.value)}
                  disabled={loading}
                  placeholder="Nhập lại mật khẩu"
                />
              </div>
            </div>

            <button type="submit" disabled={loading} className="primary-button login-button">
              {loading ? 'Đang tạo tài khoản...' : 'Tạo tài khoản sinh viên'}
            </button>
          </form>
        )}
      </div>
    </div>
  )
}
