import { useTranslation } from 'react-i18next'
import PositionsSheet from '../PositionsSheet'
import { useApi } from '../context/ApiProvider'

export default function PositionsPage() {
  const { t } = useTranslation()
  const { api } = useApi()
  return (
    <>
      <header className="page-header">
        <h1>{t('positionsSheet')}</h1>
        <p>{t('positionsSheetHint')}</p>
      </header>
      <PositionsSheet api={api} />
    </>
  )
}
