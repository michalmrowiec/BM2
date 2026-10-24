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
        // Obliczamy, ile interwa³ów minê³o od daty startowej do ostatniego zaplanowanego wykonania
        // Nastêpnie dodajemy kolejny interwa³ do daty startowej.

        return periodicity switch
        {
            Periodicity.Monthly => GetNextMonthlyOccurrence(startDate, lastPlannedDate),
            Periodicity.Yearly => GetNextYearlyOccurrence(startDate, lastPlannedDate),
            _ => throw new ArgumentOutOfRangeException(nameof(periodicity))
        };
    }

    private DateTime GetNextMonthlyOccurrence(DateTime start, DateTime last)
    {
        // Obliczamy ile pe³nych miesiêcy up³ynê³o
        int monthsSinceStart = ((last.Year - start.Year) * 12) + last.Month - start.Month;

        // Planujemy na kolejny miesi¹c (st¹d + 1)
        return start.AddMonths(monthsSinceStart + 1);
    }

    private DateTime GetNextYearlyOccurrence(DateTime start, DateTime last)
    {
        int yearsSinceStart = last.Year - start.Year;
        return start.AddYears(yearsSinceStart + 1);
    }
}