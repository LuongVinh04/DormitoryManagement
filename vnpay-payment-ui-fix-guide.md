# Hướng Dẫn Sửa Luồng Thanh Toán VNPay Cho Quản Trị Và Sinh Viên

## Mục Tiêu

Sửa luồng thanh toán VNPay để đạt các yêu cầu sau:

- Khi quản trị chọn thu tiền phòng qua VNPay, hệ thống phải tự có hoặc tự tạo hóa đơn cho phần chia của sinh viên.
- Không hiển thị câu kỹ thuật: `VNPay sandbox chặn nhúng trong iframe. Hệ thống sẽ mở cổng thanh toán trong tab mới giống luồng WebView riêng của app mẫu.`
- Dialog thanh toán chỉ hiển thị thông tin nghiệp vụ rõ ràng, không đưa chi tiết kỹ thuật cho người dùng.
- Không cố nhúng trực tiếp `sandbox.vnpayment.vn` vào `iframe` của web vì VNPay chặn nhúng iframe.
- Nếu muốn trải nghiệm giống WebView, chỉ làm được trong app/native wrapper như Flutter `InAppWebView`, Electron `BrowserWindow/BrowserView`, Tauri WebView, hoặc mở popup/tab top-level trên web.

## Nguyên Nhân Hiện Tại

Trong app Flutter cũ, màn `payprocess.dart` không tự nhúng URL VNPay trong web HTML thông thường. Luồng cũ là:

1. Flutter mở `InAppWebView`.
2. `InAppWebView` gửi `POST` tới endpoint merchant PHP: `vnpay_create_payment.php`.
3. PHP tạo URL VNPay, ký `vnp_SecureHash`, rồi `header('Location: ...')`.
4. WebView đi top-level sang VNPay.
5. Khi VNPay redirect về return URL, Flutter đọc query `vnp_ResponseCode`.

Điểm quan trọng: `InAppWebView` là WebView native, không phải iframe HTML. Trên web browser, VNPay thường chặn iframe bằng header bảo mật như `X-Frame-Options` hoặc `Content-Security-Policy frame-ancestors`, nên dialog có iframe sẽ báo kiểu `sandbox.vnpayment.vn đã từ chối kết nối`.

## File Cần Sửa

- `C:\Users\Minhhoangg\Desktop\CODE\project\backend\DormitoryManagement\Controllers\Operations\OperationsController.cs`
- `C:\Users\Minhhoangg\Desktop\CODE\project\backend\DormitoryManagement\Controllers\StudentPortal\StudentPortalController.cs`
- `C:\Users\Minhhoangg\Desktop\CODE\project\backend\DormitoryManagement\Services\VnPayService.cs`
- `C:\Users\Minhhoangg\Desktop\CODE\project\frontend\src\features\finance\FinanceSection.jsx`
- `C:\Users\Minhhoangg\Desktop\CODE\project\frontend\src\features\student-portal\StudentPortalSection.jsx`
- `C:\Users\Minhhoangg\Desktop\CODE\project\frontend\src\styles.css` hoặc file CSS đang chứa style modal/VNPay.

## Sửa Backend: Quản Trị Bấm VNPay Phải Có Hóa Đơn

Hiện luồng sinh viên đã có helper `EnsureInvoiceForShareAsync(...)` trong `StudentPortalController.cs`. Helper này tự tạo `Invoices` nếu `RoomFinanceStudentShare` chưa có hóa đơn.

Luồng quản trị trong `OperationsController.cs` tại endpoint:

```csharp
[HttpPost("room-finance-shares/{shareId:int}/vnpay/create")]
public async Task<IActionResult> CreateStudentShareVnPayPayment(int shareId)
```

không nên trả lỗi khi `share.Invoice is null`. Thay vào đó phải tự tạo hóa đơn giống luồng sinh viên.

Thay đoạn logic kiểu:

```csharp
if (share.Invoice is null)
{
    return BadRequest(new { message = "Phần chia này chưa có hóa đơn để thanh toán qua VNPay." });
}
```

bằng:

