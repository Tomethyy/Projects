import { useCallback, useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { VirtualRosterGrid } from '@shift-engine/ui'
import { useApi } from '../context/ApiProvider'
import type { RosterMatrix, ShiftTier } from '../roster-types'

export default function RosterPage() {
  const { api } = useApi()
  const { t } = useTranslation()
  const now = new Date()
  const [year, setYear] = useState(now.getFullYear())
  const [month, setMonth] = useState(now.getMonth() + 1)
  const [anchor, setAnchor] = useState('')
  const [legacySource, setLegacySource] = useState('')
  const [periodId, setPeriodId] = useState('')
  const [matrix, setMatrix] = useState<RosterMatrix | null>(null)
  const [tiers, setTiers] = useState<ShiftTier[]>([])
  const [err, setErr] = useState<string | null>(null)

  const loadMatrix = useCallback(async () => {
    setErr(null)
    try {
      const [m, tierList] = await Promise.all([
        api<RosterMatrix>(`/roster/matrix?year=${year}&month=${month}`),
        api<ShiftTier[]>('/roster/shift-tiers'),
      ])
      setMatrix(m)
      setPeriodId(m.periodId)
      setTiers(tierList)
    } catch {
      setMatrix(null)
      setPeriodId('')
    }
  }, [api, year, month])

  useEffect(() => {
    void loadMatrix()
  }, [loadMatrix])

  async function genTeamMonth(e: React.FormEvent) {
    e.preventDefault()
    setErr(null)
    try {
      const anchorDay = anchor.trim() === '' ? null : anchor.trim()
      await api('/roster/generate-team-month', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          year,
          month,
          anchorFirstWorkDay: anchorDay,
          shiftTierId: null,
          legacySource: legacySource.trim() || null,
          replaceExisting: true,
          staggerTeamAnchors: true,
          assignPosts: true,
        }),
      })
      await loadMatrix()
    } catch (ex) {
      setErr(ex instanceof Error ? ex.message : 'Generate failed')
    }
  }

  const patchTier = useCallback(
    async (assignmentId: string, shiftTierId: string) => {
      setErr(null)
      try {
        await api(`/roster/assignments/${assignmentId}/tier`, {
          method: 'PATCH',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ shiftTierId }),
        })
        setMatrix((prev) => {
          if (!prev) return prev
          const tier = tiers.find((x) => x.id === shiftTierId)
          return {
            ...prev,
            cells: prev.cells.map((c) =>
              c.assignmentId === assignmentId
                ? { ...c, shiftTierId, tierCode: tier?.code ?? c.tierCode, tierDisplayName: tier?.displayName ?? c.tierDisplayName }
                : c,
            ),
          }
        })
      } catch (ex) {
        setErr(ex instanceof Error ? ex.message : 'Update failed')
      }
    },
    [api, tiers],
  )

  const bulkPatchTiers = useCallback(
    async (updates: { assignmentId: string; shiftTierId: string }[]) => {
      setErr(null)
      try {
        await api('/roster/assignments/tiers/bulk', {
          method: 'PATCH',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            updates: updates.map((u) => ({ assignmentId: u.assignmentId, shiftTierId: u.shiftTierId })),
          }),
        })
        setMatrix((prev) => {
          if (!prev) return prev
          const byId = new Map(updates.map((u) => [u.assignmentId, u.shiftTierId]))
          return {
            ...prev,
            cells: prev.cells.map((c) => {
              if (!c.assignmentId || !byId.has(c.assignmentId)) return c
              const shiftTierId = byId.get(c.assignmentId)!
              const tier = tiers.find((x) => x.id === shiftTierId)
              return {
                ...c,
                shiftTierId,
                tierCode: tier?.code ?? c.tierCode,
                tierDisplayName: tier?.displayName ?? c.tierDisplayName,
              }
            }),
          }
        })
      } catch (ex) {
        setErr(ex instanceof Error ? ex.message : 'Bulk update failed')
      }
    },
    [api, tiers],
  )

  async function publishRoster() {
    if (!periodId) return
    setErr(null)
    try {
      await api('/roster/publish', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ rosterPeriodId: periodId }),
      })
      await loadMatrix()
    } catch (ex) {
      setErr(ex instanceof Error ? ex.message : 'Publish failed')
    }
  }

  async function assignPostsOnly() {
    if (!periodId) return
    setErr(null)
    try {
      await api(`/roster/assign-posts?periodId=${periodId}`, { method: 'POST' })
      await loadMatrix()
    } catch (ex) {
      setErr(ex instanceof Error ? ex.message : 'Post assignment failed')
    }
  }

  async function deleteMonth() {
    if (!periodId || !matrix || matrix.isPublished) return
    if (!window.confirm(t('deleteMonthConfirm', { month: `${year}-${String(month).padStart(2, '0')}` }))) return
    setErr(null)
    try {
      await api(`/roster/periods/${periodId}`, { method: 'DELETE' })
      setMatrix(null)
      setPeriodId('')
    } catch (ex) {
      setErr(ex instanceof Error ? ex.message : 'Delete failed')
    }
  }

  const gridLabels = {
    colPersonnel: t('colPersonnel'),
    colEmployee: t('colEmployee'),
    colHours: t('colHours'),
    published: t('published'),
    draft: t('draft'),
  }

  return (
    <>
      <header className="page-header">
        <h1>{t('rosterGrid')}</h1>
        <p>{t('autoRhythmHint')}</p>
        <p className="hint">{t('rosterKeyboardHint')}</p>
      </header>
      {err && <div className="alert alert-error">{err}</div>}
      {matrix && !matrix.isPublished && (
        <div className="publish-banner" role="status">
          <div>
            <strong>{t('draftBanner')}</strong>
            <p>{t('draftBannerHint')}</p>
          </div>
          <button type="button" className="btn btn-primary" onClick={() => void publishRoster()} disabled={!periodId}>
            {t('publishDraft')}
          </button>
        </div>
      )}
      <section className="card">
        <form onSubmit={genTeamMonth}>
          <div className="field-grid">
            <label className="field">
              {t('year')}
              <input type="number" value={year} onChange={(e) => setYear(+e.target.value)} />
            </label>
            <label className="field">
              {t('month')}
              <input type="number" min={1} max={12} value={month} onChange={(e) => setMonth(+e.target.value)} />
            </label>
            <label className="field">
              {t('anchorOptional')}
              <input value={anchor} onChange={(e) => setAnchor(e.target.value)} placeholder={`${year}-${String(month).padStart(2, '0')}-01`} />
            </label>
            <label className="field">
              {t('legacySource')}
              <input value={legacySource} onChange={(e) => setLegacySource(e.target.value)} />
            </label>
          </div>
          <div className="btn-row" style={{ marginTop: '1rem' }}>
            <button type="submit" className="btn btn-primary">
              {t('generateTeamMonth')}
            </button>
            <button type="button" className="btn" onClick={() => void loadMatrix()}>
              {t('refreshGrid')}
            </button>
            <button type="button" className="btn btn-primary" onClick={() => void publishRoster()} disabled={!periodId}>
              {t('publishDraft')}
            </button>
            <button type="button" className="btn" onClick={() => void assignPostsOnly()} disabled={!periodId}>
              {t('assignPosts')}
            </button>
            <button
              type="button"
              className="btn btn-danger"
              onClick={() => void deleteMonth()}
              disabled={!periodId || !matrix || matrix.isPublished}
              title={matrix?.isPublished ? t('deleteMonthPublishedBlocked') : undefined}
            >
              {t('deleteMonth')}
            </button>
          </div>
        </form>
      </section>
      <section className="card">
        <h2>
          {t('rosterGrid')}{' '}
          {matrix && (
            <span className={`badge ${matrix.isPublished ? 'badge-published' : 'badge-draft'}`}>
              {matrix.isPublished ? t('published') : t('draft')}
            </span>
          )}
        </h2>
        {matrix ? (
          <VirtualRosterGrid
            matrix={matrix}
            tiers={tiers}
            labels={gridLabels}
            onTierChange={patchTier}
            onBulkTierChange={bulkPatchTiers}
            bulkToolbar={(count, applyTier) => (
              <>
                <span>{t('bulkSelected', { count })}</span>
                <select defaultValue="" onChange={(ev) => ev.target.value && applyTier(ev.target.value)}>
                  <option value="">{t('bulkApplyTier')}</option>
                  {tiers.map((tier) => (
                    <option key={tier.id} value={tier.id}>
                      {tier.code}
                    </option>
                  ))}
                </select>
              </>
            )}
          />
        ) : (
          <p style={{ color: 'var(--app-text-muted)' }}>{t('noMatrixYet')}</p>
        )}
      </section>
    </>
  )
}
