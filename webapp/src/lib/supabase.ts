import { createClient } from '@supabase/supabase-js'

const supabaseUrl = process.env.NEXT_PUBLIC_SUPABASE_URL!
const supabaseAnonKey = process.env.NEXT_PUBLIC_SUPABASE_ANON_KEY!

export const supabase = createClient(supabaseUrl, supabaseAnonKey, {
  auth: {
    autoRefreshToken: true,
    persistSession: true,
    detectSessionInUrl: true
  }
})

// Types for our database tables
export interface RegisteredAlertByName {
  id: number
  email: string
  movie_name: string
  is_active: boolean
  created_at: string
  updated_at: string
}

export interface RegisteredAlertById {
  id: number
  email: string
  movie_id: number
  is_active: boolean
  created_at: string
  updated_at: string
}

export interface Movie {
  id: number
  title: string
  poster_url?: string
  genre?: string
  runtime?: number
  imdb_rating?: number
  synopsis?: string
  cast?: any[]
  trailer_url?: string
  release_date?: string
  status: 'now_showing' | 'upcoming'
}
