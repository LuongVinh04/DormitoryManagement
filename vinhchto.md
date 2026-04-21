# Hệ thống Quản Trị Ký Túc Xá Sinh Viên

## I. Đề tài

Xây dựng một hệ thống web quản trị ký túc xá cho sinh viên, gồm:

- Backend quản lý dữ liệu và nghiệp vụ vận hành.
- Frontend quản trị bằng ReactJS, giao diện tiếng Việt.
- Cơ sở dữ liệu dùng SQL Server.
- Hệ thống có thể chạy thực tế với đầy đủ các luồng quản lý cơ bản.

## II. Yêu cầu ban đầu

### 1. Backend

- Bổ sung các entity còn thiếu để hệ thống đủ dùng thực tế.
- Viết các API cần thiết cho toàn bộ hệ thống.
- Hoàn thiện các luồng nghiệp vụ quản lý sinh viên, phòng ở, đăng ký nội trú, hợp đồng, điện nước, hóa đơn, tài khoản người dùng.

### 2. Frontend

- Xây dựng web quản trị bằng ReactJS.
- Có trang tổng quan dạng biểu đồ, thống kê.
- Giao diện đẹp, dễ nhìn, kiểu dashboard quản trị thực tế.
- Hỗ trợ tiếng Việt.
- Responsive trên desktop, tablet, mobile.
- Kiểm tra và xử lý lỗi chồng chữ, đè nút, vỡ layout.

## III. Kết quả đã hoàn thành

## 1. Cấu trúc project đã tách lại

Project hiện đã được tổ chức lại theo đúng cấu trúc:

```text
/project
  /backend
  /frontend
  vinhchto.md
```

- `backend`: chứa toàn bộ source .NET, API, entity, service và file cấu hình.
- `frontend`: chứa source ReactJS + Vite.
- Frontend build ra static files vào `backend/DormitoryManagement/wwwroot`.

## 2. Cơ sở dữ liệu đã chuyển sang SQL Server

Hệ thống đã được chuyển từ cấu hình cũ sang SQL Server.

### Thông tin kết nối hiện tại

- Server: `localhost`
- Port: `1433`
- Database: `DormitoryManagement`
- Username: `sa`
- Password: `Dormitory@2026!`

### Connection string backend đang dùng

```json
Server=localhost,1433;Database=DormitoryManagement;User Id=sa;Password=Dormitory@2026!;TrustServerCertificate=True;MultipleActiveResultSets=true
```

### Cấu hình để connect bằng DBeaver / SSMS

- Host: `localhost`
- Port: `1433`
- Database: `DormitoryManagement`
- Authentication: `SQL Server Authentication`
- User: `sa`
- Password: `Dormitory@2026!`
- Tick `Trust Server Certificate` nếu công cụ yêu cầu

Lưu ý:

- Không dùng `Windows Authentication` cho cấu hình hiện tại.
- Không dùng `(localdb)` hoặc `mssqllocaldb`.
- Hệ thống đang dùng SQL Server chạy tại `localhost:1433`.

## 3. Backend đã được mở rộng đầy đủ hơn

### 3.1. Nhóm nghiệp vụ đã bổ sung

Backend đã được bổ sung thêm service nghiệp vụ để xử lý các luồng thực tế:

- Xếp sinh viên vào phòng.
- Chuyển sinh viên sang phòng khác.
- Trả phòng / đưa sinh viên ra khỏi phòng.
- Duyệt hồ sơ đăng ký nội trú.
- Từ chối hồ sơ đăng ký nội trú.
- Tự động tạo hóa đơn từ chỉ số điện nước.
- Cập nhật hồ sơ người dùng.
- Cập nhật trạng thái sinh viên.

### 3.2. API đã được mở rộng

#### Dashboard

- `GET /api/dashboard`

Trả về dữ liệu tổng quan:

- Tổng sinh viên
- Số phòng đang ở
- Số giường còn trống
- Hợp đồng hiệu lực
- Hóa đơn chưa thu
- Hóa đơn quá hạn
- Người dùng đang hoạt động
- Cảnh báo vận hành
- Snapshot phòng
- Công suất theo tòa
- Trạng thái phòng
- Trạng thái hóa đơn
- Doanh thu theo tháng
- Hoạt động gần đây

#### Facilities

- `GET /api/facilities/buildings`
- `POST /api/facilities/buildings`
- `PUT /api/facilities/buildings/{id}`
- `DELETE /api/facilities/buildings/{id}`

