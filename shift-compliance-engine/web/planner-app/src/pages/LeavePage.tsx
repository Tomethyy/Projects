import { useCallback, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useApi } from '../context/ApiProvider'

type LeaveRow = {
  id: string
  employeeId: string
  employee?: { personnelNumber: string; displayName: string }
  startDate: string
  endDate: string
  carryoverYear: number
  isCarryoverFrozen: boolean
}

export default function LeavePage() {
  const { api } = useApi()
  const { t } = useTranslation()
  const [year, setYear] = useState(new Date().getFullYear())
  const [carryoverYear, setCarryoverYear] = useState(new Date().getFullYear() - 1)
  const [leaves, setLeaves] = useState<LeaveRow[]>([])
  const [err, setErr] = useState<string | null>(null)

  const loadLeaves = useCallback(async () => {
    setErr(null)
    try {
      setLeaves(await api<LeaveRow[]>(`/leave?year=${year}`))
    } catch (ex) {
      setErr(ex instanceof Error ? ex.message : 'Load failed')
    }
  }, [api, year])

  async function freezeCarryover() {
    setErr(null)
    try {
      await api('/leave/carryover/freeze', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ carryoverYear, employeeId: null }),
      })
      await loadLeaves()
    } catch (ex) {
      setErr(ex instanceof Error ? ex.message : 'Freeze failed')
    }
  }

  return (
    <>
      <header className="page-header">
        <h1>{t('navLeave')}</h1>
        <p>{t('leaveSection')}</p>
      </header>
      {err && <div className="alert alert-error">{err}</div>}
      <section className="card">
        <div className="field-grid">
          <label className="field">
            {t('year')}
            <input type="number" value={year} onChange={(e) => setYear(+e.target.value)} />
          </label>
          <label className="field">
            {t('carryoverYear')}
            <input type="number" value={carryoverYear} onChange={(e) => setCarryoverYear(+e.target.value)} />
          </label>
        </div>
        <div className="btn-row" style={{ marginTop: '1rem' }}>
          <button type="button" className="btn btn-primary" onClick={() => void loadLeaves()}>
            {t('loadLeaves')}
          </button>
          <button type="button" className="btn" onClick={() => void freezeCarryover()}>
            {t('freezeCarryover')}
          </button>
        </div>
      </section>
      {leaves.length > 0 && (
        <section className="card table-wrap">
          <table className="data-table">
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
                  <td>{l.employee ? `${l.employee.personnelNumber} — ${l.employee.displayName}` : l.employeeId.slice(0, 8)}</td>
                  <td>
                    {l.startDate} – {l.endDate}
                  </td>
                  <td>{l.carryoverYear || '—'}</td>
                  <td>{l.isCarryoverFrozen ? '✓' : '—'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </section>
      )}
    </>
  )
}
