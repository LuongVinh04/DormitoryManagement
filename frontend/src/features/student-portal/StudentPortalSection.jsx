import { useEffect, useState } from 'react'
import { apiFetch } from '../../helpers'
import { useAuth } from '../../hooks/useAuth'

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
    const t = setTimeout(() => { setError(''); setNotice('') }, 4200)
    return () => clearTimeout(t)
  }, [error, notice])

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
    } catch (e) {
      setError(e.message)
    } finally {
      setLoading(false)
    }
  }

  function startEdit() {
    if (!profile?.student) return
    const s = profile.student
    setEditForm({ phone: s.phone || '', email: s.email || '', address: s.address || '', emergencyContact: s.emergencyContact || '' })
    setEditing(true)
  }

  async function saveProfile() {
    try {
      setError('')
      const res = await apiFetch('/api/student-portal/profile', { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(editForm) })
      if (!res.ok) { const d = await res.json().catch(() => ({})); throw new Error(d.message || 'L\u1ed7i c\u1eadp nh\u1eadt.') }
      setNotice('\u0110\u00e3 l\u01b0u th\u00f4ng tin c\u00e1 nh\u00e2n.')
      setEditing(false)
      await loadPortalData()
    } catch (e) { setError(e.message) }
  }

  async function submitTransfer() {
    if (!transferForm.desiredRoomId || !transferForm.reason.trim()) { setError('Vui l\u00f2ng ch\u1ecdn ph\u00f2ng v\u00e0 nh\u1eadp l\u00fd do.'); return }
    try {
      setError('')
      const res = await apiFetch('/api/student-portal/transfer-requests', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ desiredRoomId: Number(transferForm.desiredRoomId), reason: transferForm.reason }) })
      if (!res.ok) { const d = await res.json().catch(() => ({})); throw new Error(d.message || 'L\u1ed7i g\u1eedi y\u00eau c\u1ea7u.') }
      setNotice('\u0110\u00e3 g\u1eedi y\u00eau c\u1ea7u chuy\u1ec3n ph\u00f2ng.')
      setTransferForm({ desiredRoomId: '', reason: '' })
      await loadPortalData()
    } catch (e) { setError(e.message) }
  }

  async function sendMessage() {
    if (!chatForm.receiverId || !chatForm.content.trim()) { setError('Vui l\u00f2ng ch\u1ecdn ng\u01b0\u1eddi nh\u1eadn v\u00e0 nh\u1eadp n\u1ed9i dung.'); return }
    try {
      setError('')
      const res = await apiFetch('/api/student-portal/messages', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ receiverId: Number(chatForm.receiverId), content: chatForm.content }) })
      if (!res.ok) { const d = await res.json().catch(() => ({})); throw new Error(d.message || 'L\u1ed7i g\u1eedi tin nh\u1eafn.') }
      setChatForm({ ...chatForm, content: '' })
      await loadPortalData()
    } catch (e) { setError(e.message) }
  }

  async function payInvoice(invoiceId) {
    try {
      setError('')
      const res = await apiFetch(`/api/student-portal/invoices/${invoiceId}/vnpay/create`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
      })
      if (!res.ok) { const d = await res.json().catch(() => ({})); throw new Error(d.message || 'Không tạo được thanh toán VNPay.') }
      const data = await res.json()
      window.location.href = data.paymentUrl
    } catch (e) { setError(e.message) }
  }

  const currencyFormat = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })
  const dateFormat = new Intl.DateTimeFormat('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' })
  const timeFormat = new Intl.DateTimeFormat('vi-VN', { day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit' })

  if (loading) {
    return (<div className="loading-screen"><div className="loading-card"><h1>{'\u0110ang t\u1ea3i c\u1ed5ng sinh vi\u00ean...'}</h1></div></div>)
  }

  const s = profile?.student

  return (
    <div className="app-shell" style={{ display: 'block' }}>
      <div className="student-portal">
        <header className="student-portal-header">
          <div>
            <h1>{'\ud83d\udcda C\u1ed5ng Sinh Vi\u00ean'}</h1>
            <p>Xin ch\u00e0o, <strong>{user?.fullName || user?.username}</strong></p>
          </div>
          <div className="student-portal-actions">
            <button className="secondary-button" onClick={loadPortalData}>L\u00e0m m\u1edbi</button>
            <button className="ghost-button danger" onClick={logout}>{'\u0110\u0103ng xu\u1ea5t'}</button>
          </div>
        </header>

        {error && <div className="feedback error" style={{ margin: '0 auto 16px', maxWidth: 1100 }}>{error}</div>}
        {notice && <div className="feedback success" style={{ margin: '0 auto 16px', maxWidth: 1100 }}>{notice}</div>}

        <div className="student-portal-grid">
          {/* Th\u00f4ng tin c\u00e1 nh\u00e2n */}
          <section className="student-portal-card">
            <div className="student-portal-card-header">
              <h2>Th\u00f4ng tin c\u00e1 nh\u00e2n</h2>
              {!editing && <button className="ghost-button" onClick={startEdit}>Ch\u1ec9nh s\u1eeda</button>}
            </div>
            {s && !editing ? (
              <div className="student-portal-info">
                <div className="info-row"><span>M\u00e3 SV</span><strong>{s.studentCode}</strong></div>
                <div className="info-row"><span>H\u1ecd t\u00ean</span><strong>{s.name}</strong></div>
                <div className="info-row"><span>Gi\u1edbi t\u00ednh</span><strong>{s.gender}</strong></div>
                <div className="info-row"><span>Ng\u00e0y sinh</span><strong>{s.dateOfBirth ? dateFormat.format(new Date(s.dateOfBirth)) : '\u2014'}</strong></div>
                <div className="info-row"><span>{'\u0110i\u1ec7n tho\u1ea1i'}</span><strong>{s.phone || '\u2014'}</strong></div>
                <div className="info-row"><span>Email</span><strong>{s.email || '\u2014'}</strong></div>
                <div className="info-row"><span>Khoa</span><strong>{s.faculty || '\u2014'}</strong></div>
                <div className="info-row"><span>L\u1edbp</span><strong>{s.className || '\u2014'}</strong></div>
                <div className="info-row"><span>{'\u0110\u1ecba ch\u1ec9'}</span><strong>{s.address || '\u2014'}</strong></div>
                <div className="info-row"><span>Li\u00ean h\u1ec7 kh\u1ea9n</span><strong>{s.emergencyContact || '\u2014'}</strong></div>
                <div className="info-row"><span>Tr\u1ea1ng th\u00e1i</span><strong>{s.status}</strong></div>
              </div>
            ) : editing ? (
              <div className="student-portal-edit-form">
                <label>{'\u0110i\u1ec7n tho\u1ea1i'}<input value={editForm.phone} onChange={(e) => setEditForm({ ...editForm, phone: e.target.value })} /></label>
                <label>Email<input value={editForm.email} onChange={(e) => setEditForm({ ...editForm, email: e.target.value })} /></label>
                <label>{'\u0110\u1ecba ch\u1ec9'}<input value={editForm.address} onChange={(e) => setEditForm({ ...editForm, address: e.target.value })} /></label>
                <label>Li\u00ean h\u1ec7 kh\u1ea9n c\u1ea5p<input value={editForm.emergencyContact} onChange={(e) => setEditForm({ ...editForm, emergencyContact: e.target.value })} /></label>
                <div className="student-portal-edit-actions">
                  <button className="primary-button" onClick={saveProfile}>L\u01b0u</button>
                  <button className="secondary-button" onClick={() => setEditing(false)}>H\u1ee7y</button>
                </div>
              </div>
            ) : <p>Kh\u00f4ng c\u00f3 d\u1eef li\u1ec7u.</p>}
          </section>

          {/* Ph\u00f2ng \u1edf */}
          <section className="student-portal-card">
            <h2>Ph\u00f2ng \u1edf &amp; B\u1ea1n c\u00f9ng ph\u00f2ng</h2>
            {room?.room ? (
              <>
                <div className="student-portal-info">
                  <div className="info-row"><span>Ph\u00f2ng</span><strong>{room.room.roomNumber}</strong></div>
                  <div className="info-row"><span>T\u00f2a nh\u00e0</span><strong>{room.room.buildingName}</strong></div>
                  <div className="info-row"><span>S\u1ee9c ch\u1ee9a</span><strong>{room.room.currentOccupancy}/{room.room.capacity}</strong></div>
                  <div className="info-row"><span>Tr\u1ea1ng th\u00e1i ph\u00f2ng</span><strong>{room.room.status}</strong></div>
                </div>
                {room.roommates?.length > 0 && (
                  <div style={{ marginTop: 12 }}>
                    <h3>B\u1ea1n c\u00f9ng ph\u00f2ng</h3>
                    <table><thead><tr><th>M\u00e3 SV</th><th>H\u1ecd t\u00ean</th><th>Khoa</th><th>S\u0110T</th></tr></thead>
                      <tbody>{room.roommates.map((rm, i) => (<tr key={i}><td>{rm.studentCode}</td><td>{rm.name}</td><td>{rm.faculty}</td><td>{rm.phone}</td></tr>))}</tbody>
                    </table>
                  </div>
                )}
              </>
            ) : (<p style={{ padding: '1rem', color: '#64748b' }}>B\u1ea1n ch\u01b0a \u0111\u01b0\u1ee3c x\u1ebfp ph\u00f2ng.</p>)}
          </section>

          {/* T\u00e0i ch\u00ednh */}
          <section className="student-portal-card full-width">
            <h2>C\u00f4ng n\u1ee3 t\u00e0i ch\u00ednh</h2>
            {finance?.shares?.length > 0 ? (
              <table>
                <thead><tr><th>K\u1ef3</th><th>H\u00f3a \u0111\u01a1n</th><th>T\u1ed5ng ph\u00f2ng</th><th>Ph\u1ea7n b\u1ea1n</th><th>{'\u0110\u00e3 n\u1ed9p'}</th><th>C\u00f2n l\u1ea1i</th><th>Tr\u1ea1ng th\u00e1i</th><th>H\u1ea1n n\u1ed9p</th><th>Thanh to\u00e1n</th></tr></thead>
                <tbody>
                  {finance.shares.map((sh) => (
                    <tr key={sh.id}>
                      <td>{sh.billingMonth ? new Intl.DateTimeFormat('vi-VN', { month: '2-digit', year: 'numeric' }).format(new Date(sh.billingMonth)) : '\u2014'}</td>
                      <td>{sh.invoiceCode || '\u2014'}</td>
                      <td>{currencyFormat.format(sh.roomTotal)}</td>
                      <td>{currencyFormat.format(sh.expectedAmount)}</td>
                      <td>{currencyFormat.format(sh.paidAmount)}</td>
                      <td>{currencyFormat.format(sh.remainingAmount)}</td>
                      <td><span className={`table-badge ${sh.status === 'Paid' ? 'emerald' : sh.status === 'PartiallyPaid' ? 'amber' : 'rose'}`}>{sh.status === 'Paid' ? '\u0110\u00e3 n\u1ed9p' : sh.status === 'PartiallyPaid' ? 'M\u1ed9t ph\u1ea7n' : 'Ch\u01b0a n\u1ed9p'}</span></td>
                      <td>{sh.dueDate ? dateFormat.format(new Date(sh.dueDate)) : '\u2014'}</td>
                      <td>{sh.status !== 'Paid' && sh.invoiceId ? <button className="primary-button compact" onClick={() => payInvoice(sh.invoiceId)}>VNPay</button> : '\u2014'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            ) : (<p style={{ padding: '1rem', color: '#64748b' }}>Ch\u01b0a c\u00f3 c\u00f4ng n\u1ee3 t\u00e0i ch\u00ednh.</p>)}
            {finance?.roommateShares?.length > 0 ? (
              <div className="roommate-finance-block">
                <h3>Tr\u1ea1ng th\u00e1i \u0111\u00f3ng ph\u00ed c\u00e1c th\u00e0nh vi\u00ean trong ph\u00f2ng</h3>
                <table>
                  <thead><tr><th>K\u1ef3</th><th>Sinh vi\u00ean</th><th>Ph\u1ea3i n\u1ed9p</th><th>{'\u0110\u00e3 n\u1ed9p'}</th><th>C\u00f2n l\u1ea1i</th><th>Tr\u1ea1ng th\u00e1i</th></tr></thead>
                  <tbody>
                    {finance.roommateShares.map((item) => (
                      <tr key={item.id}>
                        <td>{item.billingMonth ? new Intl.DateTimeFormat('vi-VN', { month: '2-digit', year: 'numeric' }).format(new Date(item.billingMonth)) : '\u2014'}</td>
                        <td>{item.studentName} <span className="muted-line">{item.studentCode}</span></td>
                        <td>{currencyFormat.format(item.expectedAmount)}</td>
                        <td>{currencyFormat.format(item.paidAmount)}</td>
                        <td>{currencyFormat.format(item.remainingAmount)}</td>
                        <td><span className={`table-badge ${item.status === 'Paid' ? 'emerald' : item.status === 'PartiallyPaid' ? 'amber' : 'rose'}`}>{item.status === 'Paid' ? '\u0110\u00e3 n\u1ed9p' : item.status === 'PartiallyPaid' ? 'M\u1ed9t ph\u1ea7n' : 'Ch\u01b0a n\u1ed9p'}</span></td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            ) : null}
          </section>

          {/* Y\u00eau c\u1ea7u chuy\u1ec3n ph\u00f2ng */}
          <section className="student-portal-card">
            <h2>Y\u00eau c\u1ea7u chuy\u1ec3n ph\u00f2ng</h2>
            {room?.room && (
              <div className="student-portal-edit-form">
                <label>Ph\u00f2ng mu\u1ed1n chuy\u1ec3n \u0111\u1ebfn
                  <select value={transferForm.desiredRoomId} onChange={(e) => setTransferForm({ ...transferForm, desiredRoomId: e.target.value })}>
                    <option value="">{'\u2014 Ch\u1ecdn ph\u00f2ng \u2014'}</option>
                    {rooms.filter(r => r.id !== profile?.student?.roomId).map(r => (<option key={r.id} value={r.id}>{r.roomNumber} ({r.buildingName})</option>))}
                  </select>
                </label>
                <label>L\u00fd do<textarea value={transferForm.reason} onChange={(e) => setTransferForm({ ...transferForm, reason: e.target.value })} rows={3} /></label>
                <button className="primary-button" onClick={submitTransfer}>G\u1eedi y\u00eau c\u1ea7u</button>
              </div>
            )}
            {transfers.length > 0 && (
              <div style={{ marginTop: 16 }}>
                <h3>L\u1ecbch s\u1eed y\u00eau c\u1ea7u</h3>
                <table>
                  <thead><tr><th>Ph\u00f2ng hi\u1ec7n t\u1ea1i</th><th>Ph\u00f2ng mu\u1ed1n</th><th>L\u00fd do</th><th>Tr\u1ea1ng th\u00e1i</th><th>Ghi ch\u00fa</th></tr></thead>
                  <tbody>{transfers.map(t => (
                    <tr key={t.id}><td>{t.currentRoom}</td><td>{t.desiredRoom}</td><td>{t.reason}</td>
                      <td><span className={`table-badge ${t.status === 'Approved' ? 'emerald' : t.status === 'Rejected' ? 'rose' : 'amber'}`}>{t.status === 'Approved' ? '\u0110\u00e3 duy\u1ec7t' : t.status === 'Rejected' ? 'T\u1eeb ch\u1ed1i' : 'Ch\u1edd duy\u1ec7t'}</span></td>
                      <td>{t.decisionNote || '\u2014'}</td></tr>
                  ))}</tbody>
                </table>
              </div>
            )}
          </section>

          {/* Chat */}
          <section className="student-portal-card">
            <h2>Nh\u1eafn tin v\u1edbi qu\u1ea3n l\u00fd</h2>
            <div className="student-portal-edit-form">
              <label>G\u1eedi \u0111\u1ebfn
                <select value={chatForm.receiverId} onChange={(e) => setChatForm({ ...chatForm, receiverId: e.target.value })}>
                  <option value="">{'\u2014 Ch\u1ecdn qu\u1ea3n l\u00fd \u2014'}</option>
                  {managers.map(m => (<option key={m.id} value={m.id}>{m.fullName || m.username} ({m.roleName})</option>))}
                </select>
              </label>
              <label>N\u1ed9i dung<textarea value={chatForm.content} onChange={(e) => setChatForm({ ...chatForm, content: e.target.value })} rows={3} placeholder="Nh\u1eadp tin nh\u1eafn..." /></label>
              <button className="primary-button" onClick={sendMessage}>G\u1eedi</button>
            </div>
            {messages.length > 0 && (
              <div style={{ marginTop: 16, maxHeight: 300, overflowY: 'auto' }}>
                <h3>Tin nh\u1eafn</h3>
                {messages.map(msg => (
                  <div key={msg.id} className={`chat-bubble ${msg.senderId === user?.id ? 'sent' : 'received'}`}>
                    <div className="chat-meta"><strong>{msg.senderId === user?.id ? 'B\u1ea1n' : msg.senderName}</strong><span>{msg.createdAt ? timeFormat.format(new Date(msg.createdAt)) : ''}</span></div>
                    <p>{msg.content}</p>
                  </div>
                ))}
              </div>
            )}
          </section>
        </div>
      </div>
    </div>
  )
}
