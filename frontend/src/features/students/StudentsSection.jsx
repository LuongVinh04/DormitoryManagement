import { useMemo, useState } from 'react'
import { CrudPanel, ModalCard } from '../../components'
import { apiFetch, readError } from '../../helpers'

const STUDENT_FILTERS = [
  { key: 'all', label: 'Tất cả', match: () => true },
  { key: 'assignable', label: 'Đủ điều kiện xếp phòng', match: (student) => student.canAssignRoom },
  { key: 'noContract', label: 'Chưa có hợp đồng', match: (student) => student.contractState === 'NoContract' },
  { key: 'expiredGrace', label: 'Hết hạn đang ẩn phòng', match: (student) => student.contractState === 'ExpiredGrace' },
  { key: 'expired', label: 'Hết hạn/hủy', match: (student) => ['Expired', 'Cancelled'].includes(student.contractState) },
  { key: 'missingAccount', label: 'Chưa có tài khoản', match: (student) => !student.hasAccount },
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

export function StudentsSection({ data, openCreate, openEdit, deleteEntity, getPanelProps, refreshData }) {
  const [filter, setFilter] = useState('all')
  const [accountStudent, setAccountStudent] = useState(null)
  const [accountForm, setAccountForm] = useState({ username: '', email: '', password: '', isActive: true })
  const [accountError, setAccountError] = useState('')
  const [accountNotice, setAccountNotice] = useState('')
  const [savingAccount, setSavingAccount] = useState(false)

  const filteredStudents = useMemo(() => {
    const activeFilter = STUDENT_FILTERS.find((item) => item.key === filter) ?? STUDENT_FILTERS[0]
    return data.students.filter(activeFilter.match)
  }, [data.students, filter])

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

  return (
    <>
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
        <button className="secondary-button" onClick={() => downloadExcel('/api/export/students', 'danh-sach-sinh-vien.xlsx')}>
          Xuất Excel sinh viên
        </button>
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
      <CrudPanel entityKey="contracts" rows={data.contracts} onCreate={openCreate} onEdit={openEdit} onDelete={deleteEntity} panelProps={getPanelProps('students-contracts')} />

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
    </>
  )
}
