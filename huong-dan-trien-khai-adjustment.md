# Huong Dan Trien Khai Toan Bo Yeu Cau Trong adjustment.md

## 1. Muc tieu

Tai lieu nay tong hop lai toan bo yeu cau trong `adjustment.md` va bien thanh checklist ky thuat de AI/dev khac co the trien khai tiep ma khong bo sot chuc nang.

Pham vi anh huong:

- Backend ASP.NET Core
- Frontend React + Vite
- SQL Server schema/data
- Nghiep vu phong, sinh vien, hop dong, tai chinh, tai khoan sinh vien, thong bao, chat, bao cao

File goc:

- [adjustment.md](/C:/Users/Minhhoangg/Desktop/CODE/project/adjustment.md)

---

## 2. Nguyen tac trien khai

## 2.1. Khong tiep tuc cho phep cap nhat phong tuy tien

Trang thai phong cua sinh vien phai di qua nghiep vu:

- sinh vien co ho so
- sinh vien co hop dong luu tru hop le
- duoc duyet / dieu phoi vao phong
- backend cap nhat so luong phong va trang thai phong

Khong cho form ho so sinh vien tu gan `roomId` tuy tien khi tao moi.

## 2.2. Tai chinh lay phong lam don vi goc

Cong no thang duoc tao theo phong.

Sau do he thong chia tong tien phong cho cac sinh vien dang o trong phong.

Can quan sat duoc:

- phong nao da nop
- phong nao chua nop
- phong nao nop mot phan
- tung sinh vien trong phong da nop hay chua
- so tien moi sinh vien phai nop
- so tien moi sinh vien thuc te da nop

## 2.3. Moi thay doi nghiep vu quan trong phai xu ly o backend

Frontend chi giup thao tac de dung.

Backend phai la noi chan loi:

- khong huy hop dong neu sinh vien con dang o phong
- khong xep phong neu sinh vien chua co hop dong hop le
- xep/chuyen/tra phong phai cap nhat lai si so phong
- tao cong no phai tinh lai tong tien va phan bo tien cho sinh vien

---

## 3. Dieu phoi phong

## 3.1. Dang ky noi tru

Yeu cau tu `adjustment.md`:

- khi them moi dang ky noi tru, bo nhap ngay xu ly
- ngay xu ly chi duoc cap nhat khi duyet hoac tu choi
- can xem lai viec co nen bo chuc nang dang ky noi tru dang CRUD hien tai

## Huong sua

### Frontend

File can kiem tra:

- [frontend/src/constants.js](/C:/Users/Minhhoangg/Desktop/CODE/project/frontend/src/constants.js)
- [frontend/src/features/operations/OperationsSection.jsx](/C:/Users/Minhhoangg/Desktop/CODE/project/frontend/src/features/operations/OperationsSection.jsx)

Trong config `registrations.fields`:

- bo field `approvedDate` khoi form tao moi/chinh sua thong thuong
- neu can hien thi thi chi hien trong bang danh sach
- khong cho user tu nhap ngay xu ly

Neu van giu module dang ky noi tru:

- doi UI tu CRUD thuan sang danh sach cho duyet
- action chinh chi gom:
  - `Duyet`
  - `Tu choi`
  - `Xem chi tiet`

Neu quyet dinh bo chuc nang dang ky noi tru khoi menu:

- khong xoa API ngay
- chi an panel CRUD dang ky khoi UI
- giu backend de phuc vu luong sinh vien tu dang ky tai khoan sau nay

### Backend

File can kiem tra:

- [backend/DormitoryManagement/Controllers/Operations/OperationsController.cs](/C:/Users/Minhhoangg/Desktop/CODE/project/backend/DormitoryManagement/Controllers/Operations/OperationsController.cs)
- [backend/DormitoryManagement/Services/Operations/DormitoryWorkflowService.cs](/C:/Users/Minhhoangg/Desktop/CODE/project/backend/DormitoryManagement/Services/Operations/DormitoryWorkflowService.cs)

Can dam bao:

