const MIN_LOADING_MS = 200
const listeners = new Set()
let pendingRequests = 0

function notify() {
  const snapshot = { pendingRequests, active: pendingRequests > 0 }
  listeners.forEach((listener) => listener(snapshot))
}

export function subscribeLoading(listener) {
  listeners.add(listener)
  listener({ pendingRequests, active: pendingRequests > 0 })

  return () => {
    listeners.delete(listener)
  }
}

export function beginLoading() {
  pendingRequests += 1
  notify()

  const startedAt = Date.now()
  let finished = false

  return async function endLoading() {
    if (finished) return
    finished = true

    const elapsed = Date.now() - startedAt
    const waitTime = Math.max(0, MIN_LOADING_MS - elapsed)

    if (waitTime > 0) {
      await new Promise((resolve) => window.setTimeout(resolve, waitTime))
    }

    pendingRequests = Math.max(0, pendingRequests - 1)
    notify()
  }
}
