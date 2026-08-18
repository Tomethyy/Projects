import type { ReactNode } from 'react'

type Props = { title?: string; children: ReactNode; className?: string }

export function Card({ title, children, className }: Props) {
  return (
    <section className={['shift-card', className].filter(Boolean).join(' ')}>
      {title && <h2>{title}</h2>}
      {children}
    </section>
  )
}
