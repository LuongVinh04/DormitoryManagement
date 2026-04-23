import { ENTITY_CONFIGS } from '../constants'
import { formatCurrencyInput, isCurrencyField, parseNumberInput } from '../helpers'

export function EntityForm({ modal, lookups, updateModalField, errors = {} }) {
  return (
    <div className="form-grid">
      {ENTITY_CONFIGS[modal.entityKey].fields.map((field) => (
        <label key={field.name} className={field.type === 'textarea' ? 'field field-wide' : 'field'}>
          <span>{field.label}</span>
          {renderInputField(field, modal.values[field.name], lookups[field.lookup] ?? [], updateModalField)}
          {errors[field.name] ? <small className="field-error">{errors[field.name]}</small> : null}
        </label>
      ))}
    </div>
  )
}

function renderInputField(field, value, lookupItems, onChange) {
  if (field.type === 'select') {
    return (
      <select value={value} onChange={(event) => onChange(field.name, event.target.value)}>
        {field.allowEmpty ? <option value="">Chọn</option> : null}
        {field.options.map((option) => <option key={option.value} value={option.value}>{option.label}</option>)}
      </select>
    )
  }

  if (field.type === 'lookup') {
    return (
      <select value={value} onChange={(event) => onChange(field.name, event.target.value)}>
        {field.allowEmpty ? <option value="">Không chọn</option> : null}
        {lookupItems.map((item) => <option key={item.id} value={item.id}>{field.optionLabel(item)}</option>)}
      </select>
    )
  }

  if (field.type === 'lookupCode') {
    return (
      <select value={value} onChange={(event) => onChange(field.name, event.target.value)}>
        {field.allowEmpty ? <option value="">Không chọn</option> : null}
        {lookupItems.map((item) => <option key={item.id} value={item.code}>{field.optionLabel(item)}</option>)}
      </select>
    )
  }

  if (field.type === 'textarea') {
    return <textarea rows={4} value={value} onChange={(event) => onChange(field.name, event.target.value)} />
  }

  if (field.type === 'number' && isCurrencyField(field)) {
    return (
      <input
        inputMode="numeric"
        value={formatCurrencyInput(value)}
        onChange={(event) => onChange(field.name, parseNumberInput(event.target.value))}
        placeholder="0"
      />
    )
  }

  if (field.type === 'number') {
    return <input type="number" min="0" value={value} onChange={(event) => onChange(field.name, event.target.value)} />
  }

  if (field.type === 'checkbox') {
    return <div className="checkbox-row"><input type="checkbox" checked={Boolean(value)} onChange={(event) => onChange(field.name, event.target.checked)} /><span>Kích hoạt tài khoản</span></div>
  }

  return <input type={field.type} value={value} onChange={(event) => onChange(field.name, event.target.value)} />
}
