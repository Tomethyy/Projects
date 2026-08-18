import { useCallback, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useApi } from '../context/ApiProvider'

type Finding = { ruleCode: string; message: string; isBlocking: boolean; employeeId: string | null; date: string | null }
type BvFinding = { code: string; messageDe: string; severity: string }
type FixAction = {
  ruleCode: string
  assignmentId: string
  employeeId: string
  workDate: string
  action: string
  reason: string
  targetEmployeeId?: string | null
  targetEmployeeName?: string | null
}
type FixProposal = { actions: FixAction[]; remainingFindings: Finding[]; blockingBefore: number; blockingAfter: number }

export default function CompliancePage() {
  const { api } = useApi()
  const { t } = useTranslation()
  const now = new Date()
  const [year, setYear] = useState(now.getFullYear())
  const [month, setMonth] = useState(now.getMonth() + 1)
  const [periodId, setPeriodId] = useState('')
  const [findings, setFindings] = useState<Finding[] | null>(null)
  const [bv, setBv] = useState<BvFinding[] | null>(null)
  const [proposal, setProposal] = useState<FixProposal | null>(null)
  const [err, setErr] = useState<string | null>(null)

  const resolvePeriod = useCallback(async () => {
    const m = await api<{ periodId: string }>(`/roster/matrix?year=${year}&month=${month}`)
    setPeriodId(m.periodId)
    return m.periodId
  }, [api, year, month])

  async function runCheck() {
    setErr(null)
    try {
      const pid = periodId || (await resolvePeriod())
      const [c, b] = await Promise.all([
        api<Finding[]>(`/compliance/evaluate/${pid}`),
        api<BvFinding[]>(`/compliance/bv-audit/${pid}`),
      ])
      setFindings(c)
      setBv(b)
      setProposal(null)
    } catch (ex) {
      setErr(ex instanceof Error ? ex.message : 'Compliance failed')
    }
  }

  async function proposeFixes() {
    setErr(null)
    try {
      const pid = periodId || (await resolvePeriod())
      const p = await api<FixProposal>(`/compliance/propose-fixes/${pid}`, { method: 'POST' })
      setProposal(p)
    } catch (ex) {
      setErr(ex instanceof Error ? ex.message : 'Propose failed')
    }
  }

  async function applyFixes() {
    setErr(null)
    try {
      const pid = periodId || (await resolvePeriod())
      const fixes =
        proposal?.actions.map((a) => ({
          action: a.action,
          assignmentId: a.assignmentId,
          targetEmployeeId: a.targetEmployeeId ?? null,
        })) ?? null
      const p = await api<FixProposal>(`/compliance/apply-fixes/${pid}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ assignmentIds: null, fixes }),
      })
      setProposal(p)
      setFindings(p.remainingFindings)
    } catch (ex) {
      setErr(ex instanceof Error ? ex.message : 'Apply failed')
    }
  }

  const actionLabel = (action: string, target?: string | null) => {
    if (action === 'ReassignAssignment' && target) return `${t('actionReassign')} → ${target}`
    if (action === 'RemoveAssignment') return t('actionRemove')
    return action
  }

  return (
    <>
      <header className="page-header">
        <h1>{t('navCompliance')}</h1>
        <p>{t('complianceHint')}</p>
      </header>
      {err && <div className="alert alert-error">{err}</div>}
      <section className="card">
        <div className="field-grid">
          <label className="field">
            {t('year')}
            <input type="number" value={year} onChange={(e) => setYear(+e.target.value)} />
          </label>
          <label className="field">
            {t('month')}
            <input type="number" min={1} max={12} value={month} onChange={(e) => setMonth(+e.target.value)} />
          </label>
        </div>
        <div className="btn-row" style={{ marginTop: '1rem' }}>
          <button type="button" className="btn btn-primary" onClick={() => void runCheck()}>
            {t('runCompliance')}
          </button>
          <button type="button" className="btn" onClick={() => void proposeFixes()}>
            {t('proposeFixes')}
          </button>
          <button type="button" className="btn btn-danger" onClick={() => void applyFixes()} disabled={!proposal?.actions.length}>
            {t('applyFixes')}
          </button>
        </div>
      </section>
      {findings && (
        <section className="card">
          <h2>
            {t('arbzgFindings')} ({findings.filter((f) => f.isBlocking).length} blocking)
          </h2>
          <ul>
            {findings.map((f, i) => (
              <li key={`${f.ruleCode}-${i}`} style={{ color: f.isBlocking ? 'var(--app-danger)' : 'inherit' }}>
                [{f.ruleCode}] {f.message}
              </li>
            ))}
            {findings.length === 0 && <li>{t('noFindings')}</li>}
          </ul>
        </section>
      )}
      {bv && (
        <section className="card">
          <h2>{t('bvChecklist')}</h2>
          <ul>
            {bv.map((f) => (
              <li key={f.code}>
                [{f.severity}] {f.code}: {f.messageDe}
              </li>
            ))}
          </ul>
        </section>
      )}
      {proposal && proposal.actions.length > 0 && (
        <section className="card">
          <h2>
            {t('proposedFixes')} ({proposal.actions.length}) — {t('blockingAfter')}: {proposal.blockingAfter}
          </h2>
          <div className="table-wrap">
            <table className="data-table">
              <thead>
                <tr>
                  <th>{t('colDate')}</th>
                  <th>{t('auditAction')}</th>
                  <th>{t('colEmployee')}</th>
                  <th>{t('complianceReason')}</th>
                </tr>
              </thead>
              <tbody>
                {proposal.actions.map((a) => (
                  <tr key={`${a.assignmentId}-${a.action}`}>
                    <td>{a.workDate}</td>
                    <td>{actionLabel(a.action, a.targetEmployeeName)}</td>
                    <td>{a.targetEmployeeName ?? '—'}</td>
                    <td>{a.reason}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>
      )}
    </>
  )
}