- khi tao registration moi: `ApprovedDate = null`
- khi approve: set `ApprovedDate = DateTime.UtcNow`
- khi reject: set `ApprovedDate = DateTime.UtcNow`
- khong tin gia tri `ApprovedDate` tu request body

## Tieu chi hoan thanh

- Form dang ky noi tru khong con o `Ngay xu ly`
- Duyet / tu choi moi sinh `ApprovedDate`
- API khong nhan `ApprovedDate` tu client nhu nguon du lieu tin cay

---

## 3.2. Dieu phoi sinh vien theo phong

Yeu cau:

- giao dien dieu phoi sinh vien theo phong can to hon, thong minh hon, tach rieng
- them moi sinh vien vao phong hien chua cap nhat dung so luong sinh vien trong phong

## Huong sua UI

File:

- [frontend/src/features/operations/OperationsSection.jsx](/C:/Users/Minhhoangg/Desktop/CODE/project/frontend/src/features/operations/OperationsSection.jsx)
- [frontend/src/App.css](/C:/Users/Minhhoangg/Desktop/CODE/project/frontend/src/App.css)

Can tach khu dieu phoi thanh 3 vung ro rang:

- Cot trai: danh sach phong, loc theo toa, trang thai, con trong
- Khu giua: thong tin phong dang chon va danh sach sinh vien dang o
- Cot phai: thao tac xep phong, chuyen phong, tra phong

Khong nen de tat ca trong mot panel CRUD dai.

Can co thong tin nhanh:

- phong
- toa
- suc chua
- dang o
- con trong
- gioi tinh/chinh sach neu co
- danh sach sinh vien trong phong
- sinh vien cho xep phong hop le

## Huong sua backend

Moi thao tac:

- assign student
- transfer student
- remove student

phai goi lai logic tinh phong.

File lien quan:

- [backend/DormitoryManagement/Services/Facilities/RoomOccupancyService.cs](/C:/Users/Minhhoangg/Desktop/CODE/project/backend/DormitoryManagement/Services/Facilities/RoomOccupancyService.cs)
- [backend/DormitoryManagement/Services/Operations/DormitoryWorkflowService.cs](/C:/Users/Minhhoangg/Desktop/CODE/project/backend/DormitoryManagement/Services/Operations/DormitoryWorkflowService.cs)

Can dam bao:

- `CurrentOccupancy` cap nhat dung sau moi thao tac
- `Status` phong cap nhat dung:
  - `Available`
  - `Occupied`
  - `Full`
- frontend refresh lai:
  - room overview
  - rooms list
  - dashboard summary

## Tieu chi hoan thanh

- Xep sinh vien vao phong xong, si so phong tang ngay
- Tra phong xong, si so phong giam ngay
- Chuyen phong xong, phong cu giam va phong moi tang
- UI dieu phoi tach rieng, de thao tac hon CRUD hien tai

---

## 4. Sinh vien va hop dong luu tru

## 4.1. Ho so sinh vien

Yeu cau:

- khi them moi ho so sinh vien, bo chon phong
- phong chi cap nhat sau khi sinh vien duoc duyet/dieu phoi
- bo phi hang thang trong luong them moi ho so sinh vien

## Huong sua frontend

File:

- [frontend/src/constants.js](/C:/Users/Minhhoangg/Desktop/CODE/project/frontend/src/constants.js)
- [frontend/src/features/students/StudentsSection.jsx](/C:/Users/Minhhoangg/Desktop/CODE/project/frontend/src/features/students/StudentsSection.jsx)

Trong `students.fields`:

- bo `roomId` khoi form tao moi/chinh sua ho so sinh vien
- neu van can hien thi phong thi chi hien trong table/detail

Trang thai sinh vien khi tao moi nen la:

- `Waiting`
- hoac `Pending`

Khong nen la `Active` neu chua co phong/hop dong.

## Huong sua backend

File:

