
# Hướng Dẫn Sửa Tự Động Đổ Phí Khi Thêm Công Nợ Tài Chính Theo Phòng

## Vấn đề đang gặp

Hiện tại trong hệ thống đã có phần:

- `Cấu hình phí theo phòng`

Ví dụ đã lưu:

- tiền phòng
- phí vệ sinh
- phí dịch vụ
- phí internet
- phí khác
- ngày chốt

Nhưng khi sang form:

- `Thêm mới Công nợ tài chính theo phòng`

thì các ô chi phí vẫn đang là `0` hoặc phải nhập lại bằng tay.

Điều này gây bất hợp lý vì:

1. Dữ liệu phí theo phòng đã có sẵn.
2. Người dùng không nên nhập lặp lại cùng một cấu hình.
3. Dễ nhập sai, lệch số giữa `roomFeeProfiles` và `roomFinances`.

---

## Mục tiêu sửa

Khi tạo mới `Công nợ tài chính theo phòng`, hệ thống phải:

1. Khi chọn `Phòng`, tự động lấy cấu hình phí tương ứng từ `roomFeeProfiles`.
2. Tự điền vào các trường:
   - `Tiền phòng`
   - `Phí vệ sinh`
   - `Phí dịch vụ`
   - `Phí internet`
   - `Phí khác`
3. Nếu có `Kỳ điện nước`, tiếp tục tự lấy:
   - `Tiền điện`
   - `Tiền nước`
4. Tự gợi ý:
   - `Hạn thu`
   - có thể dựa trên `billingCycleDay`
5. Người dùng vẫn có thể chỉnh tay nếu cần, nhưng mặc định phải được đổ sẵn.

---

## Nguyên nhân hiện tại

## 1. Form đang là form CRUD tổng quát

File:

- [frontend/src/forms/EntityForm.jsx](/C:/Users/Minhhoangg/Desktop/CODE/project/frontend/src/forms/EntityForm.jsx)

Hiện tại form này chỉ:

- đọc `ENTITY_CONFIGS`
- render field theo cấu hình
- gọi `updateModalField(name, value)`

Nó chưa có logic nghiệp vụ riêng cho:

- `roomFinances`

Nên khi chọn `roomId`, form không biết phải tự đi lấy `roomFeeProfiles`.

## 2. ENTITY_CONFIGS chỉ mô tả field, không chứa logic tự nạp

File:

- [frontend/src/constants.js](/C:/Users/Minhhoangg/Desktop/CODE/project/frontend/src/constants.js)

Trong `roomFinances.fields` hiện đang có:

- `monthlyRoomFee`
- `electricityFee`
- `waterFee`
- `hygieneFee`
- `serviceFee`
- `internetFee`
- `otherFee`

Nhưng đây chỉ là khai báo form field.

Nó không nói:

- khi chọn `roomId` thì lấy `roomFeeProfiles`
- khi chọn `utilityId` thì lấy `electricityFee`, `waterFee`

## 3. updateModalField hiện mới xử lý cho rooms

File:

- [frontend/src/hooks/useDormitoryData.js](/C:/Users/Minhhoangg/Desktop/CODE/project/frontend/src/hooks/useDormitoryData.js)

Hiện `updateModalField()` mới có logic đặc biệt cho:

- `rooms + roomCategoryId`
- `rooms + roomZoneId`

Chưa có logic cho:

- `roomFinances + roomId`
- `roomFinances + utilityId`

Đây là nguyên nhân trực tiếp khiến form công nợ không tự đổ dữ liệu.

---

## Dữ liệu nào phải được dùng làm nguồn mặc định

## Nguồn 1: roomFeeProfiles

File config frontend:

- [frontend/src/constants.js](/C:/Users/Minhhoangg/Desktop/CODE/project/frontend/src/constants.js)

Phần `roomFeeProfiles` đang lưu:

- `monthlyRoomFee`
- `electricityUnitPrice`
- `waterUnitPrice`
- `hygieneFee`
- `serviceFee`
- `internetFee`
- `otherFee`
- `otherFeeName`
- `billingCycleDay`

Đây phải là nguồn mặc định chính khi tạo `roomFinances`.

## Nguồn 2: utilities

Nếu người dùng chọn thêm `Kỳ điện nước`, thì nên lấy tiếp:

- `electricityFee`
- `waterFee`

từ bản ghi utility tương ứng.

---

## Hành vi đúng sau khi sửa

## Trường hợp 1: Chọn phòng trước

Khi người dùng chọn `Phòng` trong form `roomFinances`:

- hệ thống tìm `roomFeeProfiles` theo `roomId`
- nếu tìm thấy:
  - đổ `monthlyRoomFee`
  - đổ `hygieneFee`
  - đổ `serviceFee`
  - đổ `internetFee`
  - đổ `otherFee`
  - gợi ý `dueDate` theo `billingCycleDay`

## Trường hợp 2: Chọn kỳ điện nước

Khi người dùng chọn `utilityId`:

- lấy `electricityFee`
- lấy `waterFee`
- nếu `billingMonth` chưa có thì lấy theo utility

## Trường hợp 3: Đã có cả phòng và utility

