import { useEffect, useRef, useState } from 'react'
import { apiFetch } from '../../helpers'
import { useAuth } from '../../hooks/useAuth'

const emptyValue = '—'

function statusLabel(status) {
  if (status === 'Paid') return 'Đã nộp'
  if (status === 'PartiallyPaid') return 'Một phần'
  if (status === 'Approved') return 'Đã duyệt'
  if (status === 'Rejected') return 'Từ chối'
  if (status === 'Pending') return 'Chờ duyệt'
  if (status === 'Unpaid') return 'Chưa nộp'
  return status || emptyValue
}

export function StudentPortalSection() {
  const { user, logout } = useAuth()
  const [profile, setProfile] = useState(null)
  const [room, setRoom] = useState(null)
  const [finance, setFinance] = useState(null)
  const [transfers, setTransfers] = useState([])
  const [messages, setMessages] = useState([])
  const [managers, setManagers] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [notice, setNotice] = useState('')
  const [editing, setEditing] = useState(false)
  const [editForm, setEditForm] = useState({ phone: '', email: '', address: '', emergencyContact: '' })
  const [transferForm, setTransferForm] = useState({ desiredRoomId: '', reason: '' })
  const [chatForm, setChatForm] = useState({ receiverId: '', content: '' })
  const [rooms, setRooms] = useState([])
  const [payingShareId, setPayingShareId] = useState(null)
  const [paymentError, setPaymentError] = useState('')
  const [paymentNotice, setPaymentNotice] = useState('')
  const [lastPaymentUrl, setLastPaymentUrl] = useState('')
  const [paymentFrame, setPaymentFrame] = useState(null)
  const chatEndRef = useRef(null)

  useEffect(() => {
    const params = new URLSearchParams(window.location.search)
    const paymentStatus = params.get('paymentStatus')
    const message = params.get('message')
    if (paymentStatus === 'success') {
      setNotice(message || 'Thanh toán VNPay thành công.')
      window.history.replaceState({}, '', window.location.pathname)
    } else if (paymentStatus === 'failed') {
      setError(message || 'Thanh toán VNPay chưa thành công.')
      window.history.replaceState({}, '', window.location.pathname)
    }
    loadPortalData()
  }, [])

  useEffect(() => {
    if (!error && !notice) return
    const timeout = setTimeout(() => {
      setError('')
      setNotice('')
    }, 4200)
    return () => clearTimeout(timeout)
  }, [error, notice])

  useEffect(() => {
    const intervalId = window.setInterval(() => {
      loadMessagesOnly(true)
    }, 3500)

    return () => window.clearInterval(intervalId)
  }, [])

  useEffect(() => {
    chatEndRef.current?.scrollIntoView({ behavior: 'smooth', block: 'end' })
  }, [messages.length])

  async function loadPortalData() {
    setLoading(true)
    setError('')
    try {
      const [profileRes, roomRes, financeRes, transferRes, msgRes, mgrRes, roomsRes] = await Promise.all([
        apiFetch('/api/student-portal/me'),
        apiFetch('/api/student-portal/room'),
        apiFetch('/api/student-portal/finance'),
        apiFetch('/api/student-portal/transfer-requests'),
        apiFetch('/api/student-portal/messages'),
        apiFetch('/api/student-portal/managers'),
        apiFetch('/api/facilities/rooms'),
      ])

      if (profileRes.ok) setProfile(await profileRes.json())
      if (roomRes.ok) setRoom(await roomRes.json())
      if (financeRes.ok) setFinance(await financeRes.json())
      if (transferRes.ok) setTransfers(await transferRes.json())
      if (msgRes.ok) setMessages(await msgRes.json())
      if (mgrRes.ok) setManagers(await mgrRes.json())
      if (roomsRes.ok) setRooms(await roomsRes.json())
    } catch (loadError) {
      setError(loadError.message)
    } finally {
      setLoading(false)
    }
  }

  async function loadMessagesOnly(silent = false) {
    try {
      const response = await apiFetch('/api/student-portal/messages', { skipGlobalLoading: true })
      if (!response.ok) {
        if (!silent) {
          const data = await response.json().catch(() => ({}))
          throw new Error(data.message || 'Không tải được tin nhắn mới nhất.')
        }
        return
      }

      setMessages(await response.json())
    } catch (messageLoadError) {
      if (!silent) {
        setError(messageLoadError.message)
      }
    }
  }

  function startEdit() {
    if (!profile?.student) return
    const student = profile.student
    setEditForm({
      phone: student.phone || '',
      email: student.email || '',
      address: student.address || '',
      emergencyContact: student.emergencyContact || '',
    })
    setEditing(true)
  }

  async function saveProfile() {
    try {
      setError('')
      const response = await apiFetch('/api/student-portal/profile', {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(editForm),
      })
      if (!response.ok) {
        const data = await response.json().catch(() => ({}))
        throw new Error(data.message || 'Lỗi cập nhật.')
      }

      setNotice('Đã lưu thông tin cá nhân.')
      setEditing(false)
      await loadPortalData()
    } catch (saveError) {
      setError(saveError.message)
    }
  }

  async function submitTransfer() {
    if (!transferForm.desiredRoomId || !transferForm.reason.trim()) {
      setError('Vui lòng chọn phòng và nhập lý do.')
      return
    }

    try {
      setError('')
      const response = await apiFetch('/api/student-portal/transfer-requests', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ desiredRoomId: Number(transferForm.desiredRoomId), reason: transferForm.reason }),
      })
      if (!response.ok) {
        const data = await response.json().catch(() => ({}))
        throw new Error(data.message || 'Lỗi gửi yêu cầu.')
      }

      setNotice('Đã gửi yêu cầu chuyển phòng.')
      setTransferForm({ desiredRoomId: '', reason: '' })
      await loadPortalData()
    } catch (transferError) {
      setError(transferError.message)
    }
  }

  async function sendMessage() {
    if (!chatForm.receiverId || !chatForm.content.trim()) {
      setError('Vui lòng chọn người nhận và nhập nội dung.')
      return
    }

    try {
      setError('')
      const response = await apiFetch('/api/student-portal/messages', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ receiverId: Number(chatForm.receiverId), content: chatForm.content }),
        skipGlobalLoading: true,
      })
      if (!response.ok) {
        const data = await response.json().catch(() => ({}))
        throw new Error(data.message || 'Lỗi gửi tin nhắn.')
      }

      setChatForm({ ...chatForm, content: '' })
      await loadMessagesOnly()
    } catch (messageError) {
      setError(messageError.message)
    }
  }

  async function payShareViaVnPay(share) {
    try {
      setError('')
      setPaymentError('')
      setPaymentNotice('')
      setLastPaymentUrl('')
      setPaymentFrame(null)
      setPayingShareId(share.id)
      const response = await apiFetch(`/api/student-portal/room-finance-shares/${share.id}/vnpay/create`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        skipGlobalLoading: true,
      })
      if (!response.ok) {
        const data = await response.json().catch(() => ({}))
        throw new Error(data.message || `Không tạo được thanh toán VNPay. Mã lỗi HTTP ${response.status}.`)
      }

      const data = await response.json()
      if (!data.paymentUrl) {
        throw new Error('API không trả về link thanh toán VNPay.')
      }

      setLastPaymentUrl(data.paymentUrl)
      setPaymentNotice('Đã tạo link thanh toán VNPay. Vui lòng hoàn tất giao dịch trong cửa sổ thanh toán.')
      setPaymentFrame({
        url: data.paymentUrl,
        share,
        amount: data.amount ?? share.remainingAmount,
        invoiceCode: data.invoiceCode ?? share.invoiceCode,
      })
      setPayingShareId(null)
    } catch (payError) {
      setPaymentError(payError.message)
      setPayingShareId(null)
    }
  }

  function canPayShare(share) {
    return share.status !== 'Paid' && Number(share.remainingAmount ?? 0) > 0
  }

  const currencyFormat = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })
  const dateFormat = new Intl.DateTimeFormat('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' })
  const monthFormat = new Intl.DateTimeFormat('vi-VN', { month: '2-digit', year: 'numeric' })
  const timeFormat = new Intl.DateTimeFormat('vi-VN', { day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit' })

  if (loading) {
    return (
      <div className="loading-screen">
        <div className="loading-card">
          <h1>Đang tải cổng sinh viên...</h1>
        </div>
      </div>
    )
  }

  const student = profile?.student

  return (
    <div className="app-shell" style={{ display: 'block' }}>
      <div className="student-portal">
        <header className="student-portal-header">
          <div>
            <h1>Cổng Sinh Viên</h1>
            <p>Xin chào, <strong>{user?.fullName || user?.username}</strong></p>
          </div>
          <div className="student-portal-actions">
            <button className="secondary-button" onClick={loadPortalData}>Làm mới</button>
            <button className="ghost-button danger" onClick={logout}>Đăng xuất</button>
          </div>
        </header>

        {error ? <div className="feedback error" style={{ margin: '0 auto 16px', maxWidth: 1100 }}>{error}</div> : null}
        {notice ? <div className="feedback success" style={{ margin: '0 auto 16px', maxWidth: 1100 }}>{notice}</div> : null}

        <div className="student-portal-grid">
          <section className="student-portal-card">
            <div className="student-portal-card-header">
              <h2>Thông tin cá nhân</h2>
              {!editing ? <button className="ghost-button" onClick={startEdit}>Chỉnh sửa</button> : null}
            </div>
            {student && !editing ? (
              <div className="student-portal-info">
                <div className="info-row"><span>Mã SV</span><strong>{student.studentCode}</strong></div>
                <div className="info-row"><span>Họ tên</span><strong>{student.name}</strong></div>
                <div className="info-row"><span>Giới tính</span><strong>{student.gender}</strong></div>
                <div className="info-row"><span>Ngày sinh</span><strong>{student.dateOfBirth ? dateFormat.format(new Date(student.dateOfBirth)) : emptyValue}</strong></div>
                <div className="info-row"><span>Điện thoại</span><strong>{student.phone || emptyValue}</strong></div>
                <div className="info-row"><span>Email</span><strong>{student.email || emptyValue}</strong></div>
                <div className="info-row"><span>Khoa</span><strong>{student.faculty || emptyValue}</strong></div>
                <div className="info-row"><span>Lớp</span><strong>{student.className || emptyValue}</strong></div>
                <div className="info-row"><span>Địa chỉ</span><strong>{student.address || emptyValue}</strong></div>
                <div className="info-row"><span>Liên hệ khẩn</span><strong>{student.emergencyContact || emptyValue}</strong></div>
                <div className="info-row"><span>Trạng thái</span><strong>{student.status}</strong></div>
              </div>
            ) : editing ? (
              <div className="student-portal-edit-form">
                <label>Điện thoại<input value={editForm.phone} onChange={(event) => setEditForm({ ...editForm, phone: event.target.value })} /></label>
                <label>Email<input value={editForm.email} onChange={(event) => setEditForm({ ...editForm, email: event.target.value })} /></label>
                <label>Địa chỉ<input value={editForm.address} onChange={(event) => setEditForm({ ...editForm, address: event.target.value })} /></label>
                <label>Liên hệ khẩn cấp<input value={editForm.emergencyContact} onChange={(event) => setEditForm({ ...editForm, emergencyContact: event.target.value })} /></label>
                <div className="student-portal-edit-actions">
                  <button className="primary-button" onClick={saveProfile}>Lưu</button>
                  <button className="secondary-button" onClick={() => setEditing(false)}>Hủy</button>
                </div>
              </div>
            ) : (
              <p>Không có dữ liệu.</p>
            )}
          </section>

          <section className="student-portal-card">
            <h2>Phòng ở & Bạn cùng phòng</h2>
            {room?.room ? (
              <>
                <div className="student-portal-info">
                  <div className="info-row"><span>Phòng</span><strong>{room.room.roomNumber}</strong></div>
                  <div className="info-row"><span>Tòa nhà</span><strong>{room.room.buildingName}</strong></div>
                  <div className="info-row"><span>Sức chứa</span><strong>{room.room.currentOccupancy}/{room.room.capacity}</strong></div>
                  <div className="info-row"><span>Trạng thái phòng</span><strong>{room.room.status}</strong></div>
                </div>
                {room.roommates?.length > 0 ? (
                  <div style={{ marginTop: 12 }}>
                    <h3>Bạn cùng phòng</h3>
                    <table>
                      <thead>
                        <tr><th>Mã SV</th><th>Họ tên</th><th>Khoa</th><th>SĐT</th></tr>
                      </thead>
                      <tbody>
                        {room.roommates.map((roommate) => (
                          <tr key={roommate.id ?? roommate.studentCode}>
                            <td>{roommate.studentCode}</td>
                            <td>{roommate.name}</td>
                            <td>{roommate.faculty}</td>
                            <td>{roommate.phone}</td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                ) : null}
              </>
            ) : (
              <p style={{ padding: '1rem', color: '#64748b' }}>Bạn chưa được xếp phòng.</p>
            )}
          </section>

          <section className="student-portal-card full-width">
            <h2>Công nợ tài chính</h2>
            <div className="student-payment-method">
              <img src="/vnpay-logo.svg" alt="VNPay" />
              <div>
                <strong>Hình thức giao dịch khả dụng: VNPay</strong>
                <span>Sinh viên chỉ thanh toán trực tuyến qua VNPay. Tiền mặt chỉ được kế toán ghi nhận tại quầy.</span>
              </div>
            </div>
            {paymentError ? <div className="feedback error payment-inline-feedback">{paymentError}</div> : null}
            {paymentNotice ? (
              <div className="feedback success payment-inline-feedback">
                {paymentNotice}
                {lastPaymentUrl ? (
                  <button className="ghost-button compact" type="button" onClick={() => window.location.assign(lastPaymentUrl)}>
                    Mở lại VNPay
                  </button>
                ) : null}
              </div>
            ) : null}
            {finance?.shares?.length > 0 ? (
              <table>
                <thead>
                  <tr>
                    <th>Kỳ</th>
                    <th>Hóa đơn</th>
                    <th>Tổng phòng</th>
                    <th>Phần bạn</th>
                    <th>Đã nộp</th>
                    <th>Còn lại</th>
                    <th>Trạng thái</th>
                    <th>Hạn nộp</th>
                    <th>Thanh toán</th>
                  </tr>
                </thead>
                <tbody>
                  {finance.shares.map((share) => (
                    <tr key={share.id}>
                      <td>{share.billingMonth ? monthFormat.format(new Date(share.billingMonth)) : emptyValue}</td>
                      <td>{share.invoiceCode || emptyValue}</td>
                      <td>{currencyFormat.format(share.roomTotal)}</td>
                      <td>{currencyFormat.format(share.expectedAmount)}</td>
                      <td>{currencyFormat.format(share.paidAmount)}</td>
                      <td>{currencyFormat.format(share.remainingAmount)}</td>
                      <td>
                        <span className={`table-badge ${share.status === 'Paid' ? 'emerald' : share.status === 'PartiallyPaid' ? 'amber' : 'rose'}`}>
                          {statusLabel(share.status)}
                        </span>
                      </td>
                      <td>{share.dueDate ? dateFormat.format(new Date(share.dueDate)) : emptyValue}</td>
                      <td>
                        {canPayShare(share) ? (
                          <button
                            type="button"
                            className="vnpay-pay-button"
                            onClick={() => payShareViaVnPay(share)}
                            disabled={payingShareId === share.id}
                            aria-label={`Thanh toán khoản công nợ ${share.invoiceCode || share.id} qua VNPay`}
                          >
                            <img src="/vnpay-logo.svg" alt="" aria-hidden="true" />
                            <span>
                              <strong>{payingShareId === share.id ? 'Đang mở VNPay...' : 'Thanh toán'}</strong>
                              <small>Hình thức giao dịch VNPay</small>
                            </span>
                          </button>
                        ) : emptyValue}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            ) : (
              <p style={{ padding: '1rem', color: '#64748b' }}>Chưa có công nợ tài chính.</p>
            )}

            {finance?.roommateShares?.length > 0 ? (
              <div className="roommate-finance-block">
                <h3>Trạng thái đóng phí các thành viên trong phòng</h3>
                <table>
                  <thead>
                    <tr><th>Kỳ</th><th>Sinh viên</th><th>Phải nộp</th><th>Đã nộp</th><th>Còn lại</th><th>Trạng thái</th></tr>
                  </thead>
                  <tbody>
                    {finance.roommateShares.map((item) => (
                      <tr key={item.id}>
                        <td>{item.billingMonth ? monthFormat.format(new Date(item.billingMonth)) : emptyValue}</td>
                        <td>{item.studentName} <span className="muted-line">{item.studentCode}</span></td>
                        <td>{currencyFormat.format(item.expectedAmount)}</td>
                        <td>{currencyFormat.format(item.paidAmount)}</td>
                        <td>{currencyFormat.format(item.remainingAmount)}</td>
                        <td>
                          <span className={`table-badge ${item.status === 'Paid' ? 'emerald' : item.status === 'PartiallyPaid' ? 'amber' : 'rose'}`}>
                            {statusLabel(item.status)}
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            ) : null}
          </section>

          <section className="student-portal-card">
            <h2>Yêu cầu chuyển phòng</h2>
            {room?.room ? (
              <div className="student-portal-edit-form">
                <label>Phòng muốn chuyển đến
                  <select value={transferForm.desiredRoomId} onChange={(event) => setTransferForm({ ...transferForm, desiredRoomId: event.target.value })}>
                    <option value="">— Chọn phòng —</option>
                    {rooms.filter((item) => item.id !== profile?.student?.roomId).map((item) => (
                      <option key={item.id} value={item.id}>{item.roomNumber} ({item.buildingName})</option>
                    ))}
                  </select>
                </label>
                <label>Lý do<textarea value={transferForm.reason} onChange={(event) => setTransferForm({ ...transferForm, reason: event.target.value })} rows={3} /></label>
                <button className="primary-button" onClick={submitTransfer}>Gửi yêu cầu</button>
              </div>
            ) : null}
            {transfers.length > 0 ? (
              <div style={{ marginTop: 16 }}>
                <h3>Lịch sử yêu cầu</h3>
                <table>
                  <thead>
                    <tr><th>Phòng hiện tại</th><th>Phòng muốn</th><th>Lý do</th><th>Trạng thái</th><th>Ghi chú</th></tr>
                  </thead>
                  <tbody>
                    {transfers.map((transfer) => (
                      <tr key={transfer.id}>
                        <td>{transfer.currentRoom}</td>
                        <td>{transfer.desiredRoom}</td>
                        <td>{transfer.reason}</td>
                        <td>
                          <span className={`table-badge ${transfer.status === 'Approved' ? 'emerald' : transfer.status === 'Rejected' ? 'rose' : 'amber'}`}>
                            {statusLabel(transfer.status)}
                          </span>
                        </td>
                        <td>{transfer.decisionNote || emptyValue}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            ) : null}
          </section>

          <section className="student-portal-card">
            <h2>Nhắn tin với quản lý</h2>
            <div className="student-portal-edit-form">
              <label>Gửi đến
                <select value={chatForm.receiverId} onChange={(event) => setChatForm({ ...chatForm, receiverId: event.target.value })}>
                  <option value="">— Chọn quản lý —</option>
                  {managers.map((manager) => (
                    <option key={manager.id} value={manager.id}>{manager.fullName || manager.username} ({manager.roleName})</option>
                  ))}
                </select>
              </label>
              <label>Nội dung<textarea value={chatForm.content} onChange={(event) => setChatForm({ ...chatForm, content: event.target.value })} rows={3} placeholder="Nhập tin nhắn..." /></label>
              <button className="primary-button" onClick={sendMessage}>Gửi</button>
            </div>
            {messages.length > 0 ? (
              <div style={{ marginTop: 16, maxHeight: 300, overflowY: 'auto' }}>
                <h3>Tin nhắn</h3>
                {messages.map((message) => (
                  <div key={message.id} className={`chat-bubble ${message.senderId === user?.id ? 'sent' : 'received'}`}>
                    <div className="chat-meta">
                      <strong>{message.senderId === user?.id ? 'Bạn' : message.senderName}</strong>
                      <span>{message.createdAt ? timeFormat.format(new Date(message.createdAt)) : ''}</span>
                    </div>
                    <p>{message.content}</p>
                  </div>
                ))}
                <div ref={chatEndRef} />
              </div>
            ) : null}
          </section>
        </div>
      </div>
      {paymentFrame ? (
        <div className="modal-backdrop" role="presentation" onMouseDown={(event) => {
          if (event.target === event.currentTarget) setPaymentFrame(null)
        }}>
          <div className="modal-card vnpay-payment-modal vnpay-frame-modal" role="dialog" aria-modal="true" aria-labelledby="vnpay-student-title">
            <div className="vnpay-payment-header">
              <img src="/vnpay-logo.svg" alt="VNPay" />
              <div>
                <h2 id="vnpay-student-title">Thanh toán qua VNPay</h2>
                <p>Hoàn tất thanh toán trong khung bên dưới. Nếu ngân hàng hoặc VNPay chặn nhúng, hãy dùng nút mở tab mới.</p>
              </div>
            </div>
            <div className="vnpay-payment-summary">
              <div>
                <span>Hóa đơn</span>
                <strong>{paymentFrame.invoiceCode || '-'}</strong>
              </div>
              <div>
                <span>Số tiền</span>
                <strong>{currencyFormat.format(paymentFrame.amount || 0)}</strong>
              </div>
              <div>
                <span>Trạng thái</span>
                <strong>Đang chờ thanh toán</strong>
              </div>
            </div>
            <div className="vnpay-frame-shell">
              <iframe
                className="vnpay-frame"
                title="Cổng thanh toán VNPay"
                src={paymentFrame.url}
                referrerPolicy="no-referrer-when-downgrade"
              />
            </div>
            <div className="modal-footer">
              <button className="secondary-button" type="button" onClick={() => setPaymentFrame(null)}>Đóng</button>
              <button className="ghost-button" type="button" onClick={() => window.open(paymentFrame.url, '_blank', 'noopener,noreferrer')}>
                Mở tab mới
              </button>
              <button className="primary-button" type="button" onClick={loadPortalData}>Làm mới trạng thái</button>
            </div>
          </div>
        </div>
      ) : null}
    </div>
  )
}
