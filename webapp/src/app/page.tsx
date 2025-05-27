'use client'

import { useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { useAuthStore } from '@/lib/store'
import { Loader2, Film } from 'lucide-react'

export default function Home() {
  const { user, loading } = useAuthStore()
  const router = useRouter()

  useEffect(() => {
    if (!loading) {
      if (user) {
        router.push('/dashboard')
      } else {
        router.push('/auth')
      }
    }
  }, [user, loading, router])

  return (
    <div className="min-h-screen flex items-center justify-center bg-background">
      <div className="text-center">
        <div className="flex justify-center mb-4">
          <div className="rounded-full bg-primary p-3">
            <Film className="h-8 w-8 text-primary-foreground animate-pulse" />
          </div>
        </div>
        <h1 className="text-2xl font-bold mb-2">Scope Ticket Notifier</h1>
        <div className="flex items-center justify-center gap-2">
          <Loader2 className="h-4 w-4 animate-spin" />
          <p className="text-muted-foreground">Loading...</p>
        </div>
      </div>
    </div>
  )
}