```csharp
var invoice = await EnsureInvoiceForShareAsync(share);
```

Sau đó dùng `invoice` để tạo link:

```csharp
var paymentUrl = vnPayService.CreatePaymentUrl(HttpContext, invoice, amount, "/finance");
```

Response trả về phải dùng `invoice`, không dùng `share.Invoice` trực tiếp:

```csharp
return Ok(new
{
    paymentMethod = "VNPAY",
    shareId = share.Id,
    invoiceId = invoice.Id,
    invoice.InvoiceCode,
    studentName = share.Student?.Name,
    amount,
    paymentUrl
});
```

Thêm helper vào `OperationsController.cs` hoặc tách ra service dùng chung:

```csharp
private async Task<Invoices> EnsureInvoiceForShareAsync(RoomFinanceStudentShare share)
{
    if (share.Invoice is not null)
    {
        return share.Invoice;
    }

    if (share.InvoiceId.HasValue)
    {
        var existingInvoice = await db.Invoices.FirstOrDefaultAsync(x => x.Id == share.InvoiceId.Value);
        if (existingInvoice is not null)
        {
            share.Invoice = existingInvoice;
            return existingInvoice;
        }
    }

    var record = await db.RoomFinanceRecords
        .Include(x => x.Utility)
        .FirstOrDefaultAsync(x => x.Id == share.RoomFinanceRecordId);

    var student = await db.Students.FirstOrDefaultAsync(x => x.Id == share.StudentId);

    if (record is null || student is null)
    {
        throw new InvalidOperationException("Không đủ dữ liệu để tạo hóa đơn cho phần chia này.");
    }

    var invoice = new Invoices
    {
        InvoiceCode = $"INV-{record.BillingMonth:yyyyMM}-{student.StudentCode}-{share.Id}",
        StudentId = student.Id,
        RoomId = record.RoomId,
        UtilityId = record.UtilityId,
        RoomFee = Math.Round(share.ExpectedAmount, 0),
        ElectricityFee = 0,
        WaterFee = 0,
        ServiceFee = 0,
        OtherFee = 0,
        Total = Math.Round(share.ExpectedAmount, 0),
        PaidAmount = 0,
        Status = "Unpaid",
        BillingMonth = record.BillingMonth,
        DueDate = record.DueDate,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    db.Invoices.Add(invoice);
    await db.SaveChangesAsync();

    share.InvoiceId = invoice.Id;
    share.Invoice = invoice;
    share.UpdatedAt = DateTime.UtcNow;
    await db.SaveChangesAsync();

    return invoice;
}
```

Nếu model `Invoices` hiện không có đủ field trong snippet, giữ đúng tên field đang có trong entity thật. Không tự thêm field mới nếu database chưa có migration.

## Sửa Backend: Trả Link Theo Kiểu Merchant Endpoint

Backend hiện có API trả JSON chứa `paymentUrl`. Cách này dùng được, nhưng nếu muốn giống Flutter/PHP hơn thì thêm một endpoint launch dạng browser top-level:

```csharp
[HttpGet("room-finance-shares/{shareId:int}/vnpay/launch")]
[Authorize]
public async Task<IActionResult> LaunchStudentShareVnPayPayment(int shareId)
{
    var result = await CreateStudentShareVnPayPaymentInternal(shareId);
    return Redirect(result.PaymentUrl);
}
```

Nên tách phần tạo payment URL ra method nội bộ để cả endpoint JSON và endpoint `launch` dùng chung:

```csharp
private async Task<VnPayCreateResult> CreateStudentShareVnPayPaymentInternal(int shareId)
```

Khi frontend cần mở cổng thanh toán, dùng URL nội bộ:

```text
/api/operations/room-finance-shares/{shareId}/vnpay/launch
```

Lợi ích:

- Gần giống PHP cũ: merchant endpoint redirect sang VNPay.
- Không đưa quá nhiều query VNPay ra frontend state.
- Popup/tab mở URL nội bộ, sau đó server redirect sang VNPay.

## Sửa Frontend: Bỏ Text Kỹ Thuật Trong Dialog

