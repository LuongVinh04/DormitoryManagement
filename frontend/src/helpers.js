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

export function renderCell(value) {
  if (typeof value === 'boolean') return value ? 'Có' : 'Không'
  if (typeof value === 'number' && value > 100000) return currencyFormat.format(value)
  if (typeof value === 'string') return localizeValue(value)
  if (String(value).includes('T') && !Number.isNaN(Date.parse(value))) return shortDate(value)
  return value ?? '-'
}

export function normalizePayload(fields, values) {
  const payload = {}

  fields.forEach((field) => {
    const value = values[field.name]

    if (field.type === 'number') {
      payload[field.name] = value === '' ? 0 : Number(value)
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
