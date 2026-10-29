export const DOMAIN_ENUMS = {
  genderPolicy: {
    Male: "Khu nam",
    Female: "Khu n\u1eef",
    Mixed: "Khu h\u1ed7n h\u1ee3p",
  },
  roomType: {
    Standard: "Ti\u00eau chu\u1ea9n",
    Premium: "Cao c\u1ea5p",
    VIP: "VIP",
  },
  roomStatus: {
    Available: "C\u00f2n tr\u1ed1ng",
    Occupied: "\u0110ang s\u1eed d\u1ee5ng",
    Full: "\u0110\u00e3 \u0111\u1ea7y",
    Maintenance: "B\u1ea3o tr\u00ec",
  },
  gender: {
    Male: "Nam",
    Female: "N\u1eef",
    Other: "Kh\u00e1c",
  },
  studentStatus: {
    Active: "\u0110ang \u1edf",
    PendingMoveIn: "Ch\u1edd nh\u1eadn ph\u00f2ng",
    Waiting: "Ch\u1edd x\u1ebfp ph\u00f2ng",
    Pending: "Ch\u1edd duy\u1ec7t",
    Inactive: "Ng\u1eebng \u1edf",
  },
  registrationStatus: {
    Pending: "Ch\u1edd duy\u1ec7t",
    Approved: "\u0110\u00e3 duy\u1ec7t",
    Rejected: "T\u1eeb ch\u1ed1i",
  },
  contractStatus: {
    Active: "Hi\u1ec7u l\u1ef1c",
    Expired: "H\u1ebft h\u1ea1n",
    Cancelled: "\u0110\u00e3 h\u1ee7y",
  },
  invoiceStatus: {
    Unpaid: "Ch\u01b0a thanh to\u00e1n",
    Paid: "\u0110\u00e3 thanh to\u00e1n",
    PartiallyPaid: "Thanh to\u00e1n m\u1ed9t ph\u1ea7n",
    Late: "Qu\u00e1 h\u1ea1n",
  },
  paymentMethod: {
    Cash: "Ti\u1ec1n m\u1eb7t",
    Transfer: "Chuy\u1ec3n kho\u1ea3n",
    Other: "Kh\u00e1c",
  },
  violationType: {
    Noise: "G\u00e2y \u1ed3n",
    Smoking: "H\u00fat thu\u1ed1c",
    PropertyDamage: "Ph\u00e1 ho\u1ea1i t\u00e0i s\u1ea3n",
  },
  violationStatus: {
    Pending: "Ch\u1edd x\u00e1c nh\u1eadn",
    Confirmed: "\u0110\u00e3 x\u00e1c nh\u1eadn",
    Cancelled: "\u0110\u00e3 h\u1ee7y",
    Paid: "\u0110\u00e3 n\u1ed9p ph\u1ea1t",
  },
  equipmentType: {
    Fan: "Qu\u1ea1t",
    AirConditioner: "\u0110i\u1ec1u h\u00f2a",
    Light: "B\u00f3ng \u0111\u00e8n",
    Other: "Kh\u00e1c",
  },
  maintenanceStatus: {
    Submitted: "Sinh vi\u00ean b\u00e1o l\u1ed7i",
    Approved: "Admin \u0111\u00e3 duy\u1ec7t",
    Rejected: "T\u1eeb ch\u1ed1i",
    InProgress: "K\u1ef9 thu\u1eadt \u0111ang x\u1eed l\u00fd",
    Done: "Ho\u00e0n t\u1ea5t",
  },
}

export function buildEnumOptions(enumName) {
  return Object.entries(DOMAIN_ENUMS[enumName]).map(([value, label]) => ({ value, label }))
}

export function buildValueLabels() {
  return Object.values(DOMAIN_ENUMS).reduce((labels, group) => ({ ...labels, ...group }), {})
}

