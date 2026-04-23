export function StatusToast({ error, notice, saving }) {
  const message = error || notice || (saving ? 'Đang xử lý yêu cầu...' : '')

  if (!message) return null

  return (
    <div className={`status-toast ${error ? 'error' : 'success'}`} role="status" aria-live="polite">
      <span>{error ? 'Lỗi' : saving ? 'Đang xử lý' : 'Thành công'}</span>
      <strong>{message}</strong>
    </div>
  )
}