- `GET /api/facilities/rooms`
- `POST /api/facilities/rooms`
- `PUT /api/facilities/rooms/{id}`
- `DELETE /api/facilities/rooms/{id}`
- `GET /api/facilities/rooms/{id}/overview`
- `GET /api/facilities/rooms/{id}/students`
- `POST /api/facilities/rooms/{roomId}/assign-student`
- `POST /api/facilities/rooms/transfer-student`
- `POST /api/facilities/rooms/{roomId}/remove-student`

#### People

- `GET /api/people/students`
- `GET /api/people/students/{id}`
- `POST /api/people/students`
- `PUT /api/people/students/{id}`
- `DELETE /api/people/students/{id}`
- `PATCH /api/people/students/{id}/status`

- `GET /api/people/users`
- `GET /api/people/users/{id}`
- `POST /api/people/users`
- `PUT /api/people/users/{id}`
- `DELETE /api/people/users/{id}`
- `PUT /api/people/users/{id}/profile`

- `GET /api/people/roles`
- `POST /api/people/roles`
- `PUT /api/people/roles/{id}`
- `DELETE /api/people/roles/{id}`

#### Operations

- `GET /api/operations/registrations`
- `POST /api/operations/registrations`
- `PUT /api/operations/registrations/{id}`
- `DELETE /api/operations/registrations/{id}`
- `POST /api/operations/registrations/{id}/approve`
- `POST /api/operations/registrations/{id}/reject`

- `GET /api/operations/contracts`
- `POST /api/operations/contracts`
- `PUT /api/operations/contracts/{id}`
- `DELETE /api/operations/contracts/{id}`

- `GET /api/operations/utilities`
- `POST /api/operations/utilities`
- `PUT /api/operations/utilities/{id}`
- `DELETE /api/operations/utilities/{id}`
- `POST /api/operations/utilities/{id}/generate-invoices`

- `GET /api/operations/invoices`
- `POST /api/operations/invoices`
- `PUT /api/operations/invoices/{id}`
- `DELETE /api/operations/invoices/{id}`
- `POST /api/operations/invoices/{id}/mark-paid`

### 3.3. Request model đã bổ sung

Đã bổ sung thêm các request model để phục vụ API:

- `StudentStatusRequest`
- `UserProfileRequest`
- `AssignStudentRequest`
- `TransferStudentRequest`
- `RemoveStudentRequest`
- `RegistrationDecisionRequest`
- `InvoicePaymentRequest`

## 4. Frontend đã được xây dựng thành dashboard quản trị thực tế

### 4.1. Các khu chức năng chính

Frontend hiện đã có giao diện quản trị tiếng Việt với các khu:

- Tổng quan
- Điều phối phòng
- Cơ sở vật chất
- Sinh viên
- Tài chính
- Quản trị

### 4.2. Các chức năng giao diện đã có

#### Tổng quan

- Hiển thị số liệu tổng hợp dạng metric cards.
- Hiển thị biểu đồ công suất theo tòa.
- Hiển thị biểu đồ trạng thái hóa đơn.
- Hiển thị biểu đồ doanh thu theo tháng.
- Hiển thị cảnh báo vận hành.
- Hiển thị snapshot nhanh của phòng.

#### Điều phối phòng

- Chọn phòng cần thao tác.
- Xem nhanh trạng thái phòng.
- Xếp sinh viên chờ vào phòng.
- Chuyển sinh viên sang phòng khác.
- Trả phòng cho sinh viên.
- Xem danh sách sinh viên đang ở trong phòng.
- Duyệt hoặc từ chối hồ sơ đăng ký nội trú.

#### Cơ sở vật chất

- CRUD tòa nhà.
- CRUD phòng ở.

#### Sinh viên

- CRUD hồ sơ sinh viên.
- CRUD hợp đồng nội trú.

#### Tài chính

- CRUD chỉ số điện nước.
- Tạo hóa đơn từ kỳ điện nước.
- CRUD hóa đơn.
- Ghi nhận thanh toán hóa đơn.

#### Quản trị

- CRUD tài khoản người dùng.
- CRUD vai trò hệ thống.

## 5. Các cải tiến giao diện đã thực hiện

### 5.1. Nâng cấp màu sắc và bố cục

Đã chỉnh lại giao diện theo hướng dashboard hiện đại:

- Màu chủ đạo xanh đậm, xanh teal, xanh dương.
- Card bo góc lớn, bóng đổ nhẹ.
- Có phần brand panel riêng.
- Có nhóm thống kê rõ ràng.
- Các panel có cấu trúc trực quan hơn.

