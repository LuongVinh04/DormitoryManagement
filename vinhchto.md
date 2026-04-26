# Hệ thống Quản Trị Ký Túc Xá Sinh Viên

## 1. Tổng quan

Project hiện được tổ chức theo cấu trúc:

```text
/project
  /backend
  /frontend
  vinhchto.md
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

Hệ thống hiện thống nhất dùng SQL Server.

### Connection string backend hiện tại

File:

- [backend/DormitoryManagement/appsettings.json](/C:/Users/Minhhoangg/Desktop/CODE/project/backend/DormitoryManagement/appsettings.json)

Giá trị hiện tại:

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

### Lưu ý quan trọng

Không dùng:

- `Windows Authentication`
- `(localdb)`
- `mssqllocaldb`

Vì project hiện tại chuẩn hóa theo SQL Server tại `localhost:1433`.

---

## 4. Cấu hình chạy backend với SQL Server

## Vấn đề cần lưu ý

Hiện code backend đọc cấu hình như sau:

- nếu `DatabaseProvider = Sqlite` thì dùng SQLite
- nếu không có hoặc không phải `Sqlite` thì dùng SQL Server qua `DefaultConnection`

File liên quan:

- [backend/DormitoryManagement/Program.cs](/C:/Users/Minhhoangg/Desktop/CODE/project/backend/DormitoryManagement/Program.cs)

Hiện tại:

- `appsettings.json` đang trỏ SQL Server
- nhưng `appsettings.Development.json` vẫn còn:
  - `"DatabaseProvider": "Sqlite"`

File:

- [backend/DormitoryManagement/appsettings.Development.json](/C:/Users/Minhhoangg/Desktop/CODE/project/backend/DormitoryManagement/appsettings.Development.json)

## Kết luận

Nếu chạy app trong môi trường Development mà không sửa file này, backend có thể ưu tiên SQLite.

## Cách thống nhất hoàn toàn sang SQL Server

Sửa `backend/DormitoryManagement/appsettings.Development.json` thành:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=DormitoryManagement;User Id=sa;Password=Dormitory@2026!;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "Key": "DormitoryManagementSecretKey!@#$2026Dev",
    "Issuer": "DormitoryHub",
    "Audience": "DormitoryUsers",
    "ExpireDays": 7
  }
}
```

Hoặc tối thiểu:

- bỏ `"DatabaseProvider": "Sqlite"`
- thêm `"DefaultConnection"` trỏ SQL Server

Sau bước này, mọi môi trường chạy local sẽ đồng nhất sang SQL Server.

---

## 5. Cách project tự tạo database và bảng

Backend hiện không chạy migration EF theo kiểu chuẩn `dotnet ef database update`.

Thay vào đó, project đang dùng:

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
- `DatabaseSeeder` sẽ seed:
  - roles
  - users
  - permissions
  - room categories
  - room zones
  - payment methods
  - buildings
  - rooms
  - students
  - registrations
  - contracts
  - utilities
  - invoices
  - room fee profiles
  - room finance records

## Các bảng / nhóm dữ liệu đã được tạo bổ sung

`DatabaseSchemaUpdater` hiện đảm bảo có các bảng:

- `RoomFeeProfiles`
- `RoomFinanceRecords`
- `RoomCategories`
- `RoomZones`
- `PaymentMethodCatalogs`

và các cột liên quan như:

- `Rooms.RoomCategoryId`
- `Rooms.RoomZoneId`

---

## 6. Generate SQL mới nhất / tạo bảng mới nhất

## Cách thực tế đang dùng trong project

Để generate schema mới nhất và tạo bảng theo code hiện tại, cách chuẩn của project là:

1. Cấu hình đúng SQL Server trong `appsettings.json` và `appsettings.Development.json`
2. Xóa database cũ nếu muốn tạo sạch hoàn toàn
3. Chạy backend
4. Để `EnsureCreated + DatabaseSchemaUpdater + Seeder` tự tạo schema và seed dữ liệu

## Quy trình đầy đủ

### Bước 1: kiểm tra SQL Server đang chạy

Đảm bảo:

- SQL Server lắng nghe tại `localhost:1433`
- database user `sa` đăng nhập được

### Bước 2: tạo hoặc xóa database nếu cần

Nếu muốn generate sạch từ đầu:

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

Ngay khi app khởi động, backend sẽ:

- tạo database objects còn thiếu
- tạo bảng mới nhất theo cơ chế hiện tại
- seed dữ liệu mặc định

## Kết luận

Trong project này, “generate SQL mới nhất” thực tế là:

- chạy backend sau khi cấu hình SQL Server đúng
- để code tự tạo bảng và seed

Không phải luồng migration EF truyền thống.

---

## 7. Nếu muốn lấy script SQL để tạo bảng thủ công

Project hiện đã có sẵn phần SQL quan trọng trong:

- [backend/DormitoryManagement/Services/Infrastructure/DatabaseSchemaUpdater.cs](/C:/Users/Minhhoangg/Desktop/CODE/project/backend/DormitoryManagement/Services/Infrastructure/DatabaseSchemaUpdater.cs)

Nếu cần cấp script cho DBA hoặc muốn chạy tay:

1. lấy block SQL trong nhánh `db.Database.IsSqlServer()`
2. chạy bằng SSMS trên database `DormitoryManagement`

Tuy nhiên cần hiểu rõ:

- script này chủ yếu bổ sung bảng mở rộng
- schema nền ban đầu vẫn đang dựa vào `EnsureCreatedAsync()`

Nên nếu muốn tạo hoàn toàn bằng SQL tay, cần:

1. hoặc để app chạy 1 lần để tạo nền
2. hoặc tự viết full script toàn bộ schema

Trong trạng thái hiện tại, cách ổn nhất vẫn là:

- tạo database rỗng
- chạy app một lần

---

## 8. Cách chạy project đầy đủ

## Backend

```powershell
cd C:\Users\Minhhoangg\Desktop\CODE\project\backend\DormitoryManagement
dotnet build
dotnet run --urls http://127.0.0.1:5101
```

## Frontend

Frontend không cần chạy dev server nếu đã build vào backend.

Nếu muốn build mới:

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

Seed hiện có các tài khoản:

- `admin` / `admin123`
- `roomoperator` / `manager123`
- `cashier` / `cashier123`

Nguồn:

- [backend/DormitoryManagement/Services/Infrastructure/DatabaseSeeder.cs](/C:/Users/Minhhoangg/Desktop/CODE/project/backend/DormitoryManagement/Services/Infrastructure/DatabaseSeeder.cs)

---

## 10. Các file quan trọng đã cập nhật

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

1. `appsettings.Development.json` phải được chỉnh sang SQL Server nếu muốn thống nhất hoàn toàn.
2. Cơ chế hiện tại là `EnsureCreated + raw SQL updater + seeder`, không phải EF migration chuẩn.
3. Nếu thay đổi entity lớn trong tương lai, cần cập nhật:
   - model EF
   - `DatabaseSchemaUpdater`
   - `DatabaseSeeder` nếu có seed mặc định
4. Nếu cần quy trình migration chuẩn hơn về lâu dài, nên chuyển dần sang:
   - `dotnet ef migrations add ...`
   - `dotnet ef database update`

