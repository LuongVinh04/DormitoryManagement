import { useMemo, useState } from 'react'
import { CrudPanel, ModalCard } from '../../components'
import { apiFetch, downloadBlob, openBlobInNewTab, readError } from '../../helpers'

const STUDENT_FILTERS = [
  { key: 'all', label: 'Tất cả', match: () => true },
  { key: 'assignable', label: 'Đủ điều kiện xếp phòng', match: (student) => student.canAssignRoom },
  { key: 'noContract', label: 'Chưa có hợp đồng', match: (student) => student.contractState === 'NoContract' },
  { key: 'expiredGrace', label: 'Hết hạn đang ẩn phòng', match: (student) => student.contractState === 'ExpiredGrace' },
  { key: 'expired', label: 'Hết hạn/hủy', match: (student) => ['Expired', 'Cancelled'].includes(student.contractState) },
  { key: 'missingAccount', label: 'Chưa có tài khoản', match: (student) => !student.hasAccount },
]

const EMPTY_STUDENT_FILTERS = {
  status: '',
  gender: '',
  faculty: '',
  className: '',
  buildingName: '',
  roomId: '',
  hasAccount: '',
  contractState: '',
}

const CONTRACT_STATE_OPTIONS = [
  { value: 'NoContract', label: 'Chưa có hợp đồng' },
  { value: 'Active', label: 'Đang hiệu lực' },
  { value: 'ExpiringSoon', label: 'Sắp hết hạn' },
  { value: 'ExpiredGrace', label: 'Hết hạn đang ẩn phòng' },
  { value: 'Expired', label: 'Đã hết hạn' },
  { value: 'Cancelled', label: 'Đã hủy' },
]

function downloadExcel(url, filename) {
  apiFetch(url).then(res => res.blob()).then(blob => {
    const a = document.createElement('a')
    a.href = URL.createObjectURL(blob)
    a.download = filename
    a.click()
    URL.revokeObjectURL(a.href)
  })
}

function buildDefaultUsername(student) {
  return String(student?.studentCode || '').trim().toLowerCase()
}

function normalizeSearchText(value) {
  return String(value ?? '')
    .trim()
    .toLowerCase()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
}

function uniqueOptions(items, selector) {
  return Array.from(new Set(items.map(selector).filter(Boolean)))
    .sort((a, b) => String(a).localeCompare(String(b), 'vi'))
}