Hệ thống phải hợp nhất:

- phí cố định từ `roomFeeProfiles`
- phí điện nước từ `utilities`

## Trường hợp 4: Không có roomFeeProfile

Nếu phòng chưa có cấu hình phí riêng:

- có thể fallback về `roomCategory`
- hoặc báo rõ:
  - `Phòng này chưa có cấu hình phí. Vui lòng tạo cấu hình phí theo phòng trước.`

Không nên để người dùng tưởng hệ thống tự tính đúng trong khi thực tế toàn bộ đang là `0`.

---

## Chỗ cần sửa trong frontend

## 1. useDormitoryData.js

File:

- [frontend/src/hooks/useDormitoryData.js](/C:/Users/Minhhoangg/Desktop/CODE/project/frontend/src/hooks/useDormitoryData.js)

### Đây là nơi quan trọng nhất

Cần mở rộng `updateModalField(name, value)` để xử lý riêng cho `roomFinances`.

### Logic cần thêm

Khi:

- `current.entityKey === 'roomFinances'`

thì:

### Nếu đổi `roomId`

1. tìm `roomFeeProfiles.find(x => String(x.roomId) === String(value))`
2. nếu có:
   - set:
     - `monthlyRoomFee`
     - `hygieneFee`
     - `serviceFee`
     - `internetFee`
     - `otherFee`
3. nếu chưa chọn utility:
   - giữ `electricityFee`, `waterFee` là `0`
4. nếu có `billingCycleDay`:
   - tính `dueDate` theo `billingMonth` hoặc tháng hiện tại

### Nếu đổi `utilityId`

1. tìm `lookups.utilities` hoặc `data.utilities`
2. lấy:
   - `electricityFee`
   - `waterFee`
   - `billingMonth`
3. nếu utility có `roomId` mà form chưa có `roomId`:
   - tự set `roomId`
4. sau đó tiếp tục đồng bộ phí cố định theo `roomFeeProfiles`

## 2. openCreate('roomFinances')

Trong [frontend/src/hooks/useDormitoryData.js](/C:/Users/Minhhoangg/Desktop/CODE/project/frontend/src/hooks/useDormitoryData.js)

Khi mở form tạo mới `roomFinances`, nên set default tốt hơn:

- `billingMonth` = ngày đầu tháng hiện tại
- `paidAmount` = `0`
- `status` = `Unpaid`
- `paymentMethod` = `''`

Có thể thêm:

- `dueDate` = rỗng hoặc gợi ý khi chọn phòng

## 3. EntityForm.jsx

File:

- [frontend/src/forms/EntityForm.jsx](/C:/Users/Minhhoangg/Desktop/CODE/project/frontend/src/forms/EntityForm.jsx)

File này không nhất thiết phải sửa quá nhiều nếu đã xử lý ở `updateModalField`.

Nhưng có thể bổ sung UX tốt hơn:

- khi `roomFinances` chưa có `roomFeeProfile`
  - hiện cảnh báo nhỏ dưới field `Phòng`
- khi tự động điền phí:
  - hiện note:
    - `Đã nạp cấu hình phí từ phòng A101`

---

## Chỗ cần sửa trong backend

Frontend tự đổ form là cần thiết, nhưng chưa đủ.

Để hệ thống an toàn và đúng nghiệp vụ, backend cũng nên có lớp fallback mặc định.

## Vì sao backend cũng phải sửa

Nếu chỉ sửa frontend:

- người dùng khác gọi API trực tiếp vẫn có thể gửi toàn bộ phí bằng `0`
- dữ liệu `roomFinances` vẫn có thể sai

Do đó backend nên tự fill lại nếu request gửi thiếu.

## Nơi cần kiểm tra

File có khả năng cần sửa:

- [backend/DormitoryManagement/Controllers/Operations/OperationsController.cs](/C:/Users/Minhhoangg/Desktop/CODE/project/backend/DormitoryManagement/Controllers/Operations/OperationsController.cs)
- hoặc service xử lý nghiệp vụ tài chính nếu project đang gom logic tại service

## Logic backend nên có

Khi tạo `RoomFinanceRecord`:

1. đọc `roomId`
2. tìm `RoomFeeProfile` theo `roomId`
3. nếu các trường phí gửi lên là `0` hoặc null:
   - lấy từ `RoomFeeProfile`
4. nếu có `utilityId`:
   - lấy thêm `electricityFee`, `waterFee`
5. tính lại:
   - `Total`
   - `RemainingAmount`
   - `Status`

## Mục tiêu

Frontend giúp tiện dùng.

Backend giúp chống sai dữ liệu.

Cả hai đều nên làm.

---

## Hướng xử lý đúng nhất

## Cách tốt hơn so với hiện tại

Thay vì để user bấm `Thêm mới công nợ` rồi nhập tay toàn bộ:

Nên có 2 luồng:

### Luồng 1: Tạo công nợ tự động từ kỳ điện nước

Project hiện đã có dấu hiệu của luồng này:

- nút `Đồng bộ từ điện nước`
- API generate từ utility

Đây nên là luồng chính.

### Luồng 2: Tạo công nợ thủ công

