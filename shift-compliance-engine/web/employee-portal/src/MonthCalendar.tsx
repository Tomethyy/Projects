import { useMemo } from 'react'
import { useTranslation } from 'react-i18next'

type Props = {
  year: number
  month: number
  shiftDates: Set<string>
  selectedDate: string | null
  onSelect: (date: string) => void
}

export default function MonthCalendar({ year, month, shiftDates, selectedDate, onSelect }: Props) {
  const { t, i18n } = useTranslation()
  const locale = i18n.language === 'de' ? 'de-DE' : 'en-US'

  const weeks = useMemo(() => {
    const first = new Date(year, month - 1, 1)
    const startPad = (first.getDay() + 6) % 7
    const daysInMonth = new Date(year, month, 0).getDate()
    const cells: (string | null)[] = [...Array(startPad).fill(null), ...Array.from({ length: daysInMonth }, (_, i) => {
      const d = i + 1
      return `${year}-${String(month).padStart(2, '0')}-${String(d).padStart(2, '0')}`
    })]
    return Array.from({ length: Math.ceil(cells.length / 7) }, (_, w) => cells.slice(w * 7, w * 7 + 7))
  }, [month, year])

  const monthLabel = new Date(year, month - 1, 1).toLocaleDateString(locale, { month: 'long', year: 'numeric' })
  const weekdayLabels = useMemo(() => {
    const base = new Date(2024, 0, 1)
    return Array.from({ length: 7 }, (_, i) =>
      new Date(base.getFullYear(), base.getMonth(), base.getDate() + i).toLocaleDateString(locale, { weekday: 'short' }),
    )
  }, [locale])

  return (
    <div className="portal-calendar" aria-label={monthLabel}>
      <div className="portal-calendar-title">{monthLabel}</div>
      <div className="portal-calendar-grid">
        {weekdayLabels.map((label) => (
          <div key={label} className="portal-calendar-weekday">
            {label}
          </div>
        ))}
        {weeks.flat().map((date, i) =>
          date ? (
            <button
              key={date}
              type="button"
              className={[
                'portal-calendar-day',
                shiftDates.has(date) ? 'has-shift' : '',
                selectedDate === date ? 'selected' : '',
              ]
                .filter(Boolean)
                .join(' ')}
              onClick={() => onSelect(date)}
              aria-label={t('selectDay', { date })}
            >
              {+date.slice(8, 10)}
            </button>
          ) : (
            <span key={`pad-${i}`} className="portal-calendar-pad" />
          ),
        )}
      </div>
    </div>
  )
}
