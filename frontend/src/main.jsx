import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import { AuthProvider } from './hooks/useAuth'
import { GlobalLoadingOverlay } from './components/GlobalLoadingOverlay'
import { AppErrorBoundary } from './components/AppErrorBoundary'
import './index.css'
import App from './App.jsx'

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <BrowserRouter>
      <AuthProvider>
        <AppErrorBoundary>
          <App />
          <GlobalLoadingOverlay />
        </AppErrorBoundary>
      </AuthProvider>
    </BrowserRouter>
  </StrictMode>,
)
