import { ActionCard, CrudPanel, Panel } from '../../components'
import { localizeValue } from '../../helpers'

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
  return (
    <>
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
                <p>{localizeValue(roomOverview.room.status)} · {roomOverview.room.currentOccupancy}/{roomOverview.room.capacity} chỗ</p>
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
              () => fetch(`/api/facilities/rooms/${selectedRoomId}/assign-student`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ studentId: Number(roomActions.assignStudentId), status: 'Active', note: 'Xếp phòng từ trung tâm điều phối.' }) }),
              'Đã xếp sinh viên vào phòng.',
            )}>
              <select value={roomActions.assignStudentId} onChange={(event) => setRoomActions((current) => ({ ...current, assignStudentId: event.target.value }))}>
                <option value="">Chọn sinh viên chờ xếp</option>
                {waitingStudents.map((student) => <option key={student.id} value={student.id}>{student.studentCode} - {student.name}</option>)}
              </select>
            </ActionCard>

            <ActionCard title="Chuyển phòng" description="Điều phối sinh viên sang phòng khác khi cần tối ưu công suất." actionLabel="Chuyển phòng" disabled={!roomActions.transferStudentId || !roomActions.transferToRoomId || saving} onAction={() => executeAction(
              () => fetch('/api/facilities/rooms/transfer-student', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ studentId: Number(roomActions.transferStudentId), toRoomId: Number(roomActions.transferToRoomId), status: 'Active', note: 'Điều chuyển phòng từ trung tâm điều phối.' }) }),
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
              () => fetch(`/api/facilities/rooms/${selectedRoomId}/remove-student`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ studentId: Number(roomActions.removeStudentId), status: 'Waiting', note: 'Đã trả phòng, chờ sắp xếp lại.' }) }),
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
                  <p>{student.faculty} · {student.className}</p>
                  <small>{student.phone}</small>
                </article>
              ))
            )}
          </div>
        </Panel>
      </section>

      <CrudPanel
        entityKey="registrations"
        rows={data.registrations}
        onCreate={openCreate}
        onEdit={openEdit}
        onDelete={deleteEntity}
        panelProps={{ ...getPanelProps('operations-registrations'), panelId: 'panel-operations-registrations' }}
        extraRowActions={(item) => [
          item.status === 'Pending' ? { label: 'Duyệt', kind: 'approve', onClick: () => executeAction(() => fetch(`/api/operations/registrations/${item.id}/approve`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ note: 'Duyệt từ giao diện vận hành.' }) }), 'Đã duyệt đăng ký và xếp phòng.') } : null,
          item.status === 'Pending' ? { label: 'Từ chối', kind: 'danger', onClick: () => executeAction(() => fetch(`/api/operations/registrations/${item.id}/reject`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ note: 'Từ chối từ giao diện vận hành.' }) }), 'Đã từ chối đăng ký.') } : null,
        ]}
      />
    </>
  )
}
