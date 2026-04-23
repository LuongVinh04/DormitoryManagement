import {
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  Line,
  LineChart,
  Pie,
  PieChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts'
import { Panel } from '../../components'
import { currencyFormat, localizeValue, numberFormat } from '../../helpers'

export function OverviewSection({ dashboard, getPanelProps }) {
  return (
    <>
      <section className="section-grid">
        <Panel title="Cảnh báo vận hành" description="Những đầu việc cần ưu tiên xử lý trong ngày." {...getPanelProps('overview-alerts')}>
          <div className="alert-list">
            {dashboard.alerts.map((alert) => (
              <div key={alert.title} className={`alert-card ${alert.level}`}>
                <div>
                  <strong>{alert.title}</strong>
                  <p>{alert.description}</p>
                </div>
                <span>{numberFormat.format(alert.value)}</span>
              </div>
            ))}
          </div>
        </Panel>

        <Panel title="Snapshot phòng" description="Trạng thái nhanh của một số phòng nổi bật." {...getPanelProps('overview-snapshots')}>
          <div className="snapshot-grid">
            {dashboard.roomSnapshots.map((room) => (
              <article key={room.id} className="snapshot-card">
                <strong>{room.roomNumber}</strong>
                <span>{room.building}</span>
                <p>{localizeValue(room.status)}</p>
                <small>{room.currentOccupancy}/{room.capacity} chỗ đang sử dụng</small>
              </article>
            ))}
          </div>
        </Panel>
      </section>

      <section className="chart-grid">
        <Panel title="Công suất theo tòa" description="So sánh mức sử dụng và công suất của từng khu nhà." {...getPanelProps('overview-occupancy-chart')}>
          <ResponsiveContainer width="100%" height={280}>
            <BarChart data={dashboard.occupancyByBuilding}>
              <CartesianGrid strokeDasharray="3 3" vertical={false} />
              <XAxis dataKey="building" />
              <YAxis />
              <Tooltip formatter={(value) => numberFormat.format(value)} />
              <Bar dataKey="occupied" fill="#0f7b6c" radius={[10, 10, 0, 0]} />
              <Bar dataKey="capacity" fill="#9bd6cf" radius={[10, 10, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </Panel>

        <Panel title="Trạng thái hóa đơn" description="Tỷ lệ hóa đơn đã thanh toán, chưa thanh toán và quá hạn." {...getPanelProps('overview-invoice-chart')}>
          <ResponsiveContainer width="100%" height={280}>
            <PieChart>
              <Pie data={dashboard.invoiceStatus} dataKey="count" nameKey="status" outerRadius={96}>
                {dashboard.invoiceStatus.map((entry, index) => (
                  <Cell key={entry.status} fill={['#0f7b6c', '#f59e0b', '#e11d48'][index % 3]} />
                ))}
              </Pie>
              <Tooltip formatter={(value) => numberFormat.format(value)} />
            </PieChart>
          </ResponsiveContainer>
        </Panel>

        <Panel title="Dòng doanh thu" description="Theo dõi tổng thu, đã thu và còn phải thu theo tháng." {...getPanelProps('overview-revenue-chart')}>
          <ResponsiveContainer width="100%" height={300}>
            <LineChart data={dashboard.monthlyRevenue}>
              <CartesianGrid strokeDasharray="3 3" vertical={false} />
              <XAxis dataKey="month" />
              <YAxis />
              <Tooltip formatter={(value) => currencyFormat.format(value)} />
              <Line type="monotone" dataKey="total" stroke="#1d4ed8" strokeWidth={3} />
              <Line type="monotone" dataKey="paid" stroke="#0f7b6c" strokeWidth={3} />
              <Line type="monotone" dataKey="unpaid" stroke="#e11d48" strokeWidth={3} />
            </LineChart>
          </ResponsiveContainer>
        </Panel>
      </section>
    </>
  )
}
