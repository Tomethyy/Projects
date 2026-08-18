import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useApi } from '../context/ApiProvider'

export default function SetupPage() {
  const { t } = useTranslation()
  const { api } = useApi()
  const [slug, setSlug] = useState('demo')
  const [tenantName, setTenantName] = useState('Demo GmbH')
  const [email, setEmail] = useState('admin@example.com')
  const [password, setPassword] = useState('StrongPassword123.')
  const [adminName, setAdminName] = useState('Admin')
  const [inviteCsv, setInviteCsv] = useState(
    'PersonnelNumber;DisplayName;Email\n1001;Max Mustermann;max@example.com',
  )
  const [result, setResult] = useState('')
  const [err, setErr] = useState<string | null>(null)

  async function wizard(e: React.FormEvent) {
    e.preventDefault()
    setErr(null)
    try {
      const r = await api<unknown>('/setup/wizard', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          tenantSlug: slug,
          tenantDisplayName: tenantName,
          defaultLocale: 'de-DE',
          adminEmail: email,
          adminPassword: password,
          adminDisplayName: adminName,
          enableAiKeyPlaceholder: true,
          employeeInviteCsv: inviteCsv.trim() || null,
        }),
      })
      setResult(JSON.stringify(r, null, 2))
    } catch (ex) {
      setErr(ex instanceof Error ? ex.message : 'Wizard failed')
    }
  }

  return (
    <>
      <header className="page-header">
        <h1>{t('wizard')}</h1>
        <p>{t('wizardDbHint')}</p>
      </header>
      {err && <div className="alert alert-error">{err}</div>}
      <form className="card" onSubmit={wizard}>
        <div className="field-grid">
          <label className="field">
            {t('tenantSlug')}
            <input value={slug} onChange={(e) => setSlug(e.target.value)} />
          </label>
          <label className="field">
            {t('tenantName')}
            <input value={tenantName} onChange={(e) => setTenantName(e.target.value)} />
          </label>
          <label className="field">
            {t('email')}
            <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} />
          </label>
          <label className="field">
            {t('adminName')}
            <input value={adminName} onChange={(e) => setAdminName(e.target.value)} />
          </label>
          <label className="field">
            {t('password')}
            <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} />
          </label>
        </div>
        <label className="field" style={{ marginTop: '0.75rem' }}>
          {t('inviteCsv')}
          <textarea rows={5} value={inviteCsv} onChange={(e) => setInviteCsv(e.target.value)} />
        </label>
        <button type="submit" className="btn btn-primary" style={{ marginTop: '0.75rem' }}>
          {t('runWizard')}
        </button>
        {result && <pre style={{ marginTop: '1rem', overflow: 'auto' }}>{result}</pre>}
      </form>
    </>
  )
}
