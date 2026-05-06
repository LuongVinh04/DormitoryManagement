import { ENTITY_CONFIGS } from "../constants";
import {
  formatCurrencyInput,
  formatDateDisplay,
  formatDateInput,
  isCurrencyField,
  parseDateDisplay,
  parseNumberInput,
} from "../helpers";

export function EntityForm({ modal, lookups, updateModalField, errors = {} }) {
  const fields = ENTITY_CONFIGS[modal.entityKey].fields.filter((field) => {
    if (modal.mode === "create" && field.editOnly) return false;
    if (modal.mode !== "create" && field.createOnly) return false;
    return true;
  });

  return (
    <div className="form-grid">
      {fields.map((field) => {
        const inputId = `${modal.entityKey}-${modal.mode}-${field.name}`;

        return (
          <div
            key={field.name}
            className={field.type === "textarea" ? "field field-wide" : "field"}
            onMouseDown={(event) => focusFieldControl(event)}
          >
            <label htmlFor={inputId}>{field.label}</label>
            <div className="field-control-shell">
              {renderInputField(
                field,
                modal.values[field.name],
                lookups[field.lookup] ?? [],
                updateModalField,
                inputId,
              )}
            </div>
            {errors[field.name] ? (
              <small className="field-error">{errors[field.name]}</small>
            ) : null}
          </div>
        );
      })}
    </div>
  );
}

function focusFieldControl(event) {
  if (event.target.closest("input, select, textarea, button, a")) {
    return;
  }

  const control = event.currentTarget.querySelector(
    'input:not([type="hidden"]), select, textarea',
  );
  if (control) {
    control.focus();
  }
}

function renderInputField(field, value, lookupItems, onChange, inputId) {
  if (field.type === "select") {
    return (
      <select
        id={inputId}
        value={value}
        onChange={(event) => onChange(field.name, event.target.value)}
      >
        {field.allowEmpty ? <option value="">Chọn</option> : null}
        {field.options.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
    );
  }

  if (field.type === "lookup") {
    return (
      <select
        id={inputId}
        value={value}
        onChange={(event) => onChange(field.name, event.target.value)}
      >
        {field.allowEmpty ? <option value="">Không chọn</option> : null}
        {lookupItems.map((item) => (
          <option
            key={item.id}
            value={field.optionValue ? field.optionValue(item) : item.id}
          >
            {field.optionLabel(item)}
          </option>
        ))}
      </select>
    );
  }

  if (field.type === "lookupCode") {
    return (
      <select
        id={inputId}
        value={value}
        onChange={(event) => onChange(field.name, event.target.value)}
      >
        {field.allowEmpty ? <option value="">Không chọn</option> : null}
        {lookupItems.map((item) => (
          <option key={item.id} value={item.code}>
            {field.optionLabel(item)}
          </option>
        ))}
      </select>
    );
  }

  if (field.type === "textarea") {
    return (
      <textarea
        id={inputId}
        rows={4}
        value={value}
        onChange={(event) => onChange(field.name, event.target.value)}
      />
    );
  }

  if (field.type === "date") {
    return (
      <DateInput
        field={field}
        value={value}
        onChange={onChange}
        inputId={inputId}
      />
    );
  }

  if (field.type === "number" && isCurrencyField(field)) {
    return (
      <input
        id={inputId}
        inputMode="numeric"
        value={formatCurrencyInput(value)}
        onChange={(event) =>
          onChange(field.name, parseNumberInput(event.target.value))
        }
        placeholder="0"
      />
    );
  }

  if (field.type === "number") {
    return (
      <input
        id={inputId}
        type="number"
        min="0"
        value={value}
        onChange={(event) => onChange(field.name, event.target.value)}
      />
    );
  }

  if (field.type === "checkbox") {
    return (
      <div className="checkbox-row">
        <input
          id={inputId}
          type="checkbox"
          checked={Boolean(value)}
          onChange={(event) => onChange(field.name, event.target.checked)}
        />
        <span>Kích hoạt tài khoản</span>
      </div>
    );
  }

  if (field.name.endsWith("Url") && field.type === "text") {
    return (
      <div className="evidence-url-field">
        <div className="evidence-url-row">
          <input
            id={inputId}
            type="text"
            value={value || ""}
            onChange={(event) => onChange(field.name, event.target.value)}
          />
          {value ? (
            <a href={value} target="_blank" rel="noreferrer">
              Xem ảnh ↗
            </a>
          ) : null}
        </div>
        <small>
          Dán đường dẫn lưu trữ ảnh minh chứng (Google Drive, Imgur, ...) vào
          đây.
        </small>
      </div>
    );
  }

  return (
    <input
      id={inputId}
      type={field.type}
      value={value}
      onChange={(event) => onChange(field.name, event.target.value)}
    />
  );
}

function DateInput({ field, value, onChange, inputId }) {
  return (
    <div className="date-input-field">
      <input
        id={inputId}
        className="date-display-input"
        type="text"
        inputMode="numeric"
        value={formatDateDisplay(value)}
        onChange={(event) =>
          onChange(field.name, parseDateDisplay(event.target.value))
        }
        placeholder="dd/mm/yyyy"
      />
      <span className="date-picker-icon" aria-hidden="true">
        <svg viewBox="0 0 24 24" focusable="false">
          <path d="M7 2v3M17 2v3M4 9h16M6 5h12a2 2 0 0 1 2 2v11a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V7a2 2 0 0 1 2-2Z" />
        </svg>
      </span>
      <input
        className="date-picker-native"
        type="date"
        value={formatDateInput(value)}
        onChange={(event) => onChange(field.name, event.target.value)}
        aria-label="Chọn ngày"
      />
    </div>
  );
}
