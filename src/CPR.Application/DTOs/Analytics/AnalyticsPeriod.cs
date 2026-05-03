using System;

namespace CPR.Application.DTOs.Analytics
{
    /// <summary>
    /// Valid period values accepted by all analytics endpoints (AC-025).
    /// </summary>
    public enum AnalyticsPeriod
    {
        /// <summary>Rolling 30 days back from today.</summary>
        Last30Days,

        /// <summary>Rolling 90 days back from today (default).</summary>
        Last90Days,

        /// <summary>Rolling 180 days back from today.</summary>
        Last180Days,

        /// <summary>The most recently completed calendar quarter.</summary>
        LastQuarter,

        /// <summary>The most recently completed calendar year (Jan 1 – Dec 31).</summary>
        LastYear,
    }

    /// <summary>
    /// Converts a <c>period</c> query-param string to a UTC date window.
    /// Returns null when the string is unrecognised (caller should return 400).
    /// </summary>
    public static class PeriodResolver
    {
        /// <summary>
        /// Parses the raw <paramref name="periodString"/> and returns the resolved period start/end,
        /// or null when the value is unrecognised.
        /// </summary>
        public static (DateTimeOffset Start, DateTimeOffset End, AnalyticsPeriod Period)? Resolve(
            string? periodString, DateTimeOffset utcNow)
        {
            var raw = string.IsNullOrWhiteSpace(periodString) ? "last_90_days" : periodString.Trim().ToLowerInvariant();

            switch (raw)
            {
                case "last_30_days":
                    return (utcNow.AddDays(-30), utcNow, AnalyticsPeriod.Last30Days);

                case "last_90_days":
                    return (utcNow.AddDays(-90), utcNow, AnalyticsPeriod.Last90Days);

                case "last_180_days":
                    return (utcNow.AddDays(-180), utcNow, AnalyticsPeriod.Last180Days);

                case "last_quarter":
                {
                    // Most recently completed calendar quarter
                    var currentQuarter = (utcNow.Month - 1) / 3 + 1;
                    int targetQuarter;
                    int targetYear;
                    if (currentQuarter == 1)
                    {
                        targetQuarter = 4;
                        targetYear = utcNow.Year - 1;
                    }
                    else
                    {
                        targetQuarter = currentQuarter - 1;
                        targetYear = utcNow.Year;
                    }

                    var qStart = new DateTimeOffset(targetYear, (targetQuarter - 1) * 3 + 1, 1, 0, 0, 0, TimeSpan.Zero);
                    var qEnd = qStart.AddMonths(3).AddTicks(-1);
                    return (qStart, qEnd, AnalyticsPeriod.LastQuarter);
                }

                case "last_year":
                {
                    var lastYear = utcNow.Year - 1;
                    var yStart = new DateTimeOffset(lastYear, 1, 1, 0, 0, 0, TimeSpan.Zero);
                    var yEnd = new DateTimeOffset(lastYear, 12, 31, 23, 59, 59, TimeSpan.Zero);
                    return (yStart, yEnd, AnalyticsPeriod.LastYear);
                }

                default:
                    return null;
            }
        }

        /// <summary>
        /// Returns the canonical string representation of a period enum value (matching API contract).
        /// </summary>
        public static string ToApiString(AnalyticsPeriod period)
        {
            return period switch
            {
                AnalyticsPeriod.Last30Days => "last_30_days",
                AnalyticsPeriod.Last90Days => "last_90_days",
                AnalyticsPeriod.Last180Days => "last_180_days",
                AnalyticsPeriod.LastQuarter => "last_quarter",
                AnalyticsPeriod.LastYear => "last_year",
                _ => "last_90_days",
            };
        }
    }
}
