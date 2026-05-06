# Hệ thống Quản Trị Ký Túc Xá Sinh Viên

## 1. Tổng quan

Project hiện được tổ chức theo cấu trúc:

```text
/project
  /backend
  /frontend
  vinhchto.md
  sql-server-thong-nhat.md
```

- `backend`: API ASP.NET Core, entity, service, cấu hình database.
- `frontend`: React + Vite, build ra static files cho backend.
- Frontend build vào:
  - `backend/DormitoryManagement/wwwroot`

Hệ thống hiện dùng:

- Backend: .NET
- Frontend: ReactJS + Vite
- Database chuẩn chạy thực tế: SQL Server

---

## 2. Trạng thái hoàn thành chính

### Backend

Đã có các nhóm chức năng:

- quản lý tòa nhà, phòng, phân khu, loại phòng
- quản lý sinh viên, hợp đồng, đăng ký nội trú
- điều phối phòng: xếp phòng, chuyển phòng, trả phòng
- điện nước, hóa đơn, công nợ tài chính theo phòng
- tài khoản, vai trò, phân quyền
- dashboard tổng quan vận hành

### Frontend

Đã có giao diện quản trị tiếng Việt với các khu:

- Tổng quan
- Danh mục
- Điều phối phòng
- Cơ sở vật chất
- Sinh viên
- Tài chính
- Quản trị

Đã có các cải tiến:

- responsive desktop/tablet/mobile
- panel thu gọn / phóng to
- loading toàn cục khi gọi API
- logo và favicon riêng
- animation nhẹ

---

## 3. SQL Server là database chuẩn

Hệ thống hiện thống nhất hoàn toàn dùng SQL Server.

### Connection string backend hiện tại

File:

- [backend/DormitoryManagement/appsettings.json](/C:/Users/Minhhoangg/Desktop/CODE/project/backend/DormitoryManagement/appsettings.json)
- [backend/DormitoryManagement/appsettings.Development.json](/C:/Users/Minhhoangg/Desktop/CODE/project/backend/DormitoryManagement/appsettings.Development.json)

Giá trị chuẩn:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=DormitoryManagement;User Id=sa;Password=Dormitory@2026!;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

### Thông tin kết nối đang dùng

- Server: `localhost`
- Port: `1433`
- Database: `DormitoryManagement`
- User: `sa`
- Password: `Dormitory@2026!`

### Kết nối bằng SSMS / DBeaver

- Host: `localhost`
- Port: `1433`
- Database: `DormitoryManagement`
- Authentication: `SQL Server Authentication`
- User: `sa`
- Password: `Dormitory@2026!`
- Bật `Trust Server Certificate` nếu công cụ yêu cầu

### Lưu ý

Không dùng:

- `Windows Authentication`
- `(localdb)`
- `mssqllocaldb`
- SQLite

---

## 4. Backend đã khóa thẳng sang SQL Server

Backend hiện đã dùng trực tiếp:

- `UseSqlServer(DefaultConnection)`

trong file:

- [backend/DormitoryManagement/Program.cs](/C:/Users/Minhhoangg/Desktop/CODE/project/backend/DormitoryManagement/Program.cs)

Kết luận:

- không còn nhánh chọn SQLite trong runtime backend
- local run và development run đều dùng SQL Server

---

## 5. Cách project tự tạo database và bảng

Backend hiện không chạy migration EF kiểu chuẩn `dotnet ef database update`.

Project đang dùng:

1. `db.Database.EnsureCreatedAsync()`
2. `DatabaseSchemaUpdater.EnsureFinancialSchemaAsync(db)`
3. `DatabaseSeeder.SeedAsync(db)`

File liên quan:

- [backend/DormitoryManagement/Program.cs](/C:/Users/Minhhoangg/Desktop/CODE/project/backend/DormitoryManagement/Program.cs)
- [backend/DormitoryManagement/Services/Infrastructure/DatabaseSeeder.cs](/C:/Users/Minhhoangg/Desktop/CODE/project/backend/DormitoryManagement/Services/Infrastructure/DatabaseSeeder.cs)
- [backend/DormitoryManagement/Services/Infrastructure/DatabaseSchemaUpdater.cs](/C:/Users/Minhhoangg/Desktop/CODE/project/backend/DormitoryManagement/Services/Infrastructure/DatabaseSchemaUpdater.cs)

## Ý nghĩa

Khi app khởi động:

- nếu database chưa có, `EnsureCreatedAsync()` sẽ tạo schema cơ bản từ model EF
- `DatabaseSchemaUpdater` sẽ chạy SQL bổ sung để tạo hoặc cập nhật các bảng cần thêm
- `DatabaseSeeder` sẽ seed dữ liệu mặc định

## Các bảng / nhóm dữ liệu được bảo đảm có