### 5.2. Hỗ trợ tiếng Việt

- Giao diện được chuyển sang tiếng Việt.
- Các label, tiêu đề, nút thao tác, trạng thái hiển thị theo ngữ cảnh tiếng Việt.
- Có ánh xạ trạng thái để hiển thị dễ hiểu hơn cho người dùng cuối.

### 5.3. Đã xử lý responsive

Đã sửa lại các lỗi responsive quan trọng:

- Text dài tự xuống dòng thay vì tràn ra ngoài.
- Nút chức năng không còn bị đè lên nhau.
- Các nhóm nút tự wrap hợp lý trên màn nhỏ.
- Header không còn chồng chữ khi màn hình hẹp.
- Card và panel co giãn tốt hơn trên tablet/mobile.
- Khu điều phối phòng hiển thị ổn định hơn khi đổi kích thước màn hình.
- Bảng dữ liệu trên mobile chuyển sang dạng card-list để dễ đọc hơn.
- Action button trong bảng hiển thị theo cột hoặc 2 cột tùy kích thước màn hình.

### 5.4. Kiểm tra lỗi font

Đã rà lại để web hiển thị tiếng Việt.  
Lưu ý: trong một số terminal PowerShell, chữ tiếng Việt có thể hiển thị sai dấu do encoding terminal, nhưng build frontend và web thực tế vẫn hoạt động bình thường. Cần phân biệt lỗi terminal với lỗi font thực tế trên trình duyệt.

## 6. Các file frontend/backend quan trọng đã cập nhật

### Frontend

- `frontend/src/App.jsx`
- `frontend/src/App.css`
- `frontend/src/components.jsx`
- `frontend/src/constants.js`
- `frontend/src/helpers.js`
- `frontend/vite.config.js`

### Backend

- `backend/DormitoryManagement/Controllers/DashboardController.cs`
- `backend/DormitoryManagement/Controllers/FacilitiesController.cs`
- `backend/DormitoryManagement/Controllers/PeopleController.cs`
- `backend/DormitoryManagement/Controllers/OperationsController.cs`
- `backend/DormitoryManagement/Models/ApiModels.cs`
- `backend/DormitoryManagement/Services/DormitoryWorkflowService.cs`
- `backend/DormitoryManagement/appsettings.json`

## 7. Trạng thái chạy hiện tại

Đã kiểm tra và xác nhận:

- Frontend build thành công bằng `npm run build`
- Backend build thành công bằng `dotnet build`
- Web hiện đang chạy được
- Trang chủ phản hồi `200`
- API dashboard phản hồi `200`

Địa chỉ đang chạy:

- `http://127.0.0.1:5101/`

## 8. Luồng sử dụng hệ thống thực tế

Ví dụ luồng vận hành:

1. Tạo tòa nhà và phòng ở.
2. Tạo hồ sơ sinh viên.
3. Tạo hồ sơ đăng ký nội trú.
4. Duyệt hồ sơ đăng ký.
5. Xếp sinh viên vào phòng.
6. Tạo hợp đồng lưu trú.
7. Ghi chỉ số điện nước.
8. Tạo hóa đơn.
9. Ghi nhận thanh toán.
10. Quản lý tài khoản admin / vận hành / kế toán.

## 9. Ghi chú thêm

- Frontend và backend hiện đã tách thư mục riêng đúng yêu cầu.
- Hệ thống hiện phù hợp để demo, phát triển tiếp hoặc hoàn thiện thêm phân quyền sâu hơn.
- Nếu tiếp tục phát triển, các bước nên làm tiếp theo là:
  - Bổ sung đăng nhập và xác thực JWT.
  - Bổ sung phân quyền theo role chi tiết hơn.
  - Tối ưu seed data.
  - Tối ưu bundle frontend để giảm kích thước file build.
  - Bổ sung test cho API và giao diện.

## 10. Kết luận

Hệ thống web quản trị ký túc xá đã được nâng cấp từ yêu cầu ban đầu thành một bộ ứng dụng có thể chạy thực tế gồm:

- Backend .NET với API đầy đủ hơn.
- Frontend ReactJS tiếng Việt.
- Dashboard thống kê và biểu đồ.
- Quản lý tòa nhà, phòng, sinh viên, đăng ký, hợp đồng, điện nước, hóa đơn, người dùng.
- Dùng SQL Server thay cho cấu hình cũ.
- Giao diện responsive và đã xử lý các lỗi text/button bị đè.
