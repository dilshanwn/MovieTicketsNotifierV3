'use client'

import { useState } from 'react'
import Link from 'next/link'
import { usePathname } from 'next/navigation'
import { useAuthStore } from '@/lib/store'
import { Button } from '@/components/ui/button'
import { 
  LayoutDashboard, 
  Film, 
  Bell, 
  LogOut, 
  Menu, 
  X,
  User
} from 'lucide-react'
import { cn } from '@/lib/utils'

const navigation = [
  { name: 'Dashboard', href: '/dashboard', icon: LayoutDashboard },
  { name: 'Movies', href: '/movies', icon: Film },
  { name: 'My Alerts', href: '/alerts', icon: Bell },
]

export function Navigation() {
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false)
  const pathname = usePathname()
  const { user, signOut } = useAuthStore()

  const handleSignOut = async () => {
    await signOut()
  }

  return (
    <>
      {/* Desktop Sidebar */}
      <div className="hidden lg:fixed lg:inset-y-0 lg:z-50 lg:flex lg:w-72 lg:flex-col">
        <div className="flex grow flex-col gap-y-5 overflow-y-auto border-r border-border bg-background px-6 py-4">
          {/* Logo */}
          <div className="flex h-16 shrink-0 items-center">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-primary p-2">
                <Film className="h-6 w-6 text-primary-foreground" />
              </div>
              <div>
                <h1 className="font-semibold text-lg">Scope Notifier</h1>
                <p className="text-xs text-muted-foreground">v3.0</p>
              </div>
            </div>
          </div>

          {/* Navigation */}
          <nav className="flex flex-1 flex-col">
            <ul role="list" className="flex flex-1 flex-col gap-y-7">
              <li>
                <ul role="list" className="-mx-2 space-y-1">
                  {navigation.map((item) => {
                    const isActive = pathname === item.href
                    return (
                      <li key={item.name}>
                        <Link
                          href={item.href}
                          className={cn(
                            isActive
                              ? 'bg-primary text-primary-foreground'
                              : 'text-foreground hover:bg-muted',
                            'group flex gap-x-3 rounded-md p-2 text-sm font-medium leading-6 transition-colors'
                          )}
                        >
                          <item.icon className="h-5 w-5 shrink-0" />
                          {item.name}
                        </Link>
                      </li>
                    )
                  })}
                </ul>
              </li>

              {/* User Section */}
              <li className="mt-auto">
                <div className="flex items-center gap-x-4 px-2 py-3 text-sm font-medium leading-6">
                  <div className="flex h-8 w-8 items-center justify-center rounded-full bg-muted">
                    <User className="h-4 w-4" />
                  </div>
                  <div className="flex-1">
                    <p className="truncate text-sm font-medium">
                      {user?.email}
                    </p>
                  </div>
                </div>
                <Button
                  variant="ghost"
                  onClick={handleSignOut}
                  className="w-full justify-start gap-x-3"
                >
                  <LogOut className="h-5 w-5" />
                  Sign Out
                </Button>
              </li>
            </ul>
          </nav>
        </div>
      </div>

      {/* Mobile Menu */}
      <div className="lg:hidden">
        <div className="sticky top-0 z-40 flex h-16 shrink-0 items-center gap-x-4 border-b border-border bg-background px-4 sm:gap-x-6 sm:px-6">
          <button
            type="button"
            className="-m-2.5 p-2.5 text-foreground lg:hidden"
            onClick={() => setMobileMenuOpen(true)}
          >
            <Menu className="h-6 w-6" />
          </button>

          <div className="flex flex-1 gap-x-4 self-stretch lg:gap-x-6">
            <div className="flex items-center gap-2">
              <Film className="h-6 w-6 text-primary" />
              <span className="font-semibold">Scope Notifier</span>
            </div>
          </div>

          <Button
            variant="ghost"
            size="sm"
            onClick={handleSignOut}
            className="gap-2"
          >
            <LogOut className="h-4 w-4" />
            <span className="sr-only">Sign Out</span>
          </Button>
        </div>

        {/* Mobile sliding menu */}
        {mobileMenuOpen && (
          <div className="relative z-50 lg:hidden">
            <div className="fixed inset-0 bg-background/80 backdrop-blur-sm" />
            <div className="fixed inset-0 flex">
              <div className="relative mr-16 flex w-full max-w-xs flex-1">
                <div className="absolute left-full top-0 flex w-16 justify-center pt-5">
                  <button
                    type="button"
                    className="-m-2.5 p-2.5"
                    onClick={() => setMobileMenuOpen(false)}
                  >
                    <X className="h-6 w-6 text-foreground" />
                  </button>
                </div>

                <div className="flex grow flex-col gap-y-5 overflow-y-auto bg-background px-6 pb-2 ring-1 ring-border">
                  <div className="flex h-16 shrink-0 items-center">
                    <div className="flex items-center gap-3">
                      <div className="rounded-lg bg-primary p-2">
                        <Film className="h-6 w-6 text-primary-foreground" />
                      </div>
                      <div>
                        <h1 className="font-semibold text-lg">Scope Notifier</h1>
                        <p className="text-xs text-muted-foreground">v3.0</p>
                      </div>
                    </div>
                  </div>
                  <nav className="flex flex-1 flex-col">
                    <ul role="list" className="flex flex-1 flex-col gap-y-7">
                      <li>
                        <ul role="list" className="-mx-2 space-y-1">
                          {navigation.map((item) => {
                            const isActive = pathname === item.href
                            return (
                              <li key={item.name}>
                                <Link
                                  href={item.href}
                                  onClick={() => setMobileMenuOpen(false)}
                                  className={cn(
                                    isActive
                                      ? 'bg-primary text-primary-foreground'
                                      : 'text-foreground hover:bg-muted',
                                    'group flex gap-x-3 rounded-md p-2 text-sm font-medium leading-6 transition-colors'
                                  )}
                                >
                                  <item.icon className="h-5 w-5 shrink-0" />
                                  {item.name}
                                </Link>
                              </li>
                            )
                          })}
                        </ul>
                      </li>
                    </ul>
                  </nav>
                </div>
              </div>
            </div>
          </div>
        )}
      </div>
    </>
  )
}
