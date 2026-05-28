export const refuelRates = [
  {
    name: 'Emergency Pad Refuel',
    price: '50,000 aUEC minimum + 35% premium',
    description:
      'Priority response for stranded pilots. Gemini dispatched within 10 minutes. Fuel billed per SCU: Quantum 1,200 aUEC, Hydrogen 250 aUEC.',
  },
  {
    name: 'Standard Hangar Hookup',
    price: '50,000 aUEC minimum',
    description:
      'Scheduled refuel at your hangar or designated pad. Service fee covers hookup; fuel per SCU at market: Quantum 1,200 aUEC, Hydrogen 250 aUEC.',
  },
  {
    name: 'Convoy / Org Bulk',
    price: '1,100 aUEC/SCU QT · 225 aUEC/SCU H2 (min. 500 SCU)',
    description:
      'Volume pricing below street rates for org fleets. 50,000 aUEC minimum service fee per call. Dedicated tanker slot and manifest logging.',
  },
  {
    name: 'Rest Stop Quick Fill',
    price: '50,000 aUEC minimum',
    description:
      'Hydrogen and quantum fuel at supported Stanton rest stops. Per SCU: Quantum 1,200 aUEC, Hydrogen 250 aUEC.',
  },
] as const;

export const bankingRates = [
  {
    name: 'Short-Term Ship Loan',
    price: '165% APR',
    description:
      'Financing for hull upgrades and component refits. Terms from 4–24 weeks. Example: 500,000 aUEC over 12 weeks ≈ 114,000 aUEC interest.',
  },
  {
    name: 'Org Line of Credit',
    price: '95% APR',
    description:
      'Revolving credit for established orgs. Preferential vs. retail. Collateral: fleet assets or aUEC reserve.',
  },
  {
    name: 'Emergency Bridge Loan',
    price: '220% APR',
    description:
      'Fast cash for insurance deductibles and emergency repairs. 48h approval. Highest Reserve desk rate.',
  },
  {
    name: 'Collateral Accepted',
    price: 'Ships, aUEC, commodities',
    description: 'Cutlass and above accepted as collateral. LTV up to 65% appraised value.',
  },
] as const;
