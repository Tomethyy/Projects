import { useCallback, useEffect, useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useApi } from '../context/ApiProvider'
import type { RosterMatrix } from '../roster-types'

type Employee = { id: string; personnelNumber: string; displayName: string }
type LedgerEntry = {
  id: string
  entryDate: string
  kind: number
  employeeId: string | null
  shiftAssignmentId: string | null
  notes: string | null
  employee?: { personnelNumber: string; displayName: string }
}
type ReplacementCandidate = { employeeId: string; displayName: string; score: number; reason: string }
type ReplanProposal = { id: string; ledgerEntryId: string; jsonPayload: string; isApplied: boolean }
type ReplacementCandidateRaw = {
  employeeId?: string
  EmployeeId?: string
  displayName?: string
  DisplayName?: string
  score?: number
  Score?: number
  reason?: string
  Reason?: string
}

const normalizeCandidates = (raw: ReplacementCandidateRaw[]): ReplacementCandidate[] =>
  raw.map((c) => ({
    employeeId: c.employeeId ?? c.EmployeeId ?? '',
    displayName: c.displayName ?? c.DisplayName ?? '',
    score: c.score ?? c.Score ?? 0,
    reason: c.reason ?? c.Reason ?? '',
  }))

const kindOptions = [
  { value: 0, key: 'kindSick' },
  { value: 1, key: 'kindCallOut' },
] as const