- [backend/DormitoryManagement/Controllers/People/PeopleController.cs](/C:/Users/Minhhoangg/Desktop/CODE/project/backend/DormitoryManagement/Controllers/People/PeopleController.cs)
- [backend/DormitoryManagement/Models/Requests/People/PeopleRequests.cs](/C:/Users/Minhhoangg/Desktop/CODE/project/backend/DormitoryManagement/Models/Requests/People/PeopleRequests.cs)

Can chan:

- tao sinh vien moi voi `RoomId` tu client
- cap nhat `RoomId` truc tiep qua profile update thong thuong

`RoomId` chi nen doi qua:

- approve registration
- assign room
- transfer room
- remove room

## Tieu chi hoan thanh

- Tao ho so sinh vien khong con dropdown phong
- API khong cho gan phong truc tiep qua create/update student
- Phong cua sinh vien chi doi qua luong dieu phoi

---

## 4.2. Hop dong luu tru la dieu kien de xep phong

Yeu cau:

- sinh vien phai co hop dong luu tru truoc
- sau do moi duoc quan tri them vao dieu phoi phong
- hien tai phong dang duoc them linh tinh gay nhieu he thong

## Huong sua nghiep vu

Truoc khi xep sinh vien vao phong, backend phai kiem tra:

- sinh vien ton tai
- co hop dong `Active`
- hop dong chua het han
- hop dong khong bi huy

Neu chua co hop dong hop le:

- khong cho xep phong
- tra message ro:
  - `Sinh vien can co hop dong luu tru hieu luc truoc khi xep phong.`

## File can sua

- [backend/DormitoryManagement/Services/Operations/DormitoryWorkflowService.cs](/C:/Users/Minhhoangg/Desktop/CODE/project/backend/DormitoryManagement/Services/Operations/DormitoryWorkflowService.cs)
- [backend/DormitoryManagement/Controllers/Facilities/FacilitiesController.cs](/C:/Users/Minhhoangg/Desktop/CODE/project/backend/DormitoryManagement/Controllers/Facilities/FacilitiesController.cs)

## Frontend

Trong danh sach sinh vien cho xep phong:

- chi hien sinh vien co hop dong hop le
- hoac hien sinh vien chua hop le nhung disabled va co ly do

## Tieu chi hoan thanh

- Khong the xep phong cho sinh vien chua co hop dong Active
- UI khong gay hieu nham rang sinh vien nao cung xep phong duoc

---

## 4.3. Khong cho huy hop dong neu sinh vien con trong phong

Yeu cau:

- Hop dong luu tru khong de chuyen trang thai da huy neu sinh vien do con trong bat ky phong nao

## Huong sua backend

Khi update contract status sang:

- `Cancelled`
- hoac trang thai tuong duong huy

phai kiem tra sinh vien:

- `Student.RoomId != null`
- hoac co ban ghi cu tru dang active

Neu con trong phong:

- chan update
- tra loi:
  - `Can tra phong cho sinh vien truoc khi huy hop dong.`

File can sua:

- [backend/DormitoryManagement/Controllers/Operations/OperationsController.cs](/C:/Users/Minhhoangg/Desktop/CODE/project/backend/DormitoryManagement/Controllers/Operations/OperationsController.cs)

## Frontend

Trong form hop dong:

- neu student dang o phong, disable option `Cancelled`
- hoac cho chon nhung backend tra loi ro va toast hien thong bao

## Tieu chi hoan thanh

- Sinh vien dang co phong thi khong huy duoc hop dong
- Sau khi tra phong, moi co the huy hop dong

---

## 5. Tai chinh

## 5.1. Chi so dien nuoc

Yeu cau:

- them moi chi so dien nuoc khong can nhap cac truong thua
- o day chi quy dinh gia dien/nuoc tung so
- moi phong co gia dien/nuoc rieng
- cac so tieu thu se theo tung phong

## Huong nghiep vu dung

`RoomFeeProfiles` la noi luu:

- `ElectricityUnitPrice`
- `WaterUnitPrice`

Moi phong co the co gia rieng.

Khi tao `Utilities`:

- chon phong
- he thong tu lay don gia dien/nuoc tu `RoomFeeProfiles`
- user nhap chi so cu/moi
- he thong tinh tien dien/nuoc

