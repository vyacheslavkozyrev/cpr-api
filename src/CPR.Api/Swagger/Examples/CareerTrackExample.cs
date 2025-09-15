using Swashbuckle.AspNetCore.Filters;
using CPR.Application.Contracts;
using System.Collections.Generic;

namespace CPR.Api.Swagger.Examples
{
    /// <summary>
    /// Provides example career track responses for Swagger UI.
    /// </summary>
    public class CareerTrackExample : IExamplesProvider<IEnumerable<CareerTrackDto>>
    {
        /// <summary>
        /// Returns example <see cref="CareerTrackDto"/> values.
        /// </summary>
        public IEnumerable<CareerTrackDto> GetExamples()
        {
            return new[]
            {
                new CareerTrackDto
                {
                    Id = System.Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Title = "Backend Engineer",
                    Description = "Tracks for backend engineering roles.",
                    CareerPathId = System.Guid.Parse("11111111-1111-1111-1111-111111111111")
                }
            };
        }
    }
}
