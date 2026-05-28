export type FaqCategory = 'Refueling' | 'Loans' | 'Contracts' | 'Payment' | 'Recruitment';

export const faqItems: { category: FaqCategory; question: string; answer: string }[] = [
  {
    category: 'Refueling',
    question: 'How does emergency refuel work?',
    answer:
      'Submit an emergency contract with your location and ship type. If Gemini-01 is online, we dispatch within ~10 minutes in Stanton. Premium rates apply for priority response.',
  },
  {
    category: 'Refueling',
    question: 'What fuel types do you provide?',
    answer:
      'Hydrogen for in-atmosphere/maneuvering and quantum fuel for jump travel. Bulk contracts can include pre-positioned fuel pods via C2 support runs.',
  },
  {
    category: 'Loans',
    question: 'What are typical loan terms?',
    answer:
      'Terms are measured in in-universe weeks (4–24). APR ranges from 95% (org credit) to 220% (emergency bridge); standard ship loans run 165% APR. A 500,000 aUEC / 12-week loan typically returns 100,000+ aUEC to the Reserve. Use the loan calculator for estimates.',
  },
  {
    category: 'Loans',
    question: 'What collateral do you accept?',
    answer:
      'Insured ships (Cutlass and above), aUEC reserves, and select commodities. LTV up to 65% of appraised value. Uninsured hulls require higher down payment.',
  },
  {
    category: 'Contracts',
    question: 'How long until I get a response?',
    answer:
      'Standard contracts: within 24 hours. Emergency refuel: immediate queue check via status board. Loan applications: pre-approval within 48 hours.',
  },
  {
    category: 'Contracts',
    question: 'Can orgs set up recurring contracts?',
    answer:
      'Yes. Bulk and hangar service contracts can be scheduled weekly or per-event. Contact the Reserve desk with your org tag and fleet manifest.',
  },
  {
    category: 'Payment',
    question: 'How do I pay?',
    answer:
      'aUEC transfer in-game is preferred. Org accounts can maintain a Reserve balance for instant settlement. Payment details provided upon contract acceptance.',
  },
  {
    category: 'Payment',
    question: 'Are services real or roleplay?',
    answer:
      'All services are in-universe roleplay unless otherwise agreed out-of-character. This site facilitates RP contracts between players.',
  },
  {
    category: 'Recruitment',
    question: 'What do Reserve contractors need?',
    answer:
      'Refuel operators: Starfarer or support ship experience, clean comms, Stanton familiarity. Finance liaisons: org banking RP experience welcome. See Recruitment section.',
  },
  {
    category: 'Recruitment',
    question: 'How do I apply to join the crew?',
    answer:
      'Submit a recruitment contract or reach out on Discord. Include your handle, availability, and ships owned.',
  },
];

export const faqCategories = [...new Set(faqItems.map((item) => item.category))];
