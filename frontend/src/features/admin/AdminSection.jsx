import { useEffect, useMemo, useState } from 'react'
import { CrudPanel, Panel } from '../../components'
import { apiFetch, readError, shortDate } from '../../helpers'
import { PermissionManager } from './PermissionManager'

export function AdminSection({ data, openCreate, openEdit, deleteEntity, getPanelProps }) {
  const [messages, setMessages] = useState([])
  const [messageForm, setMessageForm] = useState({ receiverId: '', content: '' })
  const [messageError, setMessageError] = useState('')
  const [messageNotice, setMessageNotice] = useState('')
  const [sendingMessage, setSendingMessage] = useState(false)

  const studentUsers = useMemo(
    () => data.users.filter((user) => String(user.roleName || '').toLowerCase() === 'student'),
    [data.users],
  )

  useEffect(() => {
    loadMessages()
  }, [])

  useEffect(() => {
    const intervalId = window.setInterval(() => {
      loadMessages()
    }, 3500)

    return () => window.clearInterval(intervalId)
  }, [])

  async function loadMessages() {
    try {
      const response = await apiFetch('/api/operations/messages', { skipGlobalLoading: true })
      if (response.ok) {
        setMessages(await response.json())
      }
    } catch {
      // Tin nhắn là khối phụ, không chặn các chức năng quản trị chính.
    }
  }

  async function sendMessage() {
    if (!messageForm.receiverId || !messageForm.content.trim()) {
      setMessageError('Vui lòng chọn sinh viên và nhập nội dung tin nhắn.')
      return
    }

    try {
      setSendingMessage(true)
      setMessageError('')
      setMessageNotice('')
      const response = await apiFetch('/api/student-portal/messages', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ receiverId: Number(messageForm.receiverId), content: messageForm.content.trim() }),
        skipGlobalLoading: true,
      })

      if (!response.ok) {
        throw new Error(await readError(response))
      }

      setMessageNotice('Đã gửi tin nhắn cho sinh viên.')
      setMessageForm((current) => ({ ...current, content: '' }))
      await loadMessages()
    } catch (error) {
      setMessageError(error.message)
    } finally {
      setSendingMessage(false)
    }
  }

  function replyToMessage(message) {
    setMessageForm({
      receiverId: String(message.senderId),
      content: '',
    })
    setMessageError('')
    setMessageNotice('')
  }

  return (
    <>
      <PermissionManager users={data.users} />
      <Panel
        title="Tin nhắn sinh viên"
        description="Tiếp nhận, phản hồi các tin nhắn sinh viên gửi tới quản trị viên mà không làm gián đoạn màn hình làm việc."
        {...getPanelProps('admin-messages')}
      >
        <div className="message-console admin-message-console">
          <div className="message-composer">
            <div className="field">
              <span>Người nhận</span>
              <select value={messageForm.receiverId} onChange={(event) => setMessageForm((current) => ({ ...current, receiverId: event.target.value }))}>
                <option value="">Chọn tài khoản sinh viên</option>
                {studentUsers.map((user) => (
                  <option key={user.id} value={user.id}>{user.fullName || user.username} ({user.username})</option>
                ))}
              </select>
            </div>
            <div className="field">
              <span>Nội dung trả lời</span>
              <textarea
                rows={4}
                value={messageForm.content}
                onChange={(event) => setMessageForm((current) => ({ ...current, content: event.target.value }))}
                placeholder="Nhập nội dung phản hồi cho sinh viên..."
              />
            </div>
            {messageError ? <div className="feedback error">{messageError}</div> : null}
            {messageNotice ? <div className="feedback success">{messageNotice}</div> : null}
            <div className="message-send-row">
              <button className="primary-button" onClick={sendMessage} disabled={sendingMessage}>
                {sendingMessage ? 'Đang gửi...' : 'Gửi tin nhắn'}
              </button>
              <button className="secondary-button" onClick={loadMessages} disabled={sendingMessage}>Làm mới hội thoại</button>
            </div>
          </div>

          <div className="message-list">
            {messages.length === 0 ? (
              <div className="empty-state">Chưa có tin nhắn với sinh viên.</div>
            ) : messages.map((message) => (
              <article key={message.id} className="message-card">
                <div>
                  <strong>{message.senderName || 'Người gửi'} → {message.receiverName || 'Người nhận'}</strong>
                  <span>{message.createdAt ? shortDate(message.createdAt) : ''}</span>
                </div>
                <p>{message.content}</p>
                <button className="ghost-button" onClick={() => replyToMessage(message)}>
                  Trả lời người gửi
                </button>
              </article>
            ))}
          </div>
        </div>
      </Panel>
      <CrudPanel entityKey="users" rows={data.users} onCreate={openCreate} onEdit={openEdit} onDelete={deleteEntity} panelProps={getPanelProps('admin-users')} />
      <CrudPanel entityKey="roles" rows={data.roles} onCreate={openCreate} onEdit={openEdit} onDelete={deleteEntity} panelProps={getPanelProps('admin-roles')} />
    </>
  )
}