## File can sua

Frontend:

- [frontend/src/constants.js](/C:/Users/Minhhoangg/Desktop/CODE/project/frontend/src/constants.js)
- [frontend/src/hooks/useDormitoryData.js](/C:/Users/Minhhoangg/Desktop/CODE/project/frontend/src/hooks/useDormitoryData.js)
- [frontend/src/features/finance/FinanceSection.jsx](/C:/Users/Minhhoangg/Desktop/CODE/project/frontend/src/features/finance/FinanceSection.jsx)

Backend:

- [backend/Dormitory.Models/Entities/Utilities.cs](/C:/Users/Minhhoangg/Desktop/CODE/project/backend/Dormitory.Models/Entities/Utilities.cs)
- [backend/DormitoryManagement/Controllers/Operations/OperationsController.cs](/C:/Users/Minhhoangg/Desktop/CODE/project/backend/DormitoryManagement/Controllers/Operations/OperationsController.cs)

## Can lam

- label ro: `Don gia dien / 1 so`, `Don gia nuoc / 1 so`
- khi chon phong, auto fill don gia
- tinh:
  - `ElectricityUsage = ElectricityNew - ElectricityOld`
  - `WaterUsage = WaterNew - WaterOld`
  - `ElectricityFee = ElectricityUsage * ElectricityUnitPrice`
  - `WaterFee = WaterUsage * WaterUnitPrice`

## Tieu chi hoan thanh

- Tao chi so dien nuoc khong phai nhap lai don gia neu phong da co cau hinh
- Tien dien/nuoc duoc tinh tu dong
- Backend tinh lai de tranh client gui sai

---

## 5.2. Co che thanh toan moi: tao theo phong, chia theo sinh vien

Yeu cau:

- tao cong no cho 1 phong
- tong tien phong gom dien, nuoc, ve sinh, dich vu, internet, phi khac
- he thong chia tong tien phong cho cac sinh vien trong phong
- xem duoc phong nao da nop
- xem duoc so sinh vien trong phong da nop
- bam vao phong xem tung sinh vien da nop/chua
- co the dieu chinh so tien nop cua tung sinh vien

## Can bo sung data model

Hien co:

- `RoomFinanceRecord`

Can bo sung bang chi tiet theo sinh vien, de xet tien da nop tung nguoi.

De xuat entity:

```csharp
public class RoomFinanceStudentShare : BaseEntity
{
    public int RoomFinanceRecordId { get; set; }
    public int StudentId { get; set; }
    public decimal ExpectedAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public string Status { get; set; } = "Unpaid";
    public DateTime? PaidDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
}
```

Can them:

- `DbSet<RoomFinanceStudentShare>`
- relationship trong `AppDbContext`
- schema updater SQL Server
- seed/test data neu can

## Logic chia tien

Khi tao cong no phong:

1. lay danh sach sinh vien dang o phong tai thoi diem tao cong no
2. tinh `Total` cua phong
3. chia deu mac dinh:
   - `ExpectedAmount = Total / studentCount`
4. tao share cho tung sinh vien
5. cho phep sua tay `ExpectedAmount` neu can

## API can co

De xuat:

- `GET /api/operations/room-finances/{id}/shares`
- `POST /api/operations/room-finances/{id}/generate-shares`
- `PUT /api/operations/room-finance-shares/{shareId}`
- `POST /api/operations/room-finance-shares/{shareId}/mark-paid`

## UI can co

Trong `FinanceSection`:

- bang tong quan cong no theo phong
- cot:
  - phong
  - tong tien
  - da thu
  - con lai
  - so sinh vien da nop / tong sinh vien
  - trang thai
  - han thu
- nut `Xem sinh vien`

Khi bam `Xem sinh vien`:

- mo panel/modal chi tiet
- hien danh sach sinh vien trong phong
- moi dong co:
  - MSSV
  - ho ten
  - phai nop
  - da nop
  - con lai
  - trang thai
  - action `Danh dau da nop`
  - action `Sua so tien`

