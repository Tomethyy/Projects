import type { ReactNode } from 'react'

type Props = { title: string; subtitle?: string; children?: ReactNode }

export function PageHeader({ title, subtitle, children }: Props) {
  return (
    <header className="shift-page-header">
      <div>
        <h1>{title}</h1>
        {subtitle && <p>{subtitle}</p>}
      </div>
      {children}
    </header>
  )
}
