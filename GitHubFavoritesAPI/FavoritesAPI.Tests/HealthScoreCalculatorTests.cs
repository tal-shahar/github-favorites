using RepositoryAnalysisWorker.Services;
using Xunit;

namespace FavoritesAPI.Tests;

public class HealthScoreCalculatorTests
{
    [Theory]
    [InlineData(100, 5, 20, 5, 2000, 100 * 0.4 + 25 * 1.0 + (20d / 6d) * 0.6 + 2)]
    [InlineData(10, 40, 1, 10, 100, 10 * 0.4 + 0 * 1.0 + (1d / 11d) * 0.6 + 1)]
    public void Calculate_ProducesExpectedScore(int stars, int days, int forks, int issues, int readmeLength, double expected)
    {
        var result = HealthScoreCalculator.Calculate(stars, days, forks, issues, readmeLength);

        Assert.Equal(Math.Round(expected, 4), Math.Round(result, 4));
    }
}

