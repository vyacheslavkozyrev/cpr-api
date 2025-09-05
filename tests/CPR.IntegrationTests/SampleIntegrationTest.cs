using System.Threading.Tasks;
using Xunit;

namespace CPR.IntegrationTests;

public class SampleIntegrationTest
{
    [Fact]
    public async Task CanRun()
    {
        await Task.Delay(1);
        Assert.True(true);
    }
}
