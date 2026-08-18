import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useAuth, useApi } from '../context/ApiProvider'

type LoginRes = { token: string; email: string; tenantId: string }

export default function LoginPage() {
  const { t } = useTranslation()
  const { api } = useApi()
  const { login: saveSession } = useAuth()
  const navigate = useNavigate()
  const [email, setEmail] = useState('admin@example.com')
  const [password, setPassword] = useState('StrongPassword123.')
  const [err, setErr] = useState<string | null>(null)

  async function submitLogin(e: React.FormEvent) {
    e.preventDefault()
    setErr(null)
    try {
      const r = await api<LoginRes>('/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password }),
      })
      saveSession(r.token, { email: r.email, tenantId: r.tenantId })
      navigate('/roster')
    } catch (ex) {
      setErr(ex instanceof Error ? ex.message : 'Login failed')
    }
  }

  return (
    <div className="login-page">
      <form className="card login-card" onSubmit={submitLogin}>
        <h1>{t('appTitle')}</h1>
        <p style={{ color: 'var(--app-text-muted)' }}>{t('loginSubtitle')}</p>
        {err && <div className="alert alert-error">{err}</div>}
        <label className="field">
          {t('email')}
          <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
        </label>
        <label className="field">
          {t('password')}
          <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required />
        </label>
        <button type="submit" className="btn btn-primary" style={{ marginTop: '0.75rem', width: '100%' }}>
          {t('login')}
        </button>
      </form>
    </div>
  )
}
