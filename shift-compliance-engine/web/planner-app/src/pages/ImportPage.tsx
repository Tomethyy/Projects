import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useApi } from '../context/ApiProvider'

type SecPlanRow = { rowNumber: number; personnelNumber: string | null; displayName: string | null; error: string | null }

export default function ImportPage() {
  const { getToken } = useApi()
  const { t } = useTranslation()
  const [rows, setRows] = useState<SecPlanRow[] | null>(null)
  const [err, setErr] = useState<string | null>(null)

  async function secPlanDryRun(file: File) {
    setErr(null)
    setRows(null)
    const fd = new FormData()
    fd.append('file', file)
    const headers = new Headers()
    const token = getToken()
    if (token) headers.set('Authorization', `Bearer ${token}`)
    headers.set('Accept-Language', localStorage.getItem('locale') ?? 'de')
    const res = await fetch('/api/import/secplan/dry-run', { method: 'POST', headers, body: fd })
    if (!res.ok) throw new Error((await res.text()) || res.statusText)
    const data = (await res.json()) as { rows: SecPlanRow[] }
    setRows(data.rows ?? [])
  }

  return (
    <>
      <header className="page-header">
        <h1>{t('navImport')}</h1>
        <p>{t('secPlanHint')}</p>
      </header>
      {err && <div className="alert alert-error">{err}</div>}
      <section className="card">
        <label className="field">
          {t('secPlanSection')}
          <input
            type="file"
            accept=".xlsx,.xls"
            onChange={(e) => {
              const f = e.target.files?.[0]
              if (f) void secPlanDryRun(f).catch((ex) => setErr(ex instanceof Error ? ex.message : 'Failed'))
            }}
          />
        </label>
      </section>
      {rows && (
        <section className="card">
          <h2>{t('dryRunOk')}</h2>
          <ul>
            {rows.map((r) => (
              <li key={r.rowNumber} style={{ color: r.error ? 'var(--app-danger)' : 'inherit' }}>
                #{r.rowNumber} {r.personnelNumber ?? '—'} {r.displayName ?? ''} {r.error ?? 'OK'}
              </li>
            ))}
          </ul>
        </section>
      )}
    </>
  )
}
