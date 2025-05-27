import axios from 'axios'

const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:7071/api'

// Create axios instance with default config
const api = axios.create({
  baseURL: API_BASE_URL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
})

// Request interceptor to add authentication
api.interceptors.request.use(
  (config) => {
    // Add any auth headers here if needed
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// Response interceptor for error handling
api.interceptors.response.use(
  (response) => response,
  (error) => {
    console.error('API Error:', error)
    return Promise.reject(error)
  }
)

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

export interface RegisterAlertRequest {
  email: string
  movie_name?: string
  movie_id?: number
  is_active: boolean
}

export interface AlertResponse {
  id: number
  email: string
  movie_name?: string
  movie_id?: number
  is_active: boolean
  created_at: string
  updated_at: string
}

// Movie API endpoints
export const movieAPI = {
  // Get all movies (now showing + upcoming)
  getMovies: async (): Promise<Movie[]> => {
    const response = await api.get('/movies')
    return response.data
  },

  // Get now showing movies
  getNowShowingMovies: async (): Promise<Movie[]> => {
    const response = await api.get('/movies/now-showing')
    return response.data
  },

  // Get upcoming movies
  getUpcomingMovies: async (): Promise<Movie[]> => {
    const response = await api.get('/movies/upcoming')
    return response.data
  },

  // Get movie by ID
  getMovieById: async (id: number): Promise<Movie> => {
    const response = await api.get(`/movies/${id}`)
    return response.data
  },

  // Search movies
  searchMovies: async (query: string): Promise<Movie[]> => {
    const response = await api.get(`/movies/search?q=${encodeURIComponent(query)}`)
    return response.data
  },
}

// Alert API endpoints
export const alertAPI = {
  // Create alert by movie name
  createAlertByName: async (request: RegisterAlertRequest): Promise<AlertResponse> => {
    const response = await api.post('/alerts/by-name', request)
    return response.data
  },

  // Create alert by movie ID
  createAlertById: async (request: RegisterAlertRequest): Promise<AlertResponse> => {
    const response = await api.post('/alerts/by-id', request)
    return response.data
  },

  // Get user alerts by name
  getUserAlertsByName: async (email: string): Promise<AlertResponse[]> => {
    const response = await api.get(`/alerts/by-name/${encodeURIComponent(email)}`)
    return response.data
  },

  // Get user alerts by ID
  getUserAlertsById: async (email: string): Promise<AlertResponse[]> => {
    const response = await api.get(`/alerts/by-id/${encodeURIComponent(email)}`)
    return response.data
  },

  // Update alert
  updateAlert: async (type: 'name' | 'id', alertId: number, updates: Partial<AlertResponse>): Promise<AlertResponse> => {
    const endpoint = type === 'name' ? '/alerts/by-name' : '/alerts/by-id'
    const response = await api.put(`${endpoint}/${alertId}`, updates)
    return response.data
  },

  // Delete alert
  deleteAlert: async (type: 'name' | 'id', alertId: number): Promise<void> => {
    const endpoint = type === 'name' ? '/alerts/by-name' : '/alerts/by-id'
    await api.delete(`${endpoint}/${alertId}`)
  },

  // Delete multiple alerts
  deleteMultipleAlerts: async (type: 'name' | 'id', alertIds: number[]): Promise<void> => {
    const endpoint = type === 'name' ? '/alerts/by-name/bulk' : '/alerts/by-id/bulk'
    await api.delete(endpoint, { data: { ids: alertIds } })
  },
}

// Health check endpoint
export const healthAPI = {
  checkHealth: async (): Promise<{ status: string; version: string }> => {
    const response = await api.get('/health')
    return response.data
  },
}

export default api
