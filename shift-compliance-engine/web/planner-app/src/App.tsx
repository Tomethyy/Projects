import { useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { createShiftApi } from '@shift-engine/api-client'
import './App.css'

type LoginRes = { token: string; email: string; tenantId: string }

type ShiftTier = { id: string; code: string; displayName: string }
type Assignment = {
  id: string
  workDate: string
  shiftTierId: string
  employee: { displayName: string }
  shiftTier: ShiftTier
}

export default function App() {
  const { api, getToken, setToken } = useMemo(() => createShiftApi(), [])
  const { t, i18n } = useTranslation()
  const [loggedIn, setLoggedIn] = useState(!!getToken())
  const [err, setErr] = useState<string | null>(null)
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [slug, setSlug] = useState('demo')
  const [tenantName, setTenantName] = useState('Demo GmbH')
  const [adminName, setAdminName] = useState('Admin')
  const [smtpHost, setSmtpHost] = useState('')
  const [smtpPort, setSmtpPort] = useState('')
  const [smtpUser, setSmtpUser] = useState('')
  const [smtpPass, setSmtpPass] = useState('')
  const [smtpFrom, setSmtpFrom] = useState('')
  const [aiKey, setAiKey] = useState('')
  const [aiPlaceholder, setAiPlaceholder] = useState(true)
  const [inviteCsv, setInviteCsv] = useState(
    'PersonnelNumber;DisplayName;Email\n1001;Max Mustermann;max@example.com\n1002;Erika Beispiel;',
  )
  const [wizardResult, setWizardResult] = useState<string>('')
  const [empId, setEmpId] = useState('')
  const [year, setYear] = useState(2026)
  const [pattern, setPattern] = useState(0)
  const [anchor, setAnchor] = useState('2026-01-01')
  const [ledgerId, setLedgerId] = useState('')
  const [proposal, setProposal] = useState<string>('')
  const [periodId, setPeriodId] = useState('')
  const [legacySource, setLegacySource] = useState('')
  const [tiers, setTiers] = useState<ShiftTier[]>([])
  const [assignments, setAssignments] = useState<Assignment[]>([])

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

  async function wizard(e: React.FormEvent) {
    e.preventDefault()
    setErr(null)
    try {
      const port = smtpPort.trim() === '' ? null : Number(smtpPort)
      const body = {
        tenantSlug: slug,
        tenantDisplayName: tenantName,
        defaultLocale: 'de-DE',
        adminEmail: email,
        adminPassword: password,
        adminDisplayName: adminName,
        enableAiKeyPlaceholder: aiPlaceholder,
        smtpHost: smtpHost.trim() || null,
        smtpPort: Number.isFinite(port) ? port : null,
        smtpUsername: smtpUser.trim() || null,
        smtpPassword: smtpPass || null,
        smtpFromEmail: smtpFrom.trim() || null,
        aiApiKey: aiKey.trim() || null,
        employeeInviteCsv: inviteCsv.trim() || null,
      }
      const r = await api<unknown>('/setup/wizard', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body),
      })
      setWizardResult(JSON.stringify(r, null, 2))
    } catch (ex) {
      setErr(ex instanceof Error ? ex.message : 'Wizard failed')
    }
  }

  async function genYear(e: React.FormEvent) {
    e.preventDefault()
    setErr(null)
    try {
      const gen = await api<{ id: string }>('/roster/generate-year', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          employeeId: empId,
          year,
          pattern,
          anchorFirstWorkDay: anchor,
          shiftTierId: null,
          legacySource: legacySource.trim() || null,
        }),
      })
      setPeriodId(gen.id)
      alert('Year roster generated')
    } catch (ex) {
      setErr(ex instanceof Error ? ex.message : 'Generate failed')
    }
  }

  async function sick(e: React.FormEvent) {
    e.preventDefault()
    setErr(null)
    try {
      const r = await api<{ id: string }>('/ledger/sick-or-callout', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          shiftAssignmentId: null,
          employeeId: empId,
          date: anchor,
          kind: 0,
          notes: 'sick',
        }),
      })
      setLedgerId(r.id)
    } catch (ex) {
      setErr(ex instanceof Error ? ex.message : 'Ledger failed')
    }
  }

  async function propose() {
    setErr(null)
    try {
      const p = await api<unknown>(`/replan/propose/${ledgerId}`, { method: 'POST' })
      setProposal(JSON.stringify(p, null, 2))
    } catch (ex) {
      setErr(ex instanceof Error ? ex.message : 'Propose failed')
    }
  }

  async function loadRosterGrid() {
    setErr(null)
    try {
      const [t, a] = await Promise.all([
        api<ShiftTier[]>('/roster/shift-tiers'),
        api<Assignment[]>(`/roster/assignments?periodId=${encodeURIComponent(periodId)}`),
      ])
      setTiers(t)
      setAssignments([...a].sort((x, y) => x.workDate.localeCompare(y.workDate)))
    } catch (ex) {
      setErr(ex instanceof Error ? ex.message : 'Roster load failed')
    }
  }

  async function patchTier(assignmentId: string, shiftTierId: string) {
    setErr(null)
    try {
      await api(`/roster/assignments/${assignmentId}/tier`, {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ shiftTierId }),
      })
      await loadRosterGrid()
    } catch (ex) {
      setErr(ex instanceof Error ? ex.message : 'Update failed')
    }
  }

  async function publishRoster() {
    setErr(null)
    try {
      await api('/roster/publish', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ rosterPeriodId: periodId }),
      })
      alert('Published')
    } catch (ex) {
      setErr(ex instanceof Error ? ex.message : 'Publish failed')
    }
  }

  return (
    <div className="layout">
      <header>
        <h1>{t('appTitle')}</h1>
        <select
          value={i18n.language}
          onChange={(e) => {
            localStorage.setItem('locale', e.target.value)
            void i18n.changeLanguage(e.target.value)
          }}
        >
          <option value="de">DE</option>
          <option value="en">EN</option>
        </select>
      </header>
      {err && <p className="error">{err}</p>}
      {!loggedIn ? (
        <>
          <form className="card" onSubmit={wizard}>
            <h2>{t('wizard')}</h2>
            <p>{t('wizardDbHint')}</p>
            <label>
              {t('tenantSlug')}
              <input value={slug} onChange={(e) => setSlug(e.target.value)} />
            </label>
            <label>
              {t('tenantName')}
              <input value={tenantName} onChange={(e) => setTenantName(e.target.value)} />
            </label>
            <label>
              {t('email')}
              <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} />
            </label>
            <label>
              {t('password')}
              <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} />
            </label>
            <label>
              {t('adminName')}
              <input value={adminName} onChange={(e) => setAdminName(e.target.value)} />
            </label>
            <fieldset>
              <legend>{t('smtp')}</legend>
              <label>
                SMTP host
                <input value={smtpHost} onChange={(e) => setSmtpHost(e.target.value)} placeholder="smtp.example.com" />
              </label>
              <label>
                SMTP port
                <input value={smtpPort} onChange={(e) => setSmtpPort(e.target.value)} placeholder="587" />
              </label>
              <label>
                SMTP user
                <input value={smtpUser} onChange={(e) => setSmtpUser(e.target.value)} />
              </label>
              <label>
                SMTP password
                <input type="password" value={smtpPass} onChange={(e) => setSmtpPass(e.target.value)} />
              </label>
              <label>
                From email
                <input type="email" value={smtpFrom} onChange={(e) => setSmtpFrom(e.target.value)} />
              </label>
            </fieldset>
            <label>
              {t('aiKey')}
              <input value={aiKey} onChange={(e) => setAiKey(e.target.value)} disabled={aiPlaceholder} />
            </label>
            <label className="row-inline">
              <input type="checkbox" checked={aiPlaceholder} onChange={(e) => setAiPlaceholder(e.target.checked)} />
              {t('aiPlaceholder')}
            </label>
            <label>
              {t('inviteCsv')}
              <textarea rows={6} value={inviteCsv} onChange={(e) => setInviteCsv(e.target.value)} />
            </label>
            <button type="submit">{t('runWizard')}</button>
            {wizardResult && <pre>{wizardResult}</pre>}
          </form>
          <form className="card" onSubmit={login}>
            <h2>{t('login')}</h2>
            <label>
              {t('email')}
              <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} />
            </label>
            <label>
              {t('password')}
              <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} />
            </label>
            <button type="submit">{t('login')}</button>
          </form>
        </>
      ) : (
        <>
          <button
            type="button"
            onClick={() => {
              setToken(null)
              setLoggedIn(false)
            }}
          >
            {t('logout')}
          </button>
          <form className="card" onSubmit={genYear}>
            <h2>{t('generateYear')}</h2>
            <label>
              {t('employeeId')}
              <input value={empId} onChange={(e) => setEmpId(e.target.value)} required />
            </label>
            <label>
              {t('year')}
              <input type="number" value={year} onChange={(e) => setYear(+e.target.value)} />
            </label>
            <label>
              {t('pattern')}
              <input type="number" value={pattern} onChange={(e) => setPattern(+e.target.value)} />
            </label>
            <label>
              {t('anchor')}
              <input value={anchor} onChange={(e) => setAnchor(e.target.value)} />
            </label>
            <label>
              {t('legacySource')}
              <input
                value={legacySource}
                onChange={(e) => setLegacySource(e.target.value)}
                placeholder="SecPlan / Synthetic"
              />
            </label>
            <button type="submit">{t('generateYear')}</button>
          </form>
          <section className="card">
            <h2>{t('rosterGrid')}</h2>
            <label>
              {t('periodId')}
              <input value={periodId} onChange={(e) => setPeriodId(e.target.value)} placeholder="GUID after generate" />
            </label>
            <div className="row">
              <button type="button" onClick={() => void loadRosterGrid()}>
                {t('loadRoster')}
              </button>
              <button type="button" onClick={() => void publishRoster()}>
                {t('publishDraft')}
              </button>
            </div>
            <div className="table-wrap">
              <table className="roster-table">
                <thead>
                  <tr>
                    <th>{t('colDate')}</th>
                    <th>{t('colEmployee')}</th>
                    <th>{t('colTier')}</th>
                  </tr>
                </thead>
                <tbody>
                  {assignments.map((row) => (
                    <tr key={row.id}>
                      <td>{row.workDate}</td>
                      <td>{row.employee.displayName}</td>
                      <td>
                        <select
                          value={row.shiftTierId}
                          onChange={(e) => void patchTier(row.id, e.target.value)}
                        >
                          {tiers.map((tier) => (
                            <option key={tier.id} value={tier.id}>
                              {tier.displayName} ({tier.code})
                            </option>
                          ))}
                        </select>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </section>
          <section className="card">
            <h2>{t('exceptionView')}</h2>
            <form onSubmit={sick}>
              <button type="submit">{t('recordSick')}</button>
            </form>
            <label>
              {t('ledgerId')}
              <input value={ledgerId} onChange={(e) => setLedgerId(e.target.value)} />
            </label>
            <button type="button" onClick={() => void propose()}>
              {t('proposeReplan')}
            </button>
            <pre>{proposal}</pre>
          </section>
        </>
      )}
    </div>
  )
}
