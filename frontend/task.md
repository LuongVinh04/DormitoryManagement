# Tasks — Adjustment Implementation

## Phase 1: Sửa lỗi nghiệp vụ cốt lõi ✅
- [x] 1.1 Bỏ chọn phòng (`roomId`) trong form tạo sinh viên
- [x] 1.2 Bỏ `approvedDate` trong form đăng ký nội trú
- [x] 1.3 Chặn xếp phòng nếu sinh viên chưa có hợp đồng Active
- [x] 1.4 Sửa assign/transfer/remove phòng → cập nhật `CurrentOccupancy` đúng
- [x] 1.5 Chặn hủy hợp đồng nếu sinh viên còn ở phòng
- [x] 1.6 Xóa sinh viên bỏ `window.confirm` → modal confirm UI

## Phase 2: Tài chính phòng và chia tiền sinh viên ✅
- [x] 2.1 Tạo entity [RoomFinanceStudentShare](file:///c:/Users/Minhhoangg/Desktop/CODE/project/backend/Dormitory.Models/Entities/RoomFinanceStudentShare.cs#3-17)
- [x] 2.2 Thêm API chia tiền và thu tiền theo từng sinh viên
- [x] 2.3 UI xem phòng đã nộp/chưa nộp + cột SV chia/SV nộp
- [x] 2.4 Cho phép điều chỉnh tiền từng sinh viên (API ready)

## Phase 3: Student Portal ✅
- [x] 3.1 Thêm `StudentId` vào [Users](file:///c:/Users/Minhhoangg/Desktop/CODE/project/backend/Dormitory.Models/Entities/Users.cs#9-23), role [Student](file:///c:/Users/Minhhoangg/Desktop/CODE/project/backend/Dormitory.Models/Entities/Students.cs#9-30)
- [x] 3.2 API đăng ký tài khoản sinh viên + đăng nhập
- [x] 3.3 Frontend student portal (xem phòng, tài chính, thông tin cá nhân)

## Phase 4: Yêu cầu chuyển phòng + Chat
- [/] 4.1 Entity `RoomTransferRequest` + API
- [ ] 4.2 UI sinh viên gửi yêu cầu, admin duyệt/từ chối
- [ ] 4.3 Entity `Conversation`/`Message` + API chat
- [ ] 4.4 UI chat sinh viên ↔ quản lý

## Phase 5: Báo cáo xuất Excel
- [ ] 5.1 Backend export Excel (ClosedXML)
- [ ] 5.2 Frontend nút export và download
