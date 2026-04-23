import { useState, useEffect, useMemo } from 'react'
import { apiFetch, readError } from '../../helpers'
import './permission-manager.css'

import { PermissionToolbar } from './PermissionToolbar'
import { PermissionModuleCard } from './PermissionModuleCard'
import { PermissionRow } from './PermissionRow'
import { PermissionSummaryBar } from './PermissionSummaryBar'

const PERMISSION_MODULE_META = {
  Dashboard: { label: 'Dashboard', description: 'Xem tổng thể và dữ liệu thống kê.' },
  'Cơ sở vật chất': { label: 'Cơ sở vật chất', description: 'Quản lý tòa nhà, cấu trúc phòng và tài sản.' },
  'Điều phối': { label: 'Điều phối', description: 'Phân bổ, chuyển đổi và xử lý phòng.' },
  'Sinh viên': { label: 'Sinh viên', description: 'Quản lý thông tin lưu trú của sinh viên.' },
  'Tài chính': { label: 'Tài chính', description: 'Thu phí, công nợ, định mức điện nước và hóa đơn.' },
  'Quản trị': { label: 'Quản trị', description: 'Tài khoản, phân quyền, sao lưu hệ thống.' },
  'Danh mục': { label: 'Danh mục', description: 'Cấu hình bảng biểu và tham số chung.' },
}

