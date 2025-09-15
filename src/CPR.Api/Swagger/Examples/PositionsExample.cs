using Swashbuckle.AspNetCore.Filters;
using CPR.Application.Contracts;
using System.Collections.Generic;

namespace CPR.Api.Swagger.Examples
{
    /// <summary>
    /// Provides example position responses for Swagger UI.
    /// </summary>
    public class PositionsExample : IExamplesProvider<IEnumerable<PositionDto>>
    {
        /// <summary>
        /// Returns example <see cref="PositionDto"/> values.
        /// </summary>
        public IEnumerable<PositionDto> GetExamples()
        {
            return new[]
            {
                new PositionDto
                {
                    Id = System.Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Title = "Senior Backend Engineer",
                    Description = "Designs and implements backend services.",
                    Expectations = "Own features from design through delivery; mentor juniors.",
                    CareerTrackId = System.Guid.Parse("22222222-2222-2222-2222-222222222222")
                }
            };
        }
    }
}
