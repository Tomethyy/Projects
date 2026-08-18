import { useCallback, useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { ShiftApiClient } from '@shift-engine/api-client'

export type EmployeeMaster = {
  id: string
  personnelNumber: string
  displayName: string
  contractedHoursMonthly: number
  genderCode: string | null
  primaryRole: string
  externalLegacyId: string | null
  isActive: boolean
}

type Props = {
  api: ShiftApiClient['api']
  onChanged: () => void
}

export default function PersonnelSheet({ api, onChanged }: Props) {
  const { t } = useTranslation()
  const [csvText, setCsvText] = useState('')
  const [rows, setRows] = useState<EmployeeMaster[]>([])
  const [msg, setMsg] = useState<string | null>(null)
  const [err, setErr] = useState<string | null>(null)

  const reload = useCallback(async () => {
    setErr(null)
    const [csv, list] = await Promise.all([
      api<string>('/personnel/export'),
      api<EmployeeMaster[]>('/employees'),
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
        '/personnel/import/dry-run',
        {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ csvText }),
        },
      )
      setMsg(`${t('dryRunOk')}: ${r.valid} ${t('rowsValid')}${r.errors?.length ? ` · ${r.errors.length} ${t('errors')}` : ''}`)
    } catch (ex) {
      setErr(ex instanceof Error ? ex.message : 'Dry run failed')
    }
  }

  async function importCsv() {
    setErr(null)
    setMsg(null)
    try {
      const r = await api<{ created: number; updated: number; deactivated: number }>('/personnel/import', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ csvText, deactivateMissing: false }),
      })
      setMsg(`${t('importDone')}: +${r.created} / ~${r.updated}`)
      await reload()
      onChanged()
    } catch (ex) {
      setErr(ex instanceof Error ? ex.message : 'Import failed')
    }
  }

  async function saveRow(row: EmployeeMaster) {
    setErr(null)
    try {
      await api(`/personnel/${row.id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(row),
      })
      setMsg(t('rowSaved'))
      await reload()
      onChanged()
    } catch (ex) {
      setErr(ex instanceof Error ? ex.message : 'Save failed')
    }
  }

  return (
    <section className="card">
      <h2>{t('personnelSheet')}</h2>
      <p className="hint">{t('personnelSheetHint')}</p>
      {err && <p className="error">{err}</p>}
      {msg && <p className="hint">{msg}</p>}
      <textarea
        className="csv-editor"
        rows={8}
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
      </div>
      <div className="table-wrap">
        <table className="roster-table master-table">
          <thead>
            <tr>
              <th>{t('colPersonnel')}</th>
              <th>{t('colEmployee')}</th>
              <th>{t('colHours')}</th>
              <th>{t('colGender')}</th>
              <th>{t('colRole')}</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {rows.map((row) => (
              <PersonnelRowEditor key={row.id} row={row} onSave={saveRow} t={t} />
            ))}
          </tbody>
        </table>
      </div>
    </section>
  )
}

function PersonnelRowEditor({
  row: initial,
  onSave,
  t,
}: {
  row: EmployeeMaster
  onSave: (r: EmployeeMaster) => Promise<void>
  t: (k: string) => string
}) {
  const [row, setRow] = useState(initial)
  useEffect(() => setRow(initial), [initial])

  return (
    <tr>
      <td>
        <input
          className="cell-input"
          value={row.personnelNumber}
          onChange={(e) => setRow({ ...row, personnelNumber: e.target.value })}
        />
      </td>
      <td>
        <input
          className="cell-input"
          value={row.displayName}
          onChange={(e) => setRow({ ...row, displayName: e.target.value })}
        />
      </td>
      <td>
        <input
          className="cell-input narrow"
          type="number"
          value={row.contractedHoursMonthly}
          onChange={(e) => setRow({ ...row, contractedHoursMonthly: +e.target.value })}
        />
      </td>
      <td>
        <input
          className="cell-input narrow"
          value={row.genderCode ?? ''}
          onChange={(e) => setRow({ ...row, genderCode: e.target.value || null })}
          placeholder="M/F/D/X"
        />
      </td>
      <td>
        <input
          className="cell-input"
          value={row.primaryRole}
          onChange={(e) => setRow({ ...row, primaryRole: e.target.value })}
        />
      </td>
      <td>
        <button type="button" onClick={() => void onSave(row)}>
          {t('saveRow')}
        </button>
      </td>
    </tr>
  )
}