export function PermissionManager({ users = [] }) {
  const [permissions, setPermissions] = useState([])
  const [selectedUserId, setSelectedUserId] = useState('')
  
  const [rolePermissions, setRolePermissions] = useState([])
  const [grantedOverrides, setGrantedOverrides] = useState([])
  const [deniedOverrides, setDeniedOverrides] = useState([])
  
  const [originalGranted, setOriginalGranted] = useState([])
  const [originalDenied, setOriginalDenied] = useState([])

  const [loading, setLoading] = useState(false)
  const [saving, setSaving] = useState(false)
  
  const [permissionQuery, setPermissionQuery] = useState('')
  const [filterMode, setFilterMode] = useState('all')
  
  const [collapsedModules, setCollapsedModules] = useState(() => {
    try {
      return JSON.parse(localStorage.getItem('permission-collapsed')) || {}
    } catch { return {} }
  })

  const selectedUser = users.find((u) => String(u.id) === String(selectedUserId))

  useEffect(() => {
    loadPermissions()
  }, [])
  
  useEffect(() => {
    localStorage.setItem('permission-collapsed', JSON.stringify(collapsedModules))
  }, [collapsedModules])

  useEffect(() => {
    if (selectedUser) {
      loadUserPermissions(selectedUser.id, selectedUser.roleId)
    } else {
      setRolePermissions([])
      setGrantedOverrides([])
      setDeniedOverrides([])
      setOriginalGranted([])
      setOriginalDenied([])
    }
  }, [selectedUser])

  async function loadPermissions() {
    try {
      const res = await apiFetch('/api/catalog/permissions')
      if (res.ok) setPermissions(await res.json())
    } catch {
      // silent
    }
  }

  async function loadUserPermissions(userId, roleId) {
    try {
      setLoading(true)
      const [roleRes, userRes] = await Promise.all([
        apiFetch(`/api/people/roles/${roleId}/permissions`),
        apiFetch(`/api/people/users/${userId}/permissions`)
      ])

      if (roleRes.ok) setRolePermissions(await roleRes.json())
      if (userRes.ok) {
        const udata = await userRes.json()
        setGrantedOverrides(udata.allowedPermissionIds || [])
        setDeniedOverrides(udata.deniedPermissionIds || [])
        setOriginalGranted(udata.allowedPermissionIds || [])
        setOriginalDenied(udata.deniedPermissionIds || [])
      }
    } catch {
      // silent
    } finally {
      setLoading(false)
    }
  }

  const getPermissionState = (permId) => {
    const isRoleGranted = rolePermissions.includes(permId)
    const isGrantedOverride = grantedOverrides.includes(permId)
    const isDeniedOverride = deniedOverrides.includes(permId)
    
    let source = 'none'
    let isChecked = false
    
    if (isGrantedOverride) {
      source = 'grantedOverride'
      isChecked = true
    } else if (isDeniedOverride) {
      source = 'deniedOverride'
      isChecked = false
    } else if (isRoleGranted) {
      source = 'role'
      isChecked = true
    }
    
    return { isChecked, source }
  }

  const togglePermission = (permId) => {
    const state = getPermissionState(permId)
    const isRoleGranted = rolePermissions.includes(permId)
    
    let nextGranted = [...grantedOverrides]
    let nextDenied = [...deniedOverrides]
    
    if (state.isChecked) {
      if (isRoleGranted) {
        nextDenied.push(permId)
      } else {
        nextGranted = nextGranted.filter(id => id !== permId)
      }
    } else {
      if (isRoleGranted) {
        nextDenied = nextDenied.filter(id => id !== permId)
      } else {
        nextGranted.push(permId)
      }
    }
    
    setGrantedOverrides(nextGranted)
    setDeniedOverrides(nextDenied)
  }

  const toggleModulePermissions = (modulePerms, checkAll) => {
    let nextGranted = [...grantedOverrides]
    let nextDenied = [...deniedOverrides]
    
    modulePerms.forEach(p => {
      const isRoleGranted = rolePermissions.includes(p.id)
      if (checkAll) {
        if (isRoleGranted) {
          nextDenied = nextDenied.filter(id => id !== p.id)
        } else if (!nextGranted.includes(p.id)) {
          nextGranted.push(p.id)
        }
      } else {
        if (isRoleGranted && !nextDenied.includes(p.id)) {
          nextDenied.push(p.id)
        } else {
          nextGranted = nextGranted.filter(id => id !== p.id)
        }
      }
    })
    
    setGrantedOverrides(nextGranted)
    setDeniedOverrides(nextDenied)
  }

  const onReset = () => {
    setGrantedOverrides(originalGranted)
    setDeniedOverrides(originalDenied)
  }

  const savePermissions = async () => {
    if (!selectedUserId) return
    try {
      setSaving(true)
      const res = await apiFetch(`/api/people/users/${selectedUserId}/permissions`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          allowedPermissionIds: grantedOverrides,
          deniedPermissionIds: deniedOverrides
        })
      })

      if (!res.ok) throw new Error(await readError(res))
      setOriginalGranted(grantedOverrides)
      setOriginalDenied(deniedOverrides)
      
      const evt = new CustomEvent('toast', { detail: { notice: 'Lưu cấu hình quyền thành công.' } })
      window.dispatchEvent(evt)
    } catch (err) {
      const evt = new CustomEvent('toast', { detail: { error: err.message } })
      window.dispatchEvent(evt)
    } finally {
      setSaving(false)
    }
  }

  const addedDiff = grantedOverrides.filter(x => !originalGranted.includes(x)).length + originalGranted.filter(x => !grantedOverrides.includes(x)).length
  const removedDiff = deniedOverrides.filter(x => !originalDenied.includes(x)).length + originalDenied.filter(x => !deniedOverrides.includes(x)).length
  const totalChanges = addedDiff + removedDiff

  const groupedAndFiltered = useMemo(() => {
    const query = permissionQuery.trim().toLowerCase()
    const groups = {}

    permissions.forEach(p => {
      const state = getPermissionState(p.id)
      if (query && !p.name.toLowerCase().includes(query) && !p.id.toLowerCase().includes(query)) return
      if (filterMode === 'enabled' && !state.isChecked) return
      if (filterMode === 'granted' && state.source !== 'grantedOverride') return
      if (filterMode === 'denied' && state.source !== 'deniedOverride') return

      if (!groups[p.module]) groups[p.module] = []
      groups[p.module].push({ ...p, state })
    })
    return groups
  }, [permissions, rolePermissions, grantedOverrides, deniedOverrides, permissionQuery, filterMode])

  const sortedModules = Object.keys(groupedAndFiltered).sort()

  return (
    <div className="permission-manager-shell">
      <div className="permission-header">
        <h3>Phân quyền tài khoản bán tập trung</h3>
        <p>Cấp quyền linh hoạt theo từng tài khoản, kế thừa từ vai trò gốc và cho phép override theo nhu cầu thực tế.</p>
      </div>

      <PermissionToolbar
        users={users}
        selectedUserId={selectedUserId}
        setSelectedUserId={setSelectedUserId}
        permissionQuery={permissionQuery}
        setPermissionQuery={setPermissionQuery}
        filterMode={filterMode}
        setFilterMode={setFilterMode}
      />

      {loading && <div className="page-loading">Đang tải cấu hình quyền...</div>}

      {!loading && selectedUserId && (
        <div className="permission-module-grid">
          {sortedModules.map(moduleName => {
            const modulePerms = groupedAndFiltered[moduleName]
            const enabledCount = modulePerms.filter(p => p.state.isChecked).length
            const stats = {
              total: modulePerms.length,
              enabled: enabledCount,
              allChecked: enabledCount === modulePerms.length && modulePerms.length > 0,
              partiallyChecked: enabledCount > 0 && enabledCount < modulePerms.length
            }

            return (
              <PermissionModuleCard
                key={moduleName}
                moduleName={moduleName}
                meta={PERMISSION_MODULE_META[moduleName]}
                stats={stats}
                isCollapsed={collapsedModules[moduleName]}
                onToggleCollapse={() => setCollapsedModules(m => ({ ...m, [moduleName]: !m[moduleName] }))}
                onToggleAll={(checked) => toggleModulePermissions(modulePerms, checked)}
              >
                {modulePerms.map(p => (
                  <PermissionRow
                    key={p.id}
                    permission={p}
                    state={p.state}
                    onToggle={togglePermission}
                  />
                ))}
              </PermissionModuleCard>
            )
          })}
        </div>
      )}

      {totalChanges > 0 && (
        <PermissionSummaryBar
          addedCount={addedDiff}
          removedCount={removedDiff}
          saving={saving}
          savePermissions={savePermissions}
          onReset={onReset}
        />
      )}
    </div>
  )
}