export function StudentsSection({ data, openCreate, openEdit, deleteEntity, getPanelProps, refreshData }) {
  const [filter, setFilter] = useState('all')
  const [keyword, setKeyword] = useState('')
  const [advancedFilters, setAdvancedFilters] = useState(EMPTY_STUDENT_FILTERS)
  const [accountStudent, setAccountStudent] = useState(null)
  const [accountForm, setAccountForm] = useState({ username: '', email: '', password: '', isActive: true })
  const [accountError, setAccountError] = useState('')
  const [accountNotice, setAccountNotice] = useState('')
  const [savingAccount, setSavingAccount] = useState(false)
  const [contractFile, setContractFile] = useState(null)
  const [selectedPdf, setSelectedPdf] = useState(null)
  const [contractFileError, setContractFileError] = useState('')
  const [contractFileNotice, setContractFileNotice] = useState('')
  const [uploadingContract, setUploadingContract] = useState(false)

  const studentFilterOptions = useMemo(() => ({
    statuses: uniqueOptions(data.students, (student) => student.status),
    genders: uniqueOptions(data.students, (student) => student.gender),
    faculties: uniqueOptions(data.students, (student) => student.faculty),
    classes: uniqueOptions(data.students, (student) => student.className),
    buildings: uniqueOptions(data.students, (student) => student.buildingName),
    rooms: data.rooms
      .map((room) => ({
        value: String(room.id),
        label: [room.roomNumber || room.physicalRoomNumber, room.buildingName].filter(Boolean).join(' - '),
      }))
      .filter((room) => room.value && room.label)
      .sort((a, b) => a.label.localeCompare(b.label, 'vi')),
  }), [data.rooms, data.students])

  const filteredStudents = useMemo(() => {
    const activeFilter = STUDENT_FILTERS.find((item) => item.key === filter) ?? STUDENT_FILTERS[0]
    const normalizedKeyword = normalizeSearchText(keyword)
    const searchableFields = ['studentCode', 'name', 'phone', 'email', 'faculty', 'className', 'accountUsername']

    return data.students
      .filter(activeFilter.match)
      .filter((student) => {
        if (!normalizedKeyword) return true
        return searchableFields.some((field) => normalizeSearchText(student[field]).includes(normalizedKeyword))
      })
      .filter((student) => {
        if (advancedFilters.status && student.status !== advancedFilters.status) return false
        if (advancedFilters.gender && student.gender !== advancedFilters.gender) return false
        if (advancedFilters.faculty && student.faculty !== advancedFilters.faculty) return false
        if (advancedFilters.className && student.className !== advancedFilters.className) return false
        if (advancedFilters.buildingName && student.buildingName !== advancedFilters.buildingName) return false
        if (advancedFilters.roomId && String(student.roomId ?? '') !== advancedFilters.roomId) return false
        if (advancedFilters.contractState && student.contractState !== advancedFilters.contractState) return false
        if (advancedFilters.hasAccount === 'yes' && !student.hasAccount) return false
        if (advancedFilters.hasAccount === 'no' && student.hasAccount) return false
        return true
      })
  }, [advancedFilters, data.students, filter, keyword])

  const activeFilterLabels = useMemo(() => {
    const labels = []
    if (keyword.trim()) labels.push(`Từ khóa: ${keyword.trim()}`)
    if (filter !== 'all') {
      labels.push(STUDENT_FILTERS.find((item) => item.key === filter)?.label)
    }
    if (advancedFilters.status) labels.push(`Trạng thái: ${advancedFilters.status}`)
    if (advancedFilters.gender) labels.push(`Giới tính: ${advancedFilters.gender}`)
    if (advancedFilters.faculty) labels.push(`Khoa: ${advancedFilters.faculty}`)
    if (advancedFilters.className) labels.push(`Lớp: ${advancedFilters.className}`)
    if (advancedFilters.buildingName) labels.push(`Tòa: ${advancedFilters.buildingName}`)
    if (advancedFilters.roomId) {
      labels.push(`Phòng: ${studentFilterOptions.rooms.find((room) => room.value === advancedFilters.roomId)?.label ?? advancedFilters.roomId}`)
    }
    if (advancedFilters.hasAccount) labels.push(`Tài khoản: ${advancedFilters.hasAccount === 'yes' ? 'Đã có' : 'Chưa có'}`)
    if (advancedFilters.contractState) {
      labels.push(`Hợp đồng: ${CONTRACT_STATE_OPTIONS.find((item) => item.value === advancedFilters.contractState)?.label ?? advancedFilters.contractState}`)
    }
    return labels.filter(Boolean)
  }, [advancedFilters, filter, keyword, studentFilterOptions.rooms])

  const hasStudentFilters = keyword.trim() || filter !== 'all' || Object.values(advancedFilters).some(Boolean)

  const contractRows = useMemo(() => data.contracts.map((contract) => ({
    ...contract,
    expiryLabel: buildExpiryLabel(contract),
    contractFileLabel: contract.hasPdf ? (contract.pdfFileName || 'Đã upload') : 'Chưa có',
  })), [data.contracts])

  function openAccountModal(student) {
    setAccountStudent(student)
    setAccountForm({
      username: student.accountUsername || buildDefaultUsername(student),
      email: student.accountEmail || student.email || '',
      password: '',
      isActive: student.accountIsActive ?? true,
    })
    setAccountError('')
    setAccountNotice('')
  }

  function updateAdvancedFilter(name, value) {
    setAdvancedFilters((current) => ({ ...current, [name]: value }))
  }

  function clearStudentFilters() {
    setKeyword('')
    setFilter('all')
    setAdvancedFilters(EMPTY_STUDENT_FILTERS)
  }

  async function createStudentAccount() {
    if (!accountStudent || !accountForm.username || (!accountStudent.hasAccount && !accountForm.password)) {
      setAccountError(accountStudent?.hasAccount
        ? 'Vui lòng nhập tài khoản cho sinh viên.'
        : 'Vui lòng nhập tài khoản và mật khẩu cho sinh viên.')
      return
    }

    try {
      setSavingAccount(true)
      setAccountError('')
      setAccountNotice('')
      const response = await apiFetch(`/api/people/students/${accountStudent.id}/account`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          username: accountForm.username,
          email: accountForm.email,
          password: accountForm.password,
          isActive: accountForm.isActive,
        }),
      })

      if (!response.ok) {
        throw new Error(await readError(response))
      }

      setAccountNotice(`Đã cập nhật tài khoản "${accountForm.username}" cho ${accountStudent.name}.`)
      setAccountForm((current) => ({ ...current, password: '' }))
      await refreshData?.()
    } catch (error) {
      setAccountError(error.message)
    } finally {
      setSavingAccount(false)
    }
  }

  function openContractUpload(contract) {
    setContractFile(contract)
    setSelectedPdf(null)
    setContractFileError('')
    setContractFileNotice('')
  }

  async function uploadContractPdf() {
    if (!contractFile || !selectedPdf) {
      setContractFileError('Vui lòng chọn file PDF hợp đồng.')
      return
    }

    try {
      setUploadingContract(true)
      setContractFileError('')
      setContractFileNotice('')

      const formData = new FormData()
      formData.append('file', selectedPdf)
      const response = await apiFetch(`/api/operations/contracts/${contractFile.id}/pdf`, {
        method: 'POST',
        body: formData,
      })

      if (!response.ok) {
        throw new Error(await readError(response))
      }

      setContractFileNotice(`Đã upload PDF cho hợp đồng ${contractFile.contractCode}.`)
      setSelectedPdf(null)
      await refreshData?.()
    } catch (error) {
      setContractFileError(error.message)
    } finally {
      setUploadingContract(false)
    }
  }

  async function viewContractPdf(contract) {
    try {
      await openBlobInNewTab(`/api/operations/contracts/${contract.id}/pdf/view`, contract.pdfFileName || `${contract.contractCode}.pdf`)
    } catch (error) {
      setContractFileError(error.message)
      setContractFile(contract)
    }
  }

  async function downloadContractPdf(contract) {
    try {
      await downloadBlob(`/api/operations/contracts/${contract.id}/pdf/download`, contract.pdfFileName || `${contract.contractCode}.pdf`)
    } catch (error) {
      setContractFileError(error.message)
      setContractFile(contract)
    }
  }

  return (
    <>
      <div className="student-search-panel">
        <div className="student-search-row">
          <label className="field student-search-box">
            <span>Tìm kiếm sinh viên</span>
            <input
              className="search-input"
              value={keyword}
              onChange={(event) => setKeyword(event.target.value)}
              placeholder="Tìm MSSV, họ tên, SĐT, email, lớp, khoa..."
            />
          </label>
          <button className="secondary-button" type="button" onClick={clearStudentFilters} disabled={!hasStudentFilters}>
            Xóa lọc
          </button>
          <button className="secondary-button" onClick={() => downloadExcel('/api/export/students', 'danh-sach-sinh-vien.xlsx')}>
            Xuất Excel sinh viên
          </button>
        </div>

        <div className="student-advanced-filters">
          <label className="field">
            <span>Trạng thái</span>
            <select value={advancedFilters.status} onChange={(event) => updateAdvancedFilter('status', event.target.value)}>
              <option value="">Tất cả</option>
              {studentFilterOptions.statuses.map((status) => <option key={status} value={status}>{status}</option>)}
            </select>
          </label>
          <label className="field">
            <span>Giới tính</span>
            <select value={advancedFilters.gender} onChange={(event) => updateAdvancedFilter('gender', event.target.value)}>
              <option value="">Tất cả</option>
              {studentFilterOptions.genders.map((gender) => <option key={gender} value={gender}>{gender}</option>)}
            </select>
          </label>
          <label className="field">
            <span>Khoa</span>
            <select value={advancedFilters.faculty} onChange={(event) => updateAdvancedFilter('faculty', event.target.value)}>
              <option value="">Tất cả</option>
              {studentFilterOptions.faculties.map((faculty) => <option key={faculty} value={faculty}>{faculty}</option>)}
            </select>
          </label>
          <label className="field">
            <span>Lớp</span>
            <select value={advancedFilters.className} onChange={(event) => updateAdvancedFilter('className', event.target.value)}>
              <option value="">Tất cả</option>
              {studentFilterOptions.classes.map((className) => <option key={className} value={className}>{className}</option>)}
            </select>
          </label>
          <label className="field">
            <span>Tòa nhà</span>
            <select value={advancedFilters.buildingName} onChange={(event) => updateAdvancedFilter('buildingName', event.target.value)}>
              <option value="">Tất cả</option>
              {studentFilterOptions.buildings.map((building) => <option key={building} value={building}>{building}</option>)}
            </select>
          </label>
          <label className="field">
            <span>Phòng</span>
            <select value={advancedFilters.roomId} onChange={(event) => updateAdvancedFilter('roomId', event.target.value)}>
              <option value="">Tất cả</option>
              {studentFilterOptions.rooms.map((room) => <option key={room.value} value={room.value}>{room.label}</option>)}
            </select>
          </label>
          <label className="field">
            <span>Tài khoản</span>
            <select value={advancedFilters.hasAccount} onChange={(event) => updateAdvancedFilter('hasAccount', event.target.value)}>
              <option value="">Tất cả</option>
              <option value="yes">Đã có tài khoản</option>
              <option value="no">Chưa có tài khoản</option>
            </select>
          </label>
          <label className="field">
            <span>Hợp đồng</span>
            <select value={advancedFilters.contractState} onChange={(event) => updateAdvancedFilter('contractState', event.target.value)}>
              <option value="">Tất cả</option>
              {CONTRACT_STATE_OPTIONS.map((item) => <option key={item.value} value={item.value}>{item.label}</option>)}
            </select>
          </label>
        </div>

        <div className="student-search-footer">
          <span className="toolbar-meta">Hiển thị {filteredStudents.length}/{data.students.length} sinh viên</span>
          {activeFilterLabels.length ? (
            <div className="student-filter-chips">
              {activeFilterLabels.map((label) => <span key={label}>{label}</span>)}
            </div>
          ) : null}
        </div>
      </div>

      <div className="section-toolbar split">
        <div className="student-filter-bar">
          {STUDENT_FILTERS.map((item) => {
            const count = data.students.filter(item.match).length
            return (
              <button
                key={item.key}
                type="button"
                className={filter === item.key ? 'filter-pill active' : 'filter-pill'}
                onClick={() => setFilter(item.key)}
              >
                {item.label}
                <span>{count}</span>
              </button>
            )
          })}
        </div>
      </div>

      <CrudPanel
        entityKey="students"
        rows={filteredStudents}
        onCreate={openCreate}
        onEdit={openEdit}
        onDelete={deleteEntity}
        panelProps={getPanelProps('students-students')}
        extraRowActions={(item) => [
          {
            label: item.hasAccount ? 'Sửa/Cấp lại TK' : 'Cấp tài khoản',
            kind: item.hasAccount ? 'default' : 'approve',
            onClick: () => openAccountModal(item),
          },
        ]}
      />
      <CrudPanel
        entityKey="contracts"
        rows={contractRows}
        onCreate={openCreate}
        onEdit={openEdit}
        onDelete={deleteEntity}
        panelProps={getPanelProps('students-contracts')}
        extraRowActions={(item) => [
          {
            label: 'Upload PDF',
            kind: item.hasPdf ? 'default' : 'approve',
            onClick: () => openContractUpload(item),
          },
          item.hasPdf ? {
            label: 'Xem PDF',
            kind: 'default',
            onClick: () => viewContractPdf(item),
          } : null,
          item.hasPdf ? {
            label: 'Tải PDF',
            kind: 'default',
            onClick: () => downloadContractPdf(item),
          } : null,
        ]}
      />

      {accountStudent ? (
        <ModalCard
          title={accountStudent.hasAccount ? 'Sửa/Cấp lại tài khoản sinh viên' : 'Cấp tài khoản sinh viên'}
          subtitle={accountStudent.hasAccount
            ? `Cập nhật tài khoản hoặc nhập mật khẩu mới để cấp lại quyền đăng nhập cho ${accountStudent.name} (${accountStudent.studentCode}).`
            : `Tạo tài khoản đăng nhập cổng sinh viên cho ${accountStudent.name} (${accountStudent.studentCode}).`}
          onClose={() => setAccountStudent(null)}
          footer={(
            <>
              <button className="secondary-button" onClick={() => setAccountStudent(null)}>Đóng</button>
              <button className="primary-button" onClick={createStudentAccount} disabled={savingAccount}>
                {savingAccount ? 'Đang lưu...' : accountStudent.hasAccount ? 'Lưu/Cấp lại' : 'Cấp tài khoản'}
              </button>
            </>
          )}
        >
          <div className="form-grid">
            <div className="field">
              <span>Mã sinh viên</span>
              <input value={accountStudent.studentCode} disabled />
            </div>
            <div className="field">
              <span>Email tài khoản</span>
              <input value={accountForm.email} onChange={(event) => setAccountForm((current) => ({ ...current, email: event.target.value }))} />
            </div>
            <div className="field">
              <span>Tài khoản</span>
              <input value={accountForm.username} onChange={(event) => setAccountForm((current) => ({ ...current, username: event.target.value }))} />
            </div>
            <div className="field">
              <span>{accountStudent.hasAccount ? 'Mật khẩu mới (bỏ trống nếu không đổi)' : 'Mật khẩu ban đầu'}</span>
              <input type="password" autoComplete="new-password" value={accountForm.password} onChange={(event) => setAccountForm((current) => ({ ...current, password: event.target.value }))} />
            </div>
            <label className="checkbox-row field-wide">
              <input type="checkbox" checked={accountForm.isActive} onChange={(event) => setAccountForm((current) => ({ ...current, isActive: event.target.checked }))} />
              <span>Kích hoạt tài khoản sinh viên</span>
            </label>
          </div>
          {accountError ? <div className="feedback error modal-feedback">{accountError}</div> : null}
          {accountNotice ? <div className="feedback success modal-feedback">{accountNotice}</div> : null}
        </ModalCard>
      ) : null}

      {contractFile ? (
        <ModalCard
          title={`PDF hợp đồng ${contractFile.contractCode}`}
          subtitle="Upload file PDF để sinh viên có thể xem online hoặc tải hợp đồng từ cổng sinh viên."
          onClose={() => setContractFile(null)}
          footer={(
            <>
              <button className="secondary-button" onClick={() => setContractFile(null)}>Đóng</button>
              {contractFile.hasPdf ? (
                <>
                  <button className="ghost-button" onClick={() => viewContractPdf(contractFile)}>Xem PDF</button>
                  <button className="ghost-button" onClick={() => downloadContractPdf(contractFile)}>Tải PDF</button>
                </>
              ) : null}
              <button className="primary-button" onClick={uploadContractPdf} disabled={uploadingContract}>
                {uploadingContract ? 'Đang upload...' : 'Upload PDF'}
              </button>
            </>
          )}
        >
          <div className="contract-upload-box">
            <div className="student-portal-info">
              <div className="info-row"><span>Sinh viên</span><strong>{contractFile.studentName || '-'}</strong></div>
              <div className="info-row"><span>Hết hạn</span><strong>{contractFile.endDate ? new Date(contractFile.endDate).toLocaleDateString('vi-VN') : '-'}</strong></div>
              <div className="info-row"><span>Trạng thái file</span><strong>{contractFile.hasPdf ? contractFile.pdfFileName || 'Đã upload' : 'Chưa có PDF'}</strong></div>
              <div className="info-row"><span>Cảnh báo</span><strong>{buildExpiryLabel(contractFile)}</strong></div>
            </div>
            <label className="field contract-upload-field">
              <span>Chọn file PDF</span>
              <input type="file" accept="application/pdf,.pdf" onChange={(event) => setSelectedPdf(event.target.files?.[0] ?? null)} />
            </label>
          </div>
          {contractFileError ? <div className="feedback error modal-feedback">{contractFileError}</div> : null}
          {contractFileNotice ? <div className="feedback success modal-feedback">{contractFileNotice}</div> : null}
        </ModalCard>
      ) : null}
    </>
  )
}

function buildExpiryLabel(contract) {
  if (contract.isExpired) return 'Đã hết hạn'
  if (contract.isExpiringSoon) {
    const days = Number(contract.daysUntilExpiry ?? 0)
    return days <= 0 ? 'Hết hạn hôm nay' : `Còn ${days} ngày`
  }
  if (typeof contract.daysUntilExpiry === 'number') return `Còn ${contract.daysUntilExpiry} ngày`
  return '-'
}
