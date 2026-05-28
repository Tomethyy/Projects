export interface LoanInput {
  amount: number;
  apr: number;
  termWeeks: number;
  downPayment: number;
}

export interface LoanResult {
  principal: number;
  paymentPerPeriod: number;
  totalInterest: number;
  totalCost: number;
}

/** Stanton Reserve desk: interest = principal × (APR% / 100) × term weeks */
export function calculateLoan({ amount, apr, termWeeks, downPayment }: LoanInput): LoanResult {
  const principal = Math.max(0, amount - downPayment);
  if (principal <= 0 || termWeeks <= 0) {
    return { principal: 0, paymentPerPeriod: 0, totalInterest: 0, totalCost: downPayment };
  }

  const totalInterest = principal * (apr / 100) * termWeeks;
  const financedTotal = principal + totalInterest;
  const paymentPerPeriod = financedTotal / termWeeks;
  const totalCost = downPayment + financedTotal;

  return {
    principal,
    paymentPerPeriod,
    totalInterest,
    totalCost,
  };
}
