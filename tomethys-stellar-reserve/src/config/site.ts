export const site = {
  name: "Tomethy's Stellar Reserve",
  tagline: 'Fuel the fleet. Fund the future.',
  owner: 'Tomethy',
  email: 'tomethysbank@proton.me',
  discordUrl: import.meta.env.PUBLIC_DISCORD_URL ?? 'https://discord.gg/twEqUwa8fj',
  rsiOrgUrl: import.meta.env.PUBLIC_RSI_ORG_URL ?? '#',
  formspreeId:
    import.meta.env.PUBLIC_FORMSPREE_ID ?? 'https://formspree.io/f/maqkvnpl',
  responseTime: '24 hours',
} as const;

export const navLinks = [
  { href: '/#status', label: 'Status' },
  { href: '/#services', label: 'Services' },
  { href: '/#fleet', label: 'Fleet' },
  { href: '/#coverage', label: 'Coverage' },
  { href: '/#contracts', label: 'Contracts' },
  { href: '/#faq', label: 'FAQ' },
  { href: '/contracts', label: 'Submit Contract' },
] as const;