Nếu tạo tay thì:

- chọn phòng
- hệ thống tự điền cấu hình phí
- user chỉ kiểm tra hoặc chỉnh nhẹ

Tức là:

- không còn bắt nhập lại từ đầu

---

## Gợi ý thuật toán tính dueDate

Nếu `roomFeeProfile.billingCycleDay = 10`

và `billingMonth = 2026-04-01`

thì `dueDate` nên là:

- `2026-04-10`

Nếu ngày chốt lớn hơn số ngày trong tháng:

- clamp về ngày cuối tháng

Ví dụ:

- `billingCycleDay = 31`
- tháng 2/2026

thì `dueDate = 2026-02-28`

---

## Gợi ý code logic ở frontend

Không cần viết đúng y nguyên, nhưng nên theo tinh thần sau:

```js
if (current.entityKey === 'roomFinances' && name === 'roomId') {
  const profile = data.roomFeeProfiles.find((item) => String(item.roomId) === String(value))

  if (profile) {
    nextValues.monthlyRoomFee = profile.monthlyRoomFee ?? 0
    nextValues.hygieneFee = profile.hygieneFee ?? 0
    nextValues.serviceFee = profile.serviceFee ?? 0
    nextValues.internetFee = profile.internetFee ?? 0
    nextValues.otherFee = profile.otherFee ?? 0
  }
}

if (current.entityKey === 'roomFinances' && name === 'utilityId') {
  const utility = data.utilities.find((item) => String(item.id) === String(value))

  if (utility) {
    nextValues.electricityFee = utility.electricityFee ?? 0
    nextValues.waterFee = utility.waterFee ?? 0
    nextValues.billingMonth = utility.billingMonth ?? nextValues.billingMonth
  }
}
```

Sau đó có thể gọi thêm hàm chung:

```js
applyRoomFinanceDefaults(nextValues)
```

để tính:

- dueDate
- total nếu muốn hiển thị sớm

---

## Gợi ý UX nên làm thêm

Để user hiểu vì sao form tự đổi số, nên thêm:

1. Dòng chú thích:
   - `Chi phí đã được nạp từ cấu hình phí của phòng.`
2. Nếu chưa có profile:
   - `Phòng này chưa có cấu hình phí, vui lòng tạo trước.`
3. Nút nhỏ:
   - `Nạp lại từ cấu hình phòng`

Điều này rất hữu ích nếu user sửa nhầm rồi muốn lấy lại mặc định.

---

## Tối ưu hơn nữa

Nếu muốn hệ thống thực tế hơn, nên cân nhắc:

1. Khi tạo `RoomFeeProfile` cho phòng, có thể gợi ý tự động từ `RoomCategory`.
2. Khi tạo `RoomFinanceRecord`, mặc định khóa nhẹ các ô phí cố định và chỉ cho sửa khi cần.
3. Cho phép chọn:
   - `Tạo theo cấu hình phòng`
   - `Nhập tay`

Nhưng bản sửa trước mắt chỉ cần:

- tự đổ dữ liệu
- không bắt nhập lại từ đầu

là đã đúng vấn đề.

---

## Danh sách file nên sửa

Ưu tiên:

- [frontend/src/hooks/useDormitoryData.js](/C:/Users/Minhhoangg/Desktop/CODE/project/frontend/src/hooks/useDormitoryData.js)
- [frontend/src/forms/EntityForm.jsx](/C:/Users/Minhhoangg/Desktop/CODE/project/frontend/src/forms/EntityForm.jsx)
- [frontend/src/constants.js](/C:/Users/Minhhoangg/Desktop/CODE/project/frontend/src/constants.js)

Nếu làm đầy đủ:

- [backend/DormitoryManagement/Controllers/Operations/OperationsController.cs](/C:/Users/Minhhoangg/Desktop/CODE/project/backend/DormitoryManagement/Controllers/Operations/OperationsController.cs)

---

## Tiêu chí hoàn thành

Chỉ coi là sửa xong khi đạt đủ:

1. Chọn `Phòng` trong form `roomFinances` sẽ tự đổ phí cố định từ `roomFeeProfiles`.
2. Chọn `Kỳ điện nước` sẽ tự đổ `Tiền điện`, `Tiền nước`.
3. Không còn phải nhập lại toàn bộ chi phí đã cấu hình.
4. Nếu chưa có `roomFeeProfile`, hệ thống báo rõ.
5. Backend vẫn có lớp fallback để tránh lưu sai dữ liệu nếu frontend gửi thiếu.
6. Người dùng có thể chỉnh tay sau khi hệ thống tự điền, nếu nghiệp vụ cho phép.

---

## Kết luận

Lỗi hiện tại không phải do dữ liệu phí chưa có, mà do:

- form `roomFinances` chưa có logic liên kết với `roomFeeProfiles`
- và backend chưa tự bù dữ liệu mặc định khi tạo công nợ

Hướng sửa đúng là:

- tự nạp phí theo phòng ở frontend khi chọn `roomId`
- tự nạp tiền điện nước khi chọn `utilityId`
- có fallback ở backend để đảm bảo dữ liệu luôn đúng

