import {
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  ComposedChart,
  Legend,
  Line,
  Pie,
  PieChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import { Panel } from "../../components";
import { currencyFormat, localizeValue, numberFormat } from "../../helpers";

const chartColors = ["#0f7b6c", "#1d4ed8", "#f59e0b", "#e11d48", "#7c3aed"];

function violationTypeLabel(type) {
  if (type === "Noise") return "Gây ồn";
  if (type === "Smoking") return "Hút thuốc";
  if (type === "PropertyDamage") return "Phá hoại tài sản";
  return localizeValue(type);
}

function equipmentLabel(type) {
  if (type === "Fan") return "Quạt";
  if (type === "AirConditioner") return "Điều hòa";
  if (type === "Light") return "Bóng đèn";
  if (type === "Other") return "Khác";
  return localizeValue(type);
}

function statusLabel(status) {
  return localizeValue(status);
}

function compactCurrency(value) {
  const numericValue = Number(value) || 0;
  if (Math.abs(numericValue) >= 1000000000) {
    return `${numberFormat.format(Math.round(numericValue / 1000000000))} tỷ`;
  }
  if (Math.abs(numericValue) >= 1000000) {
    return `${numberFormat.format(Math.round(numericValue / 1000000))} tr`;
  }
  if (Math.abs(numericValue) >= 1000) {
    return `${numberFormat.format(Math.round(numericValue / 1000))}k`;
  }
  return numberFormat.format(numericValue);
}

function RevenueTooltip({ active, payload, label }) {
  if (!active || !payload?.length) return null;

  const labels = {
    total: "Tổng phải thu",
    paid: "Đã thu",
    unpaid: "Còn phải thu",
  };

  return (
    <div className="revenue-tooltip">
      <strong>{label}</strong>
      {payload.map((entry) => (
        <div key={entry.dataKey}>
          <span style={{ background: entry.color }} />
          <p>{labels[entry.dataKey] ?? entry.name}</p>
          <b>{currencyFormat.format(entry.value || 0)}</b>
        </div>
      ))}
    </div>
  );
}

export function OverviewSection({ dashboard, getPanelProps }) {
  const violationSummary = dashboard.violationByType ?? [];
  const maintenanceByStatus = dashboard.maintenanceByStatus ?? [];
  const maintenanceByEquipment = dashboard.maintenanceByEquipment ?? [];
  const monthlyRevenue = dashboard.monthlyRevenue ?? [];
  const revenueTotals = monthlyRevenue.reduce(
    (totals, item) => ({
      total: totals.total + Number(item.total ?? 0),
      paid: totals.paid + Number(item.paid ?? 0),
      unpaid: totals.unpaid + Number(item.unpaid ?? 0),
    }),
    { total: 0, paid: 0, unpaid: 0 },
  );
  const revenueCollectionRate =
    revenueTotals.total > 0
      ? Math.min(100, Math.round((revenueTotals.paid / revenueTotals.total) * 100))
      : 0;
  const revenueOutstandingRate =
    revenueTotals.total > 0
      ? Math.min(100, Math.round((revenueTotals.unpaid / revenueTotals.total) * 100))
      : 0;
  const latestRevenueMonth = monthlyRevenue[monthlyRevenue.length - 1] ?? null;
  const latestMonthRate =
    latestRevenueMonth?.total > 0
      ? Math.min(
          100,
          Math.round((Number(latestRevenueMonth.paid ?? 0) / Number(latestRevenueMonth.total)) * 100),
        )
      : 0;

  return (
    <>
      <section className="section-grid">
        <Panel
          title="Cảnh báo vận hành"
          description="Những đầu việc cần ưu tiên xử lý trong ngày."
          {...getPanelProps("overview-alerts")}
        >
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

        <Panel
          title="Snapshot phòng"
          description="Trạng thái nhanh của một số phòng nổi bật."
          {...getPanelProps("overview-snapshots")}
        >
          <div className="snapshot-grid">
            {dashboard.roomSnapshots.map((room) => (
              <article key={room.id} className="snapshot-card">
                <strong>{room.roomNumber}</strong>
                <span>{room.building}</span>
                <p>{localizeValue(room.status)}</p>
                <small>
                  {room.currentOccupancy}/{room.capacity} chỗ đang sử dụng
                </small>
              </article>
            ))}
          </div>
        </Panel>
      </section>

      <section className="overview-insight-grid">
        <Panel
          title="Vi phạm & điểm rèn luyện"
          description="Theo dõi số biên bản, tiền phạt chưa thu và điểm rèn luyện bị trừ."
          {...getPanelProps("overview-violations")}
        >
          <div className="finance-highlight-grid">
            <article className="finance-highlight-card rose">
              <span>Vi phạm tháng này</span>
              <strong>
                {numberFormat.format(dashboard.summary.violationsThisMonth ?? 0)}
              </strong>
            </article>
            <article className="finance-highlight-card amber">
              <span>Tiền phạt chưa thu</span>
              <strong>
                {currencyFormat.format(dashboard.summary.unpaidViolationFines ?? 0)}
              </strong>
            </article>
            <article className="finance-highlight-card blue">
              <span>Điểm bị trừ tháng này</span>
              <strong>
                {numberFormat.format(
                  dashboard.summary.trainingPointsDeductedThisMonth ?? 0,
                )}
              </strong>
            </article>
          </div>
          <ResponsiveContainer width="100%" height={240}>
            <BarChart data={violationSummary}>
              <CartesianGrid strokeDasharray="3 3" vertical={false} />
              <XAxis dataKey="type" tickFormatter={violationTypeLabel} />
              <YAxis />
              <Tooltip
                labelFormatter={violationTypeLabel}
                formatter={(value, name) => [
                  name === "fineAmount"
                    ? currencyFormat.format(value)
                    : numberFormat.format(value),
                  name === "count"
                    ? "Số biên bản"
                    : name === "fineAmount"
                      ? "Tiền phạt"
                      : "Điểm trừ",
                ]}
              />
              <Bar dataKey="count" fill="#e11d48" radius={[10, 10, 0, 0]} />
              <Bar
                dataKey="trainingPointDeduction"
                fill="#f59e0b"
                radius={[10, 10, 0, 0]}
              />
            </BarChart>
          </ResponsiveContainer>
        </Panel>

        <Panel
          title="Bảo trì thiết bị"
          description="Theo dõi workflow sinh viên báo lỗi, admin duyệt, kỹ thuật xử lý và hoàn tất."
          {...getPanelProps("overview-maintenance")}
        >
          <div className="finance-highlight-grid">
            <article className="finance-highlight-card blue">
              <span>Đang mở</span>
              <strong>
                {numberFormat.format(dashboard.summary.openMaintenanceRequests ?? 0)}
              </strong>
            </article>
            <article className="finance-highlight-card rose">
              <span>Quá 48 giờ</span>
              <strong>
                {numberFormat.format(
                  dashboard.summary.overdueMaintenanceRequests ?? 0,
                )}
              </strong>
            </article>
          </div>
          <div className="maintenance-dashboard-grid">
            <ResponsiveContainer width="100%" height={240}>
              <PieChart>
                <Pie data={maintenanceByStatus} dataKey="count" nameKey="status" outerRadius={88}>
                  {maintenanceByStatus.map((entry, index) => (
                    <Cell
                      key={entry.status}
                      fill={chartColors[index % chartColors.length]}
                    />
                  ))}
                </Pie>
                <Tooltip
                  formatter={(value) => numberFormat.format(value)}
                  labelFormatter={statusLabel}
                />
              </PieChart>
            </ResponsiveContainer>
            <div className="maintenance-equipment-list">
              {maintenanceByEquipment.length > 0 ? (
                maintenanceByEquipment.map((item) => (
                  <div key={item.equipment} className="summary-block compact">
                    <span>{equipmentLabel(item.equipment)}</span>
                    <strong>{numberFormat.format(item.count)}</strong>
                  </div>
                ))
              ) : (
                <div className="empty-state">Chưa có yêu cầu bảo trì.</div>
              )}
            </div>
          </div>
        </Panel>
      </section>

      <section className="chart-grid">
        <Panel
          title="Công suất theo tòa"
          description="So sánh mức sử dụng và công suất của từng khu nhà."
          {...getPanelProps("overview-occupancy-chart")}
          panelId="panel-overview-occupancy-chart"
        >
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

        <Panel
          title="Trạng thái hóa đơn"
          description="Tỷ lệ hóa đơn đã thanh toán, chưa thanh toán và quá hạn."
          {...getPanelProps("overview-invoice-chart")}
        >
          <ResponsiveContainer width="100%" height={280}>
            <PieChart>
              <Pie data={dashboard.invoiceStatus} dataKey="count" nameKey="status" outerRadius={96}>
                {dashboard.invoiceStatus.map((entry, index) => (
                  <Cell
                    key={entry.status}
                    fill={chartColors[index % chartColors.length]}
                  />
                ))}
              </Pie>
              <Tooltip formatter={(value) => numberFormat.format(value)} />
            </PieChart>
          </ResponsiveContainer>
        </Panel>

        <Panel
          title="Dòng doanh thu"
          description="Theo dõi tổng thu, đã thu và còn phải thu theo tháng."
          {...getPanelProps("overview-revenue-chart")}
        >
          <div className="revenue-panel">
            <div className="revenue-block-grid">
              <article className="revenue-block total">
                <div className="revenue-block-topline">
                  <span>Tổng phải thu</span>
                  <small>{latestRevenueMonth?.month ?? "Chưa có kỳ"}</small>
                </div>
                <strong>{currencyFormat.format(revenueTotals.total)}</strong>
                <p>Toàn bộ khoản phải thu đã phát sinh qua các kỳ.</p>
                <div className="revenue-block-track">
                  <span style={{ width: revenueTotals.total > 0 ? "100%" : "0%" }} />
                </div>
                <small className="revenue-block-footnote">
                  Kỳ mới nhất đạt {latestMonthRate}% tiến độ thu.
                </small>
              </article>

              <article className="revenue-block paid">
                <div className="revenue-block-topline">
                  <span>Đã thu</span>
                  <small>{revenueCollectionRate}%</small>
                </div>
                <strong>{currencyFormat.format(revenueTotals.paid)}</strong>
                <p>Dòng tiền đã hoàn tất, dùng để đối chiếu tỷ lệ thu.</p>
                <div className="revenue-block-track">
                  <span style={{ width: `${revenueCollectionRate}%` }} />
                </div>
                <small className="revenue-block-footnote">
                  Còn {currencyFormat.format(revenueTotals.total - revenueTotals.paid)} so với tổng phải thu.
                </small>
              </article>

              <article className="revenue-block unpaid">
                <div className="revenue-block-topline">
                  <span>Còn phải thu</span>
                  <small>{revenueOutstandingRate}%</small>
                </div>
                <strong>{currencyFormat.format(revenueTotals.unpaid)}</strong>
                <p>Các khoản còn mở, cần ưu tiên nhắc thu và xử lý quá hạn.</p>
                <div className="revenue-block-track">
                  <span style={{ width: `${revenueOutstandingRate}%` }} />
                </div>
                <small className="revenue-block-footnote">
                  Theo dõi sát các kỳ có tỷ lệ nợ cao.
                </small>
              </article>
            </div>

            <div className="revenue-summary-strip">
              <article>
                <span>Tổng phải thu</span>
                <strong>{currencyFormat.format(revenueTotals.total)}</strong>
                <small>Ghi nhận qua các kỳ</small>
              </article>
              <article>
                <span>Đã thu</span>
                <strong>{currencyFormat.format(revenueTotals.paid)}</strong>
                <small>{revenueCollectionRate}% hoàn thành</small>
              </article>
              <article>
                <span>Còn phải thu</span>
                <strong>{currencyFormat.format(revenueTotals.unpaid)}</strong>
                <small>Cần theo dõi thu nợ</small>
              </article>
            </div>

            <div className="revenue-health-row">
              <div className="revenue-health-copy">
                <span>Tỷ lệ thu toàn kỳ</span>
                <strong>{revenueCollectionRate}%</strong>
              </div>
              <div className="revenue-health-track">
                <span style={{ width: `${revenueCollectionRate}%` }} />
              </div>
              <div className="revenue-latest-chip">
                <span>Kỳ mới nhất</span>
                <strong>
                  {latestRevenueMonth
                    ? `${latestRevenueMonth.month} - ${latestMonthRate}%`
                    : "Chưa có dữ liệu"}
                </strong>
              </div>
            </div>

            <div className="revenue-chart-grid">
              <article className="revenue-chart-card total">
                <div className="revenue-chart-heading">
                  <span>Tổng phải thu</span>
                  <strong>{compactCurrency(revenueTotals.total)}</strong>
                </div>
                <ResponsiveContainer width="100%" height={220}>
                  <BarChart data={monthlyRevenue} margin={{ top: 10, right: 12, bottom: 0, left: 0 }}>
                    <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#dbe3ef" />
                    <XAxis dataKey="month" tickLine={false} axisLine={false} />
                    <YAxis width={50} tickLine={false} axisLine={false} tickFormatter={compactCurrency} />
                    <Tooltip content={<RevenueTooltip />} />
                    <Bar dataKey="total" fill="#1d4ed8" radius={[8, 8, 0, 0]} barSize={34} />
                  </BarChart>
                </ResponsiveContainer>
              </article>

              <article className="revenue-chart-card paid">
                <div className="revenue-chart-heading">
                  <span>Đã thu</span>
                  <strong>{compactCurrency(revenueTotals.paid)}</strong>
                </div>
                <ResponsiveContainer width="100%" height={220}>
                  <BarChart data={monthlyRevenue} margin={{ top: 10, right: 12, bottom: 0, left: 0 }}>
                    <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#dbe3ef" />
                    <XAxis dataKey="month" tickLine={false} axisLine={false} />
                    <YAxis width={50} tickLine={false} axisLine={false} tickFormatter={compactCurrency} />
                    <Tooltip content={<RevenueTooltip />} />
                    <Bar dataKey="paid" fill="#0f7b6c" radius={[8, 8, 0, 0]} barSize={34} />
                  </BarChart>
                </ResponsiveContainer>
              </article>

              <article className="revenue-chart-card unpaid">
                <div className="revenue-chart-heading">
                  <span>Còn phải thu</span>
                  <strong>{compactCurrency(revenueTotals.unpaid)}</strong>
                </div>
                <ResponsiveContainer width="100%" height={220}>
                  <BarChart data={monthlyRevenue} margin={{ top: 10, right: 12, bottom: 0, left: 0 }}>
                    <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#dbe3ef" />
                    <XAxis dataKey="month" tickLine={false} axisLine={false} />
                    <YAxis width={50} tickLine={false} axisLine={false} tickFormatter={compactCurrency} />
                    <Tooltip content={<RevenueTooltip />} />
                    <Bar dataKey="unpaid" fill="#e11d48" radius={[8, 8, 0, 0]} barSize={34} />
                  </BarChart>
                </ResponsiveContainer>
              </article>
            </div>

            <ResponsiveContainer className="revenue-combo-chart" width="100%" height={340}>
              <ComposedChart
                data={monthlyRevenue}
                margin={{ top: 18, right: 30, bottom: 8, left: 22 }}
              >
                <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#dbe3ef" />
                <XAxis dataKey="month" tickLine={false} axisLine={false} />
                <YAxis
                  width={78}
                  tickLine={false}
                  axisLine={false}
                  tickFormatter={compactCurrency}
                />
                <Tooltip content={<RevenueTooltip />} />
                <Legend
                  verticalAlign="top"
                  align="right"
                  iconType="circle"
                  formatter={(value) =>
                    ({
                      total: "Tổng phải thu",
                      paid: "Đã thu",
                      unpaid: "Còn phải thu",
                    })[value] ?? value
                  }
                />
                <Bar
                  dataKey="paid"
                  stackId="revenue"
                  fill="#0f7b6c"
                  radius={[0, 0, 8, 8]}
                  barSize={42}
                />
                <Bar
                  dataKey="unpaid"
                  stackId="revenue"
                  fill="#e11d48"
                  radius={[8, 8, 0, 0]}
                  barSize={42}
                />
                <Line
                  type="monotone"
                  dataKey="total"
                  stroke="#1d4ed8"
                  strokeWidth={3}
                  dot={{ r: 4, strokeWidth: 2, fill: "#ffffff" }}
                  activeDot={{ r: 6 }}
                />
                <Line
                  type="monotone"
                  dataKey="unpaid"
                  stroke="#e11d48"
                  strokeDasharray="5 5"
                  strokeWidth={3}
                  dot={{ r: 4, strokeWidth: 2, fill: "#ffffff" }}
                  activeDot={{ r: 6 }}
                />
              </ComposedChart>
            </ResponsiveContainer>
          </div>
        </Panel>
      </section>
    </>
  );
}
