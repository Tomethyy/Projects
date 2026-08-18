import { useCallback, useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useApi } from '../context/ApiProvider'

type ShiftTier = {
  id: string
  code: string
  displayName: string
  startLocal: string
  endLocal: string
  isNight: boolean
}

export default function RulesPage() {
  const { api } = useApi()
  const { t } = useTranslation()
  const [tiers, setTiers] = useState<ShiftTier[]>([])
  const [msg, setMsg] = useState<string | null>(null)
  const [err, setErr] = useState<string | null>(null)

  const load = useCallback(async () => {
    setErr(null)
    const raw = await api<ShiftTier[]>('/shift-tiers')
    setTiers(raw.map((t) => ({ ...t, startLocal: String(t.startLocal).slice(0, 5), endLocal: String(t.endLocal).slice(0, 5) })))
  }, [api])

  useEffect(() => {
    void load().catch((ex) => setErr(ex instanceof Error ? ex.message : 'Load failed'))
  }, [load])

  async function save(tier: ShiftTier) {
    setErr(null)
    setMsg(null)
    try {
      await api(`/shift-tiers/${tier.id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          ...tier,
          startLocal: tier.startLocal.length === 5 ? `${tier.startLocal}:00` : tier.startLocal,
          endLocal: tier.endLocal.length === 5 ? `${tier.endLocal}:00` : tier.endLocal,
        }),
      })
      setMsg(t('rowSaved'))
      await load()
    } catch (ex) {
      setErr(ex instanceof Error ? ex.message : 'Save failed')
    }
  }

  return (
    <>
      <header className="page-header">
        <h1>{t('navRules')}</h1>
        <p>{t('rulesHint')}</p>
      </header>
      {err && <div className="alert alert-error">{err}</div>}
      {msg && <div className="alert alert-info">{msg}</div>}
      <section className="card">
        <h2>{t('shiftTiers')}</h2>
        <div className="table-wrap">
          <table className="data-table">
            <thead>
              <tr>
                <th>Code</th>
                <th>{t('colTier')}</th>
                <th>{t('colWindow')}</th>
                <th>Night</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {tiers.map((tier) => (
                <tr key={tier.id}>
                  <td>
                    <input value={tier.code} onChange={(e) => setTiers((prev) => prev.map((t) => (t.id === tier.id ? { ...t, code: e.target.value } : t)))} />
                  </td>
                  <td>
                    <input value={tier.displayName} onChange={(e) => setTiers((prev) => prev.map((t) => (t.id === tier.id ? { ...t, displayName: e.target.value } : t)))} />
                  </td>
                  <td>
                    <input style={{ width: '5rem' }} value={tier.startLocal} onChange={(e) => setTiers((prev) => prev.map((t) => (t.id === tier.id ? { ...t, startLocal: e.target.value } : t)))} />
                    {' – '}
                    <input style={{ width: '5rem' }} value={tier.endLocal} onChange={(e) => setTiers((prev) => prev.map((t) => (t.id === tier.id ? { ...t, endLocal: e.target.value } : t)))} />
                  </td>
                  <td>
                    <input type="checkbox" checked={tier.isNight} onChange={(e) => setTiers((prev) => prev.map((t) => (t.id === tier.id ? { ...t, isNight: e.target.checked } : t)))} />
                  </td>
                  <td>
                    <button type="button" className="btn" onClick={() => void save(tier)}>
                      {t('saveRow')}
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>
      <section className="card">
        <h2>ArbZG (automatisch)</h2>
        <ul>
          <li>ArbZG.WeeklyHours — max. 48 h / Kalenderwoche</li>
          <li>ArbZG.DailyRest — min. 11 h zwischen aufeinanderfolgenden Schichten</li>
        </ul>
      </section>
    </>
  )
}
