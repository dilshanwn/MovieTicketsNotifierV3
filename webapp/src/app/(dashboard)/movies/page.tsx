'use client'

import { useEffect, useState } from 'react'
import { useAuthStore } from '@/lib/store'
import { movieAPI, alertAPI } from '@/lib/api'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Badge } from '@/components/ui/badge'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { 
  Search, 
  Filter, 
  Bell, 
  Calendar,
  Clock,
  Star,
  Loader2,
  AlertCircle
} from 'lucide-react'
import Image from 'next/image'

interface Movie {
  id: number
  title: string
  poster_url?: string
  genre?: string
  runtime?: number
  imdb_rating?: number
  synopsis?: string
  release_date?: string
  status: 'now_showing' | 'upcoming'
}

export default function MoviesPage() {
  const { user } = useAuthStore()
  const [movies, setMovies] = useState<Movie[]>([])
  const [loading, setLoading] = useState(true)
  const [searchTerm, setSearchTerm] = useState('')
  const [filterStatus, setFilterStatus] = useState<'all' | 'now_showing' | 'upcoming'>('all')
  const [error, setError] = useState('')

  useEffect(() => {
    loadMovies()
  }, [])
  const loadMovies = async () => {
    setLoading(true)
    try {
      const data = await movieAPI.getMovies()
      setMovies(data)
    } catch (err) {
      setError('Failed to load movies')
      console.error('Error loading movies:', err)
    } finally {
      setLoading(false)
    }
  }
  const createAlert = async (movie: Movie) => {
    if (!user?.email) return
    
    try {
      // Create alert by movie name (default approach)
      await alertAPI.createAlertByName({
        email: user.email,
        movie_name: movie.title,
        is_active: true
      })
      
      // Show success message (you could add a toast notification here)
      console.log('Alert created successfully for movie:', movie.title)
    } catch (err) {
      console.error('Error creating alert:', err)
      // Show error message (you could add a toast notification here)
    }
  }

  const filteredMovies = movies.filter(movie => {
    const matchesSearch = movie.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         movie.genre?.toLowerCase().includes(searchTerm.toLowerCase())
    const matchesFilter = filterStatus === 'all' || movie.status === filterStatus
    
    return matchesSearch && matchesFilter
  })

  if (loading) {
    return (
      <div className="space-y-6">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Movies</h1>
          <p className="text-muted-foreground">
            Browse movies and create alerts for ticket releases
          </p>
        </div>
        <div className="flex items-center justify-center py-12">
          <div className="text-center">
            <Loader2 className="h-8 w-8 animate-spin mx-auto mb-4" />
            <p className="text-muted-foreground">Loading movies...</p>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Movies</h1>
        <p className="text-muted-foreground">
          Browse movies and create alerts for ticket releases
        </p>
      </div>

      {/* Search and Filter */}
      <div className="flex flex-col sm:flex-row gap-4">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            placeholder="Search movies by title or genre..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="pl-10"
          />
        </div>
        
        <div className="flex gap-2">
          <Button
            variant={filterStatus === 'all' ? 'default' : 'outline'}
            onClick={() => setFilterStatus('all')}
            size="sm"
          >
            All
          </Button>
          <Button
            variant={filterStatus === 'now_showing' ? 'default' : 'outline'}
            onClick={() => setFilterStatus('now_showing')}
            size="sm"
          >
            Now Showing
          </Button>
          <Button
            variant={filterStatus === 'upcoming' ? 'default' : 'outline'}
            onClick={() => setFilterStatus('upcoming')}
            size="sm"
          >
            Upcoming
          </Button>
        </div>
      </div>

      {/* Error Alert */}
      {error && (
        <Alert variant="destructive">
          <AlertCircle className="h-4 w-4" />
          <AlertDescription>{error}</AlertDescription>
        </Alert>
      )}

      {/* Movies Grid */}
      {filteredMovies.length > 0 ? (
        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {filteredMovies.map((movie) => (
            <Card key={movie.id} className="overflow-hidden">
              <div className="relative aspect-[3/4]">
                <Image
                  src={movie.poster_url || '/api/placeholder/300/400'}
                  alt={movie.title}
                  fill
                  className="object-cover"
                  sizes="(max-width: 768px) 100vw, (max-width: 1200px) 50vw, 25vw"
                />
                <div className="absolute top-2 right-2">
                  <Badge variant={movie.status === 'now_showing' ? 'default' : 'secondary'}>
                    {movie.status === 'now_showing' ? 'Now Showing' : 'Coming Soon'}
                  </Badge>
                </div>
              </div>
              
              <CardHeader className="pb-3">
                <CardTitle className="text-lg leading-tight">{movie.title}</CardTitle>
                <CardDescription className="text-xs">
                  {movie.genre}
                </CardDescription>
              </CardHeader>
              
              <CardContent className="space-y-3">
                <div className="flex items-center gap-4 text-sm text-muted-foreground">
                  {movie.runtime && (
                    <div className="flex items-center gap-1">
                      <Clock className="h-3 w-3" />
                      {movie.runtime}m
                    </div>
                  )}
                  {movie.imdb_rating && (
                    <div className="flex items-center gap-1">
                      <Star className="h-3 w-3 fill-yellow-400 text-yellow-400" />
                      {movie.imdb_rating}
                    </div>
                  )}
                  {movie.release_date && (
                    <div className="flex items-center gap-1">
                      <Calendar className="h-3 w-3" />
                      {new Date(movie.release_date).getFullYear()}
                    </div>
                  )}
                </div>
                
                {movie.synopsis && (
                  <p className="text-sm text-muted-foreground line-clamp-3">
                    {movie.synopsis}
                  </p>
                )}
                
                <Button 
                  onClick={() => createAlert(movie)}
                  className="w-full"
                  size="sm"
                >
                  <Bell className="mr-2 h-4 w-4" />
                  Create Alert
                </Button>
              </CardContent>
            </Card>
          ))}
        </div>
      ) : (
        <div className="text-center py-12">
          <AlertCircle className="mx-auto h-12 w-12 text-muted-foreground mb-4" />
          <h3 className="text-lg font-medium mb-2">No movies found</h3>
          <p className="text-muted-foreground mb-4">
            {searchTerm ? 'Try adjusting your search terms' : 'No movies available at the moment'}
          </p>
          {searchTerm && (
            <Button onClick={() => setSearchTerm('')} variant="outline">
              Clear Search
            </Button>
          )}
        </div>
      )}

      {/* Info Alert */}
      <Alert>
        <Bell className="h-4 w-4" />
        <AlertDescription>
          <strong>Pro tip:</strong> Create alerts for movies you&apos;re interested in. 
          You&apos;ll receive email notifications when tickets become available at Scope Cinemas.
        </AlertDescription>
      </Alert>
    </div>
  )
}
