export const contractTypes = [
  {
    id: 'emergency',
    title: 'Emergency Refuel',
    description: 'Stranded with empty tanks? Priority Gemini dispatch to your coordinates.',
    icon: '⚡',
  },
  {
    id: 'hangar',
    title: 'Scheduled Hangar Service',
    description: 'Book a standard hookup at your hangar or org pad.',
    icon: '🔧',
  },
  {
    id: 'bulk',
    title: 'Org Bulk Fuel Contract',
    description: 'Volume pricing for fleet ops, convoys, and org events.',
    icon: '📦',
  },
  {
    id: 'loan',
    title: 'Ship Financing Application',
    description: 'Apply for aUEC loans, lines of credit, or bridge financing.',
    icon: '💳',
  },
  {
    id: 'escort',
    title: 'Escort + Refuel Package',
    description: 'Combined logistics and light escort for high-value cargo runs.',
    icon: '🛡️',
  },
  {
    id: 'recruitment',
    title: 'Contractor Recruitment',
    description: 'Join the Reserve crew as refuel operator or finance liaison.',
    icon: '👤',
  },
] as const;

export type ContractTypeId = (typeof contractTypes)[number]['id'];

export const contractTypeOptions = contractTypes.map(({ id, title }) => ({
  value: id,
  label: title,
}));
