'use client'

import { useEffect } from 'react'
import { useAuthStore } from '@/lib/store'

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const { initialize } = useAuthStore()

  useEffect(() => {
    initialize()
  }, [initialize])

  return <>{children}</>
}
