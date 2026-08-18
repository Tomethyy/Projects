import { useCallback, useEffect, useMemo, useState } from 'react'

import { useTranslation } from 'react-i18next'

import { createShiftApi } from '@shift-engine/api-client'

import heroImg from './assets/hero.png'

import MonthCalendar from './MonthCalendar'



type LoginRes = { token: string; email: string; tenantId: string }

type MyAssignment = { workDate: string; tier: string; code: string }

type RosterRes = { assignments: MyAssignment[]; message?: string | null; draftExists?: boolean }



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

  const [draftExists, setDraftExists] = useState(false)

  const [selectedDate, setSelectedDate] = useState<string | null>(null)



  const shiftDates = useMemo(() => new Set(rows.map((r) => r.workDate)), [rows])



  const sortedRows = useMemo(

    () => [...rows].sort((a, b) => a.workDate.localeCompare(b.workDate)),

    [rows],

  )



  const visibleRows = useMemo(

    () => (selectedDate ? sortedRows.filter((r) => r.workDate === selectedDate) : sortedRows),

    [selectedDate, sortedRows],

  )



  const loadRoster = useCallback(async () => {

    setErr(null)

    setHint(null)

    try {

      const r = await api<RosterRes>(`/employee/roster?year=${year}&month=${month}`)

      setRows(r.assignments ?? [])

      setDraftExists(!!r.draftExists)

      setHint(r.message ?? (r.draftExists ? t('draftNotPublished') : null))

      setSelectedDate(null)

    } catch (ex) {

      setErr(ex instanceof Error ? ex.message : 'Load failed')

    }

  }, [api, month, t, year])



  useEffect(() => {

    if (loggedIn) void loadRoster()

  }, [loggedIn, loadRoster])



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



  function logout() {

    setToken(null)

    setLoggedIn(false)

    setRows([])

    setHint(null)

    setSelectedDate(null)

  }



  if (!loggedIn) {

    return (

      <div className="portal-login">

        <div className="portal-login-hero">

          <img src={heroImg} alt="" className="portal-hero-img" />

        </div>

        <form className="portal-card portal-login-card" onSubmit={login}>

          <h1>{t('appTitle')}</h1>

          <p>{t('loginSubtitle')}</p>

          {err && <div className="portal-alert">{err}</div>}

          <label>

            {t('email')}

            <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />

          </label>

          <label>

            {t('password')}

            <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required />

          </label>

          <button type="submit">{t('login')}</button>

        </form>

      </div>

    )

  }



  const showDraftCard = draftExists && rows.length === 0

  const showEmptyCard = !draftExists && rows.length === 0



  return (

    <div className="portal-app">

      <header className="portal-header">

        <h1>{t('appTitle')}</h1>

        <div className="portal-header-actions">

          <select

            value={i18n.language}

            onChange={(e) => {

              localStorage.setItem('locale', e.target.value)

              void i18n.changeLanguage(e.target.value)

            }}

            aria-label={t('locale')}

          >

            <option value="de">DE</option>

            <option value="en">EN</option>

          </select>

          <button type="button" onClick={logout}>

            {t('logout')}

          </button>

        </div>

      </header>

      <main className="portal-main">

        {err && <div className="portal-alert">{err}</div>}

        <section className="portal-card">

          <h2>{t('roster')}</h2>

          <p className="portal-muted">{t('publishedOnlyHint')}</p>

          <div className="portal-toolbar">

            <label>

              {t('year')}

              <input type="number" value={year} onChange={(e) => setYear(+e.target.value)} />

            </label>

            <label>

              {t('month')}

              <input type="number" min={1} max={12} value={month} onChange={(e) => setMonth(+e.target.value)} />

            </label>

            <button type="button" onClick={() => void loadRoster()}>

              {t('refresh')}

            </button>

          </div>

          <MonthCalendar

            year={year}

            month={month}

            shiftDates={shiftDates}

            selectedDate={selectedDate}

            onSelect={(date) => setSelectedDate((prev) => (prev === date ? null : date))}

          />

          {showDraftCard && (

            <div className="portal-empty-card portal-empty-draft" role="status">

              <h3>{t('emptyDraftTitle')}</h3>

              <p>{t('emptyDraftBody')}</p>

            </div>

          )}

          {showEmptyCard && (

            <div className="portal-empty-card" role="status">

              <h3>{t('emptyPublishedTitle')}</h3>

              <p>{t('noShifts')}</p>

            </div>

          )}

          {hint && !showDraftCard && <p className={draftExists ? 'portal-warn' : 'portal-muted'}>{hint}</p>}

          {visibleRows.length > 0 && (

            <>

              <h3 className="portal-shift-heading">

                {selectedDate ? t('shiftOnDay', { date: selectedDate }) : t('shiftList')}

              </h3>

              <ul className="portal-shift-list">

                {visibleRows.map((a) => (

                  <li key={`${a.workDate}-${a.code}`} className="portal-shift-card">

                    <span className="portal-shift-date">{a.workDate}</span>

                    <span className="portal-shift-tier">{a.tier}</span>

                    <span className="portal-shift-code">{a.code}</span>

                  </li>

                ))}

              </ul>

              <div className="portal-table-wrap portal-table-desktop">

                <table>

                  <thead>

                    <tr>

                      <th>{t('colDate')}</th>

                      <th>{t('colTier')}</th>

                      <th>{t('colCode')}</th>

                    </tr>

                  </thead>

                  <tbody>

                    {visibleRows.map((a) => (

                      <tr key={`tbl-${a.workDate}-${a.code}`}>

                        <td>{a.workDate}</td>

                        <td>{a.tier}</td>

                        <td>{a.code}</td>

                      </tr>

                    ))}

                  </tbody>

                </table>

              </div>

            </>

          )}

        </section>

      </main>

    </div>

  )

}

