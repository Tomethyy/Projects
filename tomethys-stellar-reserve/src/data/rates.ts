export const refuelRates = [
  {
    name: 'Emergency Pad Refuel',
    price: '2,500 aUEC base + 35% premium',
    description: 'Priority response for stranded pilots. Gemini dispatched within 10 minutes.',
  },
  {
    name: 'Standard Hangar Hookup',
    price: '1,800 aUEC flat',
    description: 'Scheduled refuel at your hangar or designated pad. Includes QT fuel top-off.',
  },
  {
    name: 'Convoy / Org Bulk',
    price: '12 aUEC per SCU (min. 500 SCU)',
    description: 'Volume pricing for org fleets. Dedicated tanker slot and manifest logging.',
  },
  {
    name: 'Rest Stop Quick Fill',
    price: '950 aUEC',
    description: 'Hydrogen and quantum fuel at supported Stanton rest stops.',
  },
] as const;

export const bankingRates = [
  {
    name: 'Short-Term Ship Loan',
    price: '8.5% APR',
    description: 'Financing for hull upgrades and component refits. Terms from 4–24 weeks.',
  },
  {
    name: 'Org Line of Credit',
    price: '6.2% APR',
    description: 'Revolving credit for established orgs. Collateral: fleet assets or aUEC reserve.',
  },
  {
    name: 'Emergency Bridge Loan',
    price: '14% APR',
    description: 'Fast cash for insurance deductibles and emergency repairs. 48h approval.',
  },
  {
    name: 'Collateral Accepted',
    price: 'Ships, aUEC, commodities',
    description: 'Cutlass and above accepted as collateral. LTV up to 65% appraised value.',
  },
] as const;
