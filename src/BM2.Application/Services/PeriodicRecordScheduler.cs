using BM2.Shared.SystemCodes;

namespace BM2.Application.Services;

public interface IPeriodicRecordScheduler
{
    DateTime CalculateNextDate(DateTime startDate, DateTime lastPlannedDate, Periodicity periodicity);
}

public class PeriodicRecordScheduler : IPeriodicRecordScheduler
{
    public DateTime CalculateNextDate(DateTime startDate, DateTime lastPlannedDate, Periodicity periodicity)
    {
        // Obliczamy, ile interwałów minęło od daty startowej do ostatniego zaplanowanego wykonania
        // Następnie dodajemy kolejny interwał do daty startowej.

        return periodicity switch
        {
            Periodicity.Monthly => GetNextMonthlyOccurrence(startDate, lastPlannedDate),
            Periodicity.Yearly => GetNextYearlyOccurrence(startDate, lastPlannedDate),
            _ => throw new ArgumentOutOfRangeException(nameof(periodicity))
        };
    }

    private DateTime GetNextMonthlyOccurrence(DateTime start, DateTime last)
    {
        // Obliczamy ile pełnych miesięcy upłynęło
        int monthsSinceStart = ((last.Year - start.Year) * 12) + last.Month - start.Month;

        // Planujemy na kolejny miesiąc (stąd + 1)
        return start.AddMonths(monthsSinceStart + 1);
    }

    private DateTime GetNextYearlyOccurrence(DateTime start, DateTime last)
    {
        int yearsSinceStart = last.Year - start.Year;
        return start.AddYears(yearsSinceStart + 1);
    }
}