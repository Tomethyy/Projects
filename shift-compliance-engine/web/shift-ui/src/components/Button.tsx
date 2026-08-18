import type { ButtonHTMLAttributes, ReactNode } from 'react'

type Variant = 'default' | 'primary' | 'danger'

type Props = ButtonHTMLAttributes<HTMLButtonElement> & {
  variant?: Variant
  children: ReactNode
}

const variantClass: Record<Variant, string> = {
  default: 'shift-btn',
  primary: 'shift-btn shift-btn-primary',
  danger: 'shift-btn shift-btn-danger',
}

export function Button({ variant = 'default', className, children, ...rest }: Props) {
  return (
    <button type="button" className={[variantClass[variant], className].filter(Boolean).join(' ')} {...rest}>
      {children}
    </button>
  )
}
