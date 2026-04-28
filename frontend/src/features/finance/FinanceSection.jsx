import { useState } from 'react'
import { CrudPanel, Panel } from '../../components'
import { apiFetch, currencyFormat, numberFormat, readError, shortDate } from '../../helpers'

function downloadExcel(url, filename) {
  apiFetch(url).then(res => res.blob()).then(blob => {
    const a = document.createElement('a')
    a.href = URL.createObjectURL(blob)
    a.download = filename
    a.click()
    URL.revokeObjectURL(a.href)
  })
}

function statusLabel(status) {
  if (status === 'Paid') return 'Đã nộp'
  if (status === 'PartiallyPaid') return 'Một phần'
  if (status === 'Late') return 'Quá hạn'
  return 'Chưa nộp'
}

export function FinanceSection({
  data,
  financeSummary,
  executeAction,
  openCreate,
  openEdit,
  deleteEntity,
  getPanelProps,
  refreshData,
}) {
  const [selectedRecord, setSelectedRecord] = useState(null)
  const [shares, setShares] = useState([])
  const [shareForms, setShareForms] = useState({})
  const [shareError, setShareError] = useState('')
  const [shareNotice, setShareNotice] = useState('')
  const [shareLoading, setShareLoading] = useState(false)

  const totalExpected = data.roomFinances.reduce((sum, item) => sum + Number(item.total ?? 0), 0)
  const totalCollected = data.roomFinances.reduce((sum, item) => sum + Number(item.paidAmount ?? 0), 0)
  const totalOutstanding = Math.max(0, totalExpected - totalCollected)
  const paidRooms = data.roomFinances.filter((item) => item.status === 'Paid').length
  const partialRooms = data.roomFinances.filter((item) => item.status === 'PartiallyPaid').length
  const lateRooms = data.roomFinances.filter((item) => item.status === 'Late').length
  const collectionRate = totalExpected > 0 ? Math.round((totalCollected / totalExpected) * 100) : 0

  async function loadShares(record) {
    try {
      setShareLoading(true)
      setShareError('')
      setShareNotice('')
      setSelectedRecord(record)
      const response = await apiFetch(`/api/operations/room-finances/${record.id}/shares`)
      if (!response.ok) {
        throw new Error(await readError(response))
      }

      const items = await response.json()
      setShares(items)
      setShareForms(Object.fromEntries(items.map((item) => [
        item.id,
        {
          expectedAmount: item.expectedAmount,
          paidAmount: Math.max(0, Number(item.remainingAmount ?? item.expectedAmount ?? 0)),
          paymentMethod: item.paymentMethod || '',
          note: item.note || '',
        },
      ])))
    } catch (error) {
      setShareError(error.message)
    } finally {
      setShareLoading(false)
    }
  }

  function updateShareForm(id, field, value) {
    setShareForms((current) => ({
      ...current,
      [id]: {
        ...(current[id] || {}),
        [field]: value,
      },
    }))
  }

  async function generateShares() {
    if (!selectedRecord) return
    await executeAction(
      () => apiFetch(`/api/operations/room-finances/${selectedRecord.id}/generate-shares`, { method: 'POST' }),
      'Đã chia tiền và tạo hóa đơn riêng cho từng sinh viên.',
    )
    await refreshData?.()
    await loadShares(selectedRecord)
  }

  async function adjustShare(share) {
    const form = shareForms[share.id] || {}
    try {
      setShareError('')
      setShareNotice('')
      const response = await apiFetch(`/api/operations/room-finance-shares/${share.id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          expectedAmount: Number(form.expectedAmount || 0),
          note: form.note || '',
        }),
      })

      if (!response.ok) {
        throw new Error(await readError(response))
      }

      setShareNotice(`Đã cập nhật phần tiền của ${share.studentName}.`)
      await refreshData?.()
      await loadShares(selectedRecord)
    } catch (error) {
      setShareError(error.message)
    }
  }

  async function payShare(share) {
    const form = shareForms[share.id] || {}
    try {
      setShareError('')
      setShareNotice('')
      const response = await apiFetch(`/api/operations/room-finance-shares/${share.id}/mark-paid`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          paidAmount: Number(form.paidAmount || 0),
          paidDate: new Date().toISOString(),
          paymentMethod: form.paymentMethod || 'Cash',
          note: form.note || 'Thu tiền theo phần chia sinh viên.',
        }),
      })

      if (!response.ok) {
        throw new Error(await readError(response))
      }

      setShareNotice(`Đã ghi nhận thanh toán của ${share.studentName}.`)
      await refreshData?.()
      await loadShares(selectedRecord)
    } catch (error) {
      setShareError(error.message)
    }
  }

  return (
    <>
      <div className="section-toolbar">
        <button className="secondary-button" onClick={() => downloadExcel('/api/export/students', 'danh-sach-sinh-vien.xlsx')}>Xuất Excel sinh viên</button>
        <button className="secondary-button" onClick={() => downloadExcel('/api/export/room-finances', 'cong-no-phong.xlsx')}>Xuất Excel công nợ</button>
        <button className="secondary-button" onClick={() => downloadExcel('/api/export/invoices', 'hoa-don.xlsx')}>Xuất Excel hóa đơn</button>
      </div>
      <CrudPanel
        entityKey="utilities"
        rows={data.utilities}
        onCreate={openCreate}
        onEdit={openEdit}
        onDelete={deleteEntity}
        panelProps={getPanelProps('finance-utilities')}
        extraRowActions={(item) => [
          {
            label: 'Tạo hóa đơn',
            kind: 'approve',
            onClick: () => executeAction(() => apiFetch(`/api/operations/utilities/${item.id}/generate-invoices`, { method: 'POST' }), 'Đã tạo hóa đơn từ kỳ điện nước.'),
          },
        ]}
      />

      <CrudPanel
        entityKey="invoices"
        rows={data.invoices}
        onCreate={openCreate}
        onEdit={openEdit}
        onDelete={deleteEntity}
        panelProps={{ ...getPanelProps('finance-invoices'), panelId: 'panel-finance-invoices' }}
        extraRowActions={(item) => [
          item.status !== 'Paid'
            ? {
                label: 'Thu tiền',
                kind: 'approve',
                onClick: () =>
                  executeAction(
                    () =>
                      apiFetch(`/api/operations/invoices/${item.id}/mark-paid`, {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({ paidDate: new Date().toISOString() }),
                      }),
                    'Đã ghi nhận thanh toán hóa đơn.',
                  ),
              }
            : null,
        ]}
      />

      <Panel title="Tổng quan tài chính theo phòng" description="Doanh thu tháng đang thể hiện tổng tiền phải thu của các kỳ tài chính hiện có. Khối này giúp nhìn rõ tiền đã thu, còn nợ và trạng thái từng phòng." {...getPanelProps('finance-summary')}>
        <div className="finance-caption">
          {financeSummary?.billingMonth ? `Kỳ theo dõi: ${new Intl.DateTimeFormat('vi-VN', { month: '2-digit', year: 'numeric' }).format(new Date(financeSummary.billingMonth))}` : 'Theo dõi tổng hợp công nợ theo từng phòng.'}
        </div>
        <div className="finance-highlight-grid">
          <article className="finance-highlight-card blue">
            <span>Tổng phải thu</span>
            <strong>{currencyFormat.format(totalExpected)}</strong>
          </article>
          <article className="finance-highlight-card emerald">
            <span>Đã thu</span>
            <strong>{currencyFormat.format(totalCollected)}</strong>
          </article>
          <article className="finance-highlight-card amber">
            <span>Còn phải thu</span>
            <strong>{currencyFormat.format(totalOutstanding)}</strong>
          </article>
          <article className="finance-highlight-card rose">
            <span>Phòng quá hạn</span>
            <strong>{numberFormat.format(lateRooms)}</strong>
          </article>
        </div>
        <div className="collection-progress-card">
          <div className="collection-progress-copy">
            <span>Tỷ lệ hoàn thành thu tiền</span>
            <strong>{collectionRate}%</strong>
          </div>
          <div className="collection-progress-bar">
            <span style={{ width: `${collectionRate}%` }} />
          </div>
        </div>
        <div className="finance-status-grid">
          <div className="summary-block">
            <span>Phòng đã thanh toán đủ</span>
            <strong>{numberFormat.format(paidRooms)}</strong>
          </div>
          <div className="summary-block">
            <span>Phòng thanh toán một phần</span>
            <strong>{numberFormat.format(partialRooms)}</strong>
          </div>
          <div className="summary-block">
            <span>Phòng còn công nợ</span>
            <strong>{numberFormat.format(data.roomFinances.filter((item) => item.status !== 'Paid').length)}</strong>
          </div>
        </div>
      </Panel>

      <CrudPanel
        entityKey="roomFeeProfiles"
        rows={data.roomFeeProfiles}
        onCreate={openCreate}
        onEdit={openEdit}
        onDelete={deleteEntity}
        panelProps={getPanelProps('finance-room-fee-profiles')}
      />

      <CrudPanel
        entityKey="roomFinances"
        rows={data.roomFinances}
        onCreate={openCreate}
        onEdit={openEdit}
        onDelete={deleteEntity}
        panelProps={{ ...getPanelProps('finance-room-finances'), panelId: 'panel-finance-room-finances' }}
        extraRowActions={(item) => [
          {
            label: 'Chi tiết SV',
            kind: 'approve',
            onClick: () => loadShares(item),
          },
          item.utilityId
            ? {
                label: 'Đồng bộ từ điện nước',
                kind: 'approve',
                onClick: () =>
                  executeAction(
                    () => apiFetch(`/api/operations/room-finances/generate-from-utility/${item.utilityId}`, { method: 'POST' }),
                    'Đã cập nhật công nợ phòng từ dữ liệu điện nước.',
                  ),
              }
            : null,
          item.shareCount === 0
            ? {
                label: 'Chia tiền + tạo hóa đơn',
                kind: 'approve',
                onClick: () =>
                  executeAction(
                    () => apiFetch(`/api/operations/room-finances/${item.id}/generate-shares`, { method: 'POST' }),
                    'Đã chia tiền và tạo hóa đơn riêng cho từng sinh viên.',
                  ),
              }
            : null,
          item.status !== 'Paid'
            ? {
                label: 'Đánh dấu đã thu',
                kind: 'approve',
                onClick: () =>
                  executeAction(
                    () =>
                      apiFetch(`/api/operations/room-finances/${item.id}/mark-paid`, {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({
                          paidAmount: Number(item.remainingAmount ?? item.total ?? 0),
                          paidDate: new Date().toISOString(),
                          paymentMethod: 'Cash',
                          paymentNote: 'Đã thu tiền phòng từ giao diện tài chính.',
                          recordedBy: 'Kế toán',
                        }),
                      }),
                    'Đã ghi nhận phòng đã nộp tiền.',
                  ),
              }
            : null,
        ]}
      />

      {selectedRecord ? (
        <Panel
          title={`Chi tiết thu tiền sinh viên - phòng ${selectedRecord.roomNumber}`}
          description="Theo dõi phần tiền từng sinh viên, chỉnh số phải nộp và ghi nhận thanh toán theo từng người."
          {...getPanelProps('finance-student-shares')}
        >
          {shareError ? <div className="feedback error">{shareError}</div> : null}
          {shareNotice ? <div className="feedback success">{shareNotice}</div> : null}
          <div className="finance-share-summary">
            <div className="summary-block">
              <span>Kỳ công nợ</span>
              <strong>{selectedRecord.billingMonth ? shortDate(selectedRecord.billingMonth) : '-'}</strong>
            </div>
            <div className="summary-block">
              <span>Tổng phòng</span>
              <strong>{currencyFormat.format(selectedRecord.total || 0)}</strong>
            </div>
            <div className="summary-block">
              <span>Đã thu phòng</span>
              <strong>{currencyFormat.format(selectedRecord.paidAmount || 0)}</strong>
            </div>
            <button className="secondary-button" onClick={() => loadShares(selectedRecord)} disabled={shareLoading}>
              {shareLoading ? 'Đang tải...' : 'Làm mới phần chia'}
            </button>
          </div>

          {shares.length === 0 ? (
            <div className="empty-state">
              Chưa có phần chia cho sinh viên trong phòng này.
              <div style={{ marginTop: 12 }}>
                <button className="primary-button" onClick={generateShares}>Chia tiền cho sinh viên</button>
              </div>
            </div>
          ) : (
            <div className="table-wrap">
              <table className="data-table finance-share-table">
                <thead>
                  <tr>
                    <th>Sinh viên</th>
                    <th>Hóa đơn</th>
                    <th>Phải nộp</th>
                    <th>Đã nộp</th>
                    <th>Còn lại</th>
                    <th>Trạng thái</th>
                    <th>Ghi nhận</th>
                    <th>Thao tác</th>
                  </tr>
                </thead>
                <tbody>
                  {shares.map((share) => {
                    const form = shareForms[share.id] || {}
                    return (
                      <tr key={share.id}>
                        <td data-label="Sinh viên">
                          <strong>{share.studentName}</strong>
                          <span className="muted-line">{share.studentCode}</span>
                        </td>
                        <td data-label="Hóa đơn">
                          <strong>{share.invoiceCode || '-'}</strong>
                          <span className="muted-line">{share.invoiceStatus || '-'}</span>
                        </td>
                        <td data-label="Phải nộp">
                          <input type="number" min="0" value={form.expectedAmount ?? ''} onChange={(event) => updateShareForm(share.id, 'expectedAmount', event.target.value)} />
                        </td>
                        <td data-label="Đã nộp">{currencyFormat.format(share.paidAmount || 0)}</td>
                        <td data-label="Còn lại">{currencyFormat.format(share.remainingAmount || 0)}</td>
                        <td data-label="Trạng thái">
                          <span className={`table-badge ${share.status === 'Paid' ? 'emerald' : share.status === 'PartiallyPaid' ? 'amber' : 'rose'}`}>
                            {statusLabel(share.status)}
                          </span>
                        </td>
                        <td data-label="Ghi nhận">
                          <div className="share-payment-fields">
                            <input type="number" min="0" value={form.paidAmount ?? ''} onChange={(event) => updateShareForm(share.id, 'paidAmount', event.target.value)} placeholder="Số tiền thu" />
                            <select value={form.paymentMethod || ''} onChange={(event) => updateShareForm(share.id, 'paymentMethod', event.target.value)}>
                              <option value="">Hình thức thu</option>
                              {data.paymentMethods.map((method) => (
                                <option key={method.id} value={method.code}>{method.name}</option>
                              ))}
                              <option value="Cash">Tiền mặt</option>
                            </select>
                            <input value={form.note || ''} onChange={(event) => updateShareForm(share.id, 'note', event.target.value)} placeholder="Ghi chú" />
                          </div>
                        </td>
                        <td data-label="Thao tác">
                          <div className="action-row">
                            <button className="ghost-button" onClick={() => adjustShare(share)}>Lưu phần tiền</button>
                            <button className="ghost-button approve" onClick={() => payShare(share)}>Thu khoản này</button>
                          </div>
                        </td>
                      </tr>
                    )
                  })}
                </tbody>
              </table>
            </div>
          )}
        </Panel>
      ) : null}
    </>
  )
}
