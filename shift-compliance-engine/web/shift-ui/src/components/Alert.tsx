type Props = { children: string; className?: string }

export function Alert({ children, className }: Props) {
  return <div className={['shift-alert', className].filter(Boolean).join(' ')}>{children}</div>
}
