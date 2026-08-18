import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import '@shift-engine/ui/tokens.css'
import '@shift-engine/ui/roster-grid.css'
import './i18n'
import './styles/app-theme.css'
import App from './App.tsx'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App />
  </StrictMode>,
)
