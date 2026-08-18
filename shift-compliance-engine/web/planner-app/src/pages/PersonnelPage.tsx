import { useTranslation } from 'react-i18next'
import PersonnelSheet from '../PersonnelSheet'
import { useApi } from '../context/ApiProvider'

export default function PersonnelPage() {
  const { t } = useTranslation()
  const { api } = useApi()
  return (
    <>
      <header className="page-header">
        <h1>{t('personnelSheet')}</h1>
        <p>{t('personnelSheetHint')}</p>
      </header>
      <PersonnelSheet api={api} onChanged={() => {}} />
    </>
  )
}