## Tieu chi hoan thanh

- Tao cong no phong sinh ra chi tiet tien tung sinh vien
- Thu tien tung sinh vien cap nhat lai tong da thu cua phong
- Nhin duoc phong nop du/chua du/nop mot phan
- Dieu chinh duoc tien tung sinh vien

---

## 5.3. Bao cao thong ke xuat Excel

Yeu cau:

- bao cao thong ke xuat Excel

## Backend

Can bo sung endpoint:

- `GET /api/reports/finance/export`
- `GET /api/reports/occupancy/export`
- `GET /api/reports/students/export`

Thu vien goi y:

- ClosedXML
- EPPlus neu da co license phu hop

Khuyen nghi dung ClosedXML de don gian.

## Frontend

Them khu `Bao cao` hoac them nut trong tung phan:

- xuat cong no
- xuat danh sach sinh vien
- xuat cong suat phong

Nut export goi API blob va download file `.xlsx`.

## Tieu chi hoan thanh

- Tai duoc file Excel
- File mo duoc trong Excel
- Cot tien co format VND
- Cot ngay co format ngay thang

---

## 6. Xoa sinh vien

Yeu cau:

- chuc nang xoa sinh vien bo alert thong bao xoa

## Hien trang kha nang cao

Frontend dang co confirm bang:

- `window.confirm`

trong `deleteEntity`.

File:

- [frontend/src/hooks/useDormitoryData.js](/C:/Users/Minhhoangg/Desktop/CODE/project/frontend/src/hooks/useDormitoryData.js)

## Huong sua

Khong dung browser alert/confirm.

Thay bang:

- modal confirm rieng
- hoac soft delete neu nghiep vu can giu lich su

Can can nhac nghiep vu:

- sinh vien co hop dong/hop dong tai chinh thi khong nen hard delete
- nen chan xoa hoac chuyen trang thai `Inactive`

## Tieu chi hoan thanh

- Khong con browser alert/confirm khi xoa sinh vien
- Neu xoa that, co modal confirm UI noi bo
- Neu sinh vien dang co lien ket, backend chan xoa va bao ly do

---

## 7. Tai khoan sinh vien va dang nhap sinh vien

## 7.1. Sinh vien tu dang ky tai khoan

Yeu cau:

- sinh vien tu dang ky tai khoan
- khi tao account sinh vien, co tai khoan dang nhap vao he thong

## Backend can bo sung

Co 2 huong:

- mo rong bang `Users` co lien ket `StudentId`
- hoac tao bang `StudentAccounts`

Khuyen nghi dung `Users.StudentId` neu he thong auth hien tai da dung `Users`.

Can them:

- `StudentId` nullable vao `Users`
- role `Student`
- permission student rieng
- endpoint dang ky:
  - `POST /api/auth/student-register`
- endpoint profile sinh vien:
  - `GET /api/student-portal/me`

## Frontend can bo sung

Them route:

- `/student/register`
- `/student`
- `/student/profile`
- `/student/room`
- `/student/finance`
- `/student/transfer-requests`
- `/student/messages`

Can phan biet role:

- admin/operator/cashier vao dashboard quan tri
- student vao student portal

## Quyen sinh vien

Sinh vien duoc:

- xem phong cua minh
- xem thong tin phong
- xem ban cung phong
- xem hoa don/cong no dien nuoc/ve sinh/dich vu cua phong
- xem phan tien minh phai nop
- sua thong tin ca nhan cho phep
- gui yeu cau chuyen phong
- xem do day cac phong de chon phong mong muon
- chat voi nguoi quan ly

Sinh vien khong duoc:

- CRUD sinh vien khac
- CRUD phong
- sua cong no
- duyet yeu cau

## Tieu chi hoan thanh

- Sinh vien tu dang ky tai khoan duoc
- Dang nhap bang tai khoan sinh vien vao portal rieng
- Khong truy cap duoc route admin

---

## 7.2. Yeu cau chuyen phong cua sinh vien

Yeu cau:

