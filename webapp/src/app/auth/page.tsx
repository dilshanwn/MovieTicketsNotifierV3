'use client'

import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { useAuthStore } from '@/lib/store'
import { LoginForm } from '@/components/auth/LoginForm'
import { SignUpForm } from '@/components/auth/SignUpForm'
import { ResetPasswordForm } from '@/components/auth/ResetPasswordForm'
import { Film, Loader2 } from 'lucide-react'

type AuthMode = 'login' | 'signup' | 'reset'

export default function AuthPage() {
  const [mode, setMode] = useState<AuthMode>('login')
  const { user, loading } = useAuthStore()
  const router = useRouter()

  useEffect(() => {
    if (!loading && user) {
      router.push('/dashboard')
    }
  }, [user, loading, router])

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <Loader2 className="h-8 w-8 animate-spin mx-auto mb-4" />
          <p className="text-muted-foreground">Loading...</p>
        </div>
      </div>
    )
  }

  if (user) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <Loader2 className="h-8 w-8 animate-spin mx-auto mb-4" />
          <p className="text-muted-foreground">Redirecting to dashboard...</p>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-background p-4">
      <div className="w-full max-w-md space-y-8">
        {/* App Logo and Title */}
        <div className="text-center">
          <div className="flex justify-center mb-4">
            <div className="rounded-full bg-primary p-3">
              <Film className="h-8 w-8 text-primary-foreground" />
            </div>
          </div>
          <h1 className="text-3xl font-bold tracking-tight">
            Scope Ticket Notifier
          </h1>
          <p className="text-muted-foreground mt-2">
            Never miss a movie ticket release again
          </p>
        </div>

        {/* Authentication Forms */}
        {mode === 'login' && (
          <LoginForm 
            onToggleMode={() => setMode('signup')}
          />
        )}

        {mode === 'signup' && (
          <SignUpForm 
            onToggleMode={() => setMode('login')}
          />
        )}

        {mode === 'reset' && (
          <ResetPasswordForm 
            onBack={() => setMode('login')}
          />
        )}

        {/* Reset Password Link */}
        {mode === 'login' && (
          <div className="text-center">
            <button
              type="button"
              onClick={() => setMode('reset')}
              className="text-sm text-muted-foreground hover:text-primary underline"
            >
              Forgot your password?
            </button>
          </div>
        )}
      </div>
    </div>
  )
}
