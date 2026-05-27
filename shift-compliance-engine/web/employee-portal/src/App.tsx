import { useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { createShiftApi } from '@shift-engine/api-client'
import './App.css'

type LoginRes = { token: string; email: string; tenantId: string }

type MyAssignment = { workDate: string; tier: string; code: string }

export default function App() {
  const { api, getToken, setToken } = useMemo(() => createShiftApi(), [])
  const { t, i18n } = useTranslation()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [err, setErr] = useState<string | null>(null)
  const [loggedIn, setLoggedIn] = useState(!!getToken())
  const [year, setYear] = useState(new Date().getFullYear())
  const [month, setMonth] = useState(new Date().getMonth() + 1)
  const [rows, setRows] = useState<MyAssignment[]>([])
  const [hint, setHint] = useState<string | null>(null)

  async function login(e: React.FormEvent) {
    e.preventDefault()
    setErr(null)
    try {
      const r = await api<LoginRes>('/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password }),
      })
      setToken(r.token)
      setLoggedIn(true)
    } catch (ex) {
      setErr(ex instanceof Error ? ex.message : 'Login failed')
    }
  }

  async function loadRoster() {
    setErr(null)
    setHint(null)
    try {
      const r = await api<{ assignments: MyAssignment[]; message?: string }>(
        `/employee/roster?year=${year}&month=${month}`,
      )
      setRows(r.assignments ?? [])
      setHint(r.message ?? null)
    } catch (ex) {
      setErr(ex instanceof Error ? ex.message : 'Load failed')
    }
  }

  function logout() {
    setToken(null)
    setLoggedIn(false)
    setRows([])
    setHint(null)
  }

  return (
    <div className="layout">
      <header>
        <h1>{t('appTitle')}</h1>
        <label>
          {t('locale')}{' '}
          <select
            value={i18n.language}
            onChange={(e) => {
              const lng = e.target.value
              localStorage.setItem('locale', lng)
              void i18n.changeLanguage(lng)
            }}
          >
            <option value="de">DE</option>
            <option value="en">EN</option>
          </select>
        </label>
      </header>
      {err && <p className="error">{err}</p>}
      {!loggedIn ? (
        <form onSubmit={login} className="card">
          <h2>{t('login')}</h2>
          <label>
            {t('email')}
            <input value={email} onChange={(e) => setEmail(e.target.value)} type="email" required />
          </label>
          <label>
            {t('password')}
            <input value={password} onChange={(e) => setPassword(e.target.value)} type="password" required />
          </label>
          <button type="submit">{t('login')}</button>
        </form>
      ) : (
        <section className="card">
          <button type="button" onClick={logout}>
            {t('logout')}
          </button>
          <h2>{t('roster')}</h2>
          <div className="row">
            <label>
              {t('year')}
              <input type="number" value={year} onChange={(e) => setYear(+e.target.value)} />
            </label>
            <label>
              {t('month')}
              <input type="number" min={1} max={12} value={month} onChange={(e) => setMonth(+e.target.value)} />
            </label>
            <button type="button" onClick={() => void loadRoster()}>
              {t('load')}
            </button>
          </div>
          {hint && <p className="hint">{hint}</p>}
          <div className="table-wrap">
            <table className="roster-table">
              <thead>
                <tr>
                  <th>{t('colDate')}</th>
                  <th>{t('colTier')}</th>
                  <th>{t('colCode')}</th>
                </tr>
              </thead>
              <tbody>
                {[...rows]
                  .sort((a, b) => a.workDate.localeCompare(b.workDate))
                  .map((a) => (
                    <tr key={`${a.workDate}-${a.code}`}>
                      <td>{a.workDate}</td>
                      <td>{a.tier}</td>
                      <td>{a.code}</td>
                    </tr>
                  ))}
              </tbody>
            </table>
          </div>
        </section>
      )}
    </div>
  )
}
