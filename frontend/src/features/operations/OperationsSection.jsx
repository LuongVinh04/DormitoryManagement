import { useEffect, useMemo, useState } from 'react'
import { ActionCard, CrudPanel, Panel } from '../../components'
import { apiFetch, localizeValue, readError, shortDate } from '../../helpers'

function transferStatusLabel(status) {
  if (status === 'Approved') return 'Đã duyệt'
  if (status === 'Rejected') return 'Từ chối'
  return 'Chờ duyệt'
}

export function OperationsSection({
  data,
  roomOverview,
  selectedRoomId,
  setSelectedRoomId,
  roomActions,
  setRoomActions,
  waitingStudents,
  executeAction,
  openCreate,
  openEdit,
  deleteEntity,
  saving,
  getPanelProps,
}) {
  const [transferRequests, setTransferRequests] = useState([])
  const [messages, setMessages] = useState([])
  const [rejectNotes, setRejectNotes] = useState({})
  const [chatForm, setChatForm] = useState({ receiverId: '', content: '' })
  const [opsError, setOpsError] = useState('')
  const [opsNotice, setOpsNotice] = useState('')

  const studentUsers = useMemo(
    () => data.users.filter((user) => String(user.roleName || '').toLowerCase() === 'student'),
    [data.users],
  )

  useEffect(() => {
    loadTransferRequests()
    loadMessages()
  }, [])

  async function loadTransferRequests() {
    try {
      const response = await apiFetch('/api/operations/transfer-requests')
      if (response.ok) setTransferRequests(await response.json())
    } catch {
      // Keep the main operations flow usable even if this optional panel fails.
    }
  }

  async function loadMessages() {
    try {
      const response = await apiFetch('/api/operations/messages', { skipGlobalLoading: true })
      if (response.ok) setMessages(await response.json())
    } catch {
      // Keep silent here; send failures are shown explicitly.
    }
  }

  async function approveTransfer(id) {
    await executeAction(
      () => apiFetch(`/api/operations/transfer-requests/${id}/approve`, { method: 'POST' }),
      'Đã duyệt yêu cầu chuyển phòng.',
    )
    await loadTransferRequests()
  }

  async function rejectTransfer(id) {
    await executeAction(
      () => apiFetch(`/api/operations/transfer-requests/${id}/reject`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ note: rejectNotes[id] || 'Từ chối yêu cầu chuyển phòng.' }),
      }),
      'Đã từ chối yêu cầu chuyển phòng.',
    )
    setRejectNotes((current) => ({ ...current, [id]: '' }))
    await loadTransferRequests()
  }

  async function sendMessage() {
    if (!chatForm.receiverId || !chatForm.content.trim()) {
      setOpsError('Vui lòng chọn sinh viên và nhập nội dung tin nhắn.')
      return
    }

    try {
      setOpsError('')
      setOpsNotice('')
      const response = await apiFetch('/api/student-portal/messages', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ receiverId: Number(chatForm.receiverId), content: chatForm.content }),
        skipGlobalLoading: true,
      })

      if (!response.ok) {
        throw new Error(await readError(response))
      }

      setOpsNotice('Đã gửi tin nhắn cho sinh viên.')
      setChatForm((current) => ({ ...current, content: '' }))
      await loadMessages()
    } catch (error) {
      setOpsError(error.message)
    }
  }

  return (
    <>
      {opsError ? <div className="feedback error">{opsError}</div> : null}
      {opsNotice ? <div className="feedback success">{opsNotice}</div> : null}

      <section className="room-ops-grid">
        <Panel title="Điều phối sinh viên theo phòng" description="Xếp phòng, chuyển phòng và trả phòng ngay trên một luồng xử lý." {...getPanelProps('operations-room-workflow')}>
          <div className="room-toolbar">
            <label className="field">
              <span>Chọn phòng</span>
              <select value={selectedRoomId} onChange={(event) => setSelectedRoomId(event.target.value)}>
                {data.rooms.map((room) => (
                  <option key={room.id} value={room.id}>{room.roomNumber} - {room.buildingName}</option>
                ))}
              </select>
            </label>
            {roomOverview?.room ? (
              <div className="room-summary-box">
                <strong>{roomOverview.room.roomNumber}</strong>
                <span>{roomOverview.room.buildingName}</span>
                <p>{localizeValue(roomOverview.room.status)} - {roomOverview.room.currentOccupancy}/{roomOverview.room.capacity} chỗ</p>
              </div>
            ) : null}
          </div>

          <div className="workflow-strip">
            <div className="workflow-step active">1. Chọn phòng</div>
            <div className="workflow-step">2. Điều phối cư trú</div>
            <div className="workflow-step">3. Kiểm tra danh sách ở</div>
          </div>

          <div className="operation-cards">
            <ActionCard title="Xếp phòng" description="Bố trí sinh viên đang chờ vào phòng hiện tại." actionLabel="Xếp vào phòng" disabled={!roomActions.assignStudentId || saving} onAction={() => executeAction(
              () => apiFetch(`/api/facilities/rooms/${selectedRoomId}/assign-student`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ studentId: Number(roomActions.assignStudentId), status: 'Active', note: 'Xếp phòng từ trung tâm điều phối.' }) }),
              'Đã xếp sinh viên vào phòng.',
            )}>
              <select value={roomActions.assignStudentId} onChange={(event) => setRoomActions((current) => ({ ...current, assignStudentId: event.target.value }))}>
                <option value="">Chọn sinh viên chờ xếp</option>
                {waitingStudents.map((student) => <option key={student.id} value={student.id}>{student.studentCode} - {student.name}</option>)}
              </select>
            </ActionCard>

            <ActionCard title="Chuyển phòng" description="Điều phối sinh viên sang phòng khác khi cần tối ưu công suất." actionLabel="Chuyển phòng" disabled={!roomActions.transferStudentId || !roomActions.transferToRoomId || saving} onAction={() => executeAction(
              () => apiFetch('/api/facilities/rooms/transfer-student', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ studentId: Number(roomActions.transferStudentId), toRoomId: Number(roomActions.transferToRoomId), status: 'Active', note: 'Điều chuyển phòng từ trung tâm điều phối.' }) }),
              'Đã chuyển phòng cho sinh viên.',
            )}>
              <select value={roomActions.transferStudentId} onChange={(event) => setRoomActions((current) => ({ ...current, transferStudentId: event.target.value }))}>
                <option value="">Chọn sinh viên đang ở</option>
                {(roomOverview?.students ?? []).map((student) => <option key={student.id} value={student.id}>{student.studentCode} - {student.name}</option>)}
              </select>
              <select value={roomActions.transferToRoomId} onChange={(event) => setRoomActions((current) => ({ ...current, transferToRoomId: event.target.value }))}>
                <option value="">Chọn phòng đích</option>
                {data.rooms.filter((room) => String(room.id) !== selectedRoomId).map((room) => <option key={room.id} value={room.id}>{room.roomNumber} - {room.buildingName}</option>)}
              </select>
            </ActionCard>

            <ActionCard title="Trả phòng" description="Đưa sinh viên ra khỏi phòng và chuyển về trạng thái chờ sắp xếp." actionLabel="Xác nhận trả phòng" disabled={!roomActions.removeStudentId || saving} onAction={() => executeAction(
              () => apiFetch(`/api/facilities/rooms/${selectedRoomId}/remove-student`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ studentId: Number(roomActions.removeStudentId), status: 'Waiting', note: 'Đã trả phòng, chờ sắp xếp lại.' }) }),
              'Đã trả phòng cho sinh viên.',
            )}>
              <select value={roomActions.removeStudentId} onChange={(event) => setRoomActions((current) => ({ ...current, removeStudentId: event.target.value }))}>
                <option value="">Chọn sinh viên cần trả phòng</option>
                {(roomOverview?.students ?? []).map((student) => <option key={student.id} value={student.id}>{student.studentCode} - {student.name}</option>)}
              </select>
            </ActionCard>
          </div>
        </Panel>

        <Panel title="Sinh viên trong phòng" description="Danh sách cư trú hiện tại của phòng đang chọn." {...getPanelProps('operations-room-students')}>
          <div className="occupant-list">
            {(roomOverview?.students ?? []).length === 0 ? (
              <div className="empty-state">Chưa có sinh viên nào trong phòng này.</div>
            ) : (
              roomOverview.students.map((student) => (
                <article key={student.id} className="occupant-card">
                  <strong>{student.name}</strong>
                  <span>{student.studentCode}</span>
                  <p>{student.faculty} - {student.className}</p>
                  <small>{student.phone}</small>
                </article>
              ))
            )}
          </div>
        </Panel>
      </section>

      <Panel title="Yêu cầu chuyển phòng từ sinh viên" description="Duyệt hoặc từ chối các yêu cầu chuyển phòng được gửi từ cổng sinh viên." {...getPanelProps('operations-transfer-requests')}>
        <div className="table-wrap">
          <table className="data-table">
            <thead>
              <tr>
                <th>Sinh viên</th>
                <th>Phòng hiện tại</th>
                <th>Phòng muốn chuyển</th>
                <th>Lý do</th>
                <th>Trạng thái</th>
                <th>Thao tác</th>
              </tr>
            </thead>
            <tbody>
              {transferRequests.length === 0 ? (
                <tr><td colSpan="6"><div className="empty-state">Chưa có yêu cầu chuyển phòng.</div></td></tr>
              ) : transferRequests.map((item) => (
                <tr key={item.id}>
                  <td data-label="Sinh viên">{item.studentCode} - {item.studentName}</td>
                  <td data-label="Phòng hiện tại">{item.currentRoom}</td>
                  <td data-label="Phòng muốn chuyển">{item.desiredRoom}</td>
                  <td data-label="Lý do">{item.reason}</td>
                  <td data-label="Trạng thái">
                    <span className={`table-badge ${item.status === 'Approved' ? 'emerald' : item.status === 'Rejected' ? 'rose' : 'amber'}`}>
                      {transferStatusLabel(item.status)}
                    </span>
                  </td>
                  <td data-label="Thao tác">
                    {item.status === 'Pending' ? (
                      <div className="stacked-actions">
                        <input
                          value={rejectNotes[item.id] || ''}
                          onChange={(event) => setRejectNotes((current) => ({ ...current, [item.id]: event.target.value }))}
                          placeholder="Ghi chú nếu từ chối"
                        />
                        <div className="action-row">
                          <button className="ghost-button approve" onClick={() => approveTransfer(item.id)}>Duyệt</button>
                          <button className="ghost-button danger" onClick={() => rejectTransfer(item.id)}>Từ chối</button>
                        </div>
                      </div>
                    ) : (
                      <span>{item.decisionNote || (item.decisionDate ? shortDate(item.decisionDate) : '-')}</span>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </Panel>

      <Panel title="Tin nhắn với sinh viên" description="Trao đổi nhanh với các tài khoản sinh viên đã được cấp quyền đăng nhập." {...getPanelProps('operations-messages')}>
        <div className="message-console">
          <div className="student-portal-edit-form">
            <label>Người nhận
              <select value={chatForm.receiverId} onChange={(event) => setChatForm((current) => ({ ...current, receiverId: event.target.value }))}>
                <option value="">Chọn tài khoản sinh viên</option>
                {studentUsers.map((user) => (
                  <option key={user.id} value={user.id}>{user.fullName || user.username} ({user.username})</option>
                ))}
              </select>
            </label>
            <label>Nội dung
              <textarea rows={3} value={chatForm.content} onChange={(event) => setChatForm((current) => ({ ...current, content: event.target.value }))} />
            </label>
            <button className="primary-button" onClick={sendMessage}>Gửi tin nhắn</button>
          </div>

          <div className="message-list">
            {messages.length === 0 ? (
              <div className="empty-state">Chưa có tin nhắn.</div>
            ) : messages.map((message) => (
              <article key={message.id} className="message-card">
                <div>
                  <strong>{message.senderName || 'Người gửi'} → {message.receiverName || 'Người nhận'}</strong>
                  <span>{message.createdAt ? shortDate(message.createdAt) : ''}</span>
                </div>
                <p>{message.content}</p>
                <button className="ghost-button" onClick={() => setChatForm((current) => ({ ...current, receiverId: String(message.senderId), content: current.content }))}>
                  Trả lời người gửi
                </button>
              </article>
            ))}
          </div>
        </div>
      </Panel>

      <CrudPanel
        entityKey="registrations"
        rows={data.registrations}
        onCreate={openCreate}
        onEdit={openEdit}
        onDelete={deleteEntity}
        panelProps={{ ...getPanelProps('operations-registrations'), panelId: 'panel-operations-registrations' }}
        extraRowActions={(item) => [
          item.status === 'Pending' ? { label: 'Duyệt', kind: 'approve', onClick: () => executeAction(() => apiFetch(`/api/operations/registrations/${item.id}/approve`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ note: 'Duyệt từ giao diện vận hành.' }) }), 'Đã duyệt đăng ký và xếp phòng.') } : null,
          item.status === 'Pending' ? { label: 'Từ chối', kind: 'danger', onClick: () => executeAction(() => apiFetch(`/api/operations/registrations/${item.id}/reject`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ note: 'Từ chối từ giao diện vận hành.' }) }), 'Đã từ chối đăng ký.') } : null,
        ]}
      />
    </>
  )
}
