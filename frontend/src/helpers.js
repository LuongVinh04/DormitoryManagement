import { VALUE_LABELS } from "./constants";
import { beginLoading } from "./loadingBus";

export const numberFormat = new Intl.NumberFormat("vi-VN");
export const currencyFormat = new Intl.NumberFormat("vi-VN", {
  style: "currency",
  currency: "VND",
  maximumFractionDigits: 0,
});

export function shortDate(value) {
  return new Intl.DateTimeFormat("vi-VN", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  }).format(new Date(value));
}

export function formatDateInput(value) {
  if (!value) return "";

  const dateValue = String(value).split("T")[0];
  if (/^\d{4}-\d{2}-\d{2}$/.test(dateValue)) return dateValue;

  const parsedValue = new Date(value);
  return Number.isNaN(parsedValue.getTime())
    ? ""
    : parsedValue.toISOString().split("T")[0];
}

export function formatDateDisplay(value) {
  if (!value) return "";
  const [year, month, day] = String(value).split("T")[0].split("-");
  if (!year || !month || !day) return String(value);
  return `${day}/${month}/${year}`;
}

export function parseDateDisplay(value) {
  const rawValue = String(value ?? "").trim();
  if (rawValue === "") return "";

  const match = rawValue.match(/^(\d{1,2})\/(\d{1,2})\/(\d{4})$/);
  if (!match) return rawValue;

  const [, day, month, year] = match;
  return `${year}-${month.padStart(2, "0")}-${day.padStart(2, "0")}`;
}

export function localizeValue(value) {
  return VALUE_LABELS[value] ?? value;
}

export function repairText(value) {
  if (typeof value !== "string" || value.length === 0) {
    return value;
  }

  try {
    return decodeURIComponent(escape(value));
  } catch {
    return value;
  }
}

export function renderCell(value) {
  if (typeof value === "boolean") return value ? "Có" : "Không";
  if (typeof value === "number" && value > 100000)
    return currencyFormat.format(value);
  if (
    typeof value === "string" &&
    /^\d{4}-\d{2}-\d{2}(T.*)?$/.test(value) &&
    !Number.isNaN(Date.parse(value))
  ) {
    return shortDate(value);
  }
  if (typeof value === "string") {
    return localizeValue(value);
  }
  return value ?? "-";
}

export function isCurrencyField(field) {
  const fieldName = field.name.toLowerCase();
  return ["amount", "fee", "price", "total", "deposit"].some((keyword) =>
    fieldName.includes(keyword),
  );
}

export function parseNumberInput(value) {
  if (typeof value === "number") return value;
  const normalized = String(value ?? "").replace(/[^\d]/g, "");
  return normalized === "" ? "" : Number(normalized);
}

export function formatCurrencyInput(value) {
  const parsedValue = parseNumberInput(value);
  return parsedValue === "" || Number.isNaN(parsedValue)
    ? ""
    : numberFormat.format(parsedValue);
}

export function validatePayload(fields, values) {
  const errors = {};

  fields.forEach((field) => {
    const value = values[field.name];
    const isEmpty = value === "" || value === null || value === undefined;
    const requiredTextFields = [
      "code",
      "name",
      "studentCode",
      "contractCode",
      "invoiceCode",
      "username",
      "passwordHash",
      "fullName",
      "email",
    ];
    const isRequiredField =
      field.required === true ||
      field.type === "select" ||
      field.type === "lookup" ||
      field.type === "lookupCode" ||
      field.type === "date" ||
      (field.type === "number" && !field.allowEmpty) ||
      requiredTextFields.includes(field.name);

    if (
      !field.allowEmpty &&
      field.type !== "checkbox" &&
      isRequiredField &&
      isEmpty
    ) {
      errors[field.name] = `${field.label} là bắt buộc.`;
      return;
    }

    if (
      field.type === "email" &&
      !isEmpty &&
      !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)
    ) {
      errors[field.name] = "Email không đúng định dạng.";
      return;
    }

    if (field.type === "number" && !isEmpty) {
      const numberValue = parseNumberInput(value);
      if (Number.isNaN(numberValue)) {
        errors[field.name] = `${field.label} phải là số.`;
        return;
      }

      if (numberValue < 0) {
        errors[field.name] = `${field.label} không được âm.`;
      }
    }
  });

  return errors;
}

export function normalizePayload(fields, values) {
  const payload = {};

  fields.forEach((field) => {
    const value = values[field.name];

    if (field.type === "number") {
      payload[field.name] = value === "" ? 0 : Number(parseNumberInput(value));
      return;
    }

    if (field.type === "lookup") {
      payload[field.name] =
        value === "" ? null : field.optionValue ? value : Number(value);
      return;
    }

    if (field.type === "checkbox") {
      payload[field.name] = Boolean(value);
      return;
    }

    if (field.type === "date" && field.allowEmpty && value === "") {
      payload[field.name] = null;
      return;
    }

    payload[field.name] = value;
  });

  return payload;
}

export async function readError(response) {
  try {
    const contentType = response.headers.get("content-type") ?? "";
    const rawMessage = contentType.includes("application/json")
      ? (await response.json()).message
      : await response.text();

    const message = String(rawMessage ?? "");
    if (
      // response.status === 409 &&
      /contractCode|contract code|mã hợp đồng|duplicate|unique|trùng/i.test(
        message,
      )
    ) {
      return "Mã hợp đồng đã tồn tại. Vui lòng sử dụng mã khác.";
    }
    return message || "Đã có lỗi xảy ra.";
  } catch {
    return "Đã có lỗi xảy ra.";
  }
}

export async function apiFetch(url, options = {}) {
  const { skipGlobalLoading = false, ...fetchOptions } = options;
  const endLoading = skipGlobalLoading ? async () => {} : beginLoading();
  const token = localStorage.getItem("dormitory_token");
  const headers = { ...fetchOptions.headers };
  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  try {
    const response = await fetch(url, { ...fetchOptions, headers });
    if (response.status === 401 || response.status === 403) {
      if (window.location.pathname !== "/login") {
        window.location.href = "/login";
      }
    }

    return response;
  } finally {
    await endLoading();
  }
}
