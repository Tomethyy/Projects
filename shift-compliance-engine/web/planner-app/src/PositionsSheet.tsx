import { useCallback, useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { ShiftApiClient } from '@shift-engine/api-client'

export type DeploymentPostRow = {
  id: string
  name: string
  windowStart: string
  windowEnd: string
  requiredHeadcount: number
  minRequiredFemale: number
  minRequiredMale: number
  isGenderIrrelevant: boolean
  requiredQualificationCode: string | null
  bufferPercent: number
}

type Props = {
  api: ShiftApiClient['api']
}

export default function PositionsSheet({ api }: Props) {
  const { t } = useTranslation()
  const [csvText, setCsvText] = useState('')
  const [rows, setRows] = useState<DeploymentPostRow[]>([])
  const [msg, setMsg] = useState<string | null>(null)
  const [err, setErr] = useState<string | null>(null)

  const reload = useCallback(async () => {
    setErr(null)
    const [csv, list] = await Promise.all([
      api<string>('/personnel/positions/export'),
      api<DeploymentPostRow[]>('/deployment/posts'),
    ])
    setCsvText(csv)
    setRows(list)
  }, [api])

  useEffect(() => {
    void reload().catch((ex) => setErr(ex instanceof Error ? ex.message : 'Load failed'))
  }, [reload])

  async function dryRun() {
    setErr(null)
    setMsg(null)
    try {
      const r = await api<{ valid: number; errors: { lineNumber: number; error: string }[] }>(
        '/personnel/positions/import/dry-run',
        {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ csvText }),
        },
      )
      setMsg(`${t('dryRunOk')}: ${r.valid} ${t('rowsValid')}`)
    } catch (ex) {
      setErr(ex instanceof Error ? ex.message : 'Dry run failed')
    }
  }

  async function importCsv() {
    setErr(null)
    setMsg(null)
    try {
      const r = await api<{ created: number }>('/personnel/positions/import', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ csvText, replaceAllPositions: true }),
      })
      setMsg(`${t('importDone')}: ${r.created} ${t('positions')}`)
      await reload()
    } catch (ex) {
      setErr(ex instanceof Error ? ex.message : 'Import failed')
    }
  }

  async function saveRow(row: DeploymentPostRow) {
    setErr(null)
    const payload = {
      ...row,
      windowStart: row.windowStart.length === 5 ? `${row.windowStart}:00` : row.windowStart,
      windowEnd: row.windowEnd.length === 5 ? `${row.windowEnd}:00` : row.windowEnd,
      minRequiredFemale: row.isGenderIrrelevant ? 0 : row.minRequiredFemale,
      minRequiredMale: row.isGenderIrrelevant ? 0 : row.minRequiredMale,
    }
    await api(`/deployment/posts/${row.id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    })
    setMsg(t('rowSaved'))
    await reload()
  }

  async function deleteRow(id: string) {
    setErr(null)
    await api(`/deployment/posts/${id}`, { method: 'DELETE' })
    await reload()
  }

  async function addRow() {
    setErr(null)
    await api<string>('/deployment/posts', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        name: t('newPost'),
        windowStart: '06:00:00',
        windowEnd: '14:00:00',
        requiredHeadcount: 1,
        minRequiredFemale: 0,
        minRequiredMale: 0,
        isGenderIrrelevant: true,
        bufferPercent: 0,
      }),
    })
    await reload()
  }

  return (
    <section className="card">
      <h2>{t('positionsSheet')}</h2>
      <p className="hint">{t('positionsSheetHint')}</p>
      {err && <p className="error">{err}</p>}
      {msg && <p className="hint">{msg}</p>}
      <textarea
        className="csv-editor"
        rows={6}
        value={csvText}
        onChange={(e) => setCsvText(e.target.value)}
        spellCheck={false}
      />
      <div className="row">
        <button type="button" onClick={() => void reload()}>
          {t('reloadFromDb')}
        </button>
        <button type="button" onClick={() => void dryRun()}>
          {t('dryRun')}
        </button>
        <button type="button" onClick={() => void importCsv()}>
          {t('importFile')}
        </button>
        <button type="button" onClick={() => void addRow()}>
          {t('addPost')}
        </button>
      </div>
      <div className="table-wrap">
        <table className="roster-table master-table">
          <thead>
            <tr>
              <th>{t('colPostName')}</th>
              <th>{t('colWindow')}</th>
              <th>{t('colHeadcount')}</th>
              <th>{t('colGenderIrrelevant')}</th>
              <th>{t('colMinFemale')}</th>
              <th>{t('colMinMale')}</th>
              <th>{t('colQual')}</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {rows.map((row) => (
              <PositionRowEditor
                key={row.id}
                row={row}
                onSave={saveRow}
                onDelete={deleteRow}
                t={t}
              />
            ))}
          </tbody>
        </table>
      </div>
    </section>
  )
}

function PositionRowEditor({
  row: initial,
  onSave,
  onDelete,
  t,
}: {
  row: DeploymentPostRow
  onSave: (r: DeploymentPostRow) => Promise<void>
  onDelete: (id: string) => Promise<void>
  t: (k: string) => string
}) {
  const [row, setRow] = useState(initial)
  useEffect(() => setRow(initial), [initial])
  const ws = row.windowStart.slice(0, 5)
  const we = row.windowEnd.slice(0, 5)

  return (
    <tr>
      <td>
        <input
          className="cell-input"
          value={row.name}
          onChange={(e) => setRow({ ...row, name: e.target.value })}
        />
      </td>
      <td>
        <div className="window-cell">
          <input
            className="cell-input narrow"
            value={ws}
            onChange={(e) => setRow({ ...row, windowStart: e.target.value })}
          />
          <span>–</span>
          <input
            className="cell-input narrow"
            value={we}
            onChange={(e) => setRow({ ...row, windowEnd: e.target.value })}
          />
        </div>
      </td>
      <td>
        <input
          className="cell-input narrow"
          type="number"
          min={1}
          value={row.requiredHeadcount}
          onChange={(e) => setRow({ ...row, requiredHeadcount: +e.target.value })}
        />
      </td>
      <td className="center-cell">
        <input
          type="checkbox"
          checked={row.isGenderIrrelevant}
          title={t('genderIrrelevantHint')}
          onChange={(e) =>
            setRow({
              ...row,
              isGenderIrrelevant: e.target.checked,
              minRequiredFemale: e.target.checked ? 0 : row.minRequiredFemale,
              minRequiredMale: e.target.checked ? 0 : row.minRequiredMale,
            })
          }
        />
      </td>
      <td>
        <input
          className="cell-input narrow"
          type="number"
          min={0}
          disabled={row.isGenderIrrelevant}
          value={row.minRequiredFemale}
          onChange={(e) => setRow({ ...row, minRequiredFemale: +e.target.value })}
        />
      </td>
      <td>
        <input
          className="cell-input narrow"
          type="number"
          min={0}
          disabled={row.isGenderIrrelevant}
          value={row.minRequiredMale}
          onChange={(e) => setRow({ ...row, minRequiredMale: +e.target.value })}
        />
      </td>
      <td>
        <input
          className="cell-input"
          value={row.requiredQualificationCode ?? ''}
          onChange={(e) => setRow({ ...row, requiredQualificationCode: e.target.value || null })}
        />
      </td>
      <td className="actions-cell">
        <button type="button" onClick={() => void onSave(row)}>
          {t('saveRow')}
        </button>
        <button type="button" className="danger-btn" onClick={() => void onDelete(row.id)}>
          {t('delete')}
        </button>
      </td>
    </tr>
  )
}
