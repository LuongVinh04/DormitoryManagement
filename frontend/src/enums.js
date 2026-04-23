export const DOMAIN_ENUMS = {
  genderPolicy: {
    Male: 'Khu nam',
    Female: 'Khu nữ',
    Mixed: 'Khu hỗn hợp',
  },
  roomType: {
    Standard: 'Tiêu chuẩn',
    Premium: 'Cao cấp',
    VIP: 'VIP',
  },
  roomStatus: {
    Available: 'Còn trống',
    Occupied: 'Đang sử dụng',
    Full: 'Đã đầy',
    Maintenance: 'Bảo trì',
  },
  gender: {
    Male: 'Nam',
    Female: 'Nữ',
    Other: 'Khác',
  },
  studentStatus: {
    Active: 'Đang ở',
    PendingMoveIn: 'Chờ nhận phòng',
    Waiting: 'Chờ xếp phòng',
    Pending: 'Chờ duyệt',
    Inactive: 'Ngừng ở',
  },
  registrationStatus: {
    Pending: 'Chờ duyệt',
    Approved: 'Đã duyệt',
    Rejected: 'Từ chối',
  },
  contractStatus: {
    Active: 'Hiệu lực',
    Expired: 'Hết hạn',
    Cancelled: 'Đã hủy',
  },
  invoiceStatus: {
    Unpaid: 'Chưa thanh toán',
    Paid: 'Đã thanh toán',
    PartiallyPaid: 'Thanh toán một phần',
    Late: 'Quá hạn',
  },
  paymentMethod: {
    Cash: 'Tiền mặt',
    Transfer: 'Chuyển khoản',
    Other: 'Khác',
  },
}

export function buildEnumOptions(enumName) {
  return Object.entries(DOMAIN_ENUMS[enumName]).map(([value, label]) => ({ value, label }))
}

export function buildValueLabels() {
  return Object.values(DOMAIN_ENUMS).reduce((labels, group) => ({ ...labels, ...group }), {})
}
