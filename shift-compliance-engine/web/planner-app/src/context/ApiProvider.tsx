import { createContext, useCallback, useContext, useMemo, useState, type ReactNode } from 'react'
import { createShiftApi, type ShiftApiClient } from '@shift-engine/api-client'

export type SessionUser = { email: string; tenantId: string }

type ApiContextValue = ShiftApiClient & {
  user: SessionUser | null
  login: (token: string, user: SessionUser) => void
  logout: () => void
}

const SESSION_KEY = 'shift_planner_session'

function readSession(): SessionUser | null {
  try {
    const raw = sessionStorage.getItem(SESSION_KEY)
    return raw ? (JSON.parse(raw) as SessionUser) : null
  } catch {
    return null
  }
}

const ApiContext = createContext<ApiContextValue | null>(null)

export function ApiProvider({ children }: { children: ReactNode }) {
  const client = useMemo(() => createShiftApi(), [])
  const [user, setUser] = useState<SessionUser | null>(() => (client.getToken() ? readSession() : null))

  const login = useCallback(
    (token: string, session: SessionUser) => {
      client.setToken(token)
      sessionStorage.setItem(SESSION_KEY, JSON.stringify(session))
      setUser(session)
    },
    [client],
  )

  const logout = useCallback(() => {
    client.setToken(null)
    sessionStorage.removeItem(SESSION_KEY)
    setUser(null)
  }, [client])

  const value = useMemo(() => ({ ...client, user, login, logout }), [client, user, login, logout])

  return <ApiContext.Provider value={value}>{children}</ApiContext.Provider>
}

export function useApi() {
  const ctx = useContext(ApiContext)
  if (!ctx) throw new Error('useApi outside ApiProvider')
  return ctx
}

export function useAuth() {
  const { getToken, user, login, logout } = useApi()
  return {
    isLoggedIn: !!getToken(),
    user,
    login,
    logout,
    getToken,
  }
}