Trong `FinanceSection.jsx`, tìm đoạn:

```jsx
<p>VNPay sandbox chặn nhúng trong iframe. Hệ thống sẽ mở cổng thanh toán trong tab mới giống luồng WebView riêng của app mẫu.</p>
```

Thay bằng nội dung nghiệp vụ:

```jsx
<p>Kiểm tra thông tin khoản thu trước khi mở cổng thanh toán VNPay.</p>
```

Hoặc bỏ hẳn thẻ `<p>` nếu muốn dialog gọn hơn.

Trong `StudentPortalSection.jsx`, cũng thay đoạn tương tự bằng:

```jsx
<p>Kiểm tra hóa đơn và số tiền cần thanh toán trước khi tiếp tục.</p>
```

## Sửa Frontend: Dialog Thanh Toán Cho Quản Trị

Dialog quản trị cần hiển thị rõ:

- Sinh viên.
- Mã sinh viên.
- Hóa đơn.
- Số tiền còn lại.
- Phương thức: VNPay.
- Trạng thái tạo link.

Nếu `invoiceCode` đang là `-`, phải xử lý theo 2 lớp:

Lớp backend:

API VNPay tự tạo invoice trước khi trả payment URL.

Lớp frontend:

Sau khi API trả về `invoiceCode`, cập nhật state của dòng share để dialog và bảng hiển thị invoice mới.

Ví dụ trong `startVnPayPayment` của `FinanceSection.jsx`:

```jsx
const data = await response.json()

setShareForms((current) => ({
  ...current,
  [share.id]: {
    ...current[share.id],
    invoiceId: data.invoiceId,
    invoiceCode: data.invoiceCode,
  },
}))

setVnPayDialog((current) => ({
  ...current,
  invoiceId: data.invoiceId,
  invoiceCode: data.invoiceCode,
  paymentUrl: data.paymentUrl,
  amount: data.amount,
}))
```

Nếu `shares` được render từ state riêng, cũng cập nhật item đó hoặc gọi lại `loadShares(selectedRecord, { clearNotice: false })`.

## Sửa Frontend: Dialog Thanh Toán Cho Sinh Viên

Sinh viên đã có endpoint:

```text
POST /api/student-portal/room-finance-shares/{shareId}/vnpay/create
```

Khi API trả về `invoiceCode`, dialog dùng:

```jsx
<strong>{paymentDialog.invoiceCode || paymentDialog.share?.invoiceCode || '-'}</strong>
```

Nếu vẫn có `-`, cần gọi lại `loadPortalData()` sau khi tạo link hoặc cập nhật state tương tự:

```jsx
setPaymentDialog({
  url: data.paymentUrl,
  share: { ...share, invoiceId: data.invoiceId, invoiceCode: data.invoiceCode },
  amount: data.amount ?? share.remainingAmount,
  invoiceCode: data.invoiceCode ?? share.invoiceCode,
})
```

## Không Nhúng Trực Tiếp VNPay Bằng iframe

Không dùng:

```jsx
<iframe src={paymentUrl} />
```

Lý do:

- VNPay có thể chặn iframe bằng `X-Frame-Options`.
- VNPay có thể chặn bằng `Content-Security-Policy: frame-ancestors`.
- Browser sẽ báo `sandbox.vnpayment.vn đã từ chối kết nối`.
- Đây là giới hạn bảo mật phía VNPay/browser, không phải lỗi React.

## Phương Án Thay Thế Dialog + WebView

### Phương Án 1: Web Chuẩn

Dùng modal dialog để xác nhận thông tin, sau đó mở tab hoặc popup top-level.

```jsx
window.open(paymentUrl, '_blank', 'noopener,noreferrer')
```

Hoặc mở endpoint nội bộ:

```jsx
window.open(`/api/operations/room-finance-shares/${share.id}/vnpay/launch`, '_blank', 'noopener,noreferrer')
```

Đây là phương án nên dùng cho web browser.

### Phương Án 2: Popup Nhìn Gần Giống Dialog

Mở popup có kích thước cố định:

