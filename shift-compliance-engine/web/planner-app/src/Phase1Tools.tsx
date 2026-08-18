import { useCallback, useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { ShiftApiClient } from '@shift-engine/api-client'

type Props = {
  api: ShiftApiClient['api']
  getToken: ShiftApiClient['getToken']
  periodId: string
  year: number
  onError: (msg: string | null) => void
}

type ComplianceFinding = {
  ruleCode: string
  message: string
  isBlocking: boolean
  employeeId: string | null
  date: string | null
}

type BvFinding = { code: string; messageDe: string; severity: string }

type LeaveRow = {
  id: string
  employeeId: string
  employee?: { personnelNumber: string; displayName: string }
  startDate: string
  endDate: string
  source: number
  carryoverYear: number
  isCarryoverFrozen: boolean
  isApproved: boolean
}

type AuditRow = {
  id: string
  action: string
  entityType: string | null
  entityId: string | null
  actorUserId: string
  createdAt: string
  detailsJson: string | null
}

type SecPlanRow = { rowNumber: number; personnelNumber: string | null; displayName: string | null; error: string | null }

export default function Phase1Tools({ api, getToken, periodId, year, onError }: Props) {
  const { t } = useTranslation()
  const [compliance, setCompliance] = useState<ComplianceFinding[] | null>(null)
  const [bvAudit, setBvAudit] = useState<BvFinding[] | null>(null)
  const [leaves, setLeaves] = useState<LeaveRow[]>([])
  const [carryoverYear, setCarryoverYear] = useState(year - 1)
  const [audit, setAudit] = useState<AuditRow[]>([])
  const [secPlanRows, setSecPlanRows] = useState<SecPlanRow[] | null>(null)

  const runCompliance = useCallback(async () => {
    if (!periodId) return
    onError(null)
    try {
      const [c, b] = await Promise.all([
        api<ComplianceFinding[]>(`/compliance/evaluate/${periodId}`),
        api<BvFinding[]>(`/compliance/bv-audit/${periodId}`),
      ])
      setCompliance(c)
      setBvAudit(b)
    } catch (ex) {
      onError(ex instanceof Error ? ex.message : 'Compliance failed')
    }
  }, [api, onError, periodId])

  const loadLeaves = useCallback(async () => {
    onError(null)
    try {
      const list = await api<LeaveRow[]>(`/leave?year=${year}`)
      setLeaves(list)
    } catch (ex) {
      onError(ex instanceof Error ? ex.message : 'Leave load failed')
    }
  }, [api, onError, year])

  const freezeCarryover = useCallback(async () => {
    onError(null)
    try {
      await api('/leave/carryover/freeze', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ carryoverYear, employeeId: null }),
      })
      await loadLeaves()
    } catch (ex) {
      onError(ex instanceof Error ? ex.message : 'Freeze failed')
    }
  }, [api, carryoverYear, loadLeaves, onError])

  const loadAudit = useCallback(async () => {
    onError(null)
    try {
      const list = await api<AuditRow[]>('/audit?limit=40')
      setAudit(list)
    } catch (ex) {
      onError(ex instanceof Error ? ex.message : 'Audit load failed')
    }
  }, [api, onError])

  const secPlanDryRun = useCallback(
    async (file: File) => {
      onError(null)
      setSecPlanRows(null)
      const fd = new FormData()
      fd.append('file', file)
      const headers = new Headers()
      const token = getToken()
      if (token) headers.set('Authorization', `Bearer ${token}`)
      headers.set('Accept-Language', localStorage.getItem('locale') ?? 'de')
      const res = await fetch('/api/import/secplan/dry-run', { method: 'POST', headers, body: fd })
      if (!res.ok) throw new Error((await res.text()) || res.statusText)
      const data = (await res.json()) as { rows: SecPlanRow[] }
      setSecPlanRows(data.rows ?? [])
    },
    [getToken, onError],
  )

  return (
    <section className="card phase1-tools">
      <h2>{t('phase1Tools')}</h2>
      <details>
        <summary>{t('complianceSection')}</summary>
        <p className="hint">{t('complianceHint')}</p>
        <button type="button" disabled={!periodId} onClick={() => void runCompliance()}>
          {t('runCompliance')}
        </button>
        {compliance && (
          <>
            <h3>{t('arbzgFindings')}</h3>
            <ul className="findings">
              {compliance.map((f, i) => (
                <li key={`${f.ruleCode}-${i}`} className={f.isBlocking ? 'blocking' : ''}>
                  [{f.ruleCode}] {f.message}
                </li>
              ))}
              {compliance.length === 0 && <li>{t('noFindings')}</li>}
            </ul>
          </>
        )}
        {bvAudit && (
          <>
            <h3>{t('bvChecklist')}</h3>
            <ul className="findings">
              {bvAudit.map((f) => (
                <li key={f.code}>
                  [{f.severity}] {f.code}: {f.messageDe}
                </li>
              ))}
            </ul>
          </>
        )}
      </details>
      <details>
        <summary>{t('leaveSection')}</summary>
        <div className="row">
          <button type="button" onClick={() => void loadLeaves()}>
            {t('loadLeaves')}
          </button>
          <label>
            {t('carryoverYear')}
            <input type="number" value={carryoverYear} onChange={(e) => setCarryoverYear(+e.target.value)} />
          </label>
          <button type="button" onClick={() => void freezeCarryover()}>
            {t('freezeCarryover')}
          </button>
        </div>
        {leaves.length > 0 && (
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>{t('colEmployee')}</th>
                  <th>{t('colDate')}</th>
                  <th>{t('carryoverYear')}</th>
                  <th>{t('frozen')}</th>
                </tr>
              </thead>
              <tbody>
                {leaves.map((l) => (
                  <tr key={l.id}>
                    <td>
                      {l.employee
                        ? `${l.employee.personnelNumber} — ${l.employee.displayName}`
                        : l.employeeId.slice(0, 8)}
                    </td>
                    <td>
                      {l.startDate} – {l.endDate}
                    </td>
                    <td>{l.carryoverYear || '—'}</td>
                    <td>{l.isCarryoverFrozen ? '✓' : '—'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </details>
      <details>
        <summary>{t('secPlanSection')}</summary>
        <p className="hint">{t('secPlanHint')}</p>
        <input
          type="file"
          accept=".xlsx,.xls"
          onChange={(e) => {
            const f = e.target.files?.[0]
            if (f) void secPlanDryRun(f).catch((ex) => onError(ex instanceof Error ? ex.message : 'SecPlan failed'))
          }}
        />
        {secPlanRows && (
          <ul className="findings">
            {secPlanRows.map((r) => (
              <li key={r.rowNumber} className={r.error ? 'blocking' : ''}>
                #{r.rowNumber} {r.personnelNumber ?? '—'} {r.displayName ?? ''} {r.error ?? 'OK'}
              </li>
            ))}
          </ul>
        )}
      </details>
      <details>
        <summary>{t('auditSection')}</summary>
        <button type="button" onClick={() => void loadAudit()}>
          {t('loadAudit')}
        </button>
        {audit.length > 0 && (
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>{t('auditWhen')}</th>
                  <th>{t('auditAction')}</th>
                  <th>{t('auditEntity')}</th>
                </tr>
              </thead>
              <tbody>
                {audit.map((a) => (
                  <tr key={a.id}>
                    <td>{new Date(a.createdAt).toLocaleString()}</td>
                    <td>{a.action}</td>
                    <td>
                      {a.entityType ?? '—'} {a.entityId?.slice(0, 8) ?? ''}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </details>
    </section>
  )
}
