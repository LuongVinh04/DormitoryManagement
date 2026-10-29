import { useMemo, useState } from 'react'
import { CrudPanel } from '../../components'

const EMPTY_ROOM_FILTERS = {
  buildingName: '',
  roomCategoryName: '',
  roomZoneName: '',
  status: '',
  floorNumber: '',
  availability: '',
}

function normalizeSearchText(value) {
  return String(value ?? '')
    .trim()
    .toLowerCase()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
}

function uniqueOptions(items, selector) {
  return Array.from(new Set(items.map(selector).filter((value) => value !== null && value !== undefined && value !== '')))
    .sort((a, b) => String(a).localeCompare(String(b), 'vi', { numeric: true }))
}

export function FacilitiesSection({ data, openCreate, openEdit, deleteEntity, getPanelProps }) {
  const [roomKeyword, setRoomKeyword] = useState('')
  const [roomFilters, setRoomFilters] = useState(EMPTY_ROOM_FILTERS)

  const roomFilterOptions = useMemo(() => ({
    buildings: uniqueOptions(data.rooms, (room) => room.buildingName),
    categories: uniqueOptions(data.rooms, (room) => room.roomCategoryName),
    zones: uniqueOptions(data.rooms, (room) => room.roomZoneName),
    statuses: uniqueOptions(data.rooms, (room) => room.status),
    floors: uniqueOptions(data.rooms, (room) => room.floorNumber),
  }), [data.rooms])

  const filteredRooms = useMemo(() => {
    const normalizedKeyword = normalizeSearchText(roomKeyword)
    const searchableFields = ['roomNumber', 'physicalRoomNumber', 'buildingName', 'roomCategoryName', 'roomZoneName', 'status']

    return data.rooms
      .filter((room) => {
        if (!normalizedKeyword) return true
        return searchableFields.some((field) => normalizeSearchText(room[field]).includes(normalizedKeyword))
      })
      .filter((room) => {
        if (roomFilters.buildingName && room.buildingName !== roomFilters.buildingName) return false
        if (roomFilters.roomCategoryName && room.roomCategoryName !== roomFilters.roomCategoryName) return false
        if (roomFilters.roomZoneName && room.roomZoneName !== roomFilters.roomZoneName) return false
        if (roomFilters.status && room.status !== roomFilters.status) return false
        if (roomFilters.floorNumber && String(room.floorNumber ?? '') !== roomFilters.floorNumber) return false
        if (roomFilters.availability === 'available' && Number(room.availableSlots ?? 0) <= 0) return false
        if (roomFilters.availability === 'full' && Number(room.availableSlots ?? 0) > 0) return false
        return true
      })
  }, [data.rooms, roomFilters, roomKeyword])

  const activeRoomFilterLabels = useMemo(() => {
    const labels = []
    if (roomKeyword.trim()) labels.push(`Từ khóa: ${roomKeyword.trim()}`)
    if (roomFilters.buildingName) labels.push(`Tòa: ${roomFilters.buildingName}`)
    if (roomFilters.roomCategoryName) labels.push(`Loại phòng: ${roomFilters.roomCategoryName}`)
    if (roomFilters.roomZoneName) labels.push(`Phân khu: ${roomFilters.roomZoneName}`)
    if (roomFilters.status) labels.push(`Trạng thái: ${roomFilters.status}`)
    if (roomFilters.floorNumber) labels.push(`Tầng: ${roomFilters.floorNumber}`)
    if (roomFilters.availability) labels.push(`Chỗ trống: ${roomFilters.availability === 'available' ? 'Còn chỗ' : 'Hết chỗ'}`)
    return labels
  }, [roomFilters, roomKeyword])

  const hasRoomFilters = roomKeyword.trim() || Object.values(roomFilters).some(Boolean)

  function updateRoomFilter(name, value) {
    setRoomFilters((current) => ({ ...current, [name]: value }))
  }

  function clearRoomFilters() {
    setRoomKeyword('')
    setRoomFilters(EMPTY_ROOM_FILTERS)
  }

  return (
    <>
      <CrudPanel entityKey="buildings" rows={data.buildings} onCreate={openCreate} onEdit={openEdit} onDelete={deleteEntity} panelProps={getPanelProps('facilities-buildings')} />

      <div className="filter-search-panel room-search-panel">
        <div className="filter-search-row">
          <label className="field filter-search-box">
            <span>Tìm kiếm phòng</span>
            <input
              className="search-input"
              value={roomKeyword}
              onChange={(event) => setRoomKeyword(event.target.value)}
              placeholder="Tìm số phòng, tòa nhà, phân khu, loại phòng..."
            />
          </label>
          <button className="secondary-button" type="button" onClick={clearRoomFilters} disabled={!hasRoomFilters}>
            Xóa lọc
          </button>
        </div>

        <div className="advanced-filter-grid room-filter-grid">
          <label className="field">
            <span>Tòa nhà</span>
            <select value={roomFilters.buildingName} onChange={(event) => updateRoomFilter('buildingName', event.target.value)}>
              <option value="">Tất cả</option>
              {roomFilterOptions.buildings.map((building) => <option key={building} value={building}>{building}</option>)}
            </select>
          </label>
          <label className="field">
            <span>Loại phòng</span>
            <select value={roomFilters.roomCategoryName} onChange={(event) => updateRoomFilter('roomCategoryName', event.target.value)}>
              <option value="">Tất cả</option>
              {roomFilterOptions.categories.map((category) => <option key={category} value={category}>{category}</option>)}
            </select>
          </label>
          <label className="field">
            <span>Phân khu</span>
            <select value={roomFilters.roomZoneName} onChange={(event) => updateRoomFilter('roomZoneName', event.target.value)}>
              <option value="">Tất cả</option>
              {roomFilterOptions.zones.map((zone) => <option key={zone} value={zone}>{zone}</option>)}
            </select>
          </label>
          <label className="field">
            <span>Trạng thái</span>
            <select value={roomFilters.status} onChange={(event) => updateRoomFilter('status', event.target.value)}>
              <option value="">Tất cả</option>
              {roomFilterOptions.statuses.map((status) => <option key={status} value={status}>{status}</option>)}
            </select>
          </label>
          <label className="field">
            <span>Tầng</span>
            <select value={roomFilters.floorNumber} onChange={(event) => updateRoomFilter('floorNumber', event.target.value)}>
              <option value="">Tất cả</option>
              {roomFilterOptions.floors.map((floor) => <option key={floor} value={floor}>{floor}</option>)}
            </select>
          </label>
          <label className="field">
            <span>Chỗ trống</span>
            <select value={roomFilters.availability} onChange={(event) => updateRoomFilter('availability', event.target.value)}>
              <option value="">Tất cả</option>
              <option value="available">Còn chỗ</option>
              <option value="full">Hết chỗ</option>
            </select>
          </label>
        </div>

        <div className="filter-search-footer">
          <span className="toolbar-meta">Hiển thị {filteredRooms.length}/{data.rooms.length} phòng</span>
          {activeRoomFilterLabels.length ? (
            <div className="filter-chips">
              {activeRoomFilterLabels.map((label) => <span key={label}>{label}</span>)}
            </div>
          ) : null}
        </div>
      </div>

      <CrudPanel entityKey="rooms" rows={filteredRooms} onCreate={openCreate} onEdit={openEdit} onDelete={deleteEntity} panelProps={getPanelProps('facilities-rooms')} />
    </>
  )
}
