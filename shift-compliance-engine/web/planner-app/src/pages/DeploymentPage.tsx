import { useCallback, useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useApi } from '../context/ApiProvider'

type GridCell = {
  postId: string
  postName: string
  date: string
  requiredHeadcount: number
  assigned: { personnelNumber: string; displayName: string; tierCode: string }[]
}

type Grid = {
  periodId: string
  year: number
  month: number
  isPublished: boolean
  days: string[]
  cells: GridCell[]
}

export default function DeploymentPage() {
  const { api } = useApi()
  const { t } = useTranslation()
  const now = new Date()
  const [year, setYear] = useState(now.getFullYear())
  const [month, setMonth] = useState(now.getMonth() + 1)
  const [grid, setGrid] = useState<Grid | null>(null)
  const [err, setErr] = useState<string | null>(null)

  const load = useCallback(async () => {
    setErr(null)
    try {
      const g = await api<Grid>(`/roster/deployment-grid?year=${year}&month=${month}`)
      setGrid(g)
    } catch (ex) {
      setGrid(null)
      setErr(ex instanceof Error ? ex.message : 'Load failed')
    }
  }, [api, year, month])

  useEffect(() => {
    void load()
  }, [load])

  const posts = grid ? [...new Map(grid.cells.map((c) => [c.postId, c.postName])).entries()] : []

  return (
    <>
      <header className="page-header">
        <h1>{t('navDeployment')}</h1>
        <p>{t('deploymentHint')}</p>
      </header>
      {err && <div className="alert alert-error">{err}</div>}
      <section className="card">
        <div className="field-grid">
          <label className="field">
            {t('year')}
            <input type="number" value={year} onChange={(e) => setYear(+e.target.value)} />
          </label>
          <label className="field">
            {t('month')}
            <input type="number" min={1} max={12} value={month} onChange={(e) => setMonth(+e.target.value)} />
          </label>
        </div>
        <button type="button" className="btn" style={{ marginTop: '0.75rem' }} onClick={() => void load()}>
          {t('refreshGrid')}
        </button>
      </section>
      {grid && (
        <section className="card">
          <h2>
            {grid.year}-{String(grid.month).padStart(2, '0')}{' '}
            <span className={`badge ${grid.isPublished ? 'badge-published' : 'badge-draft'}`}>
              {grid.isPublished ? t('published') : t('draft')}
            </span>
          </h2>
          <div className="table-wrap">
            <table className="data-table">
              <thead>
                <tr>
                  <th>{t('colPostName')}</th>
                  {grid.days.map((d) => (
                    <th key={d}>{d.slice(8)}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {posts.map(([postId, postName]) => (
                  <tr key={postId}>
                    <td>{postName}</td>
                    {grid.days.map((d) => {
                      const cell = grid.cells.find((c) => c.postId === postId && c.date === d)
                      const filled = cell?.assigned.length ?? 0
                      const req = cell?.requiredHeadcount ?? 0
                      const ok = filled >= req
                      return (
                        <td
                          key={d}
                          title={cell?.assigned.map((a) => `${a.personnelNumber} ${a.displayName}`).join(', ')}
                          style={{
                            background: ok
                              ? 'color-mix(in srgb, var(--app-success) 8%, transparent)'
                              : 'color-mix(in srgb, var(--app-danger) 12%, transparent)',
                            fontSize: '0.75rem',
                          }}
                        >
                          {filled}/{req}
                        </td>
                      )
                    })}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>
      )}
    </>
  )
}
