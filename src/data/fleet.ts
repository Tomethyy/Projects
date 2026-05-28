export const fleet = [
  {
    id: 'gemini-01',
    name: 'Starfarer Gemini',
    role: 'Primary Refuel Platform',
    image: '/images/gemini-card.png',
    heroImage: '/images/gemini-hero.png',
    specs: {
      crew: '4 recommended',
      fuelCapacity: 'High-volume dual refuel arms',
      responseTime: '~15 min avg. in Stanton',
      armament: 'Defensive turrets (escort-capable)',
    },
    description:
      'The backbone of Tomethy\'s Stellar Reserve. The Gemini handles pad refuels, hangar hookups, and convoy support with twin refueling booms and ample quantum fuel storage.',
    featured: true,
    rsiUrl: 'https://robertsspaceindustries.com/en/ship/starfarer-gemini',
  },
  {
    id: 'c2-support',
    name: 'C2 Hercules',
    role: 'Bulk Fuel Transport',
    image: '/images/support-ship.svg',
    specs: {
      crew: '2–3',
      fuelCapacity: 'Cargo bay fuel pod runs',
      responseTime: 'Scheduled convoys only',
      armament: 'Minimal — logistics focus',
    },
    description:
      'Supports org bulk contracts and rest-stop resupply runs when the Gemini is on active pad duty.',
    featured: false,
    rsiUrl: 'https://robertsspaceindustries.com/en/ship/hercules-c2',
  },
] as const;

export const flagship = fleet.find((ship) => ship.featured)!;
