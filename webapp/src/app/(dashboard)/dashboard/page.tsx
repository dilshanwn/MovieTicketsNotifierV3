'use client'

import { useEffect, useState, useCallback } from 'react'
import { useAuthStore, useAppStore } from '@/lib/store'
import { supabase } from '@/lib/supabase'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { 
  Bell, 
  Film, 
  Users, 
  TrendingUp, 
  AlertCircle,
  Plus,
  Eye,
  Trash2
} from 'lucide-react'
import Link from 'next/link'

export default function DashboardPage() {
  const { user } = useAuthStore()
  const { 
    alertsByName, 
    alertsById, 
    setAlertsByName, 
    setAlertsById,
    loadingAlerts,
    setLoadingAlerts 
  } = useAppStore()
  const [stats, setStats] = useState({
    totalAlerts: 0,
    activeAlerts: 0,
    alertsByName: 0,
    alertsById: 0
  })
  const loadUserAlerts = useCallback(async () => {
    if (!user?.email) return
    
    setLoadingAlerts(true)
    try {
      // Load alerts by name
      const { data: nameAlerts, error: nameError } = await supabase
        .from('registered_alerts_by_name')
        .select('*')
        .eq('email', user.email)
        .order('created_at', { ascending: false })

      if (nameError) throw nameError

      // Load alerts by ID
      const { data: idAlerts, error: idError } = await supabase
        .from('registered_alerts_by_id')
        .select('*')
        .eq('email', user.email)
        .order('created_at', { ascending: false })

      if (idError) throw idError

      setAlertsByName(nameAlerts || [])
      setAlertsById(idAlerts || [])
    } catch (error) {
      console.error('Error loading alerts:', error)
    } finally {
      setLoadingAlerts(false)
    }
  }, [user?.email, setAlertsByName, setAlertsById, setLoadingAlerts])
  useEffect(() => {
    if (user) {
      loadUserAlerts()
    }
  }, [user, loadUserAlerts])

  useEffect(() => {
    const totalAlerts = alertsByName.length + alertsById.length
    const activeAlerts = [...alertsByName, ...alertsById].filter(alert => alert.is_active).length
    
    setStats({
      totalAlerts,
      activeAlerts,
      alertsByName: alertsByName.length,
      alertsById: alertsById.length
    })
  }, [alertsByName, alertsById])

  const deleteAlert = async (type: 'name' | 'id', alertId: number) => {
    if (!user?.email) return

    const table = type === 'name' ? 'registered_alerts_by_name' : 'registered_alerts_by_id'
    
    try {
      const { error } = await supabase
        .from(table)
        .delete()
        .eq('id', alertId)
        .eq('email', user.email)

      if (error) throw error

      // Refresh alerts
      loadUserAlerts()
    } catch (error) {
      console.error('Error deleting alert:', error)
    }
  }

  const recentAlerts = [...alertsByName, ...alertsById]
    .sort((a, b) => new Date(b.created_at).getTime() - new Date(a.created_at).getTime())
    .slice(0, 5)

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Dashboard</h1>
        <p className="text-muted-foreground">
          Welcome back! Here&apos;s an overview of your movie alerts.
        </p>
      </div>

      {/* Stats Cards */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Alerts</CardTitle>
            <Bell className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.totalAlerts}</div>
            <p className="text-xs text-muted-foreground">
              Active: {stats.activeAlerts}
            </p>
          </CardContent>
        </Card>
        
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">By Name</CardTitle>
            <Film className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.alertsByName}</div>
            <p className="text-xs text-muted-foreground">
              Movie name alerts
            </p>
          </CardContent>
        </Card>
        
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">By ID</CardTitle>
            <Users className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.alertsById}</div>
            <p className="text-xs text-muted-foreground">
              Movie ID alerts
            </p>
          </CardContent>
        </Card>
        
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Success Rate</CardTitle>
            <TrendingUp className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">98%</div>
            <p className="text-xs text-muted-foreground">
              Notification delivery
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Quick Actions */}
      <div className="grid gap-4 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Quick Actions</CardTitle>
            <CardDescription>
              Common tasks to manage your alerts
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-2">
            <Button asChild className="w-full justify-start">
              <Link href="/movies">
                <Plus className="mr-2 h-4 w-4" />
                Browse Movies & Create Alert
              </Link>
            </Button>
            <Button asChild variant="outline" className="w-full justify-start">
              <Link href="/alerts">
                <Eye className="mr-2 h-4 w-4" />
                View All My Alerts
              </Link>
            </Button>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Recent Alerts</CardTitle>
            <CardDescription>
              Your most recently created alerts
            </CardDescription>
          </CardHeader>
          <CardContent>
            {loadingAlerts ? (
              <p className="text-sm text-muted-foreground">Loading alerts...</p>
            ) : recentAlerts.length > 0 ? (
              <div className="space-y-3">
                {recentAlerts.map((alert) => (
                  <div 
                    key={`${alert.movie_name || alert.movie_id}-${alert.id}`}
                    className="flex items-center justify-between p-2 rounded-lg border"
                  >
                    <div className="flex-1">
                      <p className="font-medium text-sm">
                        {alert.movie_name || `Movie ID: ${alert.movie_id}`}
                      </p>
                      <div className="flex items-center gap-2 mt-1">
                        <Badge 
                          variant={alert.is_active ? "default" : "secondary"}
                          className="text-xs"
                        >
                          {alert.is_active ? "Active" : "Inactive"}
                        </Badge>
                        <span className="text-xs text-muted-foreground">
                          {new Date(alert.created_at).toLocaleDateString()}
                        </span>
                      </div>
                    </div>
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => deleteAlert(
                        alert.movie_name ? 'name' : 'id', 
                        alert.id
                      )}
                    >
                      <Trash2 className="h-3 w-3" />
                    </Button>
                  </div>
                ))}
              </div>
            ) : (
              <div className="text-center py-4">
                <AlertCircle className="mx-auto h-8 w-8 text-muted-foreground mb-2" />
                <p className="text-sm text-muted-foreground">
                  No alerts created yet
                </p>
                <Button asChild size="sm" className="mt-2">
                  <Link href="/movies">Create Your First Alert</Link>
                </Button>
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Info Alert */}
      <Alert>
        <AlertCircle className="h-4 w-4" />
        <AlertDescription>
          <strong>How it works:</strong> Create alerts for movies you want to watch. 
          We&apos;ll notify you via email when tickets become available at Scope Cinemas.
        </AlertDescription>
      </Alert>
    </div>
  )
}