- `Users`
- `Roles`
- `Permissions`
- `RolePermissions`
- `UserPermissions`
- `Buildings`
- `Rooms`
- `RoomCategories`
- `RoomZones`
- `PaymentMethodCatalogs`
- `Students`
- `Registrations`
- `Contracts`
- `Utilities`
- `Invoices`
- `RoomFeeProfiles`
- `RoomFinanceRecords`

---

## 6. Generate schema SQL mới nhất

Trong project này, “generate SQL mới nhất” theo luồng hiện tại nghĩa là:

1. cấu hình đúng SQL Server
2. tạo database rỗng nếu cần
3. chạy backend
4. để backend tự tạo schema và seed

## Quy trình đầy đủ

### Bước 1: kiểm tra SQL Server đang chạy

Đảm bảo:

- SQL Server lắng nghe tại `localhost:1433`
- user `sa` đăng nhập được

### Bước 2: reset database nếu muốn tạo sạch

```sql
IF DB_ID(N'DormitoryManagement') IS NOT NULL
BEGIN
    ALTER DATABASE [DormitoryManagement] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [DormitoryManagement];
END
GO

CREATE DATABASE [DormitoryManagement];
GO
```

### Bước 3: build frontend

```powershell
cd C:\Users\Minhhoangg\Desktop\CODE\project\frontend
npm install
npm run build
```

### Bước 4: chạy backend

```powershell
cd C:\Users\Minhhoangg\Desktop\CODE\project\backend\DormitoryManagement
dotnet build
dotnet run --urls http://127.0.0.1:5101
```

### Bước 5: backend tự tạo schema

Ngay khi app chạy:

- database objects còn thiếu sẽ được tạo
- các bảng mở rộng sẽ được bảo đảm có
- dữ liệu seed mặc định sẽ được thêm

---

## 7. Nếu muốn lấy SQL script thủ công

Phần SQL Server bổ sung hiện nằm trong:

- [backend/DormitoryManagement/Services/Infrastructure/DatabaseSchemaUpdater.cs](/C:/Users/Minhhoangg/Desktop/CODE/project/backend/DormitoryManagement/Services/Infrastructure/DatabaseSchemaUpdater.cs)

Nếu cần chạy tay bằng SSMS:

1. mở file này
2. lấy block SQL trong `EnsureFinancialSchemaAsync`
3. chạy trên database `DormitoryManagement`

Tuy nhiên cách đúng của project vẫn là:

- tạo database rỗng
- chạy backend một lần

---

## 8. Cách chạy project đầy đủ

## Backend

```powershell
cd C:\Users\Minhhoangg\Desktop\CODE\project\backend\DormitoryManagement
dotnet build
dotnet run --urls http://127.0.0.1:5101
```

## Frontend

Nếu cần build mới:

```powershell
cd C:\Users\Minhhoangg\Desktop\CODE\project\frontend
npm install
npm run build
```

## URL truy cập

- [http://127.0.0.1:5101/](http://127.0.0.1:5101/)

## Kiểm tra nhanh

- trang chủ trả `200`
- `/api/dashboard` trả `200`

---

## 9. Tài khoản seed mặc định

Seed hiện có:

- `roomoperator` / `manager123`
- `cashier` / `cashier123`

Nguồn:

- [backend/DormitoryManagement/Services/Infrastructure/DatabaseSeeder.cs](/C:/Users/Minhhoangg/Desktop/CODE/project/backend/DormitoryManagement/Services/Infrastructure/DatabaseSeeder.cs)

---

## 10. Các file quan trọng

### Frontend

- `frontend/src/App.jsx`
- `frontend/src/App.css`
- `frontend/src/components.jsx`
- `frontend/src/constants.js`
- `frontend/src/helpers.js`
- `frontend/src/loadingBus.js`
- `frontend/src/components/GlobalLoadingOverlay.jsx`

### Backend

- `backend/DormitoryManagement/Program.cs`
- `backend/DormitoryManagement/appsettings.json`
- `backend/DormitoryManagement/appsettings.Development.json`
- `backend/DormitoryManagement/Services/Infrastructure/DatabaseSeeder.cs`
- `backend/DormitoryManagement/Services/Infrastructure/DatabaseSchemaUpdater.cs`
- `backend/Dormitory.Models/DataContexts/AppDbContext.cs`

---

## 11. Lưu ý kỹ thuật

1. Runtime backend hiện chỉ dùng SQL Server.
2. Cơ chế hiện tại là `EnsureCreated + raw SQL updater + seeder`, không phải EF migration chuẩn.
3. Nếu thay đổi entity lớn trong tương lai, cần cập nhật:
   - model EF
   - `DatabaseSchemaUpdater`
   - `DatabaseSeeder`
4. Nếu muốn schema versioning tốt hơn về lâu dài, nên chuyển dần sang EF migration chuẩn.