- sinh vien gui yeu cau chuyen phong
- quan tri nhan thong bao
- quan tri duyet hoac tu choi
- neu duyet thi cap nhat phong sinh vien
- neu tu choi thi khong cap nhat phong

## Backend can them entity

De xuat:

```csharp
public class RoomTransferRequest : BaseEntity
{
    public int StudentId { get; set; }
    public int? CurrentRoomId { get; set; }
    public int RequestedRoomId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime RequestedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string ReviewNote { get; set; } = string.Empty;
    public int? ReviewedByUserId { get; set; }
}
```

## API can co

Student:

- `POST /api/student-portal/transfer-requests`
- `GET /api/student-portal/transfer-requests`

Admin:

- `GET /api/operations/transfer-requests`
- `POST /api/operations/transfer-requests/{id}/approve`
- `POST /api/operations/transfer-requests/{id}/reject`

## Thong bao

Dashboard admin can co notification:

- `Co X yeu cau chuyen phong dang cho duyet`

Click vao thong bao:

- navigate `/operations`
- scroll den panel transfer requests

## Tieu chi hoan thanh

- Sinh vien gui request duoc
- Admin thay request
- Approve cap nhat phong va si so phong
- Reject giu nguyen phong
- Moi thao tac co lich su

---

## 7.3. Chat giua sinh vien va nguoi quan ly

Yeu cau:

- nguoi quan ly co the chat truc tiep voi sinh vien cua phong do
- sinh vien co the chat voi nguoi quan ly
- dung de hoi dap, bao cao su co

## Backend can them entity

De xuat:

```csharp
public class Conversation : BaseEntity
{
    public int? RoomId { get; set; }
    public int StudentId { get; set; }
    public int ManagerUserId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Status { get; set; } = "Open";
}

public class Message : BaseEntity
{
    public int ConversationId { get; set; }
    public int SenderUserId { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public DateTime? ReadAt { get; set; }
}
```

## API can co

- `GET /api/messages/conversations`
- `POST /api/messages/conversations`
- `GET /api/messages/conversations/{id}/messages`
- `POST /api/messages/conversations/{id}/messages`
- `POST /api/messages/conversations/{id}/close`

## UI

Admin:

- panel chat trong quan tri hoac dieu phoi
- loc theo phong/sinh vien
- hien unread count

Student:

- muc `Tin nhan`
- tao hoi dap / bao cao su co
- xem phan hoi cua quan ly

## Tieu chi hoan thanh

- Gui/nhan tin duoc
- Phan quyen dung student/admin
- Co unread count hoac trang thai da doc

---

## 8. Toi uu input so va form

Yeu cau:

- toi uu lai giao dien cac o input lien quan toi so dang kho nhap

## File can sua

- [frontend/src/App.css](/C:/Users/Minhhoangg/Desktop/CODE/project/frontend/src/App.css)
- [frontend/src/forms/EntityForm.jsx](/C:/Users/Minhhoangg/Desktop/CODE/project/frontend/src/forms/EntityForm.jsx)
- [frontend/src/helpers.js](/C:/Users/Minhhoangg/Desktop/CODE/project/frontend/src/helpers.js)

## Can lam

- input/select/textarea co min-height 46-48px
- focus ring ro
- khong bi overlay chan click
- input so tien hien format VND nhung van nhap duoc de dang
- cac field so luong/chi so dien nuoc khong nen format tien
- can phan biet:
  - currency fields
  - meter index fields
  - count fields

## Tieu chi hoan thanh

- Click vao input focus ngay
- Go so khong bi mat ky tu
- So dien nuoc khong bi format thanh tien
- Tien VND format dung sau khi nhap

---

## 9. Thong bao va dieu huong den noi xu ly

Tu adjustment co nhieu workflow can thong bao:

- dang ky noi tru cho duyet
- yeu cau chuyen phong cho duyet
- cong no qua han
- phong/sinh vien chua nop tien

## Backend

Mo rong `GET /api/dashboard` tra them `notifications`.

Moi notification nen co:

