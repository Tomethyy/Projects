import { useCallback, useEffect, useMemo, useRef, useState, type KeyboardEvent, type ReactNode } from 'react'
import { useVirtualizer } from '@tanstack/react-virtual'
import type { CellCoord, RosterMatrix, RosterMatrixCell, ShiftTier, SortKey, VirtualRosterGridLabels } from './types'

const STICKY_W = [72, 104, 56]
const ROW_H = 52
const DAY_W = 76

type Props = {
  matrix: RosterMatrix
  tiers: ShiftTier[]
  labels: VirtualRosterGridLabels
  onTierChange: (assignmentId: string, shiftTierId: string) => void | Promise<void>
  onBulkTierChange?: (updates: { assignmentId: string; shiftTierId: string }[]) => void | Promise<void>
  bulkToolbar?: (selectedCount: number, applyTier: (tierId: string) => void) => ReactNode
}

const cellKey = (employeeId: string, date: string) => `${employeeId}|${date}`

const shortPostLabel = (name: string) => {
  const parts = name.split(/\s+/)
  return parts.length > 1 ? parts.slice(1).join(' ').slice(0, 10) : name.slice(0, 10)
}

export function VirtualRosterGrid({
  matrix,
  tiers,
  labels,
  onTierChange,
  onBulkTierChange,
  bulkToolbar,
}: Props) {
  const parentRef = useRef<HTMLDivElement>(null)
  const [sortKey, setSortKey] = useState<SortKey>('personnelNumber')
  const [sortAsc, setSortAsc] = useState(true)
  const [focus, setFocus] = useState<CellCoord>({ row: 0, col: 0 })
  const [selected, setSelected] = useState<Set<string>>(() => new Set())

  const cellByKey = useMemo(() => {
    const m = new Map<string, RosterMatrixCell>()
    for (const c of matrix.cells) m.set(cellKey(c.employeeId, c.date), c)
    return m
  }, [matrix.cells])

  const employees = useMemo(() => {
    const rows = [...matrix.employees]
    rows.sort((a, b) => {
      const av = a[sortKey]
      const bv = b[sortKey]
      const cmp =
        typeof av === 'number' && typeof bv === 'number'
          ? av - bv
          : String(av).localeCompare(String(bv), undefined, { numeric: true })
      return sortAsc ? cmp : -cmp
    })
    return rows
  }, [matrix.employees, sortKey, sortAsc])

  const rowVirtualizer = useVirtualizer({
    count: employees.length,
    getScrollElement: () => parentRef.current,
    estimateSize: () => ROW_H,
    overscan: 10,
  })

  const virtualRows = rowVirtualizer.getVirtualItems()
  const colSpan = matrix.days.length + 3

  const toggleSort = (key: SortKey) => {
    if (sortKey === key) setSortAsc((v) => !v)
    else {
      setSortKey(key)
      setSortAsc(true)
    }
  }

  const sortMark = (key: SortKey) => (sortKey === key ? (sortAsc ? ' ▲' : ' ▼') : '')

  const toggleSelect = useCallback(
    (row: number, col: number) => {
      const emp = employees[row]
      const date = matrix.days[col]
      const cell = cellByKey.get(cellKey(emp.id, date))
      if (!cell?.assignmentId) return
      const key = cellKey(emp.id, date)
      setSelected((prev) => {
        const next = new Set(prev)
        if (next.has(key)) next.delete(key)
        else next.add(key)
        return next
      })
    },
    [cellByKey, employees, matrix.days],
  )

  const applyBulkTier = useCallback(
    (tierId: string) => {
      if (!onBulkTierChange || selected.size === 0) return
      const updates = [...selected]
        .map((key) => {
          const cell = matrix.cells.find((c) => cellKey(c.employeeId, c.date) === key)
          return cell?.assignmentId ? { assignmentId: cell.assignmentId, shiftTierId: tierId } : null
        })
        .filter((u): u is { assignmentId: string; shiftTierId: string } => u !== null)
      if (updates.length > 0) void onBulkTierChange(updates)
      setSelected(new Set())
    },
    [matrix.cells, onBulkTierChange, selected],
  )

  const onKeyDown = (e: KeyboardEvent<HTMLDivElement>) => {
    const maxRow = employees.length - 1
    const maxCol = matrix.days.length - 1
    if (e.key === 'ArrowDown') {
      e.preventDefault()
      setFocus((f) => ({ ...f, row: Math.min(maxRow, f.row + 1) }))
    } else if (e.key === 'ArrowUp') {
      e.preventDefault()
      setFocus((f) => ({ ...f, row: Math.max(0, f.row - 1) }))
    } else if (e.key === 'ArrowRight') {
      e.preventDefault()
      setFocus((f) => ({ ...f, col: Math.min(maxCol, f.col + 1) }))
    } else if (e.key === 'ArrowLeft') {
      e.preventDefault()
      setFocus((f) => ({ ...f, col: Math.max(0, f.col - 1) }))
    } else if (e.key === ' ') {
      e.preventDefault()
      toggleSelect(focus.row, focus.col)
    }
  }

  useEffect(() => {
    rowVirtualizer.scrollToIndex(focus.row, { align: 'auto' })
  }, [focus.row, rowVirtualizer])

  const tableWidth = STICKY_W[0] + STICKY_W[1] + STICKY_W[2] + matrix.days.length * DAY_W
  const paddingTop = virtualRows.length > 0 ? virtualRows[0].start : 0
  const paddingBottom =
    virtualRows.length > 0 ? rowVirtualizer.getTotalSize() - virtualRows[virtualRows.length - 1].end : 0

  return (
    <div className="shift-roster-wrap">
      <p className="shift-roster-meta">
        {matrix.periodName} · {matrix.isPublished ? labels.published : labels.draft}
      </p>
      {selected.size > 0 && (
        <div className="shift-roster-bulk">
          {bulkToolbar ? (
            bulkToolbar(selected.size, applyBulkTier)
          ) : (
            <>
              <span>{selected.size} selected</span>
              <select defaultValue="" onChange={(ev) => ev.target.value && applyBulkTier(ev.target.value)}>
                <option value="">Tier…</option>
                {tiers.map((tier) => (
                  <option key={tier.id} value={tier.id}>
                    {tier.code}
                  </option>
                ))}
              </select>
            </>
          )}
        </div>
      )}
      <div ref={parentRef} className="shift-roster-scroll" tabIndex={0} onKeyDown={onKeyDown} aria-label="Roster grid">
        <table className="shift-roster-table" style={{ width: tableWidth }}>
          <thead>
            <tr>
              <th className="shift-roster-sticky" style={{ width: STICKY_W[0], minWidth: STICKY_W[0] }}>
                <button type="button" className="shift-roster-sort" onClick={() => toggleSort('personnelNumber')}>
                  {labels.colPersonnel}
                  {sortMark('personnelNumber')}
                </button>
              </th>
              <th className="shift-roster-sticky shift-roster-sticky-2" style={{ width: STICKY_W[1], minWidth: STICKY_W[1] }}>
                <button type="button" className="shift-roster-sort" onClick={() => toggleSort('displayName')}>
                  {labels.colEmployee}
                  {sortMark('displayName')}
                </button>
              </th>
              <th className="shift-roster-sticky shift-roster-sticky-3" style={{ width: STICKY_W[2], minWidth: STICKY_W[2] }}>
                <button type="button" className="shift-roster-sort" onClick={() => toggleSort('contractedHoursMonthly')}>
                  {labels.colHours}
                  {sortMark('contractedHoursMonthly')}
                </button>
              </th>
              {matrix.days.map((d) => (
                <th key={d} className="shift-roster-day" title={d} style={{ width: DAY_W, minWidth: DAY_W }}>
                  {d.slice(8)}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {paddingTop > 0 && (
              <tr aria-hidden style={{ height: paddingTop }}>
                <td colSpan={colSpan} />
              </tr>
            )}
            {virtualRows.map((vRow) => {
              const emp = employees[vRow.index]
              return (
                <tr key={emp.id} style={{ height: ROW_H }}>
                  <td className="shift-roster-sticky" style={{ width: STICKY_W[0] }}>
                    {emp.personnelNumber}
                  </td>
                  <td className="shift-roster-sticky shift-roster-sticky-2" style={{ width: STICKY_W[1] }}>
                    {emp.displayName}
                  </td>
                  <td className="shift-roster-sticky shift-roster-sticky-3" style={{ width: STICKY_W[2] }}>
                    {emp.contractedHoursMonthly}
                  </td>
                  {matrix.days.map((d, colIdx) => {
                    const cell = cellByKey.get(cellKey(emp.id, d))
                    const isFocused = focus.row === vRow.index && focus.col === colIdx
                    const isSelected = selected.has(cellKey(emp.id, d))
                    const classes = ['shift-roster-cell', isFocused ? 'focused' : '', isSelected ? 'selected' : '']
                      .filter(Boolean)
                      .join(' ')

                    if (!cell?.assignmentId) {
                      return (
                        <td
                          key={d}
                          className={`${classes} shift-roster-off`}
                          style={{ width: DAY_W }}
                          onClick={() => setFocus({ row: vRow.index, col: colIdx })}
                        >
                          —
                        </td>
                      )
                    }

                    return (
                      <td
                        key={d}
                        className={classes}
                        style={{ width: DAY_W }}
                        title={cell.postName ?? undefined}
                        onClick={(ev) => {
                          setFocus({ row: vRow.index, col: colIdx })
                          if (ev.shiftKey) toggleSelect(vRow.index, colIdx)
                        }}
                      >
                        <select
                          className="shift-roster-tier"
                          value={cell.shiftTierId ?? ''}
                          onChange={(ev) => void onTierChange(cell.assignmentId!, ev.target.value)}
                        >
                          {tiers.map((tier) => (
                            <option key={tier.id} value={tier.id}>
                              {tier.code}
                            </option>
                          ))}
                        </select>
                        <span className="shift-roster-post">{cell.postName ? shortPostLabel(cell.postName) : '·'}</span>
                      </td>
                    )
                  })}
                </tr>
              )
            })}
            {paddingBottom > 0 && (
              <tr aria-hidden style={{ height: paddingBottom }}>
                <td colSpan={colSpan} />
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  )
}
