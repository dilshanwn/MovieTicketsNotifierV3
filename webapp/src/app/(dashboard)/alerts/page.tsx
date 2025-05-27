'use client'

import { useEffect, useState } from 'react'
import { useAuthStore, useAppStore } from '@/lib/store'
import { supabase } from '@/lib/supabase'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Badge } from '@/components/ui/badge'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { 
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { 
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { 
  Search, 
  Trash2, 
  Edit3,
  AlertCircle,
  CheckCircle,
  Loader2,
  Calendar,
  Film,
  Hash
} from 'lucide-react'
import { toast } from 'sonner'

export default function AlertsPage() {
  const { user } = useAuthStore()
  const { 
    alertsByName, 
    alertsById, 
    setAlertsByName, 
    setAlertsById,
    selectedAlerts,
    addSelectedAlert,
    removeSelectedAlert,
    clearSelectedAlerts,
    loadingAlerts,
    setLoadingAlerts 
  } = useAppStore()

  const [searchTerm, setSearchTerm] = useState('')
  const [deleteDialog, setDeleteDialog] = useState<{
    open: boolean
    type: 'single' | 'bulk'
    alertId?: number
    alertType?: 'name' | 'id'
  }>({ open: false, type: 'single' })

  useEffect(() => {
    if (user) {
      loadUserAlerts()
    }
  }, [user])

  const loadUserAlerts = async () => {
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
      toast.error('Failed to load alerts')
    } finally {
      setLoadingAlerts(false)
    }
  }

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

      toast.success('Alert deleted successfully')
      loadUserAlerts()
    } catch (error) {
      console.error('Error deleting alert:', error)
      toast.error('Failed to delete alert')
    }
  }

  const deleteBulkAlerts = async () => {
    if (!user?.email || selectedAlerts.length === 0) return

    try {
      // Group selected alerts by type
      const nameAlertIds = selectedAlerts.filter(id => 
        alertsByName.some(alert => alert.id === id)
      )
      const idAlertIds = selectedAlerts.filter(id => 
        alertsById.some(alert => alert.id === id)
      )

      // Delete name alerts
      if (nameAlertIds.length > 0) {
        const { error: nameError } = await supabase
          .from('registered_alerts_by_name')
          .delete()
          .in('id', nameAlertIds)
          .eq('email', user.email)

        if (nameError) throw nameError
      }

      // Delete ID alerts
      if (idAlertIds.length > 0) {
        const { error: idError } = await supabase
          .from('registered_alerts_by_id')
          .delete()
          .in('id', idAlertIds)
          .eq('email', user.email)

        if (idError) throw idError
      }

      toast.success(`${selectedAlerts.length} alerts deleted successfully`)
      clearSelectedAlerts()
      loadUserAlerts()
    } catch (error) {
      console.error('Error deleting alerts:', error)
      toast.error('Failed to delete alerts')
    }
  }

  const toggleAlertStatus = async (type: 'name' | 'id', alertId: number, currentStatus: boolean) => {
    if (!user?.email) return

    const table = type === 'name' ? 'registered_alerts_by_name' : 'registered_alerts_by_id'
    
    try {
      const { error } = await supabase
        .from(table)
        .update({ is_active: !currentStatus })
        .eq('id', alertId)
        .eq('email', user.email)

      if (error) throw error

      toast.success(`Alert ${!currentStatus ? 'activated' : 'deactivated'}`)
      loadUserAlerts()
    } catch (error) {
      console.error('Error updating alert:', error)
      toast.error('Failed to update alert')
    }
  }

  const allAlerts = [
    ...alertsByName.map(alert => ({ ...alert, type: 'name' as const })),
    ...alertsById.map(alert => ({ ...alert, type: 'id' as const }))
  ].sort((a, b) => new Date(b.created_at).getTime() - new Date(a.created_at).getTime())

  const filteredAlerts = allAlerts.filter(alert => {
    const searchString = alert.movie_name || `Movie ID: ${alert.movie_id}`
    return searchString.toLowerCase().includes(searchTerm.toLowerCase())
  })

  const handleDeleteConfirm = () => {
    if (deleteDialog.type === 'single' && deleteDialog.alertId && deleteDialog.alertType) {
      deleteAlert(deleteDialog.alertType, deleteDialog.alertId)
    } else if (deleteDialog.type === 'bulk') {
      deleteBulkAlerts()
    }
    setDeleteDialog({ open: false, type: 'single' })
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">My Alerts</h1>
          <p className="text-muted-foreground">
            Manage your movie ticket notification alerts
          </p>
        </div>
        
        {selectedAlerts.length > 0 && (
          <Button
            variant="destructive"
            onClick={() => setDeleteDialog({ open: true, type: 'bulk' })}
            className="gap-2"
          >
            <Trash2 className="h-4 w-4" />
            Delete Selected ({selectedAlerts.length})
          </Button>
        )}
      </div>

      {/* Search */}
      <div className="relative max-w-md">
        <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <Input
          placeholder="Search alerts..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="pl-10"
        />
      </div>

      {/* Stats */}
      <div className="grid gap-4 md:grid-cols-3">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Alerts</CardTitle>
            <Film className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{allAlerts.length}</div>
          </CardContent>
        </Card>
        
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Active</CardTitle>
            <CheckCircle className="h-4 w-4 text-green-500" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {allAlerts.filter(alert => alert.is_active).length}
            </div>
          </CardContent>
        </Card>
        
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Inactive</CardTitle>
            <AlertCircle className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {allAlerts.filter(alert => !alert.is_active).length}
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Alerts Table */}
      {loadingAlerts ? (
        <div className="flex items-center justify-center py-12">
          <div className="text-center">
            <Loader2 className="h-8 w-8 animate-spin mx-auto mb-4" />
            <p className="text-muted-foreground">Loading alerts...</p>
          </div>
        </div>
      ) : filteredAlerts.length > 0 ? (
        <Card>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead className="w-12">
                  <input
                    type="checkbox"
                    checked={selectedAlerts.length === filteredAlerts.length && filteredAlerts.length > 0}
                    onChange={(e) => {
                      if (e.target.checked) {
                        filteredAlerts.forEach(alert => addSelectedAlert(alert.id))
                      } else {
                        clearSelectedAlerts()
                      }
                    }}
                    className="rounded"
                  />
                </TableHead>
                <TableHead>Movie</TableHead>
                <TableHead>Type</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Created</TableHead>
                <TableHead className="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {filteredAlerts.map((alert) => (
                <TableRow key={`${alert.type}-${alert.id}`}>
                  <TableCell>
                    <input
                      type="checkbox"
                      checked={selectedAlerts.includes(alert.id)}
                      onChange={(e) => {
                        if (e.target.checked) {
                          addSelectedAlert(alert.id)
                        } else {
                          removeSelectedAlert(alert.id)
                        }
                      }}
                      className="rounded"
                    />
                  </TableCell>
                  <TableCell className="font-medium">
                    <div className="flex items-center gap-2">
                      {alert.type === 'name' ? (
                        <Film className="h-4 w-4 text-muted-foreground" />
                      ) : (
                        <Hash className="h-4 w-4 text-muted-foreground" />
                      )}
                      {alert.movie_name || `Movie ID: ${alert.movie_id}`}
                    </div>
                  </TableCell>
                  <TableCell>
                    <Badge variant="outline">
                      {alert.type === 'name' ? 'By Name' : 'By ID'}
                    </Badge>
                  </TableCell>
                  <TableCell>
                    <Badge 
                      variant={alert.is_active ? "default" : "secondary"}
                      className="cursor-pointer"
                      onClick={() => toggleAlertStatus(alert.type, alert.id, alert.is_active)}
                    >
                      {alert.is_active ? "Active" : "Inactive"}
                    </Badge>
                  </TableCell>
                  <TableCell>
                    <div className="flex items-center gap-1 text-sm text-muted-foreground">
                      <Calendar className="h-3 w-3" />
                      {new Date(alert.created_at).toLocaleDateString()}
                    </div>
                  </TableCell>
                  <TableCell className="text-right">
                    <div className="flex items-center justify-end gap-2">
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => toggleAlertStatus(alert.type, alert.id, alert.is_active)}
                      >
                        <Edit3 className="h-3 w-3" />
                      </Button>
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => setDeleteDialog({
                          open: true,
                          type: 'single',
                          alertId: alert.id,
                          alertType: alert.type
                        })}
                      >
                        <Trash2 className="h-3 w-3" />
                      </Button>
                    </div>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </Card>
      ) : (
        <div className="text-center py-12">
          <AlertCircle className="mx-auto h-12 w-12 text-muted-foreground mb-4" />
          <h3 className="text-lg font-medium mb-2">No alerts found</h3>
          <p className="text-muted-foreground mb-4">
            {searchTerm ? 'No alerts match your search' : 'You haven&apos;t created any alerts yet'}
          </p>
          {searchTerm ? (
            <Button onClick={() => setSearchTerm('')} variant="outline">
              Clear Search
            </Button>
          ) : (
            <Button asChild>
              <a href="/movies">Browse Movies</a>
            </Button>
          )}
        </div>
      )}

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteDialog.open} onOpenChange={(open) => setDeleteDialog(prev => ({ ...prev, open }))}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              {deleteDialog.type === 'single' ? 'Delete Alert' : 'Delete Selected Alerts'}
            </DialogTitle>
            <DialogDescription>
              {deleteDialog.type === 'single' 
                ? 'Are you sure you want to delete this alert? This action cannot be undone.'
                : `Are you sure you want to delete ${selectedAlerts.length} selected alerts? This action cannot be undone.`
              }
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDeleteDialog({ open: false, type: 'single' })}>
              Cancel
            </Button>
            <Button variant="destructive" onClick={handleDeleteConfirm}>
              Delete
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Info Alert */}
      <Alert>
        <AlertCircle className="h-4 w-4" />
        <AlertDescription>
          <strong>Tip:</strong> Click on the status badge to quickly activate or deactivate an alert.
          Use the checkboxes to select multiple alerts for bulk operations.
        </AlertDescription>
      </Alert>
    </div>
  )
}