```jsx
const width = 960
const height = 720
const left = window.screenX + (window.outerWidth - width) / 2
const top = window.screenY + (window.outerHeight - height) / 2

window.open(
  paymentUrl,
  'vnpay_payment',
  `width=${width},height=${height},left=${left},top=${top},resizable=yes,scrollbars=yes`
)
```

Ưu điểm:

- Không bị chặn iframe.
- Trải nghiệm gần giống cửa sổ thanh toán riêng.
- Vẫn tuân thủ cơ chế bảo mật của VNPay.

Nhược điểm:

- Một số browser có thể chặn popup nếu không gọi trực tiếp từ event click.
- Không phải dialog nằm trong DOM của React.

### Phương Án 3: App Native/WebView

Nếu triển khai app desktop/mobile thì có thể dùng WebView thật:

- Flutter: `InAppWebView`.
- Electron: `BrowserWindow` hoặc `BrowserView`.
- Tauri: WebView window.

Luồng giống Flutter cũ:

1. Mở WebView tới endpoint merchant nội bộ.
2. Endpoint merchant redirect sang VNPay.
3. WebView theo dõi URL return.
4. Khi có `vnp_ResponseCode=00`, gọi API xác nhận/làm mới trạng thái.

Không áp dụng được cho web React chạy trong browser thông thường.

## Sửa Nút Hành Động

Trong dialog quản trị:

- Khi chưa có `paymentUrl`, nút chính là `Tạo link thanh toán`.
- Khi đã có `paymentUrl`, nút chính là `Mở cổng VNPay`.
- Nếu chọn `Tiền mặt`, không gọi VNPay; hiển thị nút `Xác nhận đã thu`.

Label đề xuất:

```jsx
{vnPayDialog.paymentUrl ? 'Mở cổng VNPay' : 'Tạo link VNPay'}
```

Không dùng text kỹ thuật trong modal.

## Sửa API Return Sau Thanh Toán

VNPay redirect về:

```text
GET /api/student-portal/vnpay-return
```

Backend cần:

- Validate `vnp_SecureHash`.
- Lấy `vnp_TxnRef`.
- Tìm đúng invoice.
- Cập nhật `Invoices`.
- Cập nhật `RoomFinanceStudentShare`.
- Cập nhật tổng `RoomFinanceRecord`.
- Redirect về frontend với query trạng thái.

Ví dụ redirect:

```text
/finance?paymentStatus=success&invoiceId=123
```

hoặc:

```text
/?paymentStatus=success&invoiceId=123
```

Tùy màn gọi là quản trị hay sinh viên. Nếu cần phân biệt, khi tạo payment nên lưu `clientPath` hoặc cấu hình `ClientReturnUrl`.

## Bổ Sung: Sửa Layout Cổng Sinh Viên Rộng Và Cân Bằng Hơn

Hiện giao diện cổng sinh viên đang bị gom vào giữa, container quá hẹp so với màn hình desktop. Cần dàn nội dung sang ngang hơn, tận dụng chiều rộng màn hình nhưng vẫn giữ responsive tốt trên tablet/mobile.

File cần sửa chính:

- `C:\Users\Minhhoangg\Desktop\CODE\project\frontend\src\features\student-portal\StudentPortalSection.jsx`
- `C:\Users\Minhhoangg\Desktop\CODE\project\frontend\src\styles.css` hoặc file CSS đang khai báo `.student-portal-*`.

Mục tiêu giao diện:

- Desktop rộng: nội dung không bị bó ở giữa, container rộng khoảng `min(1500px, calc(100vw - 80px))`.
- Header sinh viên kéo ngang theo container, không chỉ chiếm một cột nhỏ.
- Khối thông tin cá nhân và phòng ở nằm cạnh nhau theo tỷ lệ cân bằng.
- Khối công nợ tài chính rộng hơn, dễ đọc bảng.
- Các khối chuyển phòng và nhắn tin nằm 2 cột bên dưới.
- Tablet tự chuyển còn 2 cột hoặc 1 cột tùy chiều rộng.
- Mobile chuyển 1 cột, bảng có scroll ngang hoặc card hóa.

