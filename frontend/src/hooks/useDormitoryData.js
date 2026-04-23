import { useEffect, useState } from 'react'
import { API_ENDPOINTS, ENTITY_CONFIGS } from '../constants'
import { formatDateInput, normalizePayload, readError, validatePayload } from '../helpers'

const INITIAL_DATA = {
  roomCategories: [],
  roomZones: [],
  paymentMethods: [],
  buildings: [],
  rooms: [],
  students: [],
  registrations: [],
  contracts: [],
  utilities: [],
  roomFeeProfiles: [],
  roomFinances: [],
  invoices: [],
  roles: [],
  users: [],
}

const INITIAL_ROOM_ACTIONS = {
  assignStudentId: '',
  transferStudentId: '',
  transferToRoomId: '',
  removeStudentId: '',
}

export function useDormitoryData() {
  const [dashboard, setDashboard] = useState(null)
  const [financeSummary, setFinanceSummary] = useState(null)
  const [lookups, setLookups] = useState({ buildings: [], rooms: [], students: [], roles: [], utilities: [], roomCategories: [], roomZones: [], paymentMethods: [] })
  const [data, setData] = useState(INITIAL_DATA)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [notice, setNotice] = useState('')
  const [modal, setModal] = useState(null)
  const [formErrors, setFormErrors] = useState({})
  const [selectedRoomId, setSelectedRoomId] = useState('')
  const [roomOverview, setRoomOverview] = useState(null)
  const [roomActions, setRoomActions] = useState(INITIAL_ROOM_ACTIONS)

  const activeRoomId = selectedRoomId || (data.rooms[0] ? String(data.rooms[0].id) : '')

  useEffect(() => {
    loadAll({ showLoading: true })
  }, [])

  useEffect(() => {
    if (!error && !notice) return
    const timeout = window.setTimeout(() => {
      setError('')
      setNotice('')
    }, 4200)

    return () => window.clearTimeout(timeout)
  }, [error, notice])

  useEffect(() => {
    if (activeRoomId) {
      loadRoomOverview(activeRoomId)
    }
  }, [activeRoomId])

  async function loadAll({ showLoading = false } = {}) {
    try {
      if (showLoading) {
        setLoading(true)
      }
      setError('')
      if (showLoading) {
        setNotice('')
      }

      const [dashboardRes, financeSummaryRes, lookupRes, ...entityResponses] = await Promise.all([
        fetch('/api/dashboard'),
        fetch('/api/operations/room-finances/summary'),
        fetch('/api/lookups'),
        ...Object.values(API_ENDPOINTS).map((url) => fetch(url)),
      ])

      if (!dashboardRes.ok || !financeSummaryRes.ok || !lookupRes.ok || entityResponses.some((res) => !res.ok)) {
        throw new Error('Không thể tải dữ liệu hệ thống.')
      }

      const [dashboardJson, financeSummaryJson, lookupJson, ...entityJson] = await Promise.all([
        dashboardRes.json(),
        financeSummaryRes.json(),
        lookupRes.json(),
        ...entityResponses.map((res) => res.json()),
      ])

      const nextData = {}
      Object.keys(API_ENDPOINTS).forEach((key, index) => {
        nextData[key] = entityJson[index]
      })

      setDashboard(dashboardJson)
      setFinanceSummary(financeSummaryJson)
      setLookups(lookupJson)
      setData(nextData)
    } catch (loadError) {
      setError(loadError.message)
    } finally {
      if (showLoading) {
        setLoading(false)
      }
    }
  }

  async function loadRoomOverview(roomId) {
    try {
      const response = await fetch(`/api/facilities/rooms/${roomId}/overview`)
      if (!response.ok) {
        throw new Error('Không thể tải chi tiết phòng.')
      }

      setRoomOverview(await response.json())
    } catch (roomError) {
      setError(roomError.message)
    }
  }

  async function refreshData() {
    await loadAll()
    if (activeRoomId) {
      await loadRoomOverview(activeRoomId)
    }
  }

  function openCreate(entityKey) {
    const defaults = {}
    ENTITY_CONFIGS[entityKey].fields.forEach((field) => {
      if (field.type === 'checkbox') defaults[field.name] = true
      else if (field.type === 'select') defaults[field.name] = field.options[0]?.value ?? ''
      else defaults[field.name] = ''
    })

    setFormErrors({})
    setModal({ entityKey, mode: 'create', id: null, values: defaults })
  }

  function openEdit(entityKey, item) {
    const values = {}
    ENTITY_CONFIGS[entityKey].fields.forEach((field) => {
      const rawValue = item[field.name]
      values[field.name] = field.type === 'date' ? (rawValue ? formatDateInput(rawValue) : '') : (rawValue ?? (field.type === 'checkbox' ? false : ''))
    })

    setFormErrors({})
    setModal({ entityKey, mode: 'edit', id: item.id, values })
  }

  function updateModalField(name, value) {
    setModal((current) => {
      const nextValues = { ...current.values, [name]: value }

      if (current.entityKey === 'rooms' && name === 'roomCategoryId') {
        const category = lookups.roomCategories.find((item) => String(item.id) === String(value))
        if (category) {
          nextValues.capacity = category.defaultCapacity
          nextValues.pricePerMonth = category.baseMonthlyFee
        }
      }

      if (current.entityKey === 'rooms' && name === 'roomZoneId') {
        const zone = lookups.roomZones.find((item) => String(item.id) === String(value))
        if (zone?.buildingId) {
          nextValues.buildingId = zone.buildingId
        }
      }

      return { ...current, values: nextValues }
    })
    setFormErrors((current) => {
      if (!current[name]) return current
      const nextErrors = { ...current }
      delete nextErrors[name]
      return nextErrors
    })
  }

  async function saveEntity() {
    if (!modal) return

    const { entityKey, mode, id, values } = modal
    const endpoint = mode === 'create' ? API_ENDPOINTS[entityKey] : `${API_ENDPOINTS[entityKey]}/${id}`
    const nextFormErrors = validatePayload(ENTITY_CONFIGS[entityKey].fields, values)

    if (Object.keys(nextFormErrors).length > 0) {
      setFormErrors(nextFormErrors)
      setError('Vui lòng kiểm tra lại các trường đang nhập.')
      return
    }

    try {
      setSaving(true)
      setError('')
      setNotice('')

      const response = await fetch(endpoint, {
        method: mode === 'create' ? 'POST' : 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(normalizePayload(ENTITY_CONFIGS[entityKey].fields, values)),
      })

      if (!response.ok) {
        throw new Error(await readError(response))
      }

      setNotice(mode === 'create' ? 'Đã thêm dữ liệu mới.' : 'Đã cập nhật dữ liệu.')
      setModal(null)
      setFormErrors({})
      await refreshData()
    } catch (saveError) {
      setError(saveError.message)
    } finally {
      setSaving(false)
    }
  }

  async function deleteEntity(entityKey, item) {
    const label = item.name ?? item.roomNumber ?? item.studentCode ?? item.invoiceCode ?? item.username ?? 'bản ghi'
    if (!window.confirm(`Xóa ${label}?`)) return

    try {
      setSaving(true)
      setError('')
      setNotice('')
      const response = await fetch(`${API_ENDPOINTS[entityKey]}/${item.id}`, { method: 'DELETE' })
      if (!response.ok) {
        throw new Error(await readError(response))
      }

      setNotice('Đã xóa bản ghi.')
      await refreshData()
    } catch (deleteError) {
      setError(deleteError.message)
    } finally {
      setSaving(false)
    }
  }

  async function executeAction(factory, message) {
    try {
      setSaving(true)
      setError('')
      setNotice('')
      const response = await factory()
      if (!response.ok) {
        throw new Error(await readError(response))
      }

      setNotice(message)
      await refreshData()
    } catch (actionError) {
      setError(actionError.message)
    } finally {
      setSaving(false)
    }
  }

  return {
    activeRoomId,
    dashboard,
    data,
    deleteEntity,
    error,
    executeAction,
    financeSummary,
    formErrors,
    loading,
    lookups,
    modal,
    notice,
    openCreate,
    openEdit,
    refreshData,
    roomActions,
    roomOverview,
    saveEntity,
    saving,
    selectedRoomId,
    setModal,
    setRoomActions,
    setSelectedRoomId,
    updateModalField,
  }
}
