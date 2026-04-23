import { VALUE_LABELS } from './constants'

export const numberFormat = new Intl.NumberFormat('vi-VN')
export const currencyFormat = new Intl.NumberFormat('vi-VN', {
  style: 'currency',
  currency: 'VND',
  maximumFractionDigits: 0,
})

export function shortDate(value) {
  return new Intl.DateTimeFormat('vi-VN').format(new Date(value))
}

export function formatDateInput(value) {
  return new Date(value).toISOString().split('T')[0]
}

export function localizeValue(value) {
  return VALUE_LABELS[value] ?? value
}

export function repairText(value) {
  if (typeof value !== 'string' || value.length === 0) {
    return value
  }

  try {
    return decodeURIComponent(escape(value))
  } catch {
    return value
  }
}

export function renderCell(value) {
  if (typeof value === 'boolean') return value ? 'Có' : 'Không'
  if (typeof value === 'number' && value > 100000) return currencyFormat.format(value)
  if (typeof value === 'string') return localizeValue(value)
  if (String(value).includes('T') && !Number.isNaN(Date.parse(value))) return shortDate(value)
  return value ?? '-'
}

export function isCurrencyField(field) {
  const fieldName = field.name.toLowerCase()
  return ['amount', 'fee', 'price', 'total', 'deposit'].some((keyword) => fieldName.includes(keyword))
}

export function parseNumberInput(value) {
  if (typeof value === 'number') return value
  const normalized = String(value ?? '').replace(/[^\d.-]/g, '')
  return normalized === '' || normalized === '-' ? '' : Number(normalized)
}

export function formatCurrencyInput(value) {
  const parsedValue = parseNumberInput(value)
  return parsedValue === '' || Number.isNaN(parsedValue) ? '' : numberFormat.format(parsedValue)
}

export function validatePayload(fields, values) {
  const errors = {}

  fields.forEach((field) => {
    const value = values[field.name]
    const isEmpty = value === '' || value === null || value === undefined
    const requiredTextFields = ['code', 'name', 'studentCode', 'contractCode', 'invoiceCode', 'username', 'passwordHash', 'fullName', 'email']
    const isRequiredField =
      field.required === true ||
      field.type === 'select' ||
      field.type === 'lookup' ||
      field.type === 'lookupCode' ||
      field.type === 'date' ||
      (field.type === 'number' && !field.allowEmpty) ||
      requiredTextFields.includes(field.name)

    if (!field.allowEmpty && field.type !== 'checkbox' && isRequiredField && isEmpty) {
      errors[field.name] = `${field.label} là bắt buộc.`
      return
    }

    if (field.type === 'email' && !isEmpty && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)) {
      errors[field.name] = 'Email không đúng định dạng.'
      return
    }

    if (field.type === 'number' && !isEmpty) {
      const numberValue = parseNumberInput(value)
      if (Number.isNaN(numberValue)) {
        errors[field.name] = `${field.label} phải là số.`
        return
      }

      if (numberValue < 0) {
        errors[field.name] = `${field.label} không được âm.`
      }
    }
  })

  return errors
}

export function normalizePayload(fields, values) {
  const payload = {}

  fields.forEach((field) => {
    const value = values[field.name]

    if (field.type === 'number') {
      payload[field.name] = value === '' ? 0 : Number(parseNumberInput(value))
      return
    }

    if (field.type === 'lookup') {
      payload[field.name] = value === '' ? null : Number(value)
      return
    }

    if (field.type === 'checkbox') {
      payload[field.name] = Boolean(value)
      return
    }

    if (field.type === 'date' && field.allowEmpty && value === '') {
      payload[field.name] = null
      return
    }

    payload[field.name] = value
  })

  return payload
}

export async function readError(response) {
  try {
    const contentType = response.headers.get('content-type') ?? ''
    if (contentType.includes('application/json')) {
      const data = await response.json()
      return data.message ?? 'Đã có lỗi xảy ra.'
    }

    return await response.text()
  } catch {
    return 'Đã có lỗi xảy ra.'
  }
}
