import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useApi } from '../context/ApiProvider'

type AuditRow = {
  id: string
  action: string
  entityType: string | null
  entityId: string | null
  createdAt: string
}

export default function AuditPage() {
  const { api } = useApi()
  const { t } = useTranslation()
  const [audit, setAudit] = useState<AuditRow[]>([])
  const [err, setErr] = useState<string | null>(null)

  async function load() {
    setErr(null)
    try {
      setAudit(await api<AuditRow[]>('/audit?limit=50'))
    } catch (ex) {
      setErr(ex instanceof Error ? ex.message : 'Load failed')
    }
  }

  return (
    <>
      <header className="page-header">
        <h1>{t('navAudit')}</h1>
      </header>
      {err && <div className="alert alert-error">{err}</div>}
      <section className="card">
        <button type="button" className="btn btn-primary" onClick={() => void load()}>
          {t('loadAudit')}
        </button>
      </section>
      {audit.length > 0 && (
        <section className="card table-wrap">
          <table className="data-table">
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
        </section>
      )}
    </>
  )
}
