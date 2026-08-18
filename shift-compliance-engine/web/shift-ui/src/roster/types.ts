export type ShiftTier = { id: string; code: string; displayName: string }

export type RosterMatrixCell = {
  employeeId: string
  date: string
  assignmentId: string | null
  shiftTierId: string | null
  tierCode: string | null
  tierDisplayName: string | null
  deploymentPostId: string | null
  postName: string | null
}

export type RosterMatrixEmployee = {
  id: string
  personnelNumber: string
  displayName: string
  contractedHoursMonthly: number
}

export type RosterMatrix = {
  periodId: string
  year: number
  month: number
  periodName: string
  isPublished: boolean
  days: string[]
  employees: RosterMatrixEmployee[]
  cells: RosterMatrixCell[]
}

export type SortKey = 'personnelNumber' | 'displayName' | 'contractedHoursMonthly'

export type CellCoord = { row: number; col: number }

export type VirtualRosterGridLabels = {
  colPersonnel: string
  colEmployee: string
  colHours: string
  published: string
  draft: string
}
