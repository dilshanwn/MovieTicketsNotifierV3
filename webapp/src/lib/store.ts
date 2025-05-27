import { create } from 'zustand'
import { User, Session } from '@supabase/supabase-js'
import { supabase } from '@/lib/supabase'

interface AuthState {
  user: User | null
  session: Session | null
  loading: boolean
  signIn: (email: string, password: string) => Promise<{ error: any }>
  signUp: (email: string, password: string) => Promise<{ error: any }>
  signOut: () => Promise<void>
  resetPassword: (email: string) => Promise<{ error: any }>
  initialize: () => Promise<void>
}

export const useAuthStore = create<AuthState>((set, get) => ({
  user: null,
  session: null,
  loading: true,

  signIn: async (email: string, password: string) => {
    const { data, error } = await supabase.auth.signInWithPassword({
      email,
      password,
    })
    
    if (data.user && data.session) {
      set({ user: data.user, session: data.session })
    }
    
    return { error }
  },

  signUp: async (email: string, password: string) => {
    const { data, error } = await supabase.auth.signUp({
      email,
      password,
    })
    
    return { error }
  },

  signOut: async () => {
    await supabase.auth.signOut()
    set({ user: null, session: null })
  },

  resetPassword: async (email: string) => {
    const { error } = await supabase.auth.resetPasswordForEmail(email, {
      redirectTo: `${window.location.origin}/reset-password`,
    })
    
    return { error }
  },

  initialize: async () => {
    try {
      const { data: { session } } = await supabase.auth.getSession()
      
      if (session) {
        set({ user: session.user, session, loading: false })
      } else {
        set({ user: null, session: null, loading: false })
      }

      // Listen for auth changes
      supabase.auth.onAuthStateChange((event, session) => {
        if (session) {
          set({ user: session.user, session, loading: false })
        } else {
          set({ user: null, session: null, loading: false })
        }
      })
    } catch (error) {
      console.error('Error initializing auth:', error)
      set({ loading: false })
    }
  },
}))

// App state for alerts and movies
interface AppState {
  alertsByName: any[]
  alertsById: any[]
  movies: any[]
  selectedAlerts: number[]
  loadingAlerts: boolean
  loadingMovies: boolean
  setAlertsByName: (alerts: any[]) => void
  setAlertsById: (alerts: any[]) => void
  setMovies: (movies: any[]) => void
  addSelectedAlert: (id: number) => void
  removeSelectedAlert: (id: number) => void
  clearSelectedAlerts: () => void
  setLoadingAlerts: (loading: boolean) => void
  setLoadingMovies: (loading: boolean) => void
}

export const useAppStore = create<AppState>((set, get) => ({
  alertsByName: [],
  alertsById: [],
  movies: [],
  selectedAlerts: [],
  loadingAlerts: false,
  loadingMovies: false,

  setAlertsByName: (alerts) => set({ alertsByName: alerts }),
  setAlertsById: (alerts) => set({ alertsById: alerts }),
  setMovies: (movies) => set({ movies }),
  
  addSelectedAlert: (id) => {
    const current = get().selectedAlerts
    if (!current.includes(id)) {
      set({ selectedAlerts: [...current, id] })
    }
  },
  
  removeSelectedAlert: (id) => {
    const current = get().selectedAlerts
    set({ selectedAlerts: current.filter(alertId => alertId !== id) })
  },
  
  clearSelectedAlerts: () => set({ selectedAlerts: [] }),
  setLoadingAlerts: (loading) => set({ loadingAlerts: loading }),
  setLoadingMovies: (loading) => set({ loadingMovies: loading }),
}))