### Cấu Trúc Layout Đề Xuất

Trong `StudentPortalSection.jsx`, bọc toàn bộ nội dung bằng một shell rõ ràng:

```jsx
<div className="student-portal-page">
  <div className="student-portal-shell">
    <header className="student-portal-hero">...</header>

    <div className="student-portal-overview-grid">
      <section className="student-portal-card profile-card">...</section>
      <section className="student-portal-card room-card">...</section>
    </div>

    <section className="student-portal-card finance-card full-width">...</section>

    <div className="student-portal-action-grid">
      <section className="student-portal-card transfer-card">...</section>
      <section className="student-portal-card message-card">...</section>
    </div>
  </div>
</div>
```

Không để các card tự co theo nội dung ở giữa. Shell phải quyết định chiều rộng tổng.

### CSS Layout Desktop

Thêm hoặc sửa CSS:

```css
.student-portal-page {
  min-height: 100vh;
  padding: 40px clamp(20px, 4vw, 64px);
  background:
    radial-gradient(circle at 12% 10%, rgba(20, 184, 166, 0.12), transparent 32rem),
    linear-gradient(135deg, #f7efe6 0%, #f8fafc 52%, #eef6ff 100%);
}

.student-portal-shell {
  width: min(1500px, 100%);
  margin: 0 auto;
  display: grid;
  gap: 24px;
}

.student-portal-hero {
  width: 100%;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 20px;
  padding: 24px clamp(24px, 3vw, 40px);
  border-radius: 24px;
  background: linear-gradient(120deg, #0f766e 0%, #155e9f 58%, #2454d6 100%);
  color: #fff;
  box-shadow: 0 24px 70px rgba(15, 23, 42, 0.16);
}

.student-portal-overview-grid {
  display: grid;
  grid-template-columns: minmax(420px, 1.05fr) minmax(420px, 0.95fr);
  gap: 24px;
  align-items: stretch;
}

.student-portal-action-grid {
  display: grid;
  grid-template-columns: minmax(420px, 1fr) minmax(420px, 1fr);
  gap: 24px;
}

.student-portal-card {
  min-width: 0;
  border-radius: 24px;
  padding: clamp(22px, 2.2vw, 32px);
  background: rgba(255, 255, 255, 0.9);
  border: 1px solid rgba(148, 163, 184, 0.18);
  box-shadow: 0 18px 48px rgba(15, 23, 42, 0.08);
}

.student-portal-card.full-width,
.finance-card {
  width: 100%;
}
```

### Sửa Grid Thông Tin Cá Nhân

Hiện thông tin cá nhân nhiều dòng nhưng card bị hẹp. Cần chia field thành grid linh hoạt:

```css
.student-info-grid,
.room-info-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  column-gap: 24px;
  row-gap: 14px;
}

.info-row {
  min-width: 0;
  padding-bottom: 12px;
  border-bottom: 1px solid rgba(148, 163, 184, 0.18);
}

.info-row span {
  display: block;
  font-size: 0.76rem;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  color: #64748b;
}

.info-row strong {
  display: block;
  margin-top: 4px;
  color: #0f172a;
  overflow-wrap: anywhere;
}
```

Nếu hiện JSX đang dùng class khác, map lại tên class tương ứng, không cần đổi toàn bộ markup nếu không cần.

### Sửa Bảng Công Nợ Để Rộng Và Dễ Đọc

Bảng công nợ cần tận dụng toàn bộ chiều ngang. Bọc table bằng div scroll:

```jsx
<div className="student-table-wrap">
  <table>...</table>
</div>
```

CSS:

```css
.student-table-wrap {
  width: 100%;
  overflow-x: auto;
  border-radius: 18px;
  border: 1px solid rgba(148, 163, 184, 0.16);
}

.student-table-wrap table {
  width: 100%;
  min-width: 920px;
  border-collapse: collapse;
}

.student-table-wrap th {
  white-space: nowrap;
  background: #f8fafc;
}

.student-table-wrap td {
  vertical-align: middle;
}
```

