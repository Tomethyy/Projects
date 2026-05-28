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

export function calculateLoan({ amount, apr, termWeeks, downPayment }: LoanInput): LoanResult {
  const principal = Math.max(0, amount - downPayment);
  if (principal <= 0 || termWeeks <= 0) {
    return { principal: 0, paymentPerPeriod: 0, totalInterest: 0, totalCost: downPayment };
  }

  const weeklyRate = apr / 100 / 52;
  const paymentPerPeriod =
    weeklyRate === 0
      ? principal / termWeeks
      : (principal * weeklyRate * Math.pow(1 + weeklyRate, termWeeks)) /
        (Math.pow(1 + weeklyRate, termWeeks) - 1);

  const totalCost = paymentPerPeriod * termWeeks + downPayment;
  const totalInterest = totalCost - amount;

  return {
    principal,
    paymentPerPeriod,
    totalInterest: Math.max(0, totalInterest),
    totalCost,
  };
}
