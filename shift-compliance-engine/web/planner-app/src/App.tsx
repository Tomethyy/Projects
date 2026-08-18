import { lazy, Suspense } from 'react'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { ApiProvider, useAuth } from './context/ApiProvider'
import AppLayout from './layout/AppLayout'
import LoginPage from './pages/LoginPage'

const RosterPage = lazy(() => import('./pages/RosterPage'))
const PersonnelPage = lazy(() => import('./pages/PersonnelPage'))
const PositionsPage = lazy(() => import('./pages/PositionsPage'))
const DeploymentPage = lazy(() => import('./pages/DeploymentPage'))
const CompliancePage = lazy(() => import('./pages/CompliancePage'))
const ExceptionsPage = lazy(() => import('./pages/ExceptionsPage'))
const LeavePage = lazy(() => import('./pages/LeavePage'))
const ImportPage = lazy(() => import('./pages/ImportPage'))
const AuditPage = lazy(() => import('./pages/AuditPage'))
const RulesPage = lazy(() => import('./pages/RulesPage'))
const SetupPage = lazy(() => import('./pages/SetupPage'))

function ProtectedShell() {
  const { isLoggedIn } = useAuth()
  if (!isLoggedIn) return <Navigate to="/login" replace />
  return <AppLayout />
}

function PageFallback() {
  return (
    <div className="app-content" style={{ color: 'var(--app-text-muted)' }}>
      …
    </div>
  )
}

export default function App() {
  return (
    <ApiProvider>
      <BrowserRouter>
        <Suspense fallback={<PageFallback />}>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route element={<ProtectedShell />}>
              <Route index element={<Navigate to="/roster" replace />} />
              <Route path="/roster" element={<RosterPage />} />
              <Route path="/deployment" element={<DeploymentPage />} />
              <Route path="/personnel" element={<PersonnelPage />} />
              <Route path="/positions" element={<PositionsPage />} />
              <Route path="/compliance" element={<CompliancePage />} />
              <Route path="/exceptions" element={<ExceptionsPage />} />
              <Route path="/leave" element={<LeavePage />} />
              <Route path="/import" element={<ImportPage />} />
              <Route path="/audit" element={<AuditPage />} />
              <Route path="/rules" element={<RulesPage />} />
              <Route path="/setup" element={<SetupPage />} />
            </Route>
          </Routes>
        </Suspense>
      </BrowserRouter>
    </ApiProvider>
  )
}
