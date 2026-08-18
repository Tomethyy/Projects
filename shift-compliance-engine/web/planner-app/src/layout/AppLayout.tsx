import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useAuth } from '../context/ApiProvider'

const nav = [
  { to: '/roster', key: 'navRoster' },
  { to: '/deployment', key: 'navDeployment' },
  { to: '/personnel', key: 'navPersonnel' },
  { to: '/positions', key: 'navPositions' },
  { to: '/compliance', key: 'navCompliance' },
  { to: '/exceptions', key: 'navExceptions' },
  { to: '/leave', key: 'navLeave' },
  { to: '/import', key: 'navImport' },
  { to: '/rules', key: 'navRules' },
  { to: '/audit', key: 'navAudit' },
  { to: '/setup', key: 'navSetup' },
] as const

export default function AppLayout() {
  const { t, i18n } = useTranslation()
  const { logout, user } = useAuth()
  const navigate = useNavigate()

  return (
    <div className="app-shell">
      <aside className="app-sidebar">
        <div className="app-brand">{t('appTitle')}</div>
        <nav className="app-nav">
          {nav.map(({ to, key }) => (
            <NavLink key={to} to={to} className={({ isActive }) => (isActive ? 'active' : undefined)}>
              {t(key)}
            </NavLink>
          ))}
        </nav>
      </aside>
      <div className="app-main">
        <header className="app-topbar">
          <div className="app-topbar-user">
            {user && (
              <>
                <span className="app-user-email">{user.email}</span>
                <span className="app-user-tenant">{user.tenantId.slice(0, 8)}…</span>
              </>
            )}
          </div>
          <div className="app-topbar-actions">
            <select
              value={i18n.language}
              onChange={(e) => {
                localStorage.setItem('locale', e.target.value)
                void i18n.changeLanguage(e.target.value)
              }}
              aria-label={t('locale')}
            >
              <option value="de">DE</option>
              <option value="en">EN</option>
            </select>
            <button
              type="button"
              className="btn"
              onClick={() => {
                logout()
                navigate('/login')
              }}
            >
              {t('logout')}
            </button>
          </div>
        </header>
        <main className="app-content">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