Không ép bảng co quá nhỏ vì sẽ làm chữ bị xuống dòng xấu như ảnh.

### Responsive Tablet

Ở màn vừa, giảm khoảng cách và cho grid tự đổi 1 cột:

```css
@media (max-width: 1180px) {
  .student-portal-page {
    padding: 28px 20px;
  }

  .student-portal-overview-grid,
  .student-portal-action-grid {
    grid-template-columns: 1fr;
  }
}
```

### Responsive Mobile

Trên mobile:

```css
@media (max-width: 720px) {
  .student-portal-page {
    padding: 16px;
  }

  .student-portal-shell {
    gap: 16px;
  }

  .student-portal-hero {
    flex-direction: column;
    align-items: flex-start;
    border-radius: 20px;
  }

  .student-portal-hero-actions {
    width: 100%;
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 10px;
  }

  .student-info-grid,
  .room-info-grid {
    grid-template-columns: 1fr;
  }

  .student-portal-card {
    padding: 18px;
    border-radius: 20px;
  }
}
```

Nếu có button bị chồng hoặc quá sát nhau, thêm:

```css
.student-portal-card button,
.student-portal-hero button {
  min-height: 44px;
  white-space: normal;
}
```

### Responsive Desktop Rất Rộng

Với màn rộng trên 1600px, có thể dàn thêm khu tài chính sang layout 12 cột nếu muốn:

```css
@media (min-width: 1440px) {
  .student-portal-shell {
    width: min(1560px, calc(100vw - 96px));
  }

  .finance-card {
    padding-inline: 32px;
  }
}
```

Không nên để shell chỉ rộng khoảng `980px` hoặc `1024px` như hiện tại vì sẽ tạo cảm giác bị gom một cục ở giữa.

### Checklist Kiểm Tra Giao Diện Sinh Viên

1. Màn 1920px: container rộng hơn, không bị bó ở giữa.
2. Header sinh viên cùng chiều rộng với nội dung.
3. Card thông tin cá nhân và phòng ở cân bằng, không lệch nặng sang trái/phải.
4. Bảng công nợ rộng, ít xuống dòng, có scroll ngang nếu thiếu chỗ.
5. Hai card `Yêu cầu chuyển phòng` và `Nhắn tin với quản lý` nằm cạnh nhau trên desktop.
6. Màn 1180px: layout tự chuyển 1 cột hoặc 2 cột hợp lý, không tràn.
7. Mobile 390px: không có text/button bị đè, table có scroll ngang.
8. Font tiếng Việt hiển thị đúng, không mojibake.

## Kiểm Thử Bắt Buộc

1. Vào `Tài chính`.
2. Chọn một phòng có công nợ chưa chia.
3. Bấm chia tiền để tạo `RoomFinanceStudentShare`.
4. Ở dòng sinh viên, chọn `VNPay`.
5. Bấm tạo link.
6. Dialog phải hiển thị mã hóa đơn, không còn dấu `-`.
7. Dialog không còn câu `VNPay sandbox chặn nhúng trong iframe...`.
8. Bấm mở cổng VNPay.
9. Browser phải mở tab/popup top-level, không dùng iframe.
10. Sau khi thanh toán test thành công, hệ thống cập nhật share thành `Paid`, invoice thành `Paid`, phòng cập nhật số đã thu.

## Ghi Chú Về Lỗi `code=70`

Nếu VNPay mở ra báo `code=70 - Sai chữ ký`, lỗi không nằm ở dialog hay iframe. Đây là lỗi chữ ký VNPay.

Cần kiểm tra:

- `vnp_TmnCode`.
- `vnp_HashSecret`.
- Sort tham số trước khi ký.
- Không đưa `vnp_SecureHash` và `vnp_SecureHashType` vào chuỗi ký.
- Dùng HMAC-SHA512.
- Encode tham số giống demo PHP/C# của VNPay.
- `vnp_ReturnUrl` dùng đúng URL đã đăng ký nếu merchant sandbox yêu cầu.

Dialog/WebView chỉ quyết định cách mở trang thanh toán, không làm thay đổi chữ ký.
