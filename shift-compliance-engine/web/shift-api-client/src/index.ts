export type ShiftApiClient = {
  readonly tokenKey: string
  getToken: () => string | null
  setToken: (token: string | null) => void
  api: <T>(path: string, init?: RequestInit) => Promise<T>
}

export function createShiftApi(options: { tokenKey?: string } = {}): ShiftApiClient {
  const tokenKey = options.tokenKey ?? 'shift_jwt'

  function getToken(): string | null {
    return localStorage.getItem(tokenKey)
  }

  function setToken(t: string | null): void {
    if (t) localStorage.setItem(tokenKey, t)
    else localStorage.removeItem(tokenKey)
  }

  async function api<T>(path: string, init: RequestInit = {}): Promise<T> {
    const headers = new Headers(init.headers)
    headers.set('Accept-Language', localStorage.getItem('locale') ?? 'de')
    const t = getToken()
    if (t) headers.set('Authorization', `Bearer ${t}`)
    const res = await fetch(`/api${path}`, { ...init, headers })
    if (!res.ok) {
      const text = await res.text()
      throw new Error(text || res.statusText)
    }
    if (res.status === 204) return undefined as T
    const ct = res.headers.get('content-type')
    if (ct?.includes('application/json')) return (await res.json()) as T
    return (await res.text()) as T
  }

  return { tokenKey, getToken, setToken, api }
}
