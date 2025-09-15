using Swashbuckle.AspNetCore.Filters;
using CPR.Application.Contracts;
using System.Collections.Generic;

namespace CPR.Api.Swagger.Examples
{
    /// <summary>
    /// Provides example career path responses for Swagger UI.
    /// </summary>
    public class CareerPathExample : IExamplesProvider<IEnumerable<CareerPathDto>>
    {
        /// <summary>
        /// Returns a small set of example <see cref="CareerPathDto"/> values.
        /// </summary>
        public IEnumerable<CareerPathDto> GetExamples()
        {
            return new[]
            {
                new CareerPathDto
                {
                    Id = System.Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Title = "Engineering",
                    Description = "Engineering career path covering software and system engineering roles."
                }
            };
        }
    }
}