```json
{
  "id": "transfer-requests",
  "severity": "warning",
  "title": "Yeu cau chuyen phong",
  "description": "Co 3 yeu cau dang cho duyet.",
  "count": 3,
  "route": "/operations",
  "panelKey": "operations-transfer-requests",
  "panelId": "panel-operations-transfer-requests",
  "actionLabel": "Xu ly ngay"
}
```

## Frontend

Click notification:

- navigate route
- expand panel
- scroll den panel

## Tieu chi hoan thanh

- Notification click den dung khu xu ly
- Neu panel dang thu gon thi tu mo lai

---

## 10. Thu tu uu tien trien khai

## Phase 1: Sua loi nghiep vu cot loi

1. Bo chon phong trong form tao sinh vien.
2. Bo ngay xu ly trong form dang ky noi tru.
3. Chan xep phong neu sinh vien chua co hop dong hop le.
4. Sua assign/transfer/remove phong de cap nhat si so dung.
5. Chan huy hop dong neu sinh vien con o phong.
6. Sua input so va form kho nhap.

## Phase 2: Tai chinh phong va chia tien sinh vien

1. Chuan hoa don gia dien/nuoc theo phong.
2. Tao cong no theo phong.
3. Them bang chia tien theo sinh vien.
4. UI xem phong da nop/chua nop.
5. UI xem tung sinh vien da nop/chua nop.
6. Cho phep dieu chinh tien tung sinh vien.

## Phase 3: Student portal

1. Role `Student`.
2. Dang ky tai khoan sinh vien.
3. Dang nhap sinh vien.
4. Portal xem phong, ban cung phong, tai chinh.
5. Sua thong tin ca nhan.

## Phase 4: Yeu cau chuyen phong va chat

1. Sinh vien gui yeu cau chuyen phong.
2. Admin duyet/tu choi.
3. Thong bao admin.
4. Chat sinh vien - quan ly.

## Phase 5: Bao cao

1. Export Excel cong no.
2. Export Excel sinh vien.
3. Export Excel cong suat phong.

---

## 11. Checklist khong duoc bo sot

- Dang ky noi tru khong nhap ngay xu ly.
- Sinh vien tao moi khong chon phong.
- Xep phong chi cho sinh vien co hop dong hop le.
- Xep/chuyen/tra phong cap nhat si so phong.
- Huy hop dong bi chan neu sinh vien con o phong.
- Dien/nuoc dung don gia theo phong.
- Cong no tao theo phong.
- Cong no phong chia duoc cho tung sinh vien.
- Thu tien duoc theo tung sinh vien.
- Xem duoc phong nao nop, sinh vien nao nop.
- Xoa sinh vien khong dung browser alert.
- Co export Excel.
- Sinh vien tu dang ky tai khoan.
- Sinh vien dang nhap vao portal rieng.
- Sinh vien gui yeu cau chuyen phong.
- Admin duyet/tu choi yeu cau chuyen phong.
- Co chat sinh vien - quan ly.
- Input so de nhap, khong bi loi focus.
- Notification dieu huong den dung noi xu ly.

---

## 12. Kiem thu cuoi

Sau khi trien khai, can test toi thieu cac luong:

1. Tao sinh vien moi, khong chon phong.
2. Tao hop dong cho sinh vien.
3. Xep sinh vien vao phong, si so phong cap nhat.
4. Thu phong, si so phong cap nhat.
5. Thu huy hop dong khi sinh vien con trong phong, backend phai chan.
6. Nhap dien nuoc cho phong, tien duoc tinh dung.
7. Tao cong no phong, tien chia cho tung sinh vien.
8. Danh dau 1 sinh vien da nop, trang thai phong cap nhat thanh nop mot phan.
9. Sinh vien dang ky tai khoan va dang nhap portal.
10. Sinh vien gui yeu cau chuyen phong.
11. Admin duyet yeu cau, phong sinh vien thay doi.
12. Gui tin nhan giua sinh vien va quan ly.
13. Xuat Excel thanh cong.
14. Tat ca input trong modal click/focus/nhap duoc binh thuong.

