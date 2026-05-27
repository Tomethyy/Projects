namespace ShiftEngine.Application.Payroll;

public sealed record PayrollPreviewLine(Guid EmployeeId, string Name, decimal BaseHours, decimal NightPremium, decimal SundayPremium, decimal TotalGrossEstimate);

public static class PayrollPreviewCalculator
{
    public static IReadOnlyList<PayrollPreviewLine> Preview(IEnumerable<(Guid Id, string Name, decimal Hours, bool Night, bool Sunday)> rows)
    {
        return [.. rows.Select(r =>
        {
            var night = r.Night ? r.Hours * 0.25m : 0m;
            var sun = r.Sunday ? r.Hours * 0.5m : 0m;
            return new PayrollPreviewLine(r.Id, r.Name, r.Hours, night, sun, r.Hours * 15m + night + sun);
        })];
    }
}
