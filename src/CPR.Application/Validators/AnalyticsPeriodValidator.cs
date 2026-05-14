using CPR.Application.DTOs.Analytics;
using FluentValidation;

namespace CPR.Application.Validators
{
    /// <summary>
    /// Validates the raw <c>period</c> query parameter for analytics endpoints.
    /// Maps failure to the i18n key <c>errors.analytics.invalid_period</c> (AC-026).
    /// </summary>
    public class AnalyticsPeriodValidator : AbstractValidator<string?>
    {
        private static readonly string[] ValidValues =
        {
            "last_30_days",
            "last_90_days",
            "last_180_days",
            "last_quarter",
            "last_year",
        };

        /// <summary>
        /// Initializes a new instance of <see cref="AnalyticsPeriodValidator"/>.
        /// </summary>
        public AnalyticsPeriodValidator()
        {
            // Null/empty means default (last_90_days) — always valid.
            // A non-null, non-empty value must be one of the recognised period strings.
            When(period => !string.IsNullOrWhiteSpace(period), () =>
            {
                RuleFor(period => period)
                    .Must(p => System.Array.Exists(ValidValues, v => v == p!.Trim().ToLowerInvariant()))
                    .WithMessage("errors.analytics.invalid_period");
            });
        }
    }
}
