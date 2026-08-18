import type { ReactNode } from 'react'

type Tone = 'default' | 'draft' | 'published'

type Props = { tone?: Tone; children: ReactNode; className?: string }

const toneClass: Record<Tone, string> = {
  default: 'shift-badge',
  draft: 'shift-badge shift-badge-draft',
  published: 'shift-badge shift-badge-published',
}

export function Badge({ tone = 'default', children, className }: Props) {
  return <span className={[toneClass[tone], className].filter(Boolean).join(' ')}>{children}</span>
}