export default function ExceptionsPage() {
  const { api } = useApi()
  const { t } = useTranslation()
  const today = useMemo(() => new Date().toISOString().slice(0, 10), [])
  const [date, setDate] = useState(today)
  const [employees, setEmployees] = useState<Employee[]>([])
  const [entries, setEntries] = useState<LedgerEntry[]>([])
  const [employeeId, setEmployeeId] = useState('')
  const [kind, setKind] = useState(0)
  const [notes, setNotes] = useState('')
  const [proposal, setProposal] = useState<ReplanProposal | null>(null)
  const [candidates, setCandidates] = useState<ReplacementCandidate[]>([])
  const [selectedCandidate, setSelectedCandidate] = useState('')
  const [err, setErr] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)

  const loadEmployees = useCallback(async () => {
    try {
      const list = await api<Employee[]>('/employees')
      setEmployees(list)
      if (list.length > 0) setEmployeeId((id) => id || list[0].id)
    } catch (ex) {
      setErr(ex instanceof Error ? ex.message : 'Load failed')
    }
  }, [api])

  const loadLedger = useCallback(async () => {
    setErr(null)
    try {
      const list = await api<LedgerEntry[]>(`/ledger/today?date=${date}`)
      setEntries(list)
    } catch (ex) {
      setErr(ex instanceof Error ? ex.message : 'Ledger load failed')
    }
  }, [api, date])

  useEffect(() => {
    void loadEmployees()
  }, [loadEmployees])

  useEffect(() => {
    void loadLedger()
  }, [loadLedger])

  async function findAssignmentId(empId: string, workDate: string) {
    const d = new Date(workDate)
    const year = d.getFullYear()
    const month = d.getMonth() + 1
    try {
      const matrix = await api<RosterMatrix>(`/roster/matrix?year=${year}&month=${month}`)
      return matrix.cells.find((c) => c.employeeId === empId && c.date === workDate)?.assignmentId ?? null
    } catch {
      return null
    }
  }

  async function recordSick(e: React.FormEvent) {
    e.preventDefault()
    if (!employeeId) return
    setErr(null)
    setBusy(true)
    try {
      const shiftAssignmentId = await findAssignmentId(employeeId, date)
      await api('/ledger/sick-or-callout', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          shiftAssignmentId,
          employeeId,
          date,
          kind,
          notes: notes.trim() || null,
        }),
      })
      setNotes('')
      setProposal(null)
      setCandidates([])
      await loadLedger()
    } catch (ex) {
      setErr(ex instanceof Error ? ex.message : 'Record failed')
    } finally {
      setBusy(false)
    }
  }

  async function proposeReplan(ledgerEntryId: string) {
    setErr(null)
    setBusy(true)
    setProposal(null)
    setCandidates([])
    setSelectedCandidate('')
    try {
      const p = await api<ReplanProposal>(`/replan/propose/${ledgerEntryId}`, { method: 'POST' })
      setProposal(p)
      const ranked = normalizeCandidates(JSON.parse(p.jsonPayload) as ReplacementCandidateRaw[])
      setCandidates(ranked)
      if (ranked.length > 0) setSelectedCandidate(ranked[0].employeeId)
    } catch (ex) {
      setErr(ex instanceof Error ? ex.message : 'Propose failed')
    } finally {
      setBusy(false)
    }
  }

  async function applyReplacement() {
    if (!proposal || !selectedCandidate) return
    setErr(null)
    setBusy(true)
    try {
      await api('/replan/apply', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ proposalId: proposal.id, replacementEmployeeId: selectedCandidate }),
      })
      setProposal(null)
      setCandidates([])
      await loadLedger()
    } catch (ex) {
      setErr(ex instanceof Error ? ex.message : 'Apply failed')
    } finally {
      setBusy(false)
    }
  }

  const kindLabel = (k: number) => t(kindOptions.find((o) => o.value === k)?.key ?? 'kindSick')

  return (
    <>
      <header className="page-header">
        <h1>{t('exceptionView')}</h1>
        <p>{t('exceptionHint')}</p>
      </header>
      {err && <div className="alert alert-error">{err}</div>}
      <section className="card">
        <h2>{t('recordSick')}</h2>
        <form onSubmit={recordSick}>
          <div className="field-grid">
            <label className="field">
              {t('ledgerDate')}
              <input type="date" value={date} onChange={(e) => setDate(e.target.value)} required />
            </label>
            <label className="field">
              {t('employeeForException')}
              <select value={employeeId} onChange={(e) => setEmployeeId(e.target.value)} required>
                {employees.map((e) => (
                  <option key={e.id} value={e.id}>
                    {e.personnelNumber} — {e.displayName}
                  </option>
                ))}
              </select>
            </label>
            <label className="field">
              {t('colKind')}
              <select value={kind} onChange={(e) => setKind(+e.target.value)}>
                {kindOptions.map((o) => (
                  <option key={o.value} value={o.value}>
                    {t(o.key)}
                  </option>
                ))}
              </select>
            </label>
            <label className="field">
              {t('colNotes')}
              <input value={notes} onChange={(e) => setNotes(e.target.value)} />
            </label>
          </div>
          <div className="btn-row" style={{ marginTop: '1rem' }}>
            <button type="submit" className="btn btn-primary" disabled={busy || !employeeId}>
              {t('recordSick')}
            </button>
            <button type="button" className="btn" onClick={() => void loadLedger()} disabled={busy}>
              {t('loadLedger')}
            </button>
          </div>
        </form>
      </section>
      <section className="card">
        <h2>{t('loadLedger')}</h2>
        {entries.length === 0 ? (
          <p style={{ color: 'var(--app-text-muted)' }}>{t('noLedgerEntries')}</p>
        ) : (
          <div className="table-wrap">
            <table className="data-table">
              <thead>
                <tr>
                  <th>{t('colEmployee')}</th>
                  <th>{t('colDate')}</th>
                  <th>{t('colKind')}</th>
                  <th>{t('colNotes')}</th>
                  <th />
                </tr>
              </thead>
              <tbody>
                {entries.map((entry) => (
                  <tr key={entry.id}>
                    <td>
                      {entry.employee
                        ? `${entry.employee.personnelNumber} — ${entry.employee.displayName}`
                        : entry.employeeId?.slice(0, 8) ?? '—'}
                    </td>
                    <td>{entry.entryDate}</td>
                    <td>{kindLabel(entry.kind)}</td>
                    <td>{entry.notes ?? '—'}</td>
                    <td>
                      <button
                        type="button"
                        className="btn"
                        disabled={busy || entry.kind >= 2 || !entry.shiftAssignmentId}
                        onClick={() => void proposeReplan(entry.id)}
                        title={entry.shiftAssignmentId ? undefined : t('noShiftLinked')}
                      >
                        {t('proposeReplan')}
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
      {candidates.length > 0 && proposal && (
        <section className="card">
          <h2>{t('candidates')}</h2>
          <div className="table-wrap">
            <table className="data-table">
              <thead>
                <tr>
                  <th />
                  <th>{t('colEmployee')}</th>
                  <th>{t('score')}</th>
                  <th>{t('complianceReason')}</th>
                </tr>
              </thead>
              <tbody>
                {candidates.map((c) => (
                  <tr key={c.employeeId}>
                    <td>
                      <input
                        type="radio"
                        name="candidate"
                        checked={selectedCandidate === c.employeeId}
                        onChange={() => setSelectedCandidate(c.employeeId)}
                      />
                    </td>
                    <td>{c.displayName}</td>
                    <td>{c.score}</td>
                    <td>{c.reason}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <div className="btn-row" style={{ marginTop: '1rem' }}>
            <button type="button" className="btn btn-primary" disabled={busy || !selectedCandidate} onClick={() => void applyReplacement()}>
              {t('applyReplacement')}
            </button>
          </div>
        </section>
      )}
    </>
  )
}
