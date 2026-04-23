import { createContext, useContext, useEffect, useState } from 'react'
import { apiFetch, readError } from '../helpers'

const AuthContext = createContext(null)

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null)
  const [permissions, setPermissions] = useState([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    loadUser()
  }, [])

  async function loadUser() {
    setLoading(true)
    const token = localStorage.getItem('dormitory_token')
    if (!token) {
      setLoading(false)
      return
    }

    try {
      const response = await apiFetch('/api/auth/me')
      if (response.ok) {
        const data = await response.json()
        setUser(data.user)
        setPermissions(data.permissions || [])
      } else {
        localStorage.removeItem('dormitory_token')
      }
    } catch {
      localStorage.removeItem('dormitory_token')
    } finally {
      setLoading(false)
    }
  }

  async function login(username, password) {
    const response = await apiFetch('/api/auth/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, password })
    })

    if (!response.ok) {
      throw new Error(await readError(response))
    }

    const data = await response.json()
    localStorage.setItem('dormitory_token', data.token)
    setUser(data.user)
    setPermissions(data.permissions || [])
  }

  function logout() {
    localStorage.removeItem('dormitory_token')
    setUser(null)
    setPermissions([])
    window.location.href = '/login'
  }

  function hasPermission(code) {
    if (!user) return false
    if (user.username === 'admin' || permissions.includes('permissions.manage')) return true // simplified super admin logic
    return permissions.includes(code)
  }

  function hasAnyPermission(codes) {
    if (!user) return false
    if (user.username === 'admin' || permissions.includes('permissions.manage')) return true
    return codes.some(code => permissions.includes(code))
  }

  return (
    <AuthContext.Provider value={{ user, permissions, loading, login, logout, hasPermission, hasAnyPermission }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return context
}
