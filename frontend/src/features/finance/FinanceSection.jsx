import { CrudPanel, Panel } from '../../components'
import { currencyFormat, numberFormat } from '../../helpers'

export function FinanceSection({
  data,
  financeSummary,
  executeAction,
  openCreate,
  openEdit,
  deleteEntity,
  getPanelProps,
}) {
  const totalExpected = data.roomFinances.reduce((sum, item) => sum + Number(item.total ?? 0), 0)
  const totalCollected = data.roomFinances.reduce((sum, item) => sum + Number(item.paidAmount ?? 0), 0)
  const totalOutstanding = Math.max(0, totalExpected - totalCollected)
  const paidRooms = data.roomFinances.filter((item) => item.status === 'Paid').length
  const partialRooms = data.roomFinances.filter((item) => item.status === 'PartiallyPaid').length
  const lateRooms = data.roomFinances.filter((item) => item.status === 'Late').length
  const collectionRate = totalExpected > 0 ? Math.round((totalCollected / totalExpected) * 100) : 0

  return (
    <>
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
            onClick: () => executeAction(() => fetch(`/api/operations/utilities/${item.id}/generate-invoices`, { method: 'POST' }), 'Đã tạo hóa đơn từ kỳ điện nước.'),
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
                      fetch(`/api/operations/invoices/${item.id}/mark-paid`, {
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
          item.utilityId
            ? {
                label: 'Đồng bộ từ điện nước',
                kind: 'approve',
                onClick: () =>
                  executeAction(
                    () => fetch(`/api/operations/room-finances/generate-from-utility/${item.utilityId}`, { method: 'POST' }),
                    'Đã cập nhật công nợ phòng từ dữ liệu điện nước.',
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
                      fetch(`/api/operations/room-finances/${item.id}/mark-paid`, {
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
    </>
  )
}
